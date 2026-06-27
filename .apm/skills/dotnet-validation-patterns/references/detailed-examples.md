
## Manual Validation with Validator.TryValidateObject

Run DataAnnotations validation programmatically outside the MVC/Minimal API pipeline. Useful for validating objects in background services, console apps, or domain logic.

```csharp

public static class ValidationHelper
{
    public static (bool IsValid, IReadOnlyList<ValidationResult> Errors) Validate<T>(
        T instance) where T : notnull
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);

        // validateAllProperties: true is required to check all attributes
        bool isValid = Validator.TryValidateObject(
            instance, context, results, validateAllProperties: true);

        return (isValid, results);
    }
}

// Usage in a background service
public sealed class OrderImportWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderImportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var order = await ReadNextOrderFromQueue(stoppingToken);
            var (isValid, errors) = ValidationHelper.Validate(order);

            if (!isValid)
            {
                logger.LogWarning(
                    "Invalid order skipped: {Errors}",
                    string.Join("; ", errors.Select(e => e.ErrorMessage)));
                continue;
            }

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Orders.Add(order);
            await db.SaveChangesAsync(stoppingToken);
        }
    }

    private Task<Order> ReadNextOrderFromQueue(CancellationToken ct) =>
        throw new NotImplementedException();
}

```text

**Critical:** Without `validateAllProperties: true`, `Validator.TryValidateObject` only checks `[Required]` attributes, silently skipping `[Range]`, `[StringLength]`, `[RegularExpression]`, and all other attributes.

---

## Recursive Validation for Nested Objects

`Validator.TryValidateObject` does not recurse into nested objects or collections by default. Implement recursive validation when models contain nested complex types:

```csharp

public static class RecursiveValidator
{
    public static bool TryValidateObjectRecursive(
        object instance,
        List<ValidationResult> results)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return ValidateRecursive(instance, results, visited, prefix: "");
    }

    private static bool ValidateRecursive(
        object instance,
        List<ValidationResult> results,
        HashSet<object> visited,
        string prefix)
    {
        if (!visited.Add(instance))
            return true; // Already validated -- avoid circular reference loops

        var context = new ValidationContext(instance);
        bool isValid = Validator.TryValidateObject(
            instance, context, results, validateAllProperties: true);

        foreach (var property in instance.GetType().GetProperties())
        {
            if (IsSimpleType(property.PropertyType))
                continue;

            var value = property.GetValue(instance);
            if (value is null) continue;

            var memberPrefix = string.IsNullOrEmpty(prefix)
                ? property.Name
                : $"{prefix}.{property.Name}";

            if (value is IEnumerable<object> collection)
            {
                int index = 0;
                foreach (var item in collection)
                {
                    var itemResults = new List<ValidationResult>();
                    if (!ValidateRecursive(
                        item, itemResults, visited,
                        $"{memberPrefix}[{index}]"))
                    {
                        isValid = false;
                        foreach (var result in itemResults)
                        {
                            results.Add(new ValidationResult(
                                result.ErrorMessage,
                                result.MemberNames.Select(
                                    m => $"{memberPrefix}[{index}].{m}").ToArray()));
                        }
                    }
                    index++;
                }
            }
            else if (property.PropertyType.IsClass)
            {
                var nestedResults = new List<ValidationResult>();
                if (!ValidateRecursive(value, nestedResults, visited, memberPrefix))
                {
                    isValid = false;
                    foreach (var result in nestedResults)
                    {
                        results.Add(new ValidationResult(
                            result.ErrorMessage,
                            result.MemberNames.Select(
                                m => $"{memberPrefix}.{m}").ToArray()));
                    }
                }
            }
        }

        return isValid;
    }

    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive
        || type.IsEnum
        || type == typeof(string)
        || type == typeof(decimal)
        || type == typeof(DateTime)
        || type == typeof(DateTimeOffset)
        || type == typeof(DateOnly)
        || type == typeof(TimeOnly)
        || type == typeof(TimeSpan)
        || type == typeof(Guid)
        || (Nullable.GetUnderlyingType(type) is { } underlying
            && IsSimpleType(underlying));
}

```text

**Note:** This implementation tracks visited objects via `HashSet<object>` with `ReferenceEqualityComparer` to safely handle circular reference graphs without stack overflow.

---

## Agent Gotchas

1. **Always pass `validateAllProperties: true`** to `Validator.TryValidateObject`. Without it, only `[Required]` is checked; `[Range]`, `[StringLength]`, and custom attributes are silently skipped.
2. **Options classes must use `{ get; set; }` not `{ get; init; }`** because the configuration binder and `PostConfigure` need to mutate properties after construction. Use `[Required]` for mandatory fields instead of `init`.
3. **`IValidatableObject.Validate()` runs only after all attribute validations pass.** This requires MVC model binding or `Validator.TryValidateObject` with `validateAllProperties: true`. If attribute validation fails, `Validate()` is never called. Do not rely on it for primary validation.
4. **Do not inject services into `ValidationAttribute` via constructor.** Attributes are instantiated by the runtime and cannot participate in DI. Use `validationContext.GetService<T>()` inside `IsValid()` if service access is needed, but prefer `IValidateOptions<T>` for DI-dependent validation.
5. **Do not use `[RegularExpression]` without `[GeneratedRegex]` awareness.** The attribute internally creates `Regex` instances. For performance-critical paths, validate with `[GeneratedRegex]` in a custom attribute or `IValidatableObject` instead. See [skill:dotnet-input-validation] for ReDoS prevention.
6. **Register `IValidateOptions<T>` as singleton.** The options validation infrastructure resolves validators as singletons. Registering as scoped or transient causes resolution failures.
7. **Do not forget `ValidateOnStart()`.** Without it, options validation only runs on first access to `IOptions<T>.Value`, which may be minutes into the application lifecycle. Always chain `.ValidateOnStart()` for fail-fast behavior.

---

## Prerequisites

- .NET 8.0+ (LTS baseline for `[AllowedValues]`, `[DeniedValues]`, `[Length]`, `[Base64String]`)
- `System.ComponentModel.DataAnnotations` (included in .NET SDK, no extra package)
- `Microsoft.Extensions.Options` (included in ASP.NET Core shared framework, no extra package)
- .NET 10.0 for `[ValidatableType]` source-generated validation (see [skill:dotnet-input-validation])

---

## References

- [Model Validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [System.ComponentModel.DataAnnotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [IValidateOptions](https://learn.microsoft.com/en-us/dotnet/core/extensions/options#options-validation)
- [Options Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [Validator.TryValidateObject](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validator.tryvalidateobject)

## Attribution

Adapted from [Aaronontheweb/dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills) (MIT license).
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
