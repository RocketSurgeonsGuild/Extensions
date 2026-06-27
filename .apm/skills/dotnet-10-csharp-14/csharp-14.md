# C# 14 Language Features

C# 14 introduces extension types, the `field` keyword, and null-conditional assignment.

## Extension Blocks

C# 14 introduces `extension` blocks for adding members to existing types without inheritance.

### Extension Properties

```csharp
extension<T>(IEnumerable<T> source)
{
    public bool IsEmpty => !source.Any();
    public int Count => source.Count();

    // Can use type parameters
    public T FirstOrDefault(T defaultValue) => source.FirstOrDefault() ?? defaultValue;
}
```

**Key differences from traditional extension methods:**

- Can add properties (not just methods)
- Can add static members
- Can use type parameters in the extension block
- No `this` parameter needed

### Extension Static Members

```csharp
extension(StringExtensions for string)
{
    public static bool IsNullOrWhitespace(string? s) => string.IsNullOrWhiteSpace(s);
}
```

### Extension Method Migration

**Before (C# 13 and earlier):**

```csharp
public static class EnumerableExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();
}
```

**After (C# 14):**

```csharp
extension<T>(IEnumerable<T> source)
{
    public bool IsEmpty => !source.Any();
}
```

## Field Keyword

The `field` keyword provides implicit backing field access within property accessors.

### Basic Usage

```csharp
public class User
{
    public string Name
    {
        get => field;
        set => field = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }
}
```

### Validation Without Boilerplate

**Before:**

```csharp
private string _name;
public string Name
{
    get => _name;
    set
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name cannot be empty");
        _name = value.Trim();
    }
}
```

**After:**

```csharp
public string Name
{
    get => field;
    set => field = string.IsNullOrWhiteSpace(value)
        ? throw new ArgumentException("Name cannot be empty")
        : value.Trim();
}
```

### Computed Properties with Storage

```csharp
public string DisplayName
{
    get => field ??= $"{FirstName} {LastName}";
}
```

## Null-Conditional Assignment

C# 14 adds null-conditional assignment (`?=` and `?.=`).

### Null-Conditional Assignment (`?=`)

Assign only if the target is null:

```csharp
// Assign config only if it's null
config ??= LoadDefaultConfiguration();

// Equivalent to:
if (config is null) config = LoadDefaultConfiguration();
```

### Null-Conditional Member Assignment (`?.=`)

Assign to a member only if the object is not null:

```csharp
// Set property only if user is not null
user?.Name = "John";

// Equivalent to:
if (user is not null) user.Name = "John";
```

### Real-World Examples

```csharp
// Safe configuration update
settings?.Database?.ConnectionString = GetConnectionString();

// Initialize once
logger ??= LoggerFactory.CreateLogger<Worker>();

// Chain of null checks
user?.Address?.City = "New York";
```

## Best Practices

### Extension Blocks

✅ **DO:**

- Use for collections and common interfaces
- Keep extensions focused on a single concern
- Document what types are extended

❌ **DON'T:**

- Extend primitive types extensively
- Create conflicting extensions
- Use for business logic (use classes instead)

### Field Keyword

✅ **DO:**

- Use for simple validation
- Use for computed property caching
- Keep logic simple and readable

❌ **DON'T:**

- Put complex logic in property accessors
- Use when explicit backing field is clearer
- Mix `field` and explicit backing fields in same class

### Null-Conditional Assignment

✅ **DO:**

- Use `??=` for lazy initialization
- Use `?.=` for safe member updates
- Combine with null-coalescing for defaults

❌ **DON'T:**

- Chain excessively (hard to debug)
- Use where null is unexpected (fail fast instead)
- Replace proper null checking in validation

## Migration Guide

### Step 1: Identify Extension Method Candidates

Look for extension methods that would benefit from being properties:

```csharp
// Old pattern - extension method
public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();

// New pattern - extension property
extension<T>(IEnumerable<T> source)
{
    public bool IsEmpty => !source.Any();
}
```

### Step 2: Simplify Properties with Field Keyword

Replace explicit backing fields where appropriate:

```csharp
// Before
private string _name;
public string Name { get => _name; set => _name = value?.Trim(); }

// After
public string Name { get => field; set => field = value?.Trim(); }
```

### Step 3: Adopt Null-Conditional Assignment

Replace verbose null checks:

```csharp
// Before
if (user != null && user.Address != null)
    user.Address.City = city;

// After
user?.Address?.City = city;
```

## Compatibility

C# 14 features require:

- .NET 10 or later
- LangVersion 14 or latest
- Visual Studio 2022 17.13+ or equivalent

**Backward compatibility:** Extension blocks and field keyword are source-only features and don't affect runtime
compatibility with older .NET versions.
