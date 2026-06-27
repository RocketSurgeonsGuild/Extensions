---
name: dotnet-domain-modeling
category: developer-experience
subcategory: cli
description: Models business domains. Aggregates, value objects, domain events, rich models, repositories.
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

# dotnet-domain-modeling

Domain-Driven Design tactical patterns in C#. Covers aggregate roots, entities, value objects, domain events,
integration events, domain services, repository contract design, and the distinction between rich and anemic domain
models. These patterns apply to the domain layer itself -- the pure C# model that encapsulates business rules --
independent of any persistence technology.

## Scope

- Aggregate roots, entities, and value objects
- Domain events and integration events
- Domain services and rich vs anemic model design
- Repository contract design (persistence-agnostic)

## Out of scope

- EF Core configuration and aggregate persistence mapping -- see [skill:dotnet-efcore-architecture]
- Tactical EF Core usage (DbContext lifecycle, migrations) -- see [skill:dotnet-efcore-patterns]
- Input validation at API boundaries -- see [skill:dotnet-validation-patterns]
- Data access technology selection -- see [skill:dotnet-data-access-strategy]
- Vertical slice architecture and request pipelines -- see [skill:dotnet-architecture-patterns]
- Messaging infrastructure and saga orchestration -- see [skill:dotnet-messaging-patterns]

Cross-references: [skill:dotnet-efcore-architecture] for aggregate persistence and repository implementation with EF
Core, [skill:dotnet-efcore-patterns] for DbContext configuration and migrations, [skill:dotnet-architecture-patterns]
for vertical slices and request pipeline design, [skill:dotnet-validation-patterns] for input validation patterns,
[skill:dotnet-messaging-patterns] for integration event infrastructure.

---

## Aggregate Roots and Entities

An aggregate is a cluster of domain objects treated as a single unit for data changes. The aggregate root is the entry
point -- all modifications to the aggregate pass through it.

### Entity Base Class

Entities have identity that persists across state changes. Use a base class to standardize identity and equality:

````csharp

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    // default! required for ORM hydration; Id is set immediately after construction
    public TId Id { get; protected set; } = default!;

    protected Entity() { } // Required for ORM hydration

    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other) =>
        other is not null
        && GetType() == other.GetType()
        && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override int GetHashCode() =>
        EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}

```text

### Aggregate Root Base Class

The aggregate root extends `Entity` and collects domain events:

```csharp

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

```text

### Concrete Aggregate Example

```csharp

public sealed class Order : AggregateRoot<Guid>
{
    public CustomerId CustomerId { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; } = Money.Zero("USD");

    private readonly List<OrderLine> _lines = [];
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    private Order() { } // ORM constructor

    public static Order Create(CustomerId customerId)
    {
        var order = new Order(Guid.NewGuid())
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft
        };

        order.RaiseDomainEvent(new OrderCreated(order.Id, customerId));
        return order;
    }

    public void AddLine(ProductId productId, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot modify a non-draft order.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");

        var line = new OrderLine(productId, quantity, unitPrice);
        _lines.Add(line);
        RecalculateTotal();
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be submitted.");

        if (_lines.Count == 0)
            throw new DomainException("Cannot submit an empty order.");

        Status = OrderStatus.Submitted;
        RaiseDomainEvent(new OrderSubmitted(Id, Total));
    }

    private void RecalculateTotal() =>
        Total = _lines.Aggregate(
            Money.Zero(Total.Currency),
            (sum, line) => sum.Add(line.LineTotal));
}

```text

### Aggregate Design Rules

| Rule                                        | Rationale                                                             |
| ------------------------------------------- | --------------------------------------------------------------------- |
| All mutations go through the aggregate root | Enforces invariants in one place                                      |
| Reference other aggregates by ID only       | Prevents cross-aggregate coupling; use `CustomerId` not `Customer`    |
| Keep aggregates small                       | Large aggregates cause lock contention and slow loads                 |
| One aggregate per transaction               | Cross-aggregate changes use domain events and eventual consistency    |
| Expose collections as `IReadOnlyList<T>`    | Prevents external code from bypassing root methods to mutate children |

For the EF Core persistence implications of these rules (navigation properties, owned types, cascade behavior), see
[skill:dotnet-efcore-architecture].

---

## Value Objects

Value objects have no identity -- they are defined by their attribute values. Two value objects with the same attributes
are equal. In C#, `record` and `record struct` provide natural value semantics.

### Record-Based Value Objects

```csharp

// Simple value object -- wraps a primitive to enforce constraints
public sealed record CustomerId
{
    public string Value { get; }

    public CustomerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Customer ID cannot be empty.");

        Value = value;
    }

    public override string ToString() => Value;
}

// Composite value object -- multiple properties with validation
public sealed record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state,
                   string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street is required.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Postal code is required.");

        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}

```text

### Money Value Object

Money is the canonical example of a multi-field value object with behavior:

```csharp

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int quantity) =>
        new(Amount * quantity, Currency);

    public Money Multiply(decimal factor) =>
        new(Amount * factor, Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException(
                $"Cannot operate on {Currency} and {other.Currency}.");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}

```text

### Value Object EF Core Mapping

Map value objects using owned types or value conversions (implementation in [skill:dotnet-efcore-architecture]):

```csharp

// Owned type -- maps to columns in the parent table
builder.OwnsOne(o => o.Total, money =>
{
    money.Property(m => m.Amount).HasColumnName("TotalAmount");
    money.Property(m => m.Currency).HasColumnName("TotalCurrency")
        .HasMaxLength(3);
});

// Value conversion -- single-property value objects
builder.Property(o => o.CustomerId)
    .HasConversion(
        id => id.Value,
        value => new CustomerId(value))
    .HasMaxLength(50);

```text

### When to Use Value Objects

| Use value object                                           | Use primitive                                                        |
| ---------------------------------------------------------- | -------------------------------------------------------------------- |
| Domain concept with constraints (email, money, quantity)   | Infrastructure IDs with no domain rules (correlation IDs, trace IDs) |
| Multiple properties that form a unit (address, date range) | Single value with no validation needed                               |
| Need to prevent primitive obsession in domain methods      | Simple DTO fields at API boundary                                    |

---

## Domain Events

Domain events represent something meaningful that happened in the domain. They enable loose coupling between aggregates
and trigger side effects (sending emails, updating read models, publishing integration events).

### Event Contracts

```csharp

// Marker interface for all domain events
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

// Base record for convenience
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

// Concrete events
public sealed record OrderCreated(
    Guid OrderId, CustomerId CustomerId) : DomainEventBase;

public sealed record OrderSubmitted(
    Guid OrderId, Money Total) : DomainEventBase;

public sealed record OrderCancelled(
    Guid OrderId, string Reason) : DomainEventBase;

```text

### Dispatching Domain Events

Dispatch events after `SaveChangesAsync` succeeds to ensure the aggregate state is persisted before side effects
execute:

```csharp

public sealed class DomainEventDispatcher(
    IServiceProvider serviceProvider)
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events,
        CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEvent.GetType());

            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                await ((dynamic)handler).HandleAsync(
                    (dynamic)domainEvent, ct);
            }
        }
    }
}

// Note: The (dynamic) dispatch pattern is simple but not AOT-compatible.
// For Native AOT scenarios, use a source-generated or dictionary-based
// dispatcher. See [skill:dotnet-native-aot] for AOT constraints.

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
