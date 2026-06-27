---
name: dotnet-solid-principles
description: Applies SOLID and DRY principles. C# anti-patterns, fixes, SRP compliance checks.
license: MIT
targets: ['*']
category: fundamentals
subcategory: design-principles
tags:
  - dotnet
  - skill
  - solid
  - dry
  - design-principles
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-csharp-coding-standards
  - dotnet-csharp-api-design
  - dotnet-csharp-code-smells
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for design principles'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-solid-principles

Foundational design principles for .NET applications. Covers each SOLID principle with concrete C# anti-patterns and
fixes, plus DRY guidance with nuance on when duplication is acceptable. These principles guide class design, interface
contracts, and dependency management across all .NET project types.

## Scope

- SOLID principles with C# anti-patterns and fixes
- DRY guidance and when duplication is acceptable
- SRP compliance tests and class design heuristics
- Interface segregation and dependency inversion patterns

## Out of scope

- Architectural patterns (vertical slices, request pipelines, caching) -- see [skill:dotnet-architecture-patterns]
- DI container mechanics (registration, lifetimes, keyed services) -- see [skill:dotnet-csharp-dependency-injection]
- Code smells and anti-pattern detection -- see [skill:dotnet-csharp-code-smells]

Cross-references: [skill:dotnet-architecture-patterns] for clean architecture and vertical slices,
[skill:dotnet-csharp-dependency-injection] for DI registration patterns and lifetime management,
[skill:dotnet-csharp-code-smells] for anti-pattern detection, [skill:dotnet-csharp-coding-standards] for naming and
style conventions.

---

## Single Responsibility Principle (SRP)

A class should have only one reason to change. Apply the "describe in one sentence" test: if you cannot describe what a
class does in one sentence without using "and" or "or", it likely violates SRP.

### Anti-Pattern: God Class

````csharp

// WRONG -- OrderService handles validation, persistence, email, and PDF generation
public class OrderService
{
    private readonly AppDbContext _db;
    private readonly SmtpClient _smtp;

    public OrderService(AppDbContext db, SmtpClient smtp)
    {
        _db = db;
        _smtp = smtp;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validation logic (reason to change #1)
        if (string.IsNullOrEmpty(request.CustomerId))
            throw new ArgumentException("Customer required");

        // Persistence logic (reason to change #2)
        var order = new Order { CustomerId = request.CustomerId };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Email notification (reason to change #3)
        var message = new MailMessage("noreply@shop.com", request.Email,
            "Order Confirmed", $"Order {order.Id} created.");
        await _smtp.SendMailAsync(message);

        // PDF generation (reason to change #4)
        GenerateInvoicePdf(order);

        return order;
    }

    private void GenerateInvoicePdf(Order order) { /* ... */ }
}

```text

### Fix: Separate Responsibilities

```csharp

// Each class has one reason to change
public sealed class OrderCreator(
    IOrderValidator validator,
    IOrderRepository repository,
    IOrderNotifier notifier)
{
    public async Task<Order> CreateAsync(
        CreateOrderRequest request, CancellationToken ct)
    {
        validator.Validate(request);

        var order = await repository.AddAsync(request, ct);

        await notifier.OrderCreatedAsync(order, ct);

        return order;
    }
}

public sealed class OrderValidator : IOrderValidator
{
    public void Validate(CreateOrderRequest request)
    {
        ArgumentException.ThrowIfNullOrEmpty(request.CustomerId);
        // ... validation rules
    }
}

public sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task<Order> AddAsync(
        CreateOrderRequest request, CancellationToken ct)
    {
        var order = new Order { CustomerId = request.CustomerId };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return order;
    }
}

```text

### Anti-Pattern: Fat Controller

```csharp

// WRONG -- controller contains business logic, mapping, and persistence
app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    AppDbContext db,
    ILogger<Program> logger) =>
{
    // Validation in the endpoint
    if (request.Lines.Count == 0)
        return Results.BadRequest("At least one line required");

    // Business logic in the endpoint
    var total = request.Lines.Sum(l => l.Quantity * l.Price);
    if (total > 100_000)
        return Results.BadRequest("Order exceeds credit limit");

    // Mapping in the endpoint
    var order = new Order
    {
        CustomerId = request.CustomerId,
        Total = total,
        Lines = request.Lines.Select(l => new OrderLine
        {
            ProductId = l.ProductId,
            Quantity = l.Quantity,
            Price = l.Price
        }).ToList()
    };

    // Persistence in the endpoint
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    logger.LogInformation("Order {OrderId} created", order.Id);
    return Results.Created($"/api/orders/{order.Id}", order);
});

```text

Move business logic to a handler; keep the endpoint thin:

```csharp

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    IOrderHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.CreateAsync(request, ct);
    return result switch
    {
        { IsSuccess: true } => Results.Created(
            $"/api/orders/{result.Value.Id}", result.Value),
        _ => Results.ValidationProblem(result.Errors)
    };
});

```text

---

## Open/Closed Principle (OCP)

Classes should be open for extension but closed for modification. Add new behavior by implementing new types, not by editing existing switch/if chains.

### Anti-Pattern: Switch on Type

```csharp

// WRONG -- adding a new discount type requires modifying this method
public decimal CalculateDiscount(Order order)
{
    switch (order.DiscountType)
    {
        case "Percentage":
            return order.Total * order.DiscountValue / 100;
        case "FixedAmount":
            return order.DiscountValue;
        case "BuyOneGetOneFree":
            return order.Lines
                .Where(l => l.Quantity >= 2)
                .Sum(l => l.Price);
        default:
            return 0;
    }
}

```text

### Fix: Strategy Pattern

```csharp

public interface IDiscountStrategy
{
    decimal Calculate(Order order);
}

public sealed class PercentageDiscount(decimal percentage) : IDiscountStrategy
{
    public decimal Calculate(Order order) =>
        order.Total * percentage / 100;
}

public sealed class FixedAmountDiscount(decimal amount) : IDiscountStrategy
{
    public decimal Calculate(Order order) =>
        Math.Min(amount, order.Total);
}

// New discount type -- no existing code modified
public sealed class BuyOneGetOneFreeDiscount : IDiscountStrategy
{
    public decimal Calculate(Order order) =>
        order.Lines
            .Where(l => l.Quantity >= 2)
            .Sum(l => l.Price);
}

// Usage -- resolved via DI or factory
public sealed class OrderPricing(
    IEnumerable<IDiscountStrategy> strategies)
{
    public decimal ApplyBestDiscount(Order order) =>
        strategies.Max(s => s.Calculate(order));
}

```text

### Extension via Abstract Classes

When strategies share significant behavior, use an abstract base class:

```csharp

public abstract class NotificationSender
{
    public async Task SendAsync(Notification notification, CancellationToken ct)
    {
        // Shared behavior: validation and logging
        ArgumentNullException.ThrowIfNull(notification);
        await SendCoreAsync(notification, ct);
    }

    protected abstract Task SendCoreAsync(
        Notification notification, CancellationToken ct);
}

public sealed class EmailNotificationSender(IEmailClient client)
    : NotificationSender
{
    protected override async Task SendCoreAsync(
        Notification notification, CancellationToken ct)
    {
        await client.SendEmailAsync(
            notification.Recipient, notification.Subject,
            notification.Body, ct);
    }
}

```text

---

## Liskov Substitution Principle (LSP)

Subtypes must be substitutable for their base types without altering program correctness. A subclass must honor the behavioral contract of its parent -- preconditions cannot be strengthened, postconditions cannot be weakened.

### Anti-Pattern: Throwing in Override

```csharp

public class FileStorage : IStorage
{
    public virtual Stream OpenRead(string path) =>
        File.OpenRead(path);
}

// WRONG -- ReadOnlyFileStorage violates the base contract by
// throwing on a method the base type supports
public class ReadOnlyFileStorage : FileStorage
{
    public override Stream OpenRead(string path)
    {
        if (!File.Exists(path))
            throw new InvalidOperationException(
                "Cannot open files in read-only mode");
        return base.OpenRead(path);
    }

    // Surprise: callers expecting FileStorage behavior get exceptions
}

```text

### Anti-Pattern: Collection Covariance Pitfall

```csharp

// WRONG -- List<T> is not covariant; this compiles but causes runtime issues
IList<Animal> animals = new List<Dog>(); // Compile error (correctly)

// However, arrays ARE covariant in C# -- this compiles but throws at runtime:
Animal[] animals = new Dog[10];
animals[0] = new Cat(); // ArrayTypeMismatchException at runtime!

```csharp

### Fix: Use Covariant Interfaces

```csharp

// IEnumerable<out T> and IReadOnlyList<out T> are covariant
IEnumerable<Animal> animals = new List<Dog>(); // Safe -- read-only
IReadOnlyList<Animal> readOnlyAnimals = new List<Dog>(); // Safe

// When you need mutability, keep the concrete type
List<Dog> dogs = [new Dog("Rex"), new Dog("Buddy")];
ProcessAnimals(dogs); // Pass to covariant parameter

void ProcessAnimals(IReadOnlyList<Animal> animals)
{
    foreach (var animal in animals)
        animal.Speak();
}

```text

### LSP Compliance Checklist

- Derived classes do not throw new exception types that the base does not declare
- Overrides do not add preconditions (e.g., null checks the base does not require)
- Overrides do not weaken postconditions (e.g., returning null when base guarantees non-null)
- Behavioral contracts are preserved: if `ICollection.Add` succeeds on the base, it must succeed on the derived type

---

## Interface Segregation Principle (ISP)

Clients should not be forced to depend on methods they do not use. Prefer narrow, role-specific interfaces over wide "header" interfaces.

### Anti-Pattern: Header Interface

```csharp

// WRONG -- IWorker forces all implementations to support every capability

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
