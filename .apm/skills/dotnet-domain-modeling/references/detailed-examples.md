// dispatcher. See [skill:dotnet-native-aot] for AOT constraints.

// Handler interface
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}

```text

### Saving with Event Dispatch

Use an EF Core `SaveChangesInterceptor` or a wrapper to dispatch events after save:

```csharp

public sealed class EventDispatchingSaveChangesInterceptor(
    DomainEventDispatcher dispatcher)
    : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct)
    {
        if (eventData.Context is not null)
        {
            var aggregates = eventData.Context.ChangeTracker
                .Entries<AggregateRoot<Guid>>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity)
                .ToList();

            var events = aggregates
                .SelectMany(a => a.DomainEvents)
                .ToList();

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            await dispatcher.DispatchAsync(events, ct);
        }

        return result;
    }
}

```text

### Domain Events vs Integration Events

| Aspect      | Domain Event                              | Integration Event                                       |
| ----------- | ----------------------------------------- | ------------------------------------------------------- |
| Scope       | Within a bounded context                  | Across bounded contexts / services                      |
| Transport   | In-process (dispatcher)                   | Message broker (Service Bus, RabbitMQ)                  |
| Coupling    | References domain types                   | Uses primitive/DTO types only                           |
| Reliability | Same transaction scope                    | At-least-once with idempotent consumers                 |
| Example     | `OrderSubmitted` (triggers email handler) | `OrderSubmittedIntegration` (notifies shipping service) |

A domain event handler may publish an integration event to a message broker. See [skill:dotnet-messaging-patterns] for
integration event infrastructure.

```csharp

// Domain event handler that publishes an integration event
public sealed class OrderSubmittedHandler(
    IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<OrderSubmitted>
{
    public async Task HandleAsync(
        OrderSubmitted domainEvent, CancellationToken ct)
    {
        // Map domain event to integration event (no domain types)
        await publishEndpoint.Publish(
            new OrderSubmittedIntegration(
                domainEvent.OrderId,
                domainEvent.Total.Amount,
                domainEvent.Total.Currency),
            ct);
    }
}

```text

---

## Rich vs Anemic Domain Models

### Rich Domain Model

Business logic lives inside the domain entities. Methods enforce invariants and return meaningful results:

```csharp

public sealed class ShoppingCart : AggregateRoot<Guid>
{
    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    public void AddItem(ProductId productId, int quantity, Money unitPrice)
    {
        var existing = _items.Find(i => i.ProductId == productId);

        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(productId, quantity, unitPrice));
        }
    }

    public void RemoveItem(ProductId productId)
    {
        var item = _items.Find(i => i.ProductId == productId)
            ?? throw new DomainException(
                $"Product {productId} not in cart.");

        _items.Remove(item);
    }

    public Money GetTotal(string currency) =>
        _items.Aggregate(
            Money.Zero(currency),
            (sum, item) => sum.Add(item.LineTotal));
}

```text

### Anemic Domain Model (Anti-Pattern)

Entities are data bags with public setters. Business logic lives in external services:

```csharp

// ANTI-PATTERN: Entity is just a data container
public class ShoppingCart
{
    public Guid Id { get; set; }
    public List<CartItem> Items { get; set; } = [];
}

// All logic lives here -- the entity has no behavior
public class ShoppingCartService
{
    public void AddItem(ShoppingCart cart, string productId,
        int quantity, decimal unitPrice)
    {
        var existing = cart.Items.Find(i => i.ProductId == productId);
        if (existing != null)
            existing.Quantity += quantity;
        else
            cart.Items.Add(new CartItem { ... });
    }
}

```text

### Decision Guide

| Factor               | Rich model                            | Anemic model                      |
| -------------------- | ------------------------------------- | --------------------------------- |
| Complex invariants   | Enforced in entity                    | Scattered across services         |
| Testability          | Test entity behavior directly         | Test service + entity together    |
| Discoverability      | Methods on entity show capabilities   | Must find the right service class |
| Persistence coupling | Requires ORM-friendly private setters | Simple property mapping           |
| Team familiarity     | DDD experience required               | Familiar to most developers       |

**Recommendation:** Start with a rich model for aggregates with complex business rules. Anemic models are acceptable for
simple CRUD entities where the domain logic is minimal (e.g., reference data, configuration records).

---

## Domain Services

Domain services encapsulate business logic that does not naturally belong to a single entity or value object. They
operate on domain types and enforce cross-aggregate rules.

```csharp

public sealed class PricingService
{
    public Money CalculateDiscount(
        Order order,
        CustomerTier tier,
        IReadOnlyList<PromotionRule> activePromotions)
    {
        var discount = Money.Zero(order.Total.Currency);

        // Tier-based discount
        discount = tier switch
        {
            CustomerTier.Gold => discount.Add(
                order.Total.Multiply(0.10m)),
            CustomerTier.Platinum => discount.Add(
                order.Total.Multiply(0.15m)),
            _ => discount
        };

        // Promotion-based discounts
        foreach (var promo in activePromotions)
        {
            if (promo.AppliesTo(order))
            {
                discount = discount.Add(promo.Calculate(order));
            }
        }

        return discount;
    }
}

```text

### When to Use Domain Services

- Logic requires data from **multiple aggregates** that should not reference each other
- A business rule does not belong to any single entity (e.g., pricing across products and customer tiers)
- External policy or configuration drives the logic (e.g., tax calculation rules)

Domain services should remain **pure** -- no infrastructure dependencies. If the logic needs a database or external API,
place it in an application service that calls the domain service with pre-loaded data.

---

## Repository Contracts

Repository interfaces belong in the **domain layer** and express aggregate loading and saving semantics. Implementation
details (EF Core, Dapper) live in the infrastructure layer.

```csharp

// Domain layer -- defines the contract
public interface IOrderRepository
{
    Task<Order?> FindByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// Domain layer -- unit of work abstraction (optional)
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}

```text

For EF Core repository implementations, see [skill:dotnet-efcore-architecture].

### Repository Design Rules

| Rule                                             | Rationale                                                  |
| ------------------------------------------------ | ---------------------------------------------------------- |
| One repository per aggregate root                | Child entities are accessed through the root               |
| No `IQueryable<T>` return types                  | Prevents persistence concerns from leaking into domain     |
| No generic `IRepository<T>`                      | Cannot express aggregate-specific loading rules            |
| Return domain types, not DTOs                    | Repositories serve the domain; read models use projections |
| Include `CancellationToken` on all async methods | Required for proper cancellation propagation               |

---

## Domain Exceptions

Use domain-specific exceptions to signal invariant violations. This separates domain errors from infrastructure errors:

```csharp

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner)
        : base(message, inner) { }
}

// Specific domain exceptions for different invariant violations
public sealed class InsufficientStockException(
    ProductId productId, int requested, int available)
    : DomainException(
        $"Insufficient stock for {productId}: " +
        $"requested {requested}, available {available}")
{
    public ProductId ProductId => productId;
    public int Requested => requested;
    public int Available => available;
}

```text

Map domain exceptions to HTTP responses at the API boundary (e.g., `DomainException` to 422 Unprocessable Entity). Do
not let infrastructure concerns like HTTP status codes leak into the domain layer.

---

## Agent Gotchas

1. **Do not expose public setters on aggregate properties** -- all state changes must go through methods on the
   aggregate root that enforce invariants. Use `private set` or `init` for properties.
2. **Do not create navigation properties between aggregate roots** -- reference other aggregates by ID value objects
   (e.g., `CustomerId`) not by entity navigation. Cross-aggregate navigation breaks bounded context isolation.
3. **Do not dispatch domain events inside the transaction** -- dispatch after `SaveChangesAsync` succeeds. Dispatching
   before save means side effects fire even if the save fails.
4. **Do not use domain types in integration events** -- integration events cross bounded context boundaries and must use
   primitives or DTOs. Domain type changes would break other services.
5. **Do not put validation logic only in the API layer** -- domain invariants belong in the domain model. API validation
   ([skill:dotnet-validation-patterns]) catches malformed input; domain validation enforces business rules.
6. **Do not create anemic entities with public `List<T>` properties** -- expose collections as `IReadOnlyList<T>` and
   provide mutation methods on the aggregate root that enforce business rules.
7. **Do not inject infrastructure services into domain entities** -- entities should be pure C# objects. Use domain
   services for logic that needs external data, and application services for infrastructure orchestration.

---

## References

- [Domain-driven design with EF Core](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [Implementing domain events](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
- [Value objects in DDD](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects)
- [Aggregate design rules (Vaughn Vernon)](https://www.dddcommunity.org/library/vernon_2011/)
- [EF Core owned entity types](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities)
- [Repository pattern in .NET](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
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
