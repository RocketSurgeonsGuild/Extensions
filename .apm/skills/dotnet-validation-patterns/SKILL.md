---
name: dotnet-validation-patterns
category: developer-experience
subcategory: cli
description: Validates models and IOptions. DataAnnotations, IValidatableObject, IValidateOptions<T>.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
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

# dotnet-validation-patterns

Built-in .NET validation patterns that do not require third-party packages. Covers DataAnnotations attributes,
`IValidatableObject` for cross-property validation, `IValidateOptions<T>` for options validation at startup, custom
`ValidationAttribute` authoring, and `Validator.TryValidateObject` for manual validation. Prefer these built-in
mechanisms as the default; reserve FluentValidation for complex domain rules that outgrow declarative attributes.

## Scope

- DataAnnotations attributes and Validator.TryValidateObject
- IValidatableObject for cross-property validation
- IValidateOptions<T> for options validation at startup
- Custom ValidationAttribute authoring

## Out of scope

- API pipeline integration (endpoint filters, ProblemDetails, AddValidation) -- see [skill:dotnet-input-validation]
- Options pattern binding and ValidateOnStart registration -- see [skill:dotnet-csharp-configuration]
- Architectural placement of validation in layers -- see [skill:dotnet-architecture-patterns]

Cross-references: [skill:dotnet-input-validation] for API pipeline validation and FluentValidation,
[skill:dotnet-csharp-configuration] for Options pattern binding and `ValidateOnStart()`,
[skill:dotnet-architecture-patterns] for validation placement in architecture layers,
[skill:dotnet-csharp-coding-standards] for naming conventions.

---

## Validation Approach Decision Tree

Choose the validation approach based on complexity:

1. **DataAnnotations** (default) -- declarative `[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]`
   attributes. Best for: simple property-level constraints on DTOs, request models, and options classes.
2. **`IValidatableObject`** -- implement `Validate()` for cross-property rules within the same object. Best for: date
   range comparisons, conditional required fields, business rules that span multiple properties.
3. **Custom `ValidationAttribute`** -- subclass `ValidationAttribute` for reusable property-level rules. Best for:
   domain-specific constraints (SKU format, postal code, currency code) applied across multiple models.
4. **`IValidateOptions<T>`** -- validate configuration/options classes at startup with access to DI services. Best for:
   cross-property options checks, environment-dependent validation, fail-fast startup.
5. **FluentValidation** -- third-party library for complex, testable validation with fluent API. Best for: async
   validators, database-dependent rules, deeply nested object graphs. See [skill:dotnet-input-validation] for
   FluentValidation patterns.

General guidance: start with DataAnnotations. Add `IValidatableObject` when cross-property rules emerge. Introduce
FluentValidation only when rules outgrow declarative attributes.

---

## DataAnnotations

The `System.ComponentModel.DataAnnotations` namespace provides declarative validation through attributes. These
attributes work with MVC model binding, `Validator.TryValidateObject`, and the .NET 10 source-generated validation
pipeline.

### Standard Attributes

````csharp

using System.ComponentModel.DataAnnotations;

public sealed class CreateProductRequest
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Price must be between {1} and {2}")]
    public decimal Price { get; set; }

    [RegularExpression(@"^[A-Z]{2,4}-\d{4,8}$",
        ErrorMessage = "SKU format: AA-0000 to AAAA-00000000")]
    public string? Sku { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
    public int Quantity { get; set; }
}

```text

### Attribute Reference

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Non-null, non-empty | `[Required]` |
| `[StringLength]` | Min/max length | `[StringLength(200, MinimumLength = 1)]` |
| `[Range]` | Numeric/date range | `[Range(1, 100)]` |
| `[RegularExpression]` | Pattern match | `[RegularExpression(@"^\d{5}$")]` |
| `[EmailAddress]` | Email format | `[EmailAddress]` |
| `[Phone]` | Phone format | `[Phone]` |
| `[Url]` | URL format | `[Url]` |
| `[CreditCard]` | Luhn check | `[CreditCard]` |
| `[Compare]` | Property equality | `[Compare(nameof(Password))]` |
| `[MaxLength]` / `[MinLength]` | Collection/string length | `[MaxLength(50)]` |
| `[AllowedValues]` (.NET 8+) | Value allowlist | `[AllowedValues("Draft", "Published")]` |
| `[DeniedValues]` (.NET 8+) | Value denylist | `[DeniedValues("Admin", "Root")]` |
| `[Length]` (.NET 8+) | Min and max in one | `[Length(1, 200)]` |
| `[Base64String]` (.NET 8+) | Base64 format | `[Base64String]` |

---

## Custom ValidationAttribute

Create reusable validation attributes for domain-specific rules.

### Property-Level Custom Attribute

```csharp

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
    {
        if (value is DateOnly date && date <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ValidationResult(
                ErrorMessage ?? "Date must be in the future",
                [validationContext.MemberName!]);
        }

        return ValidationResult.Success;
    }
}

// Usage
public sealed class CreateEventRequest
{
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    [FutureDate(ErrorMessage = "Event date must be in the future")]
    public DateOnly EventDate { get; set; }
}

```text

### Class-Level Custom Attribute

Apply validation across the entire object when multiple properties are involved:

```csharp

[AttributeUsage(AttributeTargets.Class)]
public sealed class DateRangeAttribute : ValidationAttribute
{
    public string StartProperty { get; set; } = "StartDate";
    public string EndProperty { get; set; } = "EndDate";

    protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        var type = value.GetType();
        var startValue = type.GetProperty(StartProperty)?.GetValue(value);
        var endValue = type.GetProperty(EndProperty)?.GetValue(value);

        if (startValue is DateOnly start && endValue is DateOnly end && end < start)
        {
            return new ValidationResult(
                ErrorMessage ?? $"{EndProperty} must be after {StartProperty}",
                [EndProperty]);
        }

        return ValidationResult.Success;
    }
}

// Usage
[DateRange(StartProperty = nameof(StartDate), EndProperty = nameof(EndDate))]
public sealed class DateRangeFilter
{
    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }
}

```text

---

## IValidatableObject

Implement `IValidatableObject` for cross-property validation within the model itself. This interface runs after all individual attribute validations pass (when using MVC model binding or `Validator.TryValidateObject` with `validateAllProperties: true`).

```csharp

public sealed class CreateOrderRequest : IValidatableObject
{
    [Required]
    [StringLength(50)]
    public required string CustomerId { get; set; }

    [Required]
    public DateOnly OrderDate { get; set; }

    public DateOnly? ShipByDate { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one line item is required")]
    public required List<OrderLineItem> Lines { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ShipByDate.HasValue && ShipByDate.Value <= OrderDate)
        {
            yield return new ValidationResult(
                "Ship-by date must be after order date",
                [nameof(ShipByDate)]);
        }

        if (Lines.Sum(l => l.Quantity * l.UnitPrice) > 1_000_000)
        {
            yield return new ValidationResult(
                "Total order value cannot exceed 1,000,000",
                [nameof(Lines)]);
        }

        // Conditional required field
        if (Lines.Any(l => l.RequiresShipping) && ShipByDate is null)
        {
            yield return new ValidationResult(
                "Ship-by date is required when order contains shippable items",
                [nameof(ShipByDate)]);
        }
    }
}

public sealed class OrderLineItem
{
    [Required]
    public required string ProductId { get; set; }

    [Range(1, 10_000)]
    public int Quantity { get; set; }

    [Range(0.01, 100_000)]
    public decimal UnitPrice { get; set; }

    public bool RequiresShipping { get; set; }
}

```text

**When to use `IValidatableObject` vs custom attribute:** Use `IValidatableObject` when the validation logic is specific to one model and involves multiple properties. Use a custom `ValidationAttribute` when the same rule applies across multiple models (reusable).

---

## IValidateOptions<T>

Use `IValidateOptions<T>` for complex validation of options/configuration classes at startup. Unlike DataAnnotations, this interface supports cross-property checks, DI-injected dependencies, and programmatic logic. See [skill:dotnet-csharp-configuration] for Options pattern binding and `ValidateOnStart()` registration.

### Basic IValidateOptions

```csharp

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = "";
    public int MaxRetryCount { get; set; } = 3;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public int MaxPoolSize { get; set; } = 100;
    public int MinPoolSize { get; set; } = 0;
}

public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add("Database connection string is required.");
        }

        if (options.MaxRetryCount is < 0 or > 10)
        {
            failures.Add("MaxRetryCount must be between 0 and 10.");
        }

        if (options.CommandTimeoutSeconds < 1)
        {
            failures.Add("CommandTimeoutSeconds must be at least 1.");
        }

        if (options.MinPoolSize > options.MaxPoolSize)
        {
            failures.Add(
                $"MinPoolSize ({options.MinPoolSize}) cannot exceed " +
                $"MaxPoolSize ({options.MaxPoolSize}).");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}

```text

### Registration

```csharp

builder.Services
    .AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateOnStart(); // Fail fast at startup

// Register the validator -- runs automatically with ValidateOnStart
builder.Services.AddSingleton<
    IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

```text

### Combining DataAnnotations with IValidateOptions

Use DataAnnotations for simple property constraints and `IValidateOptions<T>` for cross-property or environment-dependent logic:

```csharp

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required, MinLength(1)]
    public string Host { get; set; } = "";

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [Required, EmailAddress]
    public string FromAddress { get; set; } = "";

    public bool UseSsl { get; set; } = true;
}

public sealed class SmtpOptionsValidator : IValidateOptions<SmtpOptions>
{
    public ValidateOptionsResult Validate(string? name, SmtpOptions options)
    {
        if (options.UseSsl && options.Port == 25)
        {
            return ValidateOptionsResult.Fail(
                "Port 25 does not support SSL. Use 465 or 587.");
        }

        return ValidateOptionsResult.Success;
    }
}

// Registration -- both run
builder.Services
    .AddOptions<SmtpOptions>()
    .BindConfiguration(SmtpOptions.SectionName)
    .ValidateDataAnnotations()  // Simple property checks
    .ValidateOnStart();

builder.Services.AddSingleton<
    IValidateOptions<SmtpOptions>, SmtpOptionsValidator>(); // Cross-property checks

```text

---


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
