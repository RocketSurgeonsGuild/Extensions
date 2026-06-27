---
name: dotnet-gc-memory
category: performance
subcategory: memory
description: Tunes GC and memory. GC modes, LOH/POH, Gen0/1/2, Span/Memory deep patterns, ArrayPool.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-gc-memory

Garbage collection and memory management for .NET applications. Covers GC modes (workstation vs server, concurrent vs
non-concurrent), Large Object Heap (LOH) and Pinned Object Heap (POH), generational tuning (Gen0/1/2), memory pressure
notifications, deep Span<T>/Memory<T> ownership patterns beyond basics, buffer pooling with ArrayPool<T> and
MemoryPool<T>, weak references, finalizers vs IDisposable, and memory profiling with dotMemory and PerfView.

## Scope

- GC modes (workstation vs server, concurrent vs non-concurrent)
- Large Object Heap (LOH) and Pinned Object Heap (POH)
- Generational tuning (Gen0/1/2) and memory pressure
- Deep Span<T>/Memory<T> ownership patterns
- Buffer pooling with ArrayPool<T> and MemoryPool<T>
- Memory profiling with dotMemory and PerfView

## Out of scope

- Span<T>/Memory<T> syntax introduction and basic usage -- see [skill:dotnet-performance-patterns]
- Microbenchmarking setup -- see [skill:dotnet-benchmarkdotnet]
- CLI diagnostic tools (dotnet-counters, dotnet-trace, dotnet-dump) -- see [skill:dotnet-profiling]
- Channel<T> producer/consumer patterns -- see [skill:dotnet-channels]

Cross-references: [skill:dotnet-performance-patterns] for Span<T>/Memory<T> basics and sealed devirtualization,
[skill:dotnet-profiling] for runtime diagnostic tools (dotnet-counters, dotnet-trace, dotnet-dump),
[skill:dotnet-channels] for backpressure patterns that interact with memory management, [skill:dotnet-file-io] for
MemoryMappedFile usage and POH buffer patterns in file I/O.

---

## GC Modes and Configuration

### Workstation vs Server GC

| Aspect            | Workstation           | Server                       |
| ----------------- | --------------------- | ---------------------------- |
| **GC threads**    | Single thread         | One thread per logical core  |
| **Heap segments** | Single heap           | One heap per core            |
| **Pause latency** | Lower                 | Higher (more memory scanned) |
| **Throughput**    | Lower                 | Higher                       |
| **Default for**   | Console apps, desktop | ASP.NET Core web apps        |

````xml

<!-- In the .csproj file -->
<PropertyGroup>
  <ServerGarbageCollection>true</ServerGarbageCollection>
</PropertyGroup>

```csharp

```json

// Or in runtimeconfig.json
{
  "runtimeOptions": {
    "configProperties": {
      "System.GC.Server": true
    }
  }
}

```text

### Concurrent vs Non-Concurrent GC

| Mode                     | Behavior                                           | Use when                             |
| ------------------------ | -------------------------------------------------- | ------------------------------------ |
| **Concurrent** (default) | Gen2 collection runs alongside application threads | Latency-sensitive (web APIs, UI)     |
| **Non-concurrent**       | Application threads pause during Gen2 collection   | Maximum throughput, batch processing |

```json

{
  "runtimeOptions": {
    "configProperties": {
      "System.GC.Concurrent": true
    }
  }
}

```text

### DATAS (Dynamic Adaptation to Application Sizes) -- .NET 8+

DATAS dynamically adjusts GC heap size based on application memory usage patterns. It is enabled by default in .NET 8+
Server GC mode. DATAS reduces memory footprint for applications with variable load by shrinking the heap during
low-activity periods.

```json

{
  "runtimeOptions": {
    "configProperties": {
      "System.GC.DynamicAdaptationMode": 1
    }
  }
}

```text

Set to `0` to disable DATAS if you observe excessive GC frequency in steady-state workloads.

### GC Regions -- .NET 7+

Regions replace the older segment-based heap management. Each region is a small, fixed-size block of memory that the GC
can allocate and free independently. Regions are enabled by default in .NET 7+ and improve:

- Memory return to the OS after usage spikes
- Heap compaction efficiency
- Server GC scalability on high-core-count machines

No configuration is needed -- regions are the default. To revert to segments (rarely needed):

```json

{
  "runtimeOptions": {
    "configProperties": {
      "System.GC.Regions": false
    }
  }
}

```text

---

## Generational GC (Gen0/1/2)

### How Generations Work

| Generation | Contains                | Collection frequency         | Collection cost            |
| ---------- | ----------------------- | ---------------------------- | -------------------------- |
| **Gen0**   | Newly allocated objects | Very frequent (milliseconds) | Very cheap (small heap)    |
| **Gen1**   | Objects surviving Gen0  | Frequent                     | Cheap                      |
| **Gen2**   | Long-lived objects      | Infrequent                   | Expensive (full heap scan) |

Objects promote from Gen0 to Gen1 to Gen2 as they survive collections. The GC budget for Gen0 is tuned dynamically --
when Gen0 fills, a Gen0 collection triggers.

### Tuning Principles

1. **Minimize Gen0 allocation rate** -- reduce temporary object creation on hot paths. Every allocation contributes to
   Gen0 pressure.
2. **Avoid mid-life crisis** -- objects that live just long enough to promote to Gen1/Gen2 but then become garbage are
   the most expensive. They survive cheap Gen0 collections and require expensive Gen2 collections to reclaim.
3. **Reduce Gen2 collection frequency** -- Gen2 collections cause the longest pauses. Use object pooling, Span<T>, and
   value types to keep long-lived heap allocations low.

### Monitoring Generations

```bash

# Real-time GC metrics
dotnet-counters monitor --process-id <PID> \
  --counters System.Runtime[gen-0-gc-count,gen-1-gc-count,gen-2-gc-count,gc-heap-size]

```bash

```csharp

// Programmatic GC observation
var gen0 = GC.CollectionCount(0);
var gen1 = GC.CollectionCount(1);
var gen2 = GC.CollectionCount(2);
var totalMemory = GC.GetTotalMemory(forceFullCollection: false);
var memoryInfo = GC.GetGCMemoryInfo();

logger.LogInformation(
    "GC: Gen0={Gen0} Gen1={Gen1} Gen2={Gen2} Heap={HeapMB:F1}MB",
    gen0, gen1, gen2, totalMemory / (1024.0 * 1024));

```text

---

## Large Object Heap (LOH) and Pinned Object Heap (POH)

### LOH

Objects >= 85,000 bytes are allocated on the LOH. LOH collections only happen during Gen2 collections, and by default
the LOH is not compacted (causing fragmentation).

```csharp

// Force LOH compaction (use sparingly -- expensive)
GCSettings.LargeObjectHeapCompactionMode =
    GCLargeObjectHeapCompactionMode.CompactOnce;
GC.Collect();

```text

### LOH Fragmentation Prevention

| Strategy                                   | Implementation                          |
| ------------------------------------------ | --------------------------------------- |
| **ArrayPool<T>** for large arrays          | `ArrayPool<byte>.Shared.Rent(100_000)`  |
| **MemoryPool<T>** for IMemoryOwner pattern | `MemoryPool<byte>.Shared.Rent(100_000)` |
| **Pre-allocate and reuse**                 | Create large buffers once at startup    |
| **Avoid frequent large string concat**     | Use `StringBuilder` or `string.Create`  |

### POH (Pinned Object Heap) -- .NET 5+

The POH is a dedicated heap for objects that must remain at a fixed memory address (pinned). Before .NET 5, pinning
objects on the regular heap prevented compaction. The POH isolates pinned objects so they do not block compaction of
Gen0/1/2 heaps.

```csharp

// Allocate on POH -- useful for I/O buffers passed to native code
byte[] buffer = GC.AllocateArray<byte>(4096, pinned: true);

// The buffer's address will not change, safe for native interop
// and overlapped I/O without explicit GCHandle pinning

```text

Use POH for:

- I/O buffers passed to native/unmanaged code
- Memory-mapped file backing arrays
- Buffers used with `Socket.ReceiveAsync` (overlapped I/O)

---

## Span<T>/Memory<T> Deep Ownership Patterns

See [skill:dotnet-performance-patterns] for Span<T>/Memory<T> introduction and basic slicing. This section covers
ownership semantics and lifetime management for shared buffers.

### IMemoryOwner<T> for Pooled Buffers

```csharp

// Rent from MemoryPool and manage lifetime with IDisposable
using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(4096);
Memory<byte> buffer = owner.Memory[..4096]; // Slice to exact size needed

// Pass the Memory<T> to async I/O
int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
Memory<byte> data = buffer[..bytesRead];

// Process the data
await ProcessDataAsync(data, cancellationToken);
// owner.Dispose() returns the buffer to the pool

```text

### Ownership Transfer Pattern

When transferring buffer ownership between components, use `IMemoryOwner<T>` to make lifetime responsibility explicit:

```csharp

public sealed class MessageParser
{
    // Caller transfers ownership -- this method is responsible for disposal
    public async Task ProcessAsync(
        IMemoryOwner<byte> messageOwner,
        CancellationToken ct)
    {
        using (messageOwner)
        {
            Memory<byte> data = messageOwner.Memory;
            // Parse and process...
            await HandleMessageAsync(data, ct);
        }
        // Buffer returned to pool on dispose
    }
}

```text

### Span<T> Stack Discipline

```csharp

// Span<T> enforces stack-only usage (ref struct)
// These are compile-time errors:
// Span<byte> field;              // Cannot store in class/struct field
// async Task Foo(Span<byte> s);  // Cannot use in async method
// var list = new List<Span<byte>>(); // Cannot use as generic type argument

// When you need heap storage or async, use Memory<T> instead
public async Task ProcessAsync(Memory<byte> buffer, CancellationToken ct)
{
    // Can use Memory<T> in async methods
    int bytesRead = await stream.ReadAsync(buffer, ct);

    // Convert to Span<T> for synchronous processing within a method
    Span<byte> span = buffer.Span;
    ParseHeader(span[..bytesRead]);
}

```text

---

## ArrayPool<T> and MemoryPool<T>

### ArrayPool<T>

`ArrayPool<T>` reduces GC pressure by reusing array allocations. Always return rented arrays, and never assume the
returned array is exactly the requested size.

```csharp

// Rent and return pattern
byte[] buffer = ArrayPool<byte>.Shared.Rent(minimumLength: 4096);
try
{
    // IMPORTANT: Rented array may be larger than requested
    int bytesRead = await stream.ReadAsync(
        buffer.AsMemory(0, 4096), cancellationToken);
    ProcessData(buffer.AsSpan(0, bytesRead));
}
finally
{
    // clearArray: true when buffer contained sensitive data
    ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
}

```text

### Custom Pool Sizing

```csharp

// Create a custom pool for specific allocation patterns
var pool = ArrayPool<byte>.Create(
    maxArrayLength: 1_048_576,  // 1 MB max array
    maxArraysPerBucket: 50);    // Keep up to 50 arrays per size bucket

// Use for workloads with predictable buffer sizes
byte[] buffer = pool.Rent(65_536);
try
{
    // Process...
}
finally
{
    pool.Return(buffer);
}

```text

### MemoryPool<T>

`MemoryPool<T>` wraps `ArrayPool<T>` and returns `IMemoryOwner<T>` for RAII-style lifetime management:

```csharp

// MemoryPool returns IMemoryOwner<T> -- dispose to return
using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(8192);
Memory<byte> buffer = owner.Memory;

// Slice to exact size (owner.Memory may be larger)
int bytesRead = await stream.ReadAsync(buffer[..8192], ct);
await ProcessAsync(buffer[..bytesRead], ct);
// Dispose returns the underlying array to the pool

```text

### Pool Usage Guidelines

| Guideline                                            | Rationale                                              |
| ---------------------------------------------------- | ------------------------------------------------------ |
| Always return rented buffers in `finally` or `using` | Leaked buffers defeat the purpose of pooling           |
| Slice to exact size before processing                | Rented arrays may be larger than requested             |
| Use `clearArray: true` for sensitive data            | Pool reuse could expose secrets to other consumers     |
| Do not cache rented arrays in long-lived fields      | Holds pool buffers indefinitely, reducing availability |
| Prefer `MemoryPool<T>` over raw `ArrayPool<T>`       | Disposal-based lifetime is harder to misuse            |

---

## Weak References and Caching


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
