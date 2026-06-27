---
name: dotnet-csharp-concurrency-patterns
category: fundamentals
subcategory: language-patterns
description: Synchronizes threads and protects shared state. lock, SemaphoreSlim, Interlocked, concurrent collections.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for csharp tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-csharp-concurrency-patterns

Thread synchronization primitives, concurrent data structures, and a decision framework for choosing the right
concurrency mechanism. Covers `lock`/`Monitor`, `SemaphoreSlim`, `Interlocked`, `ConcurrentDictionary`,
`ConcurrentQueue`, `ReaderWriterLockSlim`, and `SpinLock`. This skill is the authoritative source for synchronization
and thread-safe data access patterns.

**Version assumptions:** .NET 8.0+ baseline. All primitives covered are available from .NET Core 1.0+ but examples use
modern C# idioms.

## Scope

- lock/Monitor, SemaphoreSlim, and Interlocked patterns
- ConcurrentDictionary, ConcurrentQueue, and concurrent collections
- ReaderWriterLockSlim and SpinLock for advanced scenarios
- Concurrency primitive decision framework

## Out of scope

- Async/await and Task-based patterns -- see [skill:dotnet-csharp-async-patterns]
- Producer/consumer with Channel<T> -- see [skill:dotnet-channels]
- Naming and style conventions -- see [skill:dotnet-csharp-coding-standards]

Cross-references: [skill:dotnet-csharp-async-patterns] for async/await patterns, [skill:dotnet-channels] for
producer/consumer, [skill:dotnet-csharp-coding-standards] for naming conventions.

---

## Concurrency Primitive Decision Framework

Choose the simplest primitive that meets the requirement. Complexity increases downward:

````text

Is the shared state a single scalar (int, long, reference)?
  YES -> Use Interlocked (lock-free, lowest overhead)

Is the shared state a key-value lookup or queue?
  YES -> Use ConcurrentDictionary / ConcurrentQueue (thread-safe by design)

Does the critical section contain `await`?
  YES -> Use SemaphoreSlim (async-compatible via WaitAsync)
  NO  -> Does the critical section need many readers, few writers?
           YES -> Use ReaderWriterLockSlim (only if profiling shows lock contention)
           NO  -> Use lock (simplest, lowest cognitive overhead)

Is the critical section extremely short (< 100 ns) with high contention?
  YES -> Consider SpinLock (advanced, measure first)

```text

### Quick Reference Table

| Primitive                   | Async-Safe        | Reentrant                        | Use Case                                               |
| --------------------------- | ----------------- | -------------------------------- | ------------------------------------------------------ |
| `lock` / `Monitor`          | No                | Yes (same thread)                | Short critical sections without `await`                |
| `SemaphoreSlim`             | Yes (`WaitAsync`) | No                               | Async-compatible mutual exclusion, throttling          |
| `Interlocked`               | N/A (lock-free)   | N/A                              | Atomic scalar operations (increment, compare-exchange) |
| `ConcurrentDictionary<K,V>` | N/A (thread-safe) | N/A                              | Thread-safe key-value cache/lookup                     |
| `ConcurrentQueue<T>`        | N/A (thread-safe) | N/A                              | Thread-safe FIFO queue                                 |
| `ReaderWriterLockSlim`      | No                | Optional (`LockRecursionPolicy`) | Many-readers/few-writers (profile-driven only)         |
| `SpinLock`                  | No                | No                               | Ultra-short critical sections under extreme contention |

---

## lock and Monitor

`lock` is syntactic sugar for `Monitor.Enter`/`Monitor.Exit`. Use it for short, synchronous critical sections.

### Correct Usage

```csharp

public sealed class Counter
{
    private readonly object _lock = new();
    private int _count;

    public void Increment()
    {
        lock (_lock)
        {
            _count++;
        }
    }

    public int GetCount()
    {
        lock (_lock)
        {
            return _count;
        }
    }
}

```text

### Lock Object Rules

| Rule                                    | Rationale                                                          |
| --------------------------------------- | ------------------------------------------------------------------ |
| Use a private, dedicated `object` field | Prevents external code from locking on the same object             |
| Never lock on `this`                    | Any external code with a reference can cause deadlocks             |
| Never lock on `typeof(T)`               | Global lock shared by all code in the AppDomain                    |
| Never lock on string literals           | String interning means different code may share the same reference |
| Never lock on value types               | Boxing creates a new object each time -- lock is never acquired    |

### Monitor.Wait / Monitor.Pulse

For signaling between threads (producer/consumer without `Channel<T>`):

```csharp

public sealed class BoundedBuffer<T>
{
    private readonly Queue<T> _queue = new();
    private readonly object _lock = new();
    private readonly int _maxSize;

    public BoundedBuffer(int maxSize) => _maxSize = maxSize;

    public void Enqueue(T item)
    {
        lock (_lock)
        {
            while (_queue.Count >= _maxSize)
                Monitor.Wait(_lock);

            _queue.Enqueue(item);
            Monitor.Pulse(_lock);
        }
    }

    public T Dequeue()
    {
        lock (_lock)
        {
            while (_queue.Count == 0)
                Monitor.Wait(_lock);

            var item = _queue.Dequeue();
            Monitor.Pulse(_lock);
            return item;
        }
    }
}

```text

For modern code, prefer `Channel<T>` (see [skill:dotnet-channels]) over Monitor.Wait/Pulse.

---

## SemaphoreSlim

The only built-in .NET synchronization primitive that supports `await`. Use it whenever a critical section contains
async operations.

### Mutual Exclusion (1,1)

```csharp

public sealed class AsyncCache
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<string, object> _cache = new();

    public async Task<T> GetOrAddAsync<T>(string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(key, out var existing))
                return (T)existing;

            var value = await factory(ct);
            _cache[key] = value!;
            return value;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

```text

### Throttling (N concurrent operations)

```csharp

public sealed class ThrottledProcessor
{
    private readonly SemaphoreSlim _throttle;

    public ThrottledProcessor(int maxConcurrency)
        => _throttle = new SemaphoreSlim(maxConcurrency, maxConcurrency);

    public async Task ProcessAllAsync(IEnumerable<WorkItem> items,
        CancellationToken ct = default)
    {
        var tasks = items.Select(async item =>
        {
            await _throttle.WaitAsync(ct);
            try
            {
                await ProcessItemAsync(item, ct);
            }
            finally
            {
                _throttle.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private Task ProcessItemAsync(WorkItem item, CancellationToken ct) =>
        Task.CompletedTask; // implementation
}

```text

### SemaphoreSlim Disposal

`SemaphoreSlim` implements `IDisposable`. Dispose it when the owning object is disposed:

```csharp

public sealed class ManagedResource : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public void Dispose() => _semaphore.Dispose();
}

```text

---

## Interlocked Operations

Lock-free atomic operations for scalar values. The lowest-overhead synchronization mechanism.

### Common Operations

```csharp

private int _counter;
private long _totalBytes;
private object? _current;

// Atomic increment / decrement
Interlocked.Increment(ref _counter);
Interlocked.Decrement(ref _counter);

// Atomic add
Interlocked.Add(ref _totalBytes, bytesRead);

// Atomic exchange -- returns the old value
var previous = Interlocked.Exchange(ref _current, newValue);

// Compare-and-swap -- only writes if current value matches expected
var original = Interlocked.CompareExchange(ref _counter,
    newValue: 10,
    comparand: 0); // Sets to 10 only if current value is 0

```text

### Volatile Read/Write

For visibility guarantees without atomicity (reading the latest value written by another thread):

```csharp

private int _flag;

// Write with release semantics (all prior writes visible to readers)
Volatile.Write(ref _flag, 1);

// Read with acquire semantics (sees all writes prior to the last Volatile.Write)
var value = Volatile.Read(ref _flag);

```text

### Interlocked vs volatile vs lock

| Mechanism             | Atomicity                                                              | Ordering        | Use Case                              |
| --------------------- | ---------------------------------------------------------------------- | --------------- | ------------------------------------- |
| `Interlocked`         | Yes                                                                    | Full fence      | Counters, flags, CAS loops            |
| `Volatile.Read/Write` | No (single read/write is naturally atomic for aligned <= pointer-size) | Acquire/release | Signal flags, publication patterns    |
| `lock`                | Yes (for entire block)                                                 | Full fence      | Multi-step operations on shared state |

---

## ConcurrentDictionary

Thread-safe key-value store. The most commonly used concurrent collection.

### Safe Patterns

```csharp

private readonly ConcurrentDictionary<int, Widget> _cache = new();

// Atomic get-or-add
var widget = _cache.GetOrAdd(id, key => LoadWidget(key));

// Atomic add-or-update
var updated = _cache.AddOrUpdate(id,
    addValueFactory: key => CreateDefault(key),
    updateValueFactory: (key, existing) => existing with { LastAccessed = DateTime.UtcNow });

// Safe removal
if (_cache.TryRemove(id, out var removed))
{
    // Process removed item
}

```text

### Delegate Execution Caveats

`GetOrAdd` and `AddOrUpdate` factory delegates may execute multiple times under contention. Only one result is stored,
but the factory runs for each competing thread:

```csharp

// WRONG -- factory has side effects (database write) that may run multiple times
var widget = _cache.GetOrAdd(id, key =>
{
    var w = new Widget(key);
    _db.Insert(w); // May execute more than once!
    return w;
});

// CORRECT -- use Lazy<T> to ensure factory runs exactly once
private readonly ConcurrentDictionary<int, Lazy<Widget>> _cache = new();

var widget = _cache.GetOrAdd(id,
    key => new Lazy<Widget>(() => LoadAndSaveWidget(key))).Value;

```text

### Composite Operations Are Not Atomic

```csharp

// WRONG -- check-then-act race condition
if (!_cache.ContainsKey(key))
{
    _cache[key] = ComputeValue(key); // Another thread may have added between check and set
}

// CORRECT -- single atomic operation
var value = _cache.GetOrAdd(key, k => ComputeValue(k));

```text

---

## ReaderWriterLockSlim

Allows concurrent reads while serializing writes. Only beneficial when reads significantly outnumber writes AND
profiling shows `lock` contention on the read path.

```csharp

public sealed class ReadHeavyCache<TKey, TValue> : IDisposable
    where TKey : notnull
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    private readonly Dictionary<TKey, TValue> _data = new();

    public TValue? TryGet(TKey key)

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
