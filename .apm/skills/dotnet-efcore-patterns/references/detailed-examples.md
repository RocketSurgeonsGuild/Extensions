        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
            }
        }

        return ValueTask.FromResult(result);
    }
}

```text

Combine with a global query filter so soft-deleted entities are excluded by default:

```csharp

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasQueryFilter(p => !p.IsDeleted);
}

// Bypass the filter when needed (e.g., admin queries)
var allProducts = await db.Products
    .IgnoreQueryFilters()
    .ToListAsync(ct);

```text

### Connection Interceptor: Dynamic Connection Strings

```csharp

public sealed class TenantConnectionInterceptor(
    ITenantProvider tenantProvider) : DbConnectionInterceptor
{
    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken ct = default)
    {
        var tenant = tenantProvider.GetCurrentTenant();
        connection.ConnectionString = tenant.ConnectionString;
        return ValueTask.FromResult(result);
    }
}

```text

### Registering Interceptors

```csharp

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(
               sp.GetRequiredService<AuditTimestampInterceptor>(),
               sp.GetRequiredService<SoftDeleteInterceptor>()));

// Register interceptors in DI
builder.Services.AddSingleton<AuditTimestampInterceptor>();
builder.Services.AddSingleton<SoftDeleteInterceptor>();

```text

---

## Compiled Queries

For queries executed very frequently with the same shape, compiled queries eliminate the overhead of expression tree
translation on every call:

```csharp

public static class CompiledQueries
{
    // Single-result compiled query -- delegate does NOT accept CancellationToken
    public static readonly Func<AppDbContext, int, Task<Order?>>
        GetOrderById = EF.CompileAsyncQuery(
            (AppDbContext db, int orderId) =>
                db.Orders
                    .AsNoTracking()
                    .Include(o => o.Items)
                    .FirstOrDefault(o => o.Id == orderId));

    // Multi-result compiled query returns IAsyncEnumerable
    public static readonly Func<AppDbContext, string, IAsyncEnumerable<Order>>
        GetOrdersByCustomer = EF.CompileAsyncQuery(
            (AppDbContext db, string customerId) =>
                db.Orders
                    .AsNoTracking()
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.CreatedAt));
}

// Usage
var order = await CompiledQueries.GetOrderById(db, orderId);

// IAsyncEnumerable results support cancellation via WithCancellation:
await foreach (var o in CompiledQueries.GetOrdersByCustomer(db, customerId)
    .WithCancellation(ct))
{
    // Process each order
}

```text

**When to use:** Compiled queries provide measurable benefit for queries that execute thousands of times per second. For
typical CRUD endpoints, standard LINQ is sufficient -- do not prematurely optimize.

**Cancellation limitation:** Single-result compiled query delegates (`Task<T?>`) do not accept `CancellationToken`. If
per-call cancellation is required, use standard async LINQ (`FirstOrDefaultAsync(ct)`) instead of a compiled query.
Multi-result compiled queries (`IAsyncEnumerable<T>`) support cancellation via `.WithCancellation(ct)` on the async
enumerable.

---

## Connection Resiliency

Transient database failures (network blips, failovers) should be handled with automatic retry. Each provider has a
built-in execution strategy:

```csharp

// PostgreSQL
options.UseNpgsql(connectionString, npgsql =>
    npgsql.EnableRetryOnFailure(
        maxRetryCount: 3,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorCodesToAdd: null));

// SQL Server
options.UseSqlServer(connectionString, sqlServer =>
    sqlServer.EnableRetryOnFailure(
        maxRetryCount: 3,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null));

```text

### Manual Execution Strategies

When you need to wrap multiple `SaveChangesAsync` calls in a single logical transaction with retries:

```csharp

var strategy = db.Database.CreateExecutionStrategy();

await strategy.ExecuteAsync(async () =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(ct);

    var order = await db.Orders.FindAsync([orderId], ct);
    order!.Status = OrderStatus.Completed;
    await db.SaveChangesAsync(ct);

    var payment = new Payment { OrderId = orderId, Amount = order.Total };
    db.Payments.Add(payment);
    await db.SaveChangesAsync(ct);

    await transaction.CommitAsync(ct);
});

```text

**Important:** The entire delegate is re-executed on retry, including the transaction. Ensure the logic is idempotent or
uses database-level uniqueness constraints to prevent duplicates.

---

## Key Principles

- **Keep DbContext short-lived** -- one per request in web apps, one per unit of work in background services via
  `IDbContextFactory<T>`
- **Default to AsNoTracking for reads** -- opt in to tracking only when you need change detection
- **Use split queries for multiple collection Includes** -- avoid Cartesian explosion
- **Never call Database.Migrate() at startup in production** -- use migration bundles or idempotent scripts
- **Register interceptors via DI** -- avoid creating interceptor instances manually
- **Enable connection resiliency** -- transient failures are a fact of life in cloud databases

---

## Agent Gotchas

1. **Do not inject `DbContext` into singleton services** -- `DbContext` is scoped. Injecting it into a singleton
   captures a stale instance. Use `IDbContextFactory<T>` instead.
2. **Do not forget `CancellationToken` propagation** -- pass `ct` to all `ToListAsync()`, `FirstOrDefaultAsync()`,
   `SaveChangesAsync()`, and other async EF Core methods. Omitting it prevents graceful request cancellation.
3. **Do not use `Database.EnsureCreated()` alongside migrations** -- `EnsureCreated()` creates the schema without
   migration history, making subsequent migrations fail. Use it only in test scenarios without migrations.
4. **Do not assume `SaveChangesAsync` is implicitly transactional across multiple calls** -- each `SaveChangesAsync()`
   is its own transaction. Wrap multiple saves in an explicit `BeginTransactionAsync()` / `CommitAsync()` block when
   atomicity is required.
5. **Do not hardcode connection strings** -- read from configuration
   (`builder.Configuration.GetConnectionString("...")`) and inject via environment variables in production.
6. **Do not forget to list required NuGet packages** -- EF Core provider packages
   (`Microsoft.EntityFrameworkCore.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`) and the design-time package
   (`Microsoft.EntityFrameworkCore.Design`) must be referenced explicitly.

---

## References

- [EF Core performance best practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- [DbContext lifetime, configuration, and initialization](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [EF Core interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [EF Core migrations overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core compiled queries](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics#compiled-queries)
- [EF Core connection resiliency](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)
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
