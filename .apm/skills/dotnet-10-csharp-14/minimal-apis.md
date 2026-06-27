# Minimal APIs in .NET 10

ASP.NET Core 10 minimal APIs replace MVC. No controllers, no action results - just maps.

## Quick Start

````csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simple endpoint
app.MapGet("/", () => "Hello World");

// With route parameters
app.MapGet("/users/{id:int}", (int id) => $"User {id}");

// With request body
app.MapPost("/users", (CreateUserRequest req) => Results.Created($"/users/{req.Id}", req));

app.Run();
```text

## TypedResults (Always Use)

`TypedResults` provides strongly-typed responses with OpenAPI support.

### Why TypedResults Over Results

| Aspect       | Results.Ok() | TypedResults.Ok() |
| ------------ | ------------ | ----------------- |
| Type safety  | ❌ object    | ✅ TResult        |
| OpenAPI      | ❌ Limited   | ✅ Full support   |
| Intellisense | ❌ Weak      | ✅ Strong         |
| Testing      | ❌ Harder    | ✅ Easier         |

### Common TypedResults

```csharp
// Success responses
TypedResults.Ok<T>(value)
TypedResults.Created<T>(uri, value)
TypedResults.CreatedAtRoute<T>(routeName, routeValues, value)
TypedResults.Accepted<T>(uri, value)
TypedResults.NoContent()

// Error responses
TypedResults.BadRequest<T>(problemDetails)
TypedResults.NotFound()
TypedResults.Conflict<T>(error)
TypedResults.Unauthorized()
TypedResults.ValidationProblem(errors)
```text

### Full Example

```csharp
app.MapGet("/users/{id:int}", async (int id, IUserService service) =>
{
    var user = await service.GetByIdAsync(id);
    return user is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(user);
});
```text

## Built-in Validation

.NET 10 adds native validation without FluentValidation.

### Basic Validation

```csharp
// Enable validation
builder.Services.AddValidation();

// Automatic validation
app.MapPost("/users", (CreateUserRequest dto) =>
    TypedResults.Created($"/users/{dto.Id}", dto));

public record CreateUserRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}
```text

### Custom Validation

```csharp
public class CreateUserRequest : IValidatableObject
{
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Password != ConfirmPassword)
            yield return new ValidationResult("Passwords must match", [nameof(ConfirmPassword)]);
    }
}
```text

### Validation Problem Details

```csharp
app.MapPost("/users", (CreateUserRequest dto) =>
{
    if (!Validate(dto, out var errors))
        return TypedResults.ValidationProblem(errors);

    // ... create user
});
```text

## Filters

Minimal API filters provide middleware-like functionality at endpoint level.

### Endpoint Filters

```csharp
app.MapGet("/admin-only", [Authorize(Roles = "Admin")] () => "Secret")
    .AddEndpointFilter(async (context, next) =>
    {
        // Before endpoint
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Admin endpoint accessed");

        var result = await next(context);

        // After endpoint
        logger.LogInformation("Admin endpoint completed");
        return result;
    });
```text

### Validation Filter

```csharp
public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.GetArgument<T>(0);
        if (validator is IValidatableObject validatable)
        {
            var validationContext = new ValidationContext(validatable);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(validator, validationContext, results, true))
            {
                var errors = results.ToDictionary(
                    r => r.MemberNames.First(),
                    r => new[] { r.ErrorMessage! });
                return TypedResults.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}

// Usage
app.MapPost("/users", (CreateUserRequest dto) => ...)
    .AddEndpointFilter<ValidationFilter<CreateUserRequest>>();
```text

## Modular Monolith Pattern

Organize by feature, not by layer.

### Project Structure

```text
src/
├── Modules/
│   ├── Users/
│   │   ├── UsersModule.cs
│   │   ├── Contracts/
│   │   ├── Features/
│   │   │   ├── CreateUser/
│   │   │   ├── GetUser/
│   │   │   └── UpdateUser/
│   │   └── Infrastructure/
│   └── Orders/
│       ├── OrdersModule.cs
│       └── ...
└── Program.cs
```text

### Module Implementation

```csharp
// UsersModule.cs
public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        group.MapPost("/", CreateUser.Handle);
        group.MapGet("/{id:int}", GetUser.Handle);
        group.MapPut("/{id:int}", UpdateUser.Handle);
        group.MapDelete("/{id:int}", DeleteUser.Handle);

        return app;
    }
}

// Feature: CreateUser/CreateUser.cs
public static class CreateUser
{
    public record Request(string Name, string Email);
    public record Response(int Id, string Name, string Email);

    public static async Task<Results<Created<Response>, ValidationProblem>> Handle(
        Request request,
        IUserService service,
        CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Match<Results<Created<Response>, ValidationProblem>>(
            user => TypedResults.Created($"/api/users/{user.Id}",
                new Response(user.Id, user.Name, user.Email)),
            errors => TypedResults.ValidationProblem(errors.ToDictionary()));
    }
}
```text

### Registration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register modules
builder.Services.AddUsersModule();
builder.Services.AddOrdersModule();

var app = builder.Build();

// Map endpoints
app.MapUsersEndpoints();
app.MapOrdersEndpoints();

app.Run();
```text

## Route Constraints

```csharp
// Type constraints
app.MapGet("/users/{id:int}", (int id) => ...);
app.MapGet("/users/{name:alpha}", (string name) => ...);
app.MapGet("/files/{filename:regex(^[a-z0-9]+$)}", (string filename) => ...);

// Range constraints
app.MapGet("/items/{id:range(1,1000)}", (int id) => ...);

// Length constraints
app.MapGet("/codes/{code:length(6)}", (string code) => ...);

// Multiple constraints
app.MapGet("/products/{id:int:min(1)}", (int id) => ...);
```text

## Endpoint Configuration

```csharp
app.MapGet("/users", GetUsers.Handle)
    .WithName("GetUsers")                    // Named endpoint
    .WithTags("Users")                       // OpenAPI tag
    .WithOpenApi(op =>                       // OpenAPI customization
    {
        op.Summary = "Get all users";
        op.Description = "Returns a paginated list of users";
        return op;
    })
    .Produces<UserDto[]>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .RequireAuthorization()                  // Auth requirement
    .AddEndpointFilter<LoggingFilter>()      // Custom filter
    .WithGroupName("v1");                    // API versioning
```text

## Best Practices

### ✅ DO

- Use `TypedResults` for all responses
- Group related endpoints with `MapGroup`
- Use record types for request/response DTOs
- Implement modular organization
- Use endpoint filters for cross-cutting concerns
- Keep endpoints thin (delegate to services)

### ❌ DON'T

- Use `Results.Ok()` (use `TypedResults.Ok()`)
- Put business logic in endpoints
- Return anonymous objects
- Use action results (MVC style)
- Forget to add validation
- Mix minimal APIs with controllers in same app

## Testing

### Integration Tests

```csharp
public class UsersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        // Arrange
        var expected = new UserDto(1, "John", "john@example.com");

        // Act
        var response = await _client.GetAsync("/api/users/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().BeEquivalentTo(expected);
    }
}
```text

### Unit Tests with TypedResults

```csharp
[Fact]
public async Task CreateUser_ReturnsCreated_WhenValid()
{
    // Arrange
    var request = new CreateUser.Request("John", "john@example.com");
    var mockService = Substitute.For<IUserService>();
    mockService.CreateAsync(request, Arg.Any<CancellationToken>())
        .Returns(new User(1, "John", "john@example.com"));

    // Act
    var result = await CreateUser.Handle(request, mockService, CancellationToken.None);

    // Assert
    result.Should().BeOfType<Results<Created<CreateUser.Response>, ValidationProblem>>();
}
```text
````
