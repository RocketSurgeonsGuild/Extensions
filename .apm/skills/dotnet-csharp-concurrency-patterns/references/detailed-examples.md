    public TValue? TryGet(TKey key)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _data.TryGetValue(key, out var value) ? value : default;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public void Set(TKey key, TValue value)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _data[key] = value;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void Dispose() => _rwLock.Dispose();
}

```text

**When NOT to use ReaderWriterLockSlim:**

- Reads and writes are roughly equal -- `lock` is simpler and faster
- Critical sections contain `await` -- not async-compatible; use `SemaphoreSlim`
- You need a concurrent dictionary -- use `ConcurrentDictionary` directly

---

## SpinLock

A low-level primitive for ultra-short critical sections where thread switching overhead exceeds the wait time. **Measure
before using.**

```csharp

private SpinLock _spinLock = new(enableThreadOwnerTracking: false);

public void UpdateCounter()
{
    bool lockTaken = false;
    try
    {
        _spinLock.Enter(ref lockTaken);
        _counter++; // Must be extremely fast -- no I/O, no allocations
    }
    finally
    {
        if (lockTaken)
            _spinLock.Exit(useMemoryBarrier: false);
    }
}

```text

**Rules:**

- Never use `SpinLock` for anything longer than ~100 nanoseconds
- Never use in async code (thread affinity required)
- Never use `enableThreadOwnerTracking: true` in production (debug only -- adds overhead)
- `SpinLock` is a `struct` -- always pass by reference, never copy

---

## Thread-Safe Patterns

### Immutable Snapshots

Prefer immutable data for sharing across threads without synchronization:

```csharp

// Thread-safe via immutability -- no locks needed for reads
private ImmutableList<Widget> _widgets = ImmutableList<Widget>.Empty;

public void AddWidget(Widget widget)
{
    // Atomic swap using Interlocked.CompareExchange loop
    ImmutableList<Widget> original, updated;
    do
    {
        original = _widgets;
        updated = original.Add(widget);
    }
    while (Interlocked.CompareExchange(ref _widgets, updated, original) != original);
}

public ImmutableList<Widget> GetWidgets() => _widgets; // No lock needed

```text

### Double-Checked Locking

For lazy initialization when `Lazy<T>` is not appropriate:

```csharp

private volatile Widget? _instance;
private readonly object _lock = new();

public Widget GetInstance()
{
    var instance = _instance;
    if (instance is not null)
        return instance;

    lock (_lock)
    {
        instance = _instance;
        if (instance is not null)
            return instance;

        instance = CreateWidget();
        _instance = instance;
        return instance;
    }
}

```text

For most cases, prefer `Lazy<T>` which handles this correctly:

```csharp

private readonly Lazy<Widget> _instance = new(() => CreateWidget());
public Widget Instance => _instance.Value;

```csharp

---

## Agent Gotchas

1. **Do not use `lock` inside `async` methods** -- `lock` is thread-affine; the continuation after `await` may resume on
   a different thread, causing `SynchronizationLockException`. Use `SemaphoreSlim.WaitAsync` instead.
2. **Do not assume `volatile` provides atomicity** -- `volatile` only provides ordering guarantees (acquire/release
   semantics). Compound operations like `_counter++` are still non-atomic on volatile fields. Use `Interlocked` for
   atomic operations.
3. **Do not use `ConcurrentDictionary.ContainsKey` followed by indexer set** -- this is a check-then-act race condition.
   Use `GetOrAdd`, `AddOrUpdate`, or `TryAdd` for atomic composite operations.
4. **Do not use `ReaderWriterLockSlim` without profiling evidence** -- it has higher overhead than `lock` and is only
   beneficial when reads significantly outnumber writes. Default to `lock` and only switch if contention is measured.
5. **Do not copy `SpinLock`** -- it is a struct. Copying creates a new, unlocked instance. Always pass by reference and
   store in a field (not a local variable that gets captured by a lambda).
6. **Do not use `lock(this)` or `lock(typeof(T))`** -- external code can acquire the same lock, causing unexpected
   contention or deadlocks. Always use a private, dedicated lock object.
7. **Do not forget to release `SemaphoreSlim` in `finally`** -- if an exception occurs between `WaitAsync` and
   `Release`, the semaphore stays acquired permanently, blocking all subsequent callers.
8. **Do not assume `GetOrAdd` factory executes exactly once** -- under contention, the factory delegate may run on
   multiple threads simultaneously. Only one result is stored, but side effects in the factory execute multiple times.
   Use `Lazy<T>` wrapping for exactly-once semantics.

---

## Prerequisites

- .NET 8.0+ SDK
- Understanding of async/await patterns (see [skill:dotnet-csharp-async-patterns])
- Understanding of producer/consumer patterns (see [skill:dotnet-channels])
- `System.Collections.Concurrent` namespace
- `System.Collections.Immutable` namespace (for immutable collection patterns)

---



## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**
- ✅ **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- ✅ **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- ✅ **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**
```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
## References

- [Threading in C# (Joseph Albahari)](https://www.albahari.com/threading/)
- [Concurrency in C# Cookbook (Stephen Cleary)](https://blog.stephencleary.com/)
- [System.Threading.Interlocked](https://learn.microsoft.com/dotnet/api/system.threading.interlocked)
- [ConcurrentDictionary best practices](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)
- [SemaphoreSlim class](https://learn.microsoft.com/dotnet/api/system.threading.semaphoreslim)
- [ReaderWriterLockSlim class](https://learn.microsoft.com/dotnet/api/system.threading.readerwriterlockslim)
````
