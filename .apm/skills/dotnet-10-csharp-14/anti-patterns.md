# Anti-Patterns in .NET 10

Common mistakes and their fixes for modern .NET development.

## HttpClient Anti-Patterns

### ❌ DON'T: new HttpClient()

```csharp
// WRONG: Creates socket exhaustion
public class BadService
{
    public async Task<Data> GetDataAsync()
    {
        using var client = new HttpClient(); // ❌ New instance per call
        return await client.GetFromJsonAsync<Data>("/api/data");
    }
}
```

### ✅ DO: Use IHttpClientFactory

```csharp
// CORRECT: HttpClient managed by factory
public class GoodService
{
    private readonly HttpClient _client;

    public GoodService(HttpClient client) // Injected by factory
    {
        _client = client;
    }

    public async Task<Data> GetDataAsync()
    {
        return await _client.GetFromJsonAsync<Data>("/api/data");
    }
}

// Registration
builder.Services.AddHttpClient<IGoodService, GoodService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### ❌ DON'T: Singleton HttpClient

```csharp
// WRONG: Stale DNS issues
public class BadService
{
    private static readonly HttpClient _client = new HttpClient(); // ❌ Static

    public async Task<Data> GetDataAsync()
    {
        return await _client.GetFromJsonAsync<Data>("/api/data");
    }
}
```

## Dependency Injection Anti-Patterns

### ❌ DON'T: Captive Dependencies

```csharp
// WRONG: Singleton depends on Scoped
public class BadBackgroundService : BackgroundService // Singleton
{
    private readonly IUserRepository _repo; // Scoped

    public BadBackgroundService(IUserRepository repo)
    {
        _repo = repo; // ❌ Captive dependency
    }
}
```

### ✅ DO: Use IServiceScopeFactory

```csharp
// CORRECT: Create scope when needed
public class GoodBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GoodBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            // ... use repo
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

### ❌ DON'T: Service Locator

```csharp
// WRONG: Hides dependencies
public class BadService
{
    public void DoWork()
    {
        var service = _serviceProvider.GetService<IEmailService>(); // ❌ Hidden dependency
        service.SendEmail();
    }
}
```

### ✅ DO: Constructor Injection

```csharp
// CORRECT: Explicit dependencies
public class GoodService
{
    private readonly IEmailService _emailService;

    public GoodService(IEmailService emailService)
    {
        _emailService = emailService; // ✅ Explicit dependency
    }

    public void DoWork()
    {
        _emailService.SendEmail();
    }
}
```

## Async/Await Anti-Patterns

### ❌ DON'T: .Result or .Wait()

```csharp
// WRONG: Blocks thread, can deadlock
public User GetUser(int id)
{
    return _repository.GetAsync(id).Result; // ❌ Blocking
}

public void SaveUser(User user)
{
    _repository.SaveAsync(user).Wait(); // ❌ Blocking
}
```

### ✅ DO: Async All the Way

```csharp
// CORRECT: Propagate async
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetAsync(id); // ✅ Async propagation
}

public async Task SaveUserAsync(User user)
{
    await _repository.SaveAsync(user); // ✅ Async propagation
}
```

### ❌ DON'T: async void

```csharp
// WRONG: No way to catch exceptions
public async void HandleButtonClick() // ❌ async void
{
    await SaveDataAsync();
}
```

### ✅ DO: async Task

```csharp
// CORRECT: Can handle exceptions
public async Task HandleButtonClickAsync() // ✅ async Task
{
    await SaveDataAsync();
}
```

### ❌ DON'T: Forget ConfigureAwait

```csharp
// WRONG: Can deadlock in UI/WPF
public async Task<Data> GetDataAsync()
{
    var result = await _httpClient.GetAsync("/data"); // ❌ Missing ConfigureAwait
    return await result.Content.ReadAsAsync<Data>();
}
```

### ✅ DO: ConfigureAwait(false)

```csharp
// CORRECT: For library code
public async Task<Data> GetDataAsync()
{
    var result = await _httpClient.GetAsync("/data").ConfigureAwait(false); // ✅
    return await result.Content.ReadAsAsync<Data>().ConfigureAwait(false); // ✅
}
```

## Database Anti-Patterns

### ❌ DON'T: N+1 Queries

```csharp
// WRONG: N+1 query problem
var orders = await context.Orders.ToListAsync(); // 1 query
foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name); // N queries - one per order!
}
```

### ✅ DO: Eager Loading

```csharp
// CORRECT: Single query with join
var orders = await context.Orders
    .Include(o => o.Customer) // ✅ Eager load
    .ToListAsync();

foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name); // No additional queries
}
```

### ❌ DON'T: Track Unnecessary Queries

```csharp
// WRONG: Tracking for read-only
var users = await context.Users.ToListAsync(); // ❌ Tracking enabled
foreach (var user in users)
{
    Console.WriteLine(user.Name); // Read-only, no tracking needed
}
```

### ✅ DO: AsNoTracking

```csharp
// CORRECT: No tracking for read-only
var users = await context.Users
    .AsNoTracking() // ✅ No tracking overhead
    .ToListAsync();
```

## Exception Anti-Patterns

### ❌ DON'T: Catch and Swallow

```csharp
// WRONG: Hides errors
try
{
    await ProcessAsync();
}
catch (Exception ex)
{
    // ❌ Swallowing exception
    _logger.LogWarning("Something went wrong");
}
```

### ✅ DO: Handle Specific Exceptions

```csharp
// CORRECT: Specific handling
try
{
    await ProcessAsync();
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed");
    throw; // Re-throw after logging
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    throw; // Don't swallow unknown exceptions
}
```

### ❌ DON'T: Exceptions for Flow Control

```csharp
// WRONG: Using exceptions for normal flow
public async Task<User> GetUserAsync(int id)
{
    try
    {
        return await _repository.GetAsync(id);
    }
    catch (Exception)
    {
        return null; // ❌ Exception for expected case
    }
}
```

### ✅ DO: Use Result Pattern

```csharp
// CORRECT: Expected failures are values
public async Task<ErrorOr<User>> GetUserAsync(int id)
{
    var user = await _repository.GetAsync(id);
    return user is null
        ? Error.NotFound("User.NotFound", $"User {id} not found")
        : user;
}

// Usage
var result = await service.GetUserAsync(id);
if (result.IsError)
{
    return result.FirstError;
}
return result.Value;
```

## Configuration Anti-Patterns

### ❌ DON'T: Missing ValidateOnStart

```csharp
// WRONG: Invalid config discovered at runtime
builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateDataAnnotations(); // ❌ No ValidateOnStart
```

### ✅ DO: Fail Fast

```csharp
// CORRECT: Invalid config fails at startup
builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart(); // ✅ Fail fast
```

### ❌ DON'T: Hardcoded Values

```csharp
// WRONG: Hardcoded connection string
public class BadRepository
{
    private const string ConnectionString = "Server=...;Database=..."; // ❌
}
```

### ✅ DO: Configuration Injection

```csharp
// CORRECT: From configuration
public class GoodRepository
{
    private readonly string _connectionString;

    public GoodRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString; // ✅ From config
    }
}
```

## Logging Anti-Patterns

### ❌ DON'T: String Interpolation

```csharp
// WRONG: Always evaluates
_logger.LogInformation($"User {userId} logged in from {ipAddress}"); // ❌
```

### ✅ DO: Structured Logging

```csharp
// CORRECT: Structured, lazy evaluation
_logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress); // ✅
```

### ❌ DON'T: Log Sensitive Data

```csharp
// WRONG: Logging PII
_logger.LogInformation("User logged in with password: {Password}", password); // ❌ NEVER
```

## DateTime Anti-Patterns

### ❌ DON'T: DateTime.Now

```csharp
// WRONG: Local time issues
var timestamp = DateTime.Now; // ❌ Local time
```

### ✅ DO: DateTime.UtcNow

```csharp
// CORRECT: UTC everywhere
var timestamp = DateTime.UtcNow; // ✅ UTC

// For display only
var localTime = timestamp.ToLocalTime();
```

## Quick Reference: Anti-Pattern Summary

| Anti-Pattern                 | Fix                           |
| ---------------------------- | ----------------------------- |
| `new HttpClient()`           | Use `IHttpClientFactory`      |
| `.Result` / `.Wait()`        | Propagate `async`/`await`     |
| `async void`                 | Use `async Task`              |
| Singleton → Scoped           | Use `IServiceScopeFactory`    |
| `catch { }`                  | Re-throw or log properly      |
| Exceptions for flow          | Use Result pattern            |
| N+1 queries                  | Use `Include()` or projection |
| Missing `ValidateOnStart`    | Always add to Options         |
| String interpolation in logs | Use structured logging        |
| `DateTime.Now`               | Use `DateTime.UtcNow`         |
| Hardcoded values             | Use configuration             |
