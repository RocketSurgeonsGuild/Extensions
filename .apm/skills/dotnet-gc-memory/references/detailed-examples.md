
### WeakReference<T>

Weak references allow the GC to collect the target object when no strong references remain. Use for caches where
reclamation under memory pressure is acceptable.

```csharp

public sealed class ImageCache
{
    private readonly ConcurrentDictionary<string, WeakReference<byte[]>> _cache = new();

    public byte[]? TryGet(string key)
    {
        if (_cache.TryGetValue(key, out var weakRef)
            && weakRef.TryGetTarget(out var data))
        {
            return data;
        }
        return null;
    }

    public void Set(string key, byte[] data)
    {
        _cache[key] = new WeakReference<byte[]>(data);
    }

    // Periodically clean up dead references
    public void Purge()
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var weakRef)
                && !weakRef.TryGetTarget(out _))
            {
                _cache.TryRemove(key, out _);
            }
        }
    }
}

```text

### When to Use Weak References

- Large object caches where memory pressure should trigger eviction
- Caches for expensive-to-compute but recreatable data (image thumbnails, rendered templates)
- Do NOT use for small objects -- the `WeakReference<T>` overhead outweighs the benefit

For most caching scenarios, prefer `MemoryCache` with size limits and expiration policies. Weak references are a last
resort when you need GC-driven eviction.

---

## Finalizers vs IDisposable

### IDisposable (Preferred)

Implement `IDisposable` to release unmanaged resources deterministically:

```csharp

public sealed class NativeBufferWrapper : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    public NativeBufferWrapper(int size)
    {
        _handle = Marshal.AllocHGlobal(size);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Marshal.FreeHGlobal(_handle);
        _handle = IntPtr.Zero;
        // No GC.SuppressFinalize needed -- no finalizer
    }
}

```text

### Finalizer (Safety Net Only)

Finalizers run on the GC finalizer thread when an object is collected. They are a safety net for unmanaged resources
that were not disposed explicitly.

```csharp

public class UnmanagedResourceHolder : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    public UnmanagedResourceHolder(int size)
    {
        _handle = Marshal.AllocHGlobal(size);
    }

    ~UnmanagedResourceHolder()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            // Free managed resources
        }

        // Free unmanaged resources
        if (_handle != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_handle);
            _handle = IntPtr.Zero;
        }
    }
}

```text

### Finalizer Costs

| Cost                                                  | Impact                                            |
| ----------------------------------------------------- | ------------------------------------------------- |
| Objects with finalizers survive at least one extra GC | Promotes to Gen1/Gen2, increasing memory pressure |
| Finalizer thread is single-threaded                   | Slow finalizers block all other finalization      |
| Execution order is non-deterministic                  | Cannot depend on other finalizable objects        |
| Not guaranteed to run on process exit                 | Critical cleanup may not execute                  |

**Rule:** Use `sealed` classes with `IDisposable` (no finalizer) unless you own unmanaged handles. Only add a finalizer
as a safety net for unmanaged resources.

---

## Memory Pressure Notifications

### GC.AddMemoryPressure / RemoveMemoryPressure

Inform the GC about unmanaged memory allocations so it accounts for them in collection decisions:

```csharp

public sealed class NativeImageBuffer : IDisposable
{
    private readonly IntPtr _buffer;
    private readonly long _size;
    private bool _disposed;

    public NativeImageBuffer(long sizeBytes)
    {
        _size = sizeBytes;
        _buffer = Marshal.AllocHGlobal((IntPtr)sizeBytes);
        GC.AddMemoryPressure(sizeBytes);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Marshal.FreeHGlobal(_buffer);
        GC.RemoveMemoryPressure(_size);
    }
}

```text

### GC.GetGCMemoryInfo for Adaptive Behavior

```csharp

// React to memory pressure in application logic
var memoryInfo = GC.GetGCMemoryInfo();
double loadPercent = (double)memoryInfo.MemoryLoadBytes
    / memoryInfo.TotalAvailableMemoryBytes * 100;

if (loadPercent > 85)
{
    logger.LogWarning("High memory pressure: {Load:F1}%", loadPercent);
    // Shed load: reduce cache sizes, reject non-critical requests
}

```text

---

## Memory Profiling

### dotMemory (JetBrains)

dotMemory provides heap snapshots and allocation tracking with a visual UI. Use it for investigating memory leaks and
high-allocation hot paths.

**Workflow:**

1. Attach dotMemory to the running process (or launch with profiling enabled)
2. Capture a baseline snapshot after application warm-up
3. Execute the scenario under investigation
4. Capture a second snapshot
5. Compare snapshots to identify retained objects and growth

**Key views:**

- **Sunburst** -- shows allocation tree by type hierarchy
- **Dominator tree** -- shows which objects prevent GC of retained memory
- **Survived objects** -- objects allocated between snapshots that survived GC

### PerfView

PerfView is a free Microsoft tool for detailed GC and allocation analysis. It uses ETW (Event Tracing for Windows)
events for low-overhead profiling.

```bash

# Collect GC and allocation events for 30 seconds
PerfView.exe /GCCollectOnly /MaxCollectSec:30 collect

# Collect allocation stacks (higher overhead)
PerfView.exe /ClrEvents:GC+Stack /MaxCollectSec:30 collect

```text

**Key PerfView views:**

- **GCStats** -- GC pause times, generation counts, promotion rates, fragmentation
- **GC Heap Alloc Stacks** -- call stacks responsible for allocations
- **Any Stacks** -- CPU sampling for identifying hot methods

### Profiling Workflow

1. **Identify the symptom** -- high memory usage, growing Gen2, frequent Gen2 collections, LOH fragmentation
2. **Monitor with dotnet-counters** (see [skill:dotnet-profiling]) to confirm GC metrics match the symptom
3. **Profile with dotMemory or PerfView** to identify the objects and allocation sites
4. **Apply fixes** -- pool buffers, use Span<T>, reduce allocations, fix leaks
5. **Validate with BenchmarkDotNet** (see [skill:dotnet-benchmarkdotnet]) `[MemoryDiagnoser]` to confirm improvement
6. **Monitor in production** via OpenTelemetry runtime metrics (see [skill:dotnet-observability])

---

## Agent Gotchas

1. **Do not default to workstation GC for ASP.NET Core applications** -- server GC is the default and correct choice for
   web workloads. Workstation GC has lower throughput on multi-core servers. Only override for specific
   latency-sensitive scenarios.
2. **Do not forget to return ArrayPool buffers** -- leaked pool buffers are worse than regular allocations because they
   hold pool capacity indefinitely. Always use `try/finally` or `IMemoryOwner<T>` with `using`.
3. **Do not assume rented arrays are the requested size** -- `ArrayPool<T>.Rent()` may return an array larger than
   requested. Always slice to the exact size needed before processing.
4. **Do not add finalizers to classes that only use managed resources** -- finalizers promote objects to Gen1/Gen2 and
   add overhead to GC. Use `sealed class` with `IDisposable` (no finalizer) for managed-only cleanup.
5. **Do not call GC.Collect() in production code** -- forcing full collections causes long pauses and disrupts the GC's
   dynamic tuning. Use `GC.AddMemoryPressure()` to hint at unmanaged memory instead.
6. **Do not ignore LOH fragmentation** -- large arrays (>= 85,000 bytes) allocated and freed repeatedly fragment the
   LOH. Use `ArrayPool<T>` to rent and return large buffers instead of allocating new arrays.
7. **Do not cache IMemoryOwner<T> in long-lived fields without disposal tracking** -- the underlying pooled buffer is
   held indefinitely, preventing pool reuse. Transfer ownership explicitly or limit cache lifetimes.

---

## References

- [Fundamentals of garbage collection](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals)
- [Workstation and server GC](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/workstation-server-gc)
- [Large Object Heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap)
- [Pinned Object Heap](https://devblogs.microsoft.com/dotnet/internals-of-the-poh/)
- [Memory<T> and Span<T> usage guidelines](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines)
- [ArrayPool<T> class](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1)
- [GC.GetGCMemoryInfo](https://learn.microsoft.com/en-us/dotnet/api/system.gc.getgcmemoryinfo)
- [PerfView GC analysis tutorial](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/debug-highcpu?tabs=windows#analyze-with-perfview)
- [Stephen Toub -- Performance Improvements in .NET series](https://devblogs.microsoft.com/dotnet/author/toub/)
  (published annually)
- [IDisposable pattern](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
````

## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**

- **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**

```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
