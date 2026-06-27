---
name: dotnet-efcore-architecture
category: data
subcategory: ef-core
description: Designs EF Core data layer architecture. Read/write split, aggregate boundaries, N+1 governance.
license: MIT
targets: ['*']
tags: [architecture, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for architecture tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-efcore-architecture

Strategic architectural patterns for EF Core data layers. Covers read/write model separation, aggregate boundary design,
repository vs direct DbContext policy, N+1 query governance, row limit enforcement, and projection patterns. These
patterns guide how to structure a data layer -- not how to write individual queries (see [skill:dotnet-efcore-patterns]
for tactical usage).

## Scope

- Read/write model separation and CQRS patterns
- Aggregate boundary design and repository policy
- N+1 query governance and row limit enforcement
- Projection patterns and query optimization strategy

## Out of scope

- Tactical EF Core usage (DbContext lifecycle, AsNoTracking, migrations, interceptors) -- see
  [skill:dotnet-efcore-patterns]
- Data access technology selection (EF Core vs Dapper vs ADO.NET) -- see [skill:dotnet-data-access-strategy]
- DI container mechanics -- see [skill:dotnet-csharp-dependency-injection]
- Async patterns -- see [skill:dotnet-csharp-async-patterns]
- Integration testing data layers -- see [skill:dotnet-integration-testing]

Cross-references: [skill:dotnet-efcore-patterns] for tactical DbContext usage and migrations,
[skill:dotnet-data-access-strategy] for technology selection, [skill:dotnet-csharp-dependency-injection] for service
registration, [skill:dotnet-csharp-async-patterns] for async query patterns.

---

## Package Prerequisites

Examples in this skill use PostgreSQL (`UseNpgsql`). Substitute the provider package for your database:

| Database   | Provider Package                          |
| ---------- | ----------------------------------------- |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL`   |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` |
| SQLite     | `Microsoft.EntityFrameworkCore.Sqlite`    |

All examples also require the core `Microsoft.EntityFrameworkCore` package (pulled in transitively by provider
packages).

---

## Read/Write Model Separation

Separate read models (queries) from write models (commands) to optimize each path independently. This is not full CQRS
-- it is a practical separation using EF Core features.

### Approach: Separate DbContext Types

````csharp

// Write context: full change tracking, navigation properties, interceptors
public sealed class WriteDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
    }
}

// Read context: no-tracking by default, optimized for projections
public sealed class ReadDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Note: this is supplemental -- primary config is in DI registration
    }
}

```text

### Registration

```csharp

// Write context: standard tracking, connection resiliency
builder.Services.AddDbContext<WriteDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

// Read context: no-tracking, optionally pointed at a read replica
builder.Services.AddDbContext<ReadDbContext>(options =>
    options.UseNpgsql(readReplicaConnectionString ?? connectionString)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

```text

### When to Separate

| Scenario                            | Recommendation                                      |
| ----------------------------------- | --------------------------------------------------- |
| Simple CRUD app                     | Single `DbContext` with per-query `AsNoTracking()`  |
| Read-heavy API with complex queries | Separate read/write contexts                        |
| Read replica database               | Separate contexts with different connection strings |
| CQRS architecture                   | Separate contexts, possibly separate models         |

**Start simple.** Use a single `DbContext` and per-query `AsNoTracking()` until you have a concrete reason to split
(different connection strings, divergent model shapes, or query complexity that justifies dedicated read models).

---

## Aggregate Boundaries

An aggregate is a cluster of entities that are always loaded and saved together as a consistency boundary. EF Core maps
well to aggregate-oriented design when navigation properties follow aggregate boundaries.

### Defining Aggregates

```csharp

// Order is the aggregate root -- it owns OrderItems
public sealed class Order
{
    public int Id { get; private set; }
    public string CustomerId { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Owned collection -- part of the Order aggregate
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(int productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft order.");

        _items.Add(new OrderItem(productId, quantity, unitPrice));
    }
}

// OrderItem belongs to the Order aggregate -- no independent access
public sealed class OrderItem
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    internal OrderItem(int productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    private OrderItem() { } // EF Core constructor
}

```text

### EF Core Configuration for Aggregates

```csharp

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Status).HasConversion<string>();

        // Owned collection navigation -- cascade delete, no independent DbSet
        builder.OwnsMany(o => o.Items, items =>
        {
            items.WithOwner().HasForeignKey("OrderId");
            items.Property(i => i.ProductId).IsRequired();
        });

        // Alternatively, if OrderItem needs its own table with explicit FK:
        // builder.HasMany(o => o.Items)
        //     .WithOne()
        //     .HasForeignKey("OrderId")
        //     .OnDelete(DeleteBehavior.Cascade);
        //
        // builder.Navigation(o => o.Items)
        //     .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

```text

### Aggregate Design Rules

1. **Load the entire aggregate** -- do not load partial aggregates. Use `Include()` for the owned collections.
2. **Save through the aggregate root** -- call `SaveChangesAsync()` on the root, not on child entities independently.
3. **Reference other aggregates by ID** -- do not create navigation properties between aggregate roots. Use `CustomerId`
   (foreign key value), not `Customer` (navigation property).
4. **Keep aggregates small** -- large aggregates cause lock contention and slow loads. If a collection grows unbounded
   (e.g., audit logs), it does not belong in the aggregate.
5. **One aggregate per transaction** -- modifying multiple aggregates in a single transaction creates coupling. Use
   domain events or eventual consistency for cross-aggregate operations.

---

## Repository Policy

Whether to use the repository pattern or access `DbContext` directly is a team decision. Both approaches are valid in
.NET.

### Option A: Direct DbContext Access

```csharp

public sealed class CreateOrderHandler(WriteDbContext db)
{
    public async Task<int> HandleAsync(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        var order = new Order(command.CustomerId);

        foreach (var item in command.Items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        return order.Id;
    }
}

```text

**Pros:** Simple, no abstraction overhead, full LINQ power, easy to debug. **Cons:** Business logic can leak into query
methods, harder to unit test without a database.

### Option B: Repository per Aggregate Root

```csharp

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class OrderRepository(WriteDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        await db.Orders.AddAsync(order, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return db.SaveChangesAsync(ct);
    }
}

```text

**Pros:** Testable without a database, encapsulates query logic, enforces aggregate loading rules. **Cons:** Extra
abstraction layer, can become a leaky abstraction if LINQ is exposed, repository per aggregate can proliferate.

### Decision Guide

| Factor               | Direct DbContext               | Repository                    |
| -------------------- | ------------------------------ | ----------------------------- |
| Team size            | Small, aligned                 | Large, varied experience      |
| Test strategy        | Integration tests with real DB | Unit tests with mocked repos  |
| Query complexity     | High (reports, projections)    | Low-medium (CRUD, aggregates) |
| Aggregate discipline | Enforced by convention         | Enforced by interface         |

**Do not create generic repositories** (`IRepository<T>`). They add abstraction without value -- the generic interface
cannot express aggregate-specific loading rules (which Includes to use, which filters to apply). Repository interfaces
should be specific to the aggregate root they serve.

---

## N+1 Query Governance

N+1 queries are the most common EF Core performance problem. They occur when code iterates over a collection and
executes a query per element, instead of loading all data upfront.

### Detection

Enable sensitive logging in development to see SQL queries:

```csharp

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging()  // Development only
           .EnableDetailedErrors());      // Development only

```text

### Common N+1 Patterns and Fixes

**Pattern 1: Lazy loading in a loop**

```csharp

// BAD: N+1 -- each order.Items triggers a query
var orders = await db.Orders.ToListAsync(ct);
foreach (var order in orders)
{
    var total = order.Items.Sum(i => i.Quantity * i.UnitPrice); // Lazy load!
}

// GOOD: Eager load with Include
var orders = await db.Orders
    .Include(o => o.Items)
    .ToListAsync(ct);

```text

**Pattern 2: Querying inside a loop**

```csharp

// BAD: N+1 -- one query per customer
foreach (var customerId in customerIds)
{
    var orders = await db.Orders
        .Where(o => o.CustomerId == customerId)
        .ToListAsync(ct);
    // ...
}

// GOOD: Single query with Contains
var orders = await db.Orders
    .Where(o => customerIds.Contains(o.CustomerId))
    .ToListAsync(ct);

```text

**Pattern 3: Missing projection**

```csharp

// BAD: Loads full entity graph, then maps in memory
var orders = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Customer)
    .ToListAsync(ct);
var dtos = orders.Select(o => new OrderDto(...));

// GOOD: Project in the query -- no tracking, no extra data loaded
var dtos = await db.Orders
    .Select(o => new OrderDto
    {
        Id = o.Id,
        CustomerName = o.Customer.Name,
        ItemCount = o.Items.Count,
        Total = o.Items.Sum(i => i.Quantity * i.UnitPrice)
    })
    .ToListAsync(ct);

```text


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
