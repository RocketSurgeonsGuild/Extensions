---
name: dotnet-csharp-coding-standards
description: Defines baseline C# conventions loaded first -- naming, file layout, style rules.
license: MIT
targets: ['*']
category: fundamentals
subcategory: coding-standards
tags:
  - csharp
  - dotnet
  - skill
  - coding-standards
  - naming
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-csharp-modern-patterns
  - dotnet-csharp-nullable-reference-types
  - dotnet-editorconfig
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

# dotnet-csharp-coding-standards

Modern .NET coding standards based on Microsoft Framework Design Guidelines and C# Coding Conventions. This skill covers
naming, file organization, and code style rules that agents should follow when generating or reviewing C# code.

## Activation Guidance

Load this skill by default for any task that plans, designs, generates, modifies, or reviews C#/.NET code. Do not wait
for explicit user wording such as "coding standards", "style", or "conventions". If code will be produced, this skill
should be active before implementation starts. This skill is a baseline dependency that should be loaded before
domain-specific C#/.NET skills.

Cross-references: [skill:dotnet-csharp-modern-patterns] for language feature usage, [skill:dotnet-csharp-async-patterns]
for async naming conventions, [skill:dotnet-solid-principles] for SOLID, DRY, and SRP design principles at the class and
interface level.

## Scope

- Naming conventions (PascalCase, camelCase, I-prefix for interfaces)
- File organization and namespace conventions
- Code style rules (expression bodies, using directives, var usage)
- EditorConfig integration for style enforcement

## Out of scope

- Language feature patterns (records, pattern matching) -- see [skill:dotnet-csharp-modern-patterns]
- Async naming and await conventions -- see [skill:dotnet-csharp-async-patterns]
- SOLID/DRY design principles -- see [skill:dotnet-solid-principles]
- Code smells and anti-patterns -- see [skill:dotnet-csharp-code-smells]

---

## Naming Conventions

### General Rules

| Element                   | Convention                | Example                        |
| ------------------------- | ------------------------- | ------------------------------ |
| Namespaces                | PascalCase, dot-separated | `MyCompany.MyProduct.Core`     |
| Classes, Records, Structs | PascalCase                | `OrderService`, `OrderSummary` |
| Interfaces                | `I` + PascalCase          | `IOrderRepository`             |
| Methods                   | PascalCase                | `GetOrderAsync`                |
| Properties                | PascalCase                | `OrderDate`                    |
| Events                    | PascalCase                | `OrderCompleted`               |
| Public constants          | PascalCase                | `MaxRetryCount`                |
| Private fields            | `_camelCase`              | `_orderRepository`             |
| Parameters, locals        | camelCase                 | `orderId`, `totalAmount`       |
| Type parameters           | `T` or `T` + PascalCase   | `T`, `TKey`, `TValue`          |
| Enum members              | PascalCase                | `OrderStatus.Pending`          |

### Async Method Naming

Suffix async methods with `Async`:

````csharp

// Correct
public Task<Order> GetOrderAsync(int id);
public ValueTask SaveChangesAsync(CancellationToken ct);

// Wrong
public Task<Order> GetOrder(int id);      // missing Async suffix
public Task<Order> GetOrderTask(int id);  // wrong suffix

```text

Exception: Event handlers and interface implementations where the framework does not use the `Async` suffix (e.g.,
ASP.NET Core middleware `InvokeAsync` is already named by the framework).

### Boolean Naming

Prefix booleans with `is`, `has`, `can`, `should`, or similar:

```csharp

public bool IsActive { get; set; }
public bool HasOrders { get; }
public bool CanDelete(Order order);

```csharp

### Collection Naming

Use plural nouns for collections:

```csharp

public IReadOnlyList<Order> Orders { get; }    // not OrderList
public Dictionary<string, int> CountsByName { get; } // descriptive

```csharp

---

## File Organization

### One Type Per File

Each top-level type (class, record, struct, interface, enum) should be in its own file, named exactly as the type.
Nested types stay in the containing type's file.

```text

OrderService.cs        -> public class OrderService
IOrderRepository.cs    -> public interface IOrderRepository
OrderStatus.cs         -> public enum OrderStatus
OrderSummary.cs        -> public record OrderSummary

```csharp

### File-Scoped Namespaces

Always use file-scoped namespaces (C# 10+):

```csharp

// Correct
namespace MyApp.Services;

public class OrderService { }

// Avoid: block-scoped namespace adds unnecessary indentation
namespace MyApp.Services
{
    public class OrderService { }
}

```text

### Using Directives

Place `using` directives at the top of the file, outside the namespace. With `<ImplicitUsings>enable</ImplicitUsings>`
(default in modern .NET), common namespaces are already imported. Only add explicit `using` statements for namespaces
not covered by implicit usings.

Order of `using` directives:

1. `System.*` namespaces
2. Third-party namespaces
3. Project namespaces

### Directory Structure

Organize by feature or layer, matching namespace hierarchy:

```text

src/MyApp/
  Features/
    Orders/
      OrderService.cs
      IOrderRepository.cs
      OrderEndpoints.cs
    Users/
      UserService.cs
  Infrastructure/
    Persistence/
      OrderRepository.cs

```csharp

---

## Code Style

### Braces

Always use braces for control flow, even for single-line bodies:

```csharp

// Correct
if (order.IsValid)
{
    Process(order);
}

// Avoid
if (order.IsValid)
    Process(order);

```text

### Expression-Bodied Members

Use expression bodies for single-expression members:

```csharp

// Properties
public string FullName => $"{FirstName} {LastName}";

// Methods (single expression only)
public override string ToString() => $"Order #{Id}";

// Avoid for multi-statement methods -- use block body instead

```text

### `var` Usage

Use `var` when the type is obvious from the right-hand side:

```csharp

// Type is obvious: use var
var orders = new List<Order>();
var customer = GetCustomerById(id);
var name = "Alice";

// Type is not obvious: use explicit type
IOrderRepository repo = serviceProvider.GetRequiredService<IOrderRepository>();
decimal total = CalculateTotal(items);

```text

### Null Handling

Prefer pattern matching over null checks:

```csharp

// Preferred
if (order is not null) { }
if (order is { Status: OrderStatus.Active }) { }

// Acceptable
if (order != null) { }

// Avoid
if (order is object) { }
if (!(order is null)) { }

```text

Use null-conditional and null-coalescing operators:

```csharp

var name = customer?.Name ?? "Unknown";
var orders = customer?.Orders ?? [];
items ??= [];

```csharp

### String Handling

Prefer string interpolation over concatenation or `string.Format`:

```csharp

// Preferred
var message = $"Order {orderId} totals {total:C2}";

// For complex interpolations, use raw string literals (C# 11+)
var json = $$"""
    {
        "id": {{orderId}},
        "name": "{{name}}"
    }
    """;

// Avoid
var message = string.Format("Order {0} totals {1:C2}", orderId, total);
var message = "Order " + orderId + " totals " + total.ToString("C2");

```text

---

## Access Modifiers

Always specify access modifiers explicitly. Do not rely on defaults:

```csharp

// Correct
public class OrderService
{
    private readonly IOrderRepository _repo;
    internal void ProcessBatch() { }
}

// Avoid: implicit internal class, implicit private field
class OrderService
{
    readonly IOrderRepository _repo;
}

```text

### Modifier Order

Follow the standard order:

```text

access (public/private/protected/internal) -> static -> extern -> new ->
virtual/abstract/override/sealed -> readonly -> volatile -> async -> partial

```text

```csharp

public static readonly int MaxSize = 100;
protected virtual async Task<Order> LoadAsync() => await repo.GetDefaultAsync();
public sealed override string ToString() => Name;

```csharp

---

## Type Design

These conventions implement SOLID and DRY principles at the code level. For comprehensive coverage with anti-patterns
and fixes, see [skill:dotnet-solid-principles].

### Seal Classes by Default

Seal classes that are not designed for inheritance. This improves performance (devirtualization) and communicates
intent:

```csharp

public sealed class OrderService(IOrderRepository repo)
{
    // Not designed for inheritance
}

```text

Only leave classes unsealed when you explicitly design them as base classes.

### Prefer Composition Over Inheritance

```csharp

// Preferred: composition
public sealed class OrderProcessor(IValidator validator, INotifier notifier)
{
    public async Task ProcessAsync(Order order)
    {
        await validator.ValidateAsync(order);
        await notifier.NotifyAsync(order);
    }
}

// Avoid: deep inheritance hierarchies
public class BaseProcessor { }
public class ValidatingProcessor : BaseProcessor { }
public class NotifyingValidatingProcessor : ValidatingProcessor { }

```text

### Interface Segregation

Keep interfaces focused. Prefer multiple small interfaces over one large one:

```csharp

// Preferred
public interface IOrderReader
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
}

public interface IOrderWriter
{

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
