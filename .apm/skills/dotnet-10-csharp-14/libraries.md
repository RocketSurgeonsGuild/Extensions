# Recommended Libraries for .NET 10

Battle-tested packages for common scenarios.

## Validation

### FluentValidation

```bash
dotnet add package FluentValidation.DependencyInjectionExtensions
```

```csharp
// Validator
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Age)
            .InclusiveRange(18, 120)
            .When(x => x.Age.HasValue);
    }
}

// Registration
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

// Usage
public class UserService
{
    private readonly IValidator<CreateUserRequest> _validator;

    public async Task<ErrorOr<User>> CreateAsync(CreateUserRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return validation.Errors.ToErrorOr<User>();
        }
        // ... create user
    }
}
```

## Result Pattern

### ErrorOr

```bash
dotnet add package ErrorOr
```

```csharp
// Service returns ErrorOr
public async Task<ErrorOr<User>> GetUserAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    return user is null
        ? Error.NotFound("User.NotFound", $"User {id} not found")
        : user;
}

// Controller uses it
var result = await _service.GetUserAsync(id);
return result.Match(
    user => TypedResults.Ok(user),
    errors => errors.First().Code switch
    {
        "User.NotFound" => TypedResults.NotFound(),
        _ => TypedResults.BadRequest(errors)
    });
```

## Mediator Pattern

### Mediator (Source Generator)

High-performance mediator implementation using Roslyn source generators. Zero reflection, minimal allocations.

```bash
dotnet add package Mediator.SourceGenerator
dotnet add package Mediator.Abstractions
```

```csharp
// Message
public sealed record CreateUserCommand(string Name, string Email) : ICommand<ErrorOr<User>>;

// Handler
public sealed class CreateUserHandler : ICommandHandler<CreateUserCommand, ErrorOr<User>>
{
    private readonly IUserRepository _repository;

    public async ValueTask<ErrorOr<User>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var user = new User(command.Name, command.Email);
        await _repository.CreateAsync(user, ct);
        return user;
    }
}

// Registration
builder.Services.AddMediator(options =>
    options.ServiceLifetime = ServiceLifetime.Scoped);

// Usage
var result = await mediator.Send(new CreateUserCommand("John", "john@example.com"), ct);
```

**Key differences from MediatR:**

- Source generator eliminates reflection
- `ICommand<T>` / `IQuery<T>` / `INotification` interfaces
- `ValueTask<T>` returns for better performance
- Automatic handler discovery at compile time
- ~10x faster than reflection-based mediators

## API Documentation

### Scalar (Modern Swagger Alternative)

```bash
dotnet add package Scalar.AspNetCore
```

```csharp
// Registration
builder.Services.AddOpenApi();

var app = builder.Build();

// Scalar UI
app.MapScalarApiReference(options =>
{
    options.Title = "My API";
    options.Theme = ScalarTheme.DeepSpace;
});
```

## Testing

### FluentAssertions

```bash
dotnet add package FluentAssertions
```

```csharp
result.Should().NotBeNull();
result.Name.Should().Be("John");
result.Should().BeEquivalentTo(expected);
```

### NSubstitute

```bash
dotnet add package NSubstitute
```

```csharp
var repo = Substitute.For<IUserRepository>();
repo.GetByIdAsync(1).Returns(new User(1, "John", "john@example.com"));
await repo.Received(1).CreateAsync(Arg.Any<User>());
```

## Logging

### Serilog

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq  # Optional: centralized logging
```

See [infrastructure.md](infrastructure.md) for Serilog configuration.

## Quick Reference

| Category   | Library          | Package                                          |
| ---------- | ---------------- | ------------------------------------------------ |
| Validation | FluentValidation | `FluentValidation.DependencyInjectionExtensions` |

| Result | ErrorOr | `ErrorOr` | | Mediator | Mediator | `Mediator.SourceGenerator` | | API Docs | Scalar |
`Scalar.AspNetCore` | | Testing | FluentAssertions | `FluentAssertions` | | Testing | NSubstitute | `NSubstitute` | |
Logging | Serilog | `Serilog.AspNetCore` |
