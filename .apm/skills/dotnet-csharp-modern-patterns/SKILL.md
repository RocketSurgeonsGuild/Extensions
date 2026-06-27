---
name: dotnet-csharp-modern-patterns
description: Using records, pattern matching, primary constructors, collection expressions. C# 12-15 by TFM.
license: MIT
targets: ['*']
category: fundamentals
subcategory: language-patterns
tags:
  - csharp
  - dotnet
  - skill
  - language-patterns
  - records
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-csharp-coding-standards
  - dotnet-csharp-async-patterns
  - dotnet-10-csharp-14
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

# dotnet-csharp-modern-patterns

Modern C# language feature guidance adapted to the project's target framework. Always run
[skill:dotnet-version-detection] first to determine TFM and C# version.

## Scope

- Records, pattern matching, primary constructors, collection expressions
- C# 12-15 feature usage mapped to TFM
- Language feature adoption guidance

## Out of scope

- Naming and style conventions -- see [skill:dotnet-csharp-coding-standards]
- Async/await patterns -- see [skill:dotnet-csharp-async-patterns]
- Source generator usage (GeneratedRegex, LoggerMessage) -- see [skill:dotnet-csharp-source-generators]

Cross-references: [skill:dotnet-csharp-coding-standards] for naming/style conventions,
[skill:dotnet-csharp-async-patterns] for async-specific patterns.

---

## Quick Reference: TFM to C# Version

| TFM     | C#           | Key Language Features                                        |
| ------- | ------------ | ------------------------------------------------------------ |
| net8.0  | 12           | Primary constructors, collection expressions, alias any type |
| net9.0  | 13           | `params` collections, `Lock` type, partial properties        |
| net10.0 | 14           | `field` keyword, extension blocks, `nameof` unbound generics |
| net11.0 | 15 (preview) | Collection expression `with()` arguments                     |

---

## Records

Use records for immutable data transfer objects, value semantics, and domain modeling where equality is based on values
rather than identity.

### Record Classes (reference type)

````csharp

// Positional record: concise, immutable, value equality
public record OrderSummary(int OrderId, decimal Total, DateOnly OrderDate);

// With additional members
public record Customer(string Name, string Email)
{
    public string DisplayName => $"{Name} <{Email}>";
}

```text

### Record Structs (value type, C# 10+)

```csharp

// Positional record struct: value type with value semantics
public readonly record struct Point(double X, double Y);

// Mutable record struct (rare -- prefer readonly)
public record struct MutablePoint(double X, double Y);

```text

### When to Use Records vs Classes

| Use Case                             | Prefer                   |
| ------------------------------------ | ------------------------ |
| DTOs, API responses                  | `record`                 |
| Domain value objects (Money, Email)  | `readonly record struct` |
| Entities with identity (User, Order) | `class`                  |
| High-throughput, small data          | `readonly record struct` |
| Inheritance needed                   | `record` (class-based)   |

### Non-destructive Mutation

```csharp

var updated = order with { Total = order.Total + tax };

```csharp

---

## Primary Constructors (C# 12+, net8.0+)

Capture constructor parameters directly in the class/struct body. Parameters become available throughout the type but
are **not** fields or properties -- they are captured state.

### For Services (DI injection)

```csharp

public class OrderService(IOrderRepository repo, ILogger<OrderService> logger)
{
    public async Task<Order> GetAsync(int id)
    {
        logger.LogInformation("Fetching order {OrderId}", id);
        return await repo.GetByIdAsync(id);
    }
}

```text

### Gotchas

- Primary constructor parameters are **mutable** captures, not `readonly` fields. If immutability matters, assign to a
  `readonly` field in the body.
- Do not use primary constructors when you need to validate parameters at construction time -- use a traditional
  constructor with guard clauses instead.
- For records, positional parameters become public properties automatically. For classes/structs, they remain private
  captures.

```csharp

// Explicit readonly field when immutability matters
public class Config(string connectionString)
{
    private readonly string _connectionString = connectionString
        ?? throw new ArgumentNullException(nameof(connectionString));
}

```text

---

## Collection Expressions (C# 12+, net8.0+)

Unified syntax for creating collections with `[...]`.

```csharp

// Array
int[] numbers = [1, 2, 3];

// List
List<string> names = ["Alice", "Bob"];

// Span
ReadOnlySpan<byte> bytes = [0x00, 0xFF];

// Spread operator
int[] combined = [..first, ..second, 99];

// Empty collection
List<int> empty = [];

```text

### Collection Expression with Arguments (C# 15 preview, net11.0+)

Specify capacity, comparers, or other constructor arguments:

```csharp

// Capacity hint
List<int> nums = [with(capacity: 1000), ..Generate()];

// Custom comparer
HashSet<string> set = [with(comparer: StringComparer.OrdinalIgnoreCase), "Alice", "bob"];

// Dictionary with comparer
Dictionary<string, int> map = [with(comparer: StringComparer.OrdinalIgnoreCase),
    new("key1", 1), new("key2", 2)];

```text

> **net11.0+ only.** Requires `<LangVersion>preview</LangVersion>`. Do not use on earlier TFMs.

---

## Pattern Matching

### Switch Expressions (C# 8+)

```csharp

string GetDiscount(Customer customer) => customer switch
{
    { Tier: "Gold", YearsActive: > 5 } => "30%",
    { Tier: "Gold" } => "20%",
    { Tier: "Silver" } => "10%",
    _ => "0%"
};

```text

### List Patterns (C# 11+)

```csharp

bool IsValid(int[] data) => data is [> 0, .., > 0]; // first and last positive

string Describe(int[] values) => values switch
{
    [] => "empty",
    [var single] => $"single: {single}",
    [var first, .., var last] => $"range: {first}..{last}"
};

```text

### Type and Property Patterns

```csharp

decimal CalculateShipping(object package) => package switch
{
    Letter { Weight: < 50 } => 0.50m,
    Parcel { Weight: var w } when w < 1000 => 5.00m + w * 0.01m,
    Parcel { IsOversized: true } => 25.00m,
    _ => 10.00m
};

```text

---

## `required` Members (C# 11+)

Force callers to initialize properties at construction via object initializers.

```csharp

public class UserDto
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? Phone { get; init; }
}

// Compiler enforces Name and Email
var user = new UserDto { Name = "Alice", Email = "alice@example.com" };

```text

Useful for DTOs that need to be deserialized (System.Text.Json honors `required` in .NET 8+).

---

## `field` Keyword (C# 14, net10.0+)

Access the compiler-generated backing field directly in property accessors.

```csharp

public class TemperatureSensor
{
    public double Reading
    {
        get => field;
        set => field = value >= -273.15
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value));
    }
}

```text

Replaces the manual pattern of declaring a private field plus a property with custom logic. Use when you need validation
or transformation in a setter without a separate backing field.

> **net10.0+ only.** On earlier TFMs, use a traditional private field.

---

## Extension Blocks (C# 14, net10.0+)

Group extension members for a type in a single block.

```csharp

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source) where T : class
    {
        public IEnumerable<T> WhereNotNull()
            => source.Where(x => x is not null);

        public bool IsEmpty()
            => !source.Any();
    }
}

```text

> **net10.0+ only.** On earlier TFMs, use traditional `static` extension methods.

---

## Alias Any Type (`using`, C# 12+, net8.0+)

```csharp

using Point = (double X, double Y);
using UserId = System.Guid;

Point origin = (0, 0);
UserId id = UserId.NewGuid();

```text

Useful for tuple aliases and domain type aliases without creating a full type.

---

## `params` Collections (C# 13, net9.0+)

`params` now supports additional collection types beyond arrays, including `Span<T>`, `ReadOnlySpan<T>`, and types
implementing certain collection interfaces.

```csharp

public void Log(params ReadOnlySpan<string> messages)
{
    foreach (var msg in messages)
        Console.WriteLine(msg);
}

// Callers: compiler may avoid heap allocation with span-based params
Log("hello", "world");

```text

> **net9.0+ only.** On net8.0, `params` only supports arrays.

---

## `Lock` Type (C# 13, net9.0+)

Use `System.Threading.Lock` instead of `object` for locking.

```csharp

private readonly Lock _lock = new();

public void DoWork()
{
    lock (_lock)
    {
        // thread-safe operation
    }
}

```text

`Lock` provides a `Scope`-based API for advanced scenarios and is more expressive than `lock (object)`.

> **net9.0+ only.** On net8.0, use `private readonly object _gate = new();` and `lock (_gate)`.

---

## Partial Properties (C# 13, net9.0+)

Partial properties enable source generators to define property signatures that users implement, or vice versa.

```csharp

// In generated file
public partial class ViewModel
{
    public partial string Name { get; set; }
}

// In user file
public partial class ViewModel
{
    private string _name = "";

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
