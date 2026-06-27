---
name: dotnet-efcore-patterns
description: EF Core, DbContext, AsNoTracking, query splitting.
license: MIT
targets: ['*']
category: data
subcategory: ef-core
tags:
  - data
  - dotnet
  - skill
  - ef-core
  - database
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-efcore-architecture
  - dotnet-data-access-strategy
  - dotnet-domain-modeling
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for data tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-efcore-patterns

Tactical patterns for Entity Framework Core in .NET applications. Covers DbContext lifetime management, read-only query
optimization, query splitting, migration workflows, interceptors, compiled queries, and connection resiliency. These
patterns apply to EF Core 8+ and are compatible with SQL Server, PostgreSQL, and SQLite providers.

## Scope

- DbContext lifecycle and scoped registration
- AsNoTracking and read-only query optimization
- Query splitting and compiled queries
- Migration workflows and migration bundles for production
- SaveChanges and connection interceptors
- Connection resiliency configuration
- DbContextFactory for background services and Blazor Server

## Out of scope

- Strategic data architecture (read/write split, aggregate boundaries) -- see [skill:dotnet-efcore-architecture]
- Data access technology selection (EF Core vs Dapper vs ADO.NET) -- see [skill:dotnet-data-access-strategy]
- DI container mechanics -- see [skill:dotnet-csharp-dependency-injection]
- Testing EF Core with fixtures -- see [skill:dotnet-integration-testing]
- Domain modeling with DDD patterns -- see [skill:dotnet-domain-modeling]

Cross-references: [skill:dotnet-csharp-dependency-injection] for service registration and DbContext lifetime,
[skill:dotnet-csharp-async-patterns] for cancellation token propagation in queries, [skill:dotnet-efcore-architecture]
for strategic data patterns, [skill:dotnet-data-access-strategy] for data access technology selection.

---

## DbContext Lifecycle

`DbContext` is a unit of work and should be short-lived. In ASP.NET Core, register it as scoped (one per request):

````csharp

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

```csharp

### Lifetime Rules

| Scenario              | Lifetime            | Registration                |
| --------------------- | ------------------- | --------------------------- |
| Web API / MVC request | Scoped (default)    | `AddDbContext<T>()`         |
| Background service    | Scoped via factory  | `AddDbContextFactory<T>()`  |
| Blazor Server         | Scoped via factory  | `AddDbContextFactory<T>()`  |
| Console app           | Transient or manual | `new AppDbContext(options)` |

### DbContextFactory for Long-Lived Services

Background services and Blazor Server circuits outlive a single scope. Use `IDbContextFactory<T>` to create short-lived
contexts on demand:

```csharp

public sealed class OrderProcessor(
    IDbContextFactory<AppDbContext> contextFactory)
{
    public async Task ProcessBatchAsync(CancellationToken ct)
    {
        // Each iteration gets its own short-lived DbContext
        await using var db = await contextFactory.CreateDbContextAsync(ct);

        var pending = await db.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(ct);

        foreach (var order in pending)
        {
            order.Status = OrderStatus.Processing;
        }

        await db.SaveChangesAsync(ct);
    }
}

```text

Register the factory:

```csharp

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

```csharp

**Important:** `AddDbContextFactory<T>()` also registers `AppDbContext` itself as scoped, so controllers and
request-scoped services can still inject `AppDbContext` directly.

### Pooling

`AddDbContextPool<T>()` and `AddPooledDbContextFactory<T>()` reuse `DbContext` instances to reduce allocation overhead.
Use pooling when throughput matters and your context has no injected scoped services:

```csharp

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(connectionString),
    poolSize: 128);  // default is 1024

```csharp

**Pooling constraints:** Pooled contexts are reset and reused. Do not store per-request state on the `DbContext`
subclass. Do not inject scoped services into the constructor -- use `IDbContextFactory<T>` with pooling
(`AddPooledDbContextFactory<T>()`) if you need factory semantics.

---

## AsNoTracking for Read-Only Queries

By default, EF Core tracks all entities returned by queries, enabling change detection on `SaveChangesAsync()`. For
read-only queries, disable tracking to reduce memory and CPU overhead:

```csharp

// Per-query opt-out
var orders = await db.Orders
    .AsNoTracking()
    .Where(o => o.CustomerId == customerId)
    .ToListAsync(ct);

// Per-query with identity resolution (deduplicates entities in the result set)
var ordersWithItems = await db.Orders
    .AsNoTrackingWithIdentityResolution()
    .Include(o => o.Items)
    .Where(o => o.Status == OrderStatus.Active)
    .ToListAsync(ct);

```text

### Default No-Tracking at the Context Level

For read-heavy services, set no-tracking as the default:

```csharp

builder.Services.AddDbContext<ReadOnlyDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

```csharp

Then opt-in to tracking only when needed:

```csharp

var order = await readOnlyDb.Orders
    .AsTracking()
    .FirstAsync(o => o.Id == orderId, ct);

```csharp

---

## Query Splitting

When loading collections via `Include()`, EF Core generates a single SQL query with JOINs by default. This produces a
Cartesian explosion when multiple collections are included.

### The Problem: Cartesian Explosion

```csharp

// Single query: produces Cartesian product of OrderItems x Payments
var orders = await db.Orders
    .Include(o => o.Items)      // N items
    .Include(o => o.Payments)   // M payments
    .ToListAsync(ct);
// Result set: N x M rows per order

```text

### The Solution: Split Queries

```csharp

var orders = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .AsSplitQuery()
    .ToListAsync(ct);
// Executes 3 separate queries: Orders, Items, Payments

```text

### Tradeoffs

| Approach               | Pros                                       | Cons                                         |
| ---------------------- | ------------------------------------------ | -------------------------------------------- |
| Single query (default) | Atomic snapshot, one round-trip            | Cartesian explosion with multiple Includes   |
| Split query            | No Cartesian explosion, less data transfer | Multiple round-trips, no atomicity guarantee |

**Rule of thumb:** Use `AsSplitQuery()` when including two or more collection navigations. Use the default single query
for single-collection includes or when atomicity matters.

### Global Default

Set split queries as the default at the provider level:

```csharp

options.UseNpgsql(connectionString, npgsql =>
    npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

```csharp

Then opt-in to single queries where atomicity is needed:

```csharp

var result = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .AsSingleQuery()
    .ToListAsync(ct);

```text

---

## Migrations

### Migration Workflow

```bash

# Create a migration after model changes
dotnet ef migrations add AddOrderStatus \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api

# Review the generated SQL before applying
dotnet ef migrations script \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api \
    --idempotent \
    --output migrations.sql

# Apply in development
dotnet ef database update \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api

```text

### Migration Bundles for Production

Migration bundles produce a self-contained executable for CI/CD pipelines -- no `dotnet ef` tooling needed on the
deployment server:

```bash

# Build the bundle
dotnet ef migrations bundle \
    --project src/MyApp.Infrastructure \
    --startup-project src/MyApp.Api \
    --output efbundle \
    --self-contained

# Run in production -- pass connection string explicitly via --connection
./efbundle --connection "Host=prod-db;Database=myapp;Username=deploy;Password=<DB_PASSWORD_PLACEHOLDER>"

# Alternatively, configure the bundle to read from an environment variable
# by setting the connection string key in your DbContext's OnConfiguring or
# appsettings.json, then pass the env var at runtime:
# ConnectionStrings__DefaultConnection="Host=..." ./efbundle

```json

### Migration Best Practices

1. **Always generate idempotent scripts** for production deployments (`--idempotent` flag).
2. **Never call `Database.Migrate()` at application startup** in production -- it races with horizontal scaling and
   lacks rollback. Use migration bundles or idempotent scripts applied from CI/CD.
3. **Keep migrations additive** -- add columns with defaults, add tables, add indexes. Avoid destructive changes (drop
   column, rename table) in the same release as code changes.
4. **Review generated code** -- EF Core migration scaffolding can produce unexpected SQL. Always review the `Up()` and
   `Down()` methods.
5. **Use separate migration projects** -- keep migrations in an infrastructure project, not the API project. Specify
   `--project` and `--startup-project` explicitly.

### Data Seeding

Use `HasData()` for reference data that should be part of migrations:

```csharp

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<OrderStatus>().HasData(
        new OrderStatus { Id = 1, Name = "Pending" },
        new OrderStatus { Id = 2, Name = "Processing" },
        new OrderStatus { Id = 3, Name = "Completed" },
        new OrderStatus { Id = 4, Name = "Cancelled" });
}

```text

**Important:** `HasData()` uses primary key values for identity. Changing a seed value's PK in a later migration deletes
the old row and inserts a new one -- it does not update in place.

---

## Interceptors

EF Core interceptors allow cross-cutting concerns to be injected into the database pipeline without modifying entity
logic. Interceptors run for every operation of their type.

### SaveChanges Interceptor: Automatic Audit Timestamps

```csharp

public sealed class AuditTimestampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return ValueTask.FromResult(result);

        var now = DateTimeOffset.UtcNow;

        foreach (var entry in eventData.Context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return ValueTask.FromResult(result);
    }
}

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}

```text

### Soft Delete Interceptor

```csharp

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return ValueTask.FromResult(result);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<ISoftDeletable>())
        {

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
