    ProductEndpoints.cs  # Route group registration

```csharp

---

## JSON Configuration

Minimal APIs use `System.Text.Json` by default. Configure JSON options globally for all Minimal API endpoints:

```csharp

// ConfigureHttpJsonOptions applies to Minimal APIs ONLY, not MVC controllers
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

```json

**Gotcha:** `ConfigureHttpJsonOptions` configures JSON serialization for Minimal APIs only. MVC controllers use a
separate pipeline -- configure via `builder.Services.AddControllers().AddJsonOptions(...)`. Mixing them up has no
effect.

---

## Parameter Binding

Minimal APIs bind parameters from route, query, headers, body, and DI automatically based on type and attribute
annotations.

```csharp

// Route parameter (from URL segment)
app.MapGet("/products/{id:int}", (int id) => ...);

// Query string
app.MapGet("/products", ([FromQuery] int page, [FromQuery] int pageSize) => ...);

// Header
app.MapGet("/products", ([FromHeader(Name = "X-Correlation-Id")] string correlationId) => ...);

// Body (JSON deserialized)
app.MapPost("/products", (CreateProductDto dto) => ...);

// DI-injected services (resolved automatically)
app.MapGet("/products", (AppDbContext db, ILogger<Program> logger) => ...);

// AsParameters: bind a complex object from multiple sources
app.MapGet("/products", ([AsParameters] ProductQuery query) => ...);

public record ProductQuery(
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20,
    [FromQuery] string? SortBy = null);

```text

---

## Agent Gotchas

1. **Do not use `Results` when `TypedResults` is available** -- `Results.Ok(value)` returns `IResult` and the OpenAPI
   generator cannot infer response schemas. Use `TypedResults.Ok(value)` to enable automatic schema generation.
2. **Do not forget `ConfigureHttpJsonOptions` only applies to Minimal APIs** -- MVC controllers need
   `.AddControllers().AddJsonOptions()` separately.
3. **Do not apply validation logic inline in every endpoint** -- use endpoint filters or cross-reference
   [skill:dotnet-input-validation] for centralized validation patterns.
4. **Do not register filters in the wrong order** -- first-registered filter is outermost. Put broad filters (logging)
   first, specific filters (validation) closer to the handler.
5. **Do not put all endpoints in `Program.cs`** -- organize into extension method classes or Carter modules once you
   have more than a handful of endpoints.

---

## Prerequisites

- .NET 8.0+ (LTS baseline for Minimal APIs with route groups and endpoint filters)
- .NET 10.0 for built-in OpenAPI 3.1, SSE, and built-in validation support
- `Microsoft.AspNetCore.OpenApi` for OpenAPI document generation
- `Carter` (optional) for auto-discovery endpoint modules

---

## Knowledge Sources

Minimal API patterns in this skill are grounded in guidance from:

- **David Fowler** -- AspNetCoreDiagnosticScenarios
  ([github.com/davidfowl/AspNetCoreDiagnosticScenarios](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios)).
  Authoritative source on ASP.NET Core request pipeline design, middleware best practices, and diagnostic anti-patterns.

> These sources inform the patterns and rationale presented above. This skill does not claim to represent or speak for
> any individual.

---

## References

- [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0)
- [Route Groups](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers?view=aspnetcore-10.0#route-groups)
- [Endpoint Filters](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters?view=aspnetcore-10.0)
- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0)
- [Carter Library](https://github.com/CarterCommunity/Carter)
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
