---
name: dotnet-linq-optimization
category: performance
subcategory: patterns
description: Optimizes LINQ queries. IQueryable vs IEnumerable, compiled queries, deferred exec, allocations.
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

# dotnet-linq-optimization

LINQ performance patterns for .NET applications. Covers the critical distinction between `IQueryable<T>` server-side
evaluation and `IEnumerable<T>` client-side materialization, compiled queries for EF Core hot paths, deferred execution
pitfalls, LINQ-to-Objects allocation patterns and when to drop to manual loops, and Span-based alternatives for
zero-allocation processing.

## Scope

- IQueryable vs IEnumerable materialization pitfalls
- Compiled queries for EF Core hot paths
- Deferred execution and multiple enumeration detection
- LINQ-to-Objects allocation patterns and manual loop alternatives

## Out of scope

- EF Core DbContext lifecycle and migrations -- see [skill:dotnet-efcore-patterns]
- Strategic data architecture (N+1 governance, read/write split) -- see [skill:dotnet-efcore-architecture]
- Span<T> and Memory<T> fundamentals -- see [skill:dotnet-performance-patterns]
- Microbenchmarking setup -- see [skill:dotnet-benchmarkdotnet]

Cross-references: [skill:dotnet-efcore-patterns] for compiled queries in EF Core context and DbContext usage,
[skill:dotnet-performance-patterns] for Span<T>/Memory<T> foundations and ArrayPool patterns,
[skill:dotnet-benchmarkdotnet] for measuring LINQ optimization impact.

---

## IQueryable vs IEnumerable Materialization

The most impactful LINQ performance decision is where evaluation happens: on the database server (`IQueryable<T>`) or in
application memory (`IEnumerable<T>`).

### The Problem

````csharp

// DANGEROUS: Materializes entire table into memory, then filters in C#
IEnumerable<Order> orders = dbContext.Orders;
var recent = orders.Where(o => o.CreatedAt > cutoff).ToList();
// SQL: SELECT * FROM Orders  (no WHERE clause!)

// CORRECT: Filter executes on the database server
IQueryable<Order> orders = dbContext.Orders;
var recent = orders.Where(o => o.CreatedAt > cutoff).ToList();
// SQL: SELECT ... FROM Orders WHERE CreatedAt > @cutoff

```text

### When Materialization Happens

| Operation                                           | Effect                                    |
| --------------------------------------------------- | ----------------------------------------- |
| `ToList()`, `ToArray()`, `ToDictionary()`           | Executes query, loads results into memory |
| `foreach` / `await foreach`                         | Executes query, streams results           |
| `AsEnumerable()`                                    | Switches from server to client evaluation |
| `Count()`, `Any()`, `First()`, `Single()`           | Executes query, returns scalar            |
| `Where()`, `Select()`, `OrderBy()` on `IQueryable`  | Builds expression tree (no execution)     |
| `Where()`, `Select()`, `OrderBy()` on `IEnumerable` | Deferred in-memory evaluation             |

### Common Mistakes

```csharp

// MISTAKE 1: AsEnumerable() before filtering
var results = dbContext.Orders
    .AsEnumerable()           // <-- switches to client evaluation
    .Where(o => o.Total > 100)  // runs in memory, not SQL
    .ToList();

// MISTAKE 2: Calling a C# method in IQueryable predicate
var results = dbContext.Orders
    .Where(o => IsHighValue(o))  // Cannot translate to SQL; throws or falls back
    .ToList();

// FIX: Use expression-compatible predicates or call after materialization
var results = dbContext.Orders
    .Where(o => o.Total > 100)   // SQL-translatable
    .AsEnumerable()
    .Where(o => IsHighValue(o))  // C# logic after materialization
    .ToList();

// MISTAKE 3: Projecting too many columns
var names = dbContext.Orders.ToList().Select(o => o.CustomerName);
// Loads ALL columns, then picks one in memory

// FIX: Project before materializing
var names = dbContext.Orders.Select(o => o.CustomerName).ToList();
// SQL: SELECT CustomerName FROM Orders

```text

### Detection Checklist

- Any `AsEnumerable()` or cast to `IEnumerable<T>` before `Where`/`Select` is a potential server-bypass
- EF Core logs `Microsoft.EntityFrameworkCore.Query` at Warning level when it falls back to client evaluation
- Enable `ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))` during development

---

## Compiled Queries for EF Core Hot Paths

Compiled queries eliminate the per-call expression tree compilation overhead. For queries executed thousands of times
per second, this can reduce overhead significantly.

### Standard Compiled Query

```csharp

public sealed class OrderRepository(AppDbContext db)
{
    // Compiled once, reused across all calls
    private static readonly Func<AppDbContext, Guid, Task<Order?>>
        s_findById = EF.CompileAsyncQuery(
            (AppDbContext ctx, Guid id) =>
                ctx.Orders.FirstOrDefault(o => o.Id == id));

    private static readonly Func<AppDbContext, DateTime, IAsyncEnumerable<Order>>
        s_findRecent = EF.CompileAsyncQuery(
            (AppDbContext ctx, DateTime cutoff) =>
                ctx.Orders
                    .Where(o => o.CreatedAt > cutoff)
                    .OrderByDescending(o => o.CreatedAt));

    public Task<Order?> FindByIdAsync(Guid id) =>
        s_findById(db, id);

    public IAsyncEnumerable<Order> FindRecentAsync(DateTime cutoff) =>
        s_findRecent(db, cutoff);
}

```text

### When to Use Compiled Queries

| Scenario                                      | Use compiled query?                         |
| --------------------------------------------- | ------------------------------------------- |
| High-frequency lookups (auth, caching)        | Yes                                         |
| Admin dashboard queries (low frequency)       | No -- overhead is negligible                |
| Queries with dynamic predicates (user search) | No -- cannot parameterize shape             |
| Queries with `Include()` that varies          | No -- includes change expression tree shape |

### Limitations

- Compiled queries cannot use dynamic `Include()` or conditional `Where()` clauses that change the expression tree shape
- Parameters must be simple types (no complex objects or collections)
- `EF.CompileAsyncQuery` returns `Task<T>` for single results or `IAsyncEnumerable<T>` for collections

---

## Deferred Execution Pitfalls

LINQ uses deferred execution: query operators build a pipeline that executes only when results are consumed. This is
powerful but creates subtle bugs.

### Multiple Enumeration

```csharp

// BUG: Enumerates the database query twice
IQueryable<Order> query = dbContext.Orders.Where(o => o.Status == Status.Active);

var count = query.Count();         // Executes SQL (1st query)
var items = query.ToList();        // Executes SQL again (2nd query)

// FIX: Materialize once
var items = dbContext.Orders
    .Where(o => o.Status == Status.Active)
    .ToList();

var count = items.Count;           // In-memory, no SQL

```text

### Closure Capture in Loops

```csharp

// BUG: All queries capture the same loop variable 'i' by reference
var queries = new List<IQueryable<Order>>();
for (int i = 0; i < statuses.Length; i++)
{
    queries.Add(dbContext.Orders.Where(o => o.Status == statuses[i]));
    // 'i' is captured by reference -- all queries use final value of i
}

// FIX: Copy to a local variable inside the loop body
for (int i = 0; i < statuses.Length; i++)
{
    var localStatus = statuses[i];
    queries.Add(dbContext.Orders.Where(o => o.Status == localStatus));
}

```text

Note: C# 5+ `foreach` loop variables are scoped per iteration and do not exhibit this bug. The `for` loop index variable
is shared across iterations, making this a common pitfall when building deferred LINQ queries in a loop.

### Deferred Execution in Method Returns

```csharp

// DANGEROUS: Returns an unevaluated query -- caller may not realize
// the DbContext could be disposed before enumeration
public IEnumerable<Order> GetActiveOrders()
{
    return dbContext.Orders.Where(o => o.Status == Status.Active);
    // Not evaluated yet -- DbContext may be disposed when caller iterates
}

// SAFE: Materialize before returning
public async Task<List<Order>> GetActiveOrdersAsync(CancellationToken ct)
{
    return await dbContext.Orders
        .Where(o => o.Status == Status.Active)
        .ToListAsync(ct);
}

```text

---

## LINQ-to-Objects Allocation Patterns

LINQ operators on in-memory collections allocate iterators, delegates, and intermediate collections. For hot paths
processing thousands of items per second, these allocations can cause GC pressure.

### Allocation Sources

| Operation                        | Allocations                        |
| -------------------------------- | ---------------------------------- |
| `Where()`, `Select()`            | Iterator object + delegate         |
| `ToList()`, `ToArray()`          | New collection + possible resizing |
| `OrderBy()`                      | Full copy for sorting              |
| `GroupBy()`                      | Dictionary + grouping objects      |
| `SelectMany()`                   | Iterator + inner iterators         |
| Lambda capture of local variable | Closure object per captured scope  |

### When LINQ Allocation Matters

LINQ allocations are negligible for most code. Optimize only when:

- Processing is on a hot path (called thousands of times per second)
- BenchmarkDotNet shows significant `Allocated` bytes
- GC metrics (Gen0 collections/sec) indicate pressure

### Manual Loop Alternatives

```csharp

// LINQ: Allocates iterator + delegate + List<T>
var result = items
    .Where(x => x.IsActive)
    .Select(x => x.Name)
    .ToList();

// Manual loop: Single List<T> allocation, no iterator/delegate overhead
var result = new List<string>(items.Count);
foreach (var item in items)
{
    if (item.IsActive)
    {
        result.Add(item.Name);
    }
}

```text

```csharp

// LINQ: Allocates iterator + delegate + bool boxing (Any)
var hasActive = items.Any(x => x.IsActive);

// Manual loop: Zero allocations beyond the enumerator
var hasActive = false;
foreach (var item in items)
{
    if (item.IsActive)
    {
        hasActive = true;
        break;
    }
}

```text

### Reducing Allocations Without Abandoning LINQ

Before dropping to manual loops, consider these intermediate steps:

```csharp

// 1. Use Array.Find / Array.Exists for arrays (no iterator allocation)
var first = Array.Find(items, x => x.IsActive);
var exists = Array.Exists(items, x => x.IsActive);

// 2. Pre-size collections when count is known
var result = new List<string>(items.Length);
result.AddRange(items.Where(x => x.IsActive).Select(x => x.Name));

// 3. Use static lambdas to avoid delegate allocation (C# 9+)
var result = items.Where(static x => x.IsActive).ToList();
// Note: static lambdas prevent accidental closure capture
// but the delegate is already cached by the compiler for
// non-capturing lambdas; the main benefit is enforcement

```text

---

## Span-Based Alternatives for Collection Processing

For the highest-performance scenarios, `Span<T>` and `ReadOnlySpan<T>` enable stack-based, zero-allocation processing.
These APIs are not LINQ-compatible but cover common patterns.

### Span Search and Filter

```csharp

// Zero-allocation contains check on an array
ReadOnlySpan<int> values = stackalloc int[] { 1, 2, 3, 4, 5 };
bool found = values.Contains(3);

// Zero-allocation index search
int index = values.IndexOf(4);

```text

### MemoryExtensions for String Processing

```csharp

// Zero-allocation split and iterate
ReadOnlySpan<char> csv = "alice,bob,charlie";
foreach (var segment in csv.Split(','))
{
    ReadOnlySpan<char> value = csv[segment];
    // Process each value without allocating strings
}

// Zero-allocation trim and compare
ReadOnlySpan<char> input = "  hello  ";
bool match = input.Trim().SequenceEqual("hello");

```text

### When to Use Span Over LINQ

| Scenario                              | Approach                                   |
| ------------------------------------- | ------------------------------------------ |
| Parsing CSV/log lines in a tight loop | `ReadOnlySpan<char>` + `Split`             |
| Searching sorted arrays               | `Span<T>.BinarySearch`                     |
| Processing byte buffers from I/O      | `ReadOnlySpan<byte>` slicing               |
| General business logic on collections | LINQ (readability over micro-optimization) |

See [skill:dotnet-performance-patterns] for comprehensive Span<T>/Memory<T> patterns and ArrayPool<T> usage.

---

## Query Optimization Patterns

### Projection Before Materialization

Always select only the columns you need:

```csharp

// BAD: Loads entire entity graph
var orders = await dbContext.Orders
    .Include(o => o.Lines)
    .Include(o => o.Customer)
    .ToListAsync(ct);

var summaries = orders.Select(o => new
{
    o.Id,
    o.Customer.Name,
    Total = o.Lines.Sum(l => l.Price * l.Quantity)
});

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
