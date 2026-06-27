
### Governance Checklist

- **Disable lazy loading** -- do not install `Microsoft.EntityFrameworkCore.Proxies` or configure
  `UseLazyLoadingProxies()`. Eager loading via `Include()` or projection via `Select()` makes data access explicit.
- **Review queries in code review** -- look for loops that access navigation properties or call `FindAsync` /
  `FirstOrDefaultAsync` per element.
- **Use query tags** -- `db.Orders.TagWith("GetOrderSummary")` makes queries identifiable in logs and profiling tools.
- **Set up EF Core logging in development** -- every lazy load or unexpected query is visible in the console output.

---

## Row Limits and Pagination

Unbounded queries are a production risk. Always limit the number of rows returned.

### Keyset Pagination (Recommended)

Keyset pagination (also called cursor-based or seek pagination) is more efficient than offset pagination for large
datasets:

```csharp

public async Task<PagedResult<OrderSummary>> GetOrdersAsync(
    string customerId,
    int? afterId,
    int pageSize,
    CancellationToken ct)
{
    const int maxPageSize = 100;
    pageSize = Math.Min(pageSize, maxPageSize);

    var query = db.Orders
        .AsNoTracking()
        .Where(o => o.CustomerId == customerId);

    if (afterId.HasValue)
    {
        query = query.Where(o => o.Id > afterId.Value);
    }

    var items = await query
        .OrderBy(o => o.Id)
        .Take(pageSize + 1)  // Fetch one extra to detect "has next page"
        .Select(o => new OrderSummary
        {
            Id = o.Id,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            Total = o.Items.Sum(i => i.Quantity * i.UnitPrice)
        })
        .ToListAsync(ct);

    var hasNext = items.Count > pageSize;
    if (hasNext)
    {
        items.RemoveAt(items.Count - 1);
    }

    return new PagedResult<OrderSummary>
    {
        Items = items,
        HasNextPage = hasNext,
        NextCursor = hasNext ? items[^1].Id : null
    };
}

```text

### Offset Pagination (Simple Cases)

For admin UIs or small datasets where exact page numbers matter:

```csharp

var page = await db.Orders
    .AsNoTracking()
    .OrderBy(o => o.CreatedAt)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(ct);

```text

**Warning:** Offset pagination degrades at scale -- `OFFSET 10000` forces the database to scan and discard 10,000 rows.
Prefer keyset pagination for user-facing APIs.

### Row Limit Enforcement

Set a hard upper bound on all queries to prevent accidental full-table scans:

```csharp

// Interceptor approach: enforce max rows at the DbContext level
public sealed class RowLimitInterceptor : IQueryExpressionInterceptor
{
    private const int MaxRows = 1000;

    public Expression QueryCompilationStarting(
        Expression queryExpression,
        QueryExpressionEventData eventData)
    {
        // This is a simplified illustration -- actual implementation requires
        // expression tree analysis to detect existing Take() calls.
        // Consider using a code review rule or analyzer instead.
        return queryExpression;
    }
}

```text

**Practical approach:** Rather than a runtime interceptor, enforce row limits through:

1. **Code review convention** -- every `ToListAsync()` must have `Take(N)` or be a `Select()` projection with `Take(N)`.
2. **API-level page size caps** -- validate `pageSize` in the request pipeline before it reaches the query.
3. **Query tags** -- annotate queries with `TagWith()` to identify unbounded queries in monitoring.

---

## Projection Patterns

Projections (`Select()`) are the most effective optimization for read queries. They reduce data transfer, skip change
tracking, and eliminate N+1 risks.

### Typed Projections

```csharp

public sealed record OrderSummary
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = default!;
    public int ItemCount { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

var summaries = await db.Orders
    .Select(o => new OrderSummary
    {
        Id = o.Id,
        CustomerName = o.Customer.Name,
        ItemCount = o.Items.Count,
        Total = o.Items.Sum(i => i.Quantity * i.UnitPrice),
        CreatedAt = o.CreatedAt
    })
    .OrderByDescending(o => o.CreatedAt)
    .Take(50)
    .ToListAsync(ct);

```text

### Advantages Over Entity Loading

| Concern             | Entity + Include          | Projection (Select)   |
| ------------------- | ------------------------- | --------------------- |
| Change tracking     | Yes (unless AsNoTracking) | No                    |
| Data transferred    | All columns               | Only selected columns |
| N+1 risk            | Yes (lazy nav props)      | No (computed in SQL)  |
| Cartesian explosion | Yes (multiple Includes)   | No (single query)     |
| Type safety         | Entity types              | DTO/record types      |

**Rule:** Use projections for all read-only endpoints that return DTOs. Reserve entity loading for commands that modify
data.

---

## Key Principles

- **Separate read and write paths** when you have different optimization needs -- do not force a single model to serve
  both
- **Design aggregates around consistency boundaries** -- not around database tables
- **Reference other aggregates by ID** -- navigation properties between aggregate roots create coupling
- **Ban lazy loading** -- make all data access explicit through `Include()` or `Select()`
- **Enforce row limits** -- every query that returns a list must have an upper bound
- **Project early** -- use `Select()` to push computation to the database and reduce data transfer
- **Prefer keyset pagination** over offset pagination for scalability

---

## Agent Gotchas

1. **Do not create navigation properties between aggregate roots** -- use foreign key values (e.g., `CustomerId`)
   instead of navigation properties (e.g., `Customer`). Cross-aggregate navigation properties break the consistency
   boundary and encourage loading data that belongs to another aggregate.
2. **Do not create generic repositories** (`IRepository<T>`) -- they cannot express aggregate-specific loading rules and
   become leaky abstractions. Create one repository interface per aggregate root with explicit methods.
3. **Do not use `UseLazyLoadingProxies()`** -- lazy loading hides N+1 queries and makes performance unpredictable. Use
   `Include()` for eager loading or `Select()` for projections.
4. **Do not return `IQueryable<T>` from repositories** -- it leaks persistence concerns to callers and makes query
   behavior unpredictable (e.g., multiple enumeration, client-side evaluation). Return materialized results (`List<T>`,
   `T?`).
5. **Do not write `ToListAsync()` without `Take()` on unbounded queries** -- full table scans are a production incident
   waiting to happen. Always limit the result set.
6. **Do not put audit logs or event streams inside aggregates** -- unbounded collections cause slow loads and lock
   contention. Model them as separate entities or dedicated stores.

---

## References

- [EF Core performance best practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- [EF Core loading related data](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)
- [EF Core global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [Domain-driven design with EF Core](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [EF Core query tags](https://learn.microsoft.com/en-us/ef/core/querying/tags)
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
