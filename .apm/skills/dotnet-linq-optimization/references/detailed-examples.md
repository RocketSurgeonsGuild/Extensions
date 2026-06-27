});

// GOOD: Project in the query -- single SQL with computed columns
var summaries = await dbContext.Orders
    .Select(o => new
    {
        o.Id,
        CustomerName = o.Customer.Name,
        Total = o.Lines.Sum(l => l.Price * l.Quantity)
    })
    .ToListAsync(ct);

```text

### Pagination with Keyset (Seek) Method

```csharp

// Offset pagination: O(N) -- server must skip rows
var page = await dbContext.Orders
    .OrderBy(o => o.Id)
    .Skip(pageSize * pageNumber)
    .Take(pageSize)
    .ToListAsync(ct);

// Keyset pagination: O(1) -- index seek
var page = await dbContext.Orders
    .Where(o => o.Id > lastSeenId)
    .OrderBy(o => o.Id)
    .Take(pageSize)
    .ToListAsync(ct);

```text

### Batch Operations

```csharp

// BAD: N UPDATE statements (one per tracked entity change)
foreach (var order in orders)
{
    order.Status = OrderStatus.Archived;
}
await dbContext.SaveChangesAsync(ct);
// Generates N individual UPDATE statements in a single round-trip

// GOOD: EF Core 7+ ExecuteUpdateAsync (single SQL statement)
await dbContext.Orders
    .Where(o => o.CreatedAt < cutoff)
    .ExecuteUpdateAsync(
        s => s.SetProperty(o => o.Status, OrderStatus.Archived),
        ct);

```text

---

## Agent Gotchas

1. **Do not cast IQueryable<T> to IEnumerable<T> before filtering** -- this silently switches from server-side SQL
   evaluation to client-side in-memory evaluation, potentially loading entire tables. Check for `AsEnumerable()`,
   explicit casts, or method signatures that accept `IEnumerable<T>`.
2. **Do not return IQueryable<T> from repository methods** -- callers can compose additional operators, but the
   DbContext may be disposed before enumeration. Return materialized collections (`List<T>`) or use
   `IAsyncEnumerable<T>`.
3. **Do not optimize LINQ allocations without benchmarks** -- LINQ iterator overhead is negligible for most business
   logic. Use [skill:dotnet-benchmarkdotnet] `[MemoryDiagnoser]` to prove allocations matter before replacing LINQ with
   manual loops.
4. **Do not use compiled queries with dynamic predicates** -- compiled queries cache the expression tree shape. If the
   query shape changes per call (conditional includes, dynamic filters), the compiled query throws or produces wrong
   results.
5. **Do not enumerate a deferred query multiple times** -- each enumeration re-executes the underlying source (database
   query, network call). Materialize with `ToList()` when the result will be consumed more than once.
6. **Do not use `Skip()`/`Take()` for deep pagination** -- offset pagination is O(N) on the database. Use keyset (seek)
   pagination with a `Where` clause on the last-seen key for consistent performance regardless of page depth.

---

## References

- [EF Core query evaluation](https://learn.microsoft.com/en-us/ef/core/querying/client-eval)
- [EF Core compiled queries](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics#compiled-queries)
- [EF Core efficient querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
- [LINQ execution model (deferred vs immediate)](https://learn.microsoft.com/en-us/dotnet/csharp/linq/get-started/introduction-to-linq-queries#deferred-execution)
- [MemoryExtensions class](https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions)
- [EF Core ExecuteUpdate and ExecuteDelete](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete)
- [Keyset pagination in EF Core](https://learn.microsoft.com/en-us/ef/core/querying/pagination#keyset-pagination)
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
