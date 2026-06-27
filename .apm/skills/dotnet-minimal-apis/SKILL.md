---
name: dotnet-minimal-apis
description: Builds ASP.NET Core Minimal APIs -- route groups, filters, TypedResults, OpenAPI.
license: MIT
targets: ['*']
category: web
subcategory: minimal-apis
tags:
  - web
  - dotnet
  - skill
  - minimal-apis
  - api
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-architecture-patterns
  - dotnet-middleware-patterns
  - dotnet-api-versioning
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for web tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-minimal-apis

Minimal APIs are Microsoft's recommended approach for new ASP.NET Core HTTP API projects. They provide a lightweight,
lambda-based programming model with first-class OpenAPI support, endpoint filters for cross-cutting concerns, and route
groups for organization at scale.

## Scope

- Route groups and endpoint organization
- Endpoint filters for cross-cutting concerns
- TypedResults for compile-time response type safety
- Parameter binding (route, query, body, services)
- JSON configuration with ConfigureHttpJsonOptions
- Carter library integration for auto-discovery modules

## Out of scope

- API versioning strategies -- see [skill:dotnet-api-versioning]
- Input validation frameworks -- see [skill:dotnet-input-validation]
- Architectural patterns (vertical slices, CQRS) -- see [skill:dotnet-architecture-patterns]
- Authentication and authorization -- see [skill:dotnet-api-security]
- OpenAPI document generation -- see [skill:dotnet-openapi]
- gRPC and real-time communication -- see [skill:dotnet-grpc] and [skill:dotnet-realtime-communication]

Cross-references: [skill:dotnet-architecture-patterns] for organizing large APIs, [skill:dotnet-input-validation] for
request validation, [skill:dotnet-api-versioning] for versioning strategies, [skill:dotnet-openapi] for OpenAPI
customization.

---

## Route Groups

Route groups organize related endpoints under a shared prefix, applying common configuration (filters, metadata,
authorization) once. They replace repetitive chaining of `MapGet`/`MapPost` with shared prefixes.

````csharp

var app = builder.Build();

// Group endpoints under /api/products with shared configuration
var products = app.MapGroup("/api/products")
    .WithTags("Products")
    .RequireAuthorization();

products.MapGet("/", async (AppDbContext db) =>
    TypedResults.Ok(await db.Products.ToListAsync()));

products.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product product
        ? TypedResults.Ok(product)
        : TypedResults.NotFound());

products.MapPost("/", async (CreateProductDto dto, AppDbContext db) =>
{
    var product = new Product { Name = dto.Name, Price = dto.Price };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/products/{product.Id}", product);
});

products.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    if (await db.Products.FindAsync(id) is not Product product)
        return TypedResults.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});

```text

### Nested Groups

Groups can be nested to compose prefixes and filters:

```csharp

var api = app.MapGroup("/api")
    .AddEndpointFilter<RequestLoggingFilter>();

var v1 = api.MapGroup("/v1");
var products = v1.MapGroup("/products").WithTags("Products");
var orders = v1.MapGroup("/orders").WithTags("Orders");

// Registers as: GET /api/v1/products
products.MapGet("/", GetProducts);
// Registers as: POST /api/v1/orders
orders.MapPost("/", CreateOrder);

```text

---

## Endpoint Filters

Endpoint filters provide a pipeline for cross-cutting concerns (logging, validation, authorization enrichment) similar
to MVC action filters but specific to Minimal APIs.

### IEndpointFilter Interface

```csharp

public sealed class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // Extract the argument of type T from the endpoint parameters
        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (argument is null)
            return TypedResults.BadRequest("Request body is required");

        var result = await validator.ValidateAsync(argument);
        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(
                result.ToDictionary());
        }

        return await next(context);
    }
}

```text

### Applying Filters

```csharp

// Apply to a single endpoint
products.MapPost("/", CreateProduct)
    .AddEndpointFilter<ValidationFilter<CreateProductDto>>();

// Apply to an entire route group
var products = app.MapGroup("/api/products")
    .AddEndpointFilter<RequestLoggingFilter>();

// Inline filter using a lambda
products.MapGet("/{id:int}", GetProductById)
    .AddEndpointFilter(async (context, next) =>
    {
        var id = context.GetArgument<int>(0);
        if (id <= 0)
            return TypedResults.BadRequest("ID must be positive");

        return await next(context);
    });

```text

### Filter Execution Order

Filters execute in registration order (first registered = outermost). The endpoint handler runs after all filters pass:

```text

Request -> Filter1 -> Filter2 -> Filter3 -> Handler
Response <- Filter1 <- Filter2 <- Filter3 <-

```text

---

## TypedResults

Always use `TypedResults` (static factory) instead of `Results` (interface factory) for Minimal API return values.
`TypedResults` returns concrete types that the OpenAPI metadata generator can inspect at build time, producing accurate
response schemas automatically.

```csharp

// PREFERRED: TypedResults -- concrete return types, auto-generates OpenAPI metadata
products.MapGet("/{id:int}", async Task<Results<Ok<Product>, NotFound>> (
    int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product product
        ? TypedResults.Ok(product)
        : TypedResults.NotFound());

// AVOID: Results -- returns IResult, OpenAPI generator cannot infer response types
products.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product product
        ? Results.Ok(product)
        : Results.NotFound());

```text

### Union Return Types

Use `Results<T1, T2, ...>` to declare all possible response types for a single endpoint. This enables accurate OpenAPI
documentation with multiple response codes:

```csharp

products.MapPost("/", async Task<Results<Created<Product>, ValidationProblem, Conflict>> (
    CreateProductDto dto, AppDbContext db) =>
{
    if (await db.Products.AnyAsync(p => p.Sku == dto.Sku))
        return TypedResults.Conflict();

    var product = new Product { Name = dto.Name, Sku = dto.Sku, Price = dto.Price };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/products/{product.Id}", product);
});

```text

---

## OpenAPI 3.1 Integration

.NET 10 adds built-in OpenAPI 3.1 support via `Microsoft.AspNetCore.OpenApi`. Minimal APIs generate OpenAPI metadata
from `TypedResults`, parameter bindings, and attributes automatically.

```csharp

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Serves /openapi/v1.json
}

```json

### Enriching Metadata

```csharp

products.MapGet("/{id:int}", GetProductById)
    .WithName("GetProductById")
    .WithSummary("Get a product by its ID")
    .WithDescription("Returns the product details for the specified ID, or 404 if not found.")
    .Produces<Product>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

```text

For advanced OpenAPI customization (document transformers, operation transformers, schema customization), see
[skill:dotnet-openapi].

---

## Organization Patterns for Scale

As an API grows beyond a handful of endpoints, organize endpoints into separate static classes or extension methods.

### Extension Method Pattern

```csharp

// ProductEndpoints.cs
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return group;
    }

    private static async Task<Ok<List<Product>>> GetAll(AppDbContext db) =>
        TypedResults.Ok(await db.Products.ToListAsync());

    private static async Task<Results<Ok<Product>, NotFound>> GetById(
        int id, AppDbContext db) =>
        await db.Products.FindAsync(id) is Product p
            ? TypedResults.Ok(p)
            : TypedResults.NotFound();

    private static async Task<Created<Product>> Create(
        CreateProductDto dto, AppDbContext db)
    {
        var product = new Product { Name = dto.Name, Price = dto.Price };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/api/products/{product.Id}", product);
    }

    private static async Task<Results<NoContent, NotFound>> Update(
        int id, UpdateProductDto dto, AppDbContext db)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return TypedResults.NotFound();

        product.Name = dto.Name;
        product.Price = dto.Price;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        int id, AppDbContext db)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return TypedResults.NotFound();

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}

// Program.cs
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapCustomerEndpoints();

```csharp

### Carter Library

For projects that prefer auto-discovery of endpoint modules, the Carter library provides an `ICarterModule` interface:

```csharp

// <PackageReference Include="Carter" Version="8.*" />
public sealed class ProductModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (AppDbContext db) =>
            TypedResults.Ok(await db.Products.ToListAsync()));

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Products.FindAsync(id) is Product p
                ? TypedResults.Ok(p)
                : TypedResults.NotFound());
    }
}

// Program.cs
builder.Services.AddCarter();
var app = builder.Build();
app.MapCarter(); // Auto-discovers and registers all ICarterModule implementations

```csharp

### Vertical Slice Organization

For projects using vertical slice architecture (see [skill:dotnet-architecture-patterns]), each feature owns its
endpoints, handlers, and models in a single directory:

```text

Features/
  Products/
    GetProducts.cs       # Endpoint + handler + response DTO
    CreateProduct.cs     # Endpoint + handler + request/response DTOs
    UpdateProduct.cs
    DeleteProduct.cs
    ProductEndpoints.cs  # Route group registration

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
