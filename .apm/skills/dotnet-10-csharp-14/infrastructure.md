# Infrastructure Patterns in .NET 10

Options pattern, resilience, channels, caching, logging, and data access.

## Options Pattern

### Configuration Classes

````csharp
public class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    [StringLength(200)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MaxRetryCount { get; set; } = 3;

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
```text

### Registration (CRITICAL: ValidateOnStart)

```csharp
builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart(); // CRITICAL - fails fast on invalid config
```text

### Usage

```csharp
public class UserService
{
    private readonly DatabaseOptions _options;

    public UserService(IOptions<DatabaseOptions> options)
    {
        _options = options.Value; // Snapshot at startup
    }

    // Or for reloadable options
    public UserService(IOptionsMonitor<DatabaseOptions> options)
    {
        _options = options.CurrentValue; // Live updates
        options.OnChange(updated => _options = updated);
    }

    // Or for per-request options
    public UserService(IOptionsSnapshot<DatabaseOptions> options)
    {
        _options = options.Value; // Per-request snapshot
    }
}
```text

### IOptions Selection Guide

| Scenario     | Use                   | Why                       |
| ------------ | --------------------- | ------------------------- |
| Startup only | `IOptions<T>`         | Single value, no reload   |
| Live updates | `IOptionsMonitor<T>`  | Reacts to config changes  |
| Per-request  | `IOptionsSnapshot<T>` | Consistent within request |

## HTTP Resilience

### Standard Resilience Handler

```csharp
builder.Services.AddHttpClient<IUserApiClient, UserApiClient>()
    .AddStandardResilienceHandler(options =>
    {
        // Retry strategy
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.Retry.BackoffType = DelayBackoffType.Exponential;

        // Circuit breaker
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

        // Timeout
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);

        // Total request timeout
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    });
```text

### Custom Resilience Strategy

```csharp
builder.Services.AddHttpClient<IApiClient, ApiClient>()
    .AddResilienceHandler("custom", builder =>
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            OnRetry = args =>
            {
                // Log retry attempt
                return ValueTask.CompletedTask;
            }
        });

        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(30),
            FailureRatio = 0.5,
            MinimumThroughput = 20,
            BreakDuration = TimeSpan.FromSeconds(60),
            OnOpened = args =>
            {
                // Log circuit opened
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                // Log circuit closed
                return ValueTask.CompletedTask;
            }
        });

        builder.AddTimeout(TimeSpan.FromSeconds(30));
    });
```text

### Policy Selection

```csharp
// Different policies for different clients
builder.Services.AddHttpClient<ICriticalApiClient, CriticalApiClient>()
    .AddStandardResilienceHandler(); // Default

builder.Services.AddHttpClient<IBackgroundApiClient, BackgroundApiClient>()
    .AddResilienceHandler("background", builder =>
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 10,
            Delay = TimeSpan.FromSeconds(5),
            BackoffType = DelayBackoffType.Exponential
        });
    });
```text

## Channels

### Bounded Channel (Recommended)

```csharp
public class BackgroundProcessor : BackgroundService
{
    private readonly Channel<WorkItem> _channel;

    public BackgroundProcessor()
    {
        _channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait // Block when full
        });
    }

    public async Task QueueWorkAsync(WorkItem item, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(item, ct);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessAsync(item, stoppingToken);
        }
    }
}
```text

### Channel with Drop Policy

```csharp
// When dropping items is acceptable
_channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.DropOldest // Drop oldest when full
});
```text

### Unbounded Channel (Use Carefully)

```csharp
// Only when memory is not a concern
_channel = Channel.CreateUnbounded<WorkItem>();
```text

### Channel Type Selection

```text
Trust producer? → Unbounded (fastest)
↓ No
Can drop? → Bounded + DropOldest
↓ No
Bounded + Wait (safest)
```text

## Health Checks

### Basic Setup

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddCheck<ExternalApiHealthCheck>("external-api")
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        return allocated < 1024 * 1024 * 1024 // 1GB
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Degraded("Memory usage high");
    });

var app = builder.Build();

// Health endpoint
app.MapHealthChecks("/health");

// Detailed health for monitoring
app.MapHealthChecks("/health/detailed", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var json = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }
});
```text

### Custom Health Check

```csharp
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly IExternalApiClient _client;

    public ExternalApiHealthCheck(IExternalApiClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("API unavailable", ex);
        }
    }
}
```text

## Output Caching

```csharp
builder.Services.AddOutputCache();

var app = builder.Build();

app.UseOutputCache();

// Cache all GET requests for 60 seconds
app.MapGet("/users", GetUsers.Handle)
    .CacheOutput();

// Cache with policy
app.MapGet("/users/{id}", GetUser.Handle)
    .CacheOutput(p => p
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("users")
        .VaryByQuery("includeDetails"));

// Invalidate cache
app.MapPost("/users", CreateUser.Handle)
    .CacheOutput(p => p.NoCache()) // Don't cache POSTs
    .AddEndpointFilter(async (context, next) =>
    {
        var result = await next(context);
        // Invalidate related caches
        await context.HttpContext.OutputCache.EvictByTagAsync("users");
        return result;
    });
```text

## Logging with Serilog

### Configuration

```csharp
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7)
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
});
```text

### Structured Logging

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<User> GetAsync(int id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);

        try
        {
            var user = await _repository.GetAsync(id);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found", id);
                return null;
            }

            _logger.LogInformation("User {UserId} found: {UserName}", id, user.Name);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            throw;
        }
    }
}
```text

## Keyed Services

```csharp
// Register multiple implementations
builder.Services.AddKeyedScoped<IEmailService, SmtpEmailService>("smtp");
builder.Services.AddKeyedScoped<IEmailService, SendGridEmailService>("sendgrid");

// Inject by key
public class NotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService([FromKeyedServices("sendgrid")] IEmailService emailService)
    {
        _emailService = emailService;
    }
}

// Or resolve dynamically
public class NotificationService
{
    private readonly IServiceProvider _provider;

    public async Task SendAsync(string provider, Email email)
    {
        var service = _provider.GetRequiredKeyedService<IEmailService>(provider);
        await service.SendAsync(email);
    }
}
```text

## EF Core

### DbContext Registration

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Database"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});
```text

### Repository Pattern

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    // ... other implementations
}
```text
````
