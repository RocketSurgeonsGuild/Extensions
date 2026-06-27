---
name: dotnet-input-validation
category: web
subcategory: validation
description: Validates HTTP request inputs. .NET 10 AddValidation, FluentValidation, ProblemDetails.
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

# dotnet-input-validation

Comprehensive input validation patterns for .NET APIs. Covers the .NET 10 built-in validation system, FluentValidation
for complex business rules, Data Annotations for simple models, endpoint filters for Minimal API integration,
ProblemDetails error responses, and security-focused validation techniques.

## Scope

- .NET 10 built-in validation (AddValidation, ValidatableType, source generators)
- FluentValidation validators, DI registration, endpoint filters
- Data Annotations attributes, custom ValidationAttribute, IValidatableObject
- Endpoint filters for validation as a cross-cutting concern
- ProblemDetails error responses (RFC 9457)
- Security-focused validation (ReDoS prevention, allowlist, file upload)

## Out of scope

- Blazor form validation (EditForm, DataAnnotationsValidator) -- see [skill:dotnet-blazor-components]
- OWASP injection prevention principles -- see [skill:dotnet-security-owasp]
- Architectural patterns for validation placement -- see [skill:dotnet-architecture-patterns]
- Options pattern ValidateDataAnnotations -- see [skill:dotnet-csharp-configuration]

Cross-references: [skill:dotnet-security-owasp] for OWASP injection prevention, [skill:dotnet-architecture-patterns] for
architectural validation strategy, [skill:dotnet-minimal-apis] for Minimal API pipeline integration,
[skill:dotnet-csharp-configuration] for Options pattern validation.

---

## Validation Framework Decision Tree

Choose the validation framework based on project requirements:

1. **.NET 10 Built-in Validation (`AddValidation`)** -- default for new .NET 10+ projects. Source-generator-based,
   AOT-compatible, auto-discovers types from Minimal API handlers. Best for: greenfield projects targeting .NET 10+.
2. **FluentValidation** -- when validation rules are complex (cross-property, conditional, database-dependent). Rich
   fluent API with testable validator classes. Best for: complex business rules, domain validation.
3. **Data Annotations** -- when models need simple declarative validation (`[Required]`, `[Range]`). Widely understood,
   works with MVC model binding and `IValidatableObject` for cross-property checks. Best for: simple DTOs, shared
   models.
4. **MiniValidation** -- lightweight Data Annotations runner without MVC model binding overhead. Best for:
   micro-services with simple validation (see [skill:dotnet-architecture-patterns] for details).

General guidance: prefer .NET 10 built-in validation for new projects. Use FluentValidation when rules outgrow
annotations. Do not mix multiple frameworks in the same request DTO -- pick one per model type and stay consistent.

---

## .NET 10 Built-in Validation

.NET 10 introduces `Microsoft.Extensions.Validation` with source-generator-based validation that integrates directly
into the Minimal API pipeline. It auto-discovers validatable types from endpoint handler parameters and runs validation
via an endpoint filter.

### Setup

````csharp

// <PackageReference Include="Microsoft.Extensions.Validation" Version="10.*" />
builder.Services.AddValidation();

var app = builder.Build();
// Validation runs automatically via endpoint filter for Minimal API handlers

```text

`AddValidation()` scans for types annotated with `[ValidatableType]` and generates validation logic at compile time
using source generators, ensuring Native AOT compatibility.

### Defining Validatable Types

```csharp

[ValidatableType]
public partial class CreateProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal Price { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z]{2,4}-\d{4,8}$", ErrorMessage = "SKU format: AA-0000")]
    public required string Sku { get; set; }
}

```text

The `partial` keyword is required because the source generator emits validation logic into the same type. The
`[ValidatableType]` attribute triggers code generation at compile time -- no reflection at runtime.

### How It Works

1. Source generator discovers `[ValidatableType]` classes and emits `IValidatableObject`-like validation logic.
2. `AddValidation()` registers an endpoint filter that inspects Minimal API handler parameters.
3. When a request arrives, the filter validates parameters before the handler executes.
4. On failure, returns a `ValidationProblem` response automatically.

**Gotcha:** `AddValidation()` integrates with Minimal APIs via endpoint filters. MVC controllers use their own model
validation pipeline and do not participate in this filter-based system. For controllers, Data Annotations and
`ModelState.IsValid` remain the standard approach.

---

## FluentValidation

FluentValidation provides a fluent API for building strongly-typed validation rules. It excels at complex business
validation with cross-property rules, conditional logic, and database-dependent checks.

### Validator Definition

```csharp

// <PackageReference Include="FluentValidation" Version="11.*" />
// <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
public sealed class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.OrderDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Order date cannot be in the future");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("Order must have at least one line item");

        RuleForEach(x => x.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(l => l.ProductId).NotEmpty();
                line.RuleFor(l => l.Quantity).GreaterThan(0);
                line.RuleFor(l => l.UnitPrice).GreaterThan(0);
            });

        // Conditional rule
        When(x => x.ShippingMethod == ShippingMethod.Express, () =>
        {
            RuleFor(x => x.ShippingAddress)
                .NotNull()
                .WithMessage("Express shipping requires an address");
        });
    }
}

```text

### DI Registration with Assembly Scanning

```csharp

// Registers all AbstractValidator<T> implementations from the assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

```csharp

### Manual Validation Pattern (Recommended)

FluentValidation's ASP.NET pipeline auto-validation is deprecated. Use manual validation in endpoint handlers or
endpoint filters instead:

```csharp

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    IValidator<CreateOrderRequest> validator,
    AppDbContext db) =>
{
    var result = await validator.ValidateAsync(request);
    if (!result.IsValid)
    {
        return TypedResults.ValidationProblem(result.ToDictionary());
    }

    var order = new Order { CustomerId = request.CustomerId };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/orders/{order.Id}", order);
});

```text

### FluentValidation Endpoint Filter

For reusable validation across multiple endpoints, create a generic endpoint filter (see also
[skill:dotnet-minimal-apis] for filter pipeline details):

```csharp

public sealed class FluentValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
            return TypedResults.BadRequest("Request body is required");

        var result = await validator.ValidateAsync(argument);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        return await next(context);
    }
}

// Apply to endpoints
products.MapPost("/", CreateProduct)
    .AddEndpointFilter<FluentValidationFilter<CreateProductDto>>();

```text

**Gotcha:** Do not use the deprecated `FluentValidation.AspNetCore` auto-validation pipeline. It was removed in
FluentValidation 11. Use manual validation or endpoint filters as shown above.

---

## Data Annotations

Data Annotations provide declarative validation through attributes. They work with MVC model binding, Minimal API
binding, and the .NET 10 `AddValidation()` source generator.

### Standard Attributes

```csharp

public sealed class UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 1)]
    public required string Name { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Price must be between {1} and {2}")]
    public decimal Price { get; set; }

    [RegularExpression(@"^[A-Z]{2,4}-\d{4,8}$")]
    public string? Sku { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Url]
    public string? WebsiteUrl { get; set; }

    [Phone]
    public string? SupportPhone { get; set; }
}

```text

### Custom ValidationAttribute

```csharp

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is DateOnly date && date <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ValidationResult(
                ErrorMessage ?? "Date must be in the future",
                new[] { context.MemberName! });
        }
        return ValidationResult.Success;
    }
}

// Usage
public sealed class CreateEventDto
{
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    [FutureDate(ErrorMessage = "Event date must be in the future")]
    public DateOnly EventDate { get; set; }
}

```text

### IValidatableObject for Cross-Property Validation

```csharp

public sealed class DateRangeDto : IValidatableObject
{
    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Range(1, 365)]
    public int MaxDays { get; set; } = 30;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EndDate < StartDate)
        {
            yield return new ValidationResult(
                "End date must be after start date",
                new[] { nameof(EndDate) });
        }

        if ((EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days > MaxDays)
        {
            yield return new ValidationResult(
                $"Date range cannot exceed {MaxDays} days",
                new[] { nameof(StartDate), nameof(EndDate) });
        }
    }
}

```text

**Gotcha:** Options pattern classes must use `{ get; set; }` not `{ get; init; }` because the configuration binder needs
to mutate properties after construction. Validation attributes on `init`-only properties work for request DTOs but fail
for options classes bound via `IConfiguration`. See [skill:dotnet-csharp-configuration] for Options pattern validation.

---

## Endpoint Filters for Validation

Endpoint filters integrate validation into the Minimal API request pipeline as a cross-cutting concern. Filters execute
before the handler, enabling centralized validation logic.

### Generic Data Annotations Filter

```csharp

public sealed class DataAnnotationsValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
            return TypedResults.BadRequest("Request body is required");

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(argument);

        if (!Validator.TryValidateObject(argument, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .Where(r => r.MemberNames.Any())
                .GroupBy(r => r.MemberNames.First())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.ErrorMessage ?? "Validation failed").ToArray());

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}

// Apply to endpoints or route groups
products.MapPost("/", CreateProduct)
    .AddEndpointFilter<DataAnnotationsValidationFilter<CreateProductDto>>();

products.MapPut("/{id:int}", UpdateProduct)
    .AddEndpointFilter<DataAnnotationsValidationFilter<UpdateProductDto>>();

```text


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
