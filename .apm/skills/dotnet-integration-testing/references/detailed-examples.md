
        Assert.Equal("cust-1", order!.CustomerId);
    }
}

```text

---

## .NET Aspire Testing

.NET Aspire provides `DistributedApplicationTestingBuilder` for testing multi-service applications orchestrated with
Aspire. This tests the actual distributed topology including service discovery, configuration, and health checks.

### Package

```xml

<PackageReference Include="Aspire.Hosting.Testing" Version="9.*" />

```xml

### Basic Aspire Test

```csharp

public class AspireIntegrationTests
{
    [Fact]
    public async Task ApiService_ReturnsHealthy()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyApp_AppHost>();

        await using var app = await builder.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api-service");

        var response = await httpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiService_WithDatabase_ReturnsOrders()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyApp_AppHost>();

        await using var app = await builder.BuildAsync();
        await app.StartAsync();

        // Wait for resources to be healthy
        var resourceNotification = app.Services
            .GetRequiredService<ResourceNotificationService>();
        await resourceNotification
            .WaitForResourceHealthyAsync("api-service")
            .WaitAsync(TimeSpan.FromSeconds(60));

        var httpClient = app.CreateHttpClient("api-service");
        var response = await httpClient.GetAsync("/api/orders");

        response.EnsureSuccessStatusCode();
    }
}

```text

### Aspire with Service Overrides

Replace services in the Aspire app model for testing:

```csharp

[Fact]
public async Task ApiService_WithMockedExternalDependency()
{
    var builder = await DistributedApplicationTestingBuilder
        .CreateAsync<Projects.MyApp_AppHost>();

    // Override configuration for the API service
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler();
    });

    await using var app = await builder.BuildAsync();
    await app.StartAsync();

    var httpClient = app.CreateHttpClient("api-service");
    var response = await httpClient.GetAsync("/api/orders");

    response.EnsureSuccessStatusCode();
}

```text

---

## Database Fixture Patterns

### Per-Test Isolation with Transactions

Roll back each test's changes using a transaction scope:

```csharp

public class TransactionalTestBase : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly PostgresFixture _postgres;
    private AppDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;

    public TransactionalTestBase(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    protected AppDbContext Context => _context;

    public async ValueTask InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .Options;
        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _context.DisposeAsync();
    }
}

public class OrderTests : TransactionalTestBase
{
    public OrderTests(PostgresFixture postgres) : base(postgres) { }

    [Fact]
    public async Task Insert_ValidOrder_Persists()
    {
        Context.Orders.Add(new Order { CustomerId = "cust-1", Total = 50m });
        await Context.SaveChangesAsync();

        var count = await Context.Orders.CountAsync();
        Assert.Equal(1, count);
        // Transaction rolls back after test -- database stays clean
    }
}

```text

### Per-Test Isolation with Respawn

Use Respawn to reset database state between tests by deleting data instead of rolling back transactions. This is useful
when transaction rollback is not feasible (e.g., testing code that commits its own transactions):

```csharp

// NuGet: Respawn
// Combined fixture: owns the container AND the respawner
public class RespawnablePostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        // Run migrations or EnsureCreated before creating respawner
        // so it knows which tables to clean
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}

```text

---

## Test Isolation Strategies

### Strategy Comparison

| Strategy                       | Speed    | Isolation | Complexity | Best For                                             |
| ------------------------------ | -------- | --------- | ---------- | ---------------------------------------------------- |
| **Transaction rollback**       | Fastest  | High      | Low        | Tests that use a single DbContext                    |
| **Respawn (data deletion)**    | Fast     | High      | Medium     | Tests where code commits its own transactions        |
| **Fresh container per class**  | Slow     | Highest   | Low        | Tests that modify schema or need complete isolation  |
| **Shared container + cleanup** | Moderate | Medium    | Medium     | Test suites with many classes sharing infrastructure |

### Container Lifecycle Recommendations

```text

Per-test:       Too slow. Never spin up a container per test.
Per-class:      Good isolation, acceptable speed with ICollectionFixture.
Per-collection: Best balance -- share one container across related test classes.
Per-assembly:   Fastest but requires careful cleanup between tests.

```text

Use `ICollectionFixture<T>` (see [skill:dotnet-xunit]) to share a single container across multiple test classes while
running those classes sequentially to avoid data conflicts.

---

## Testing with Redis

```csharp

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();
    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition("Redis")]
public class RedisCollection : ICollectionFixture<RedisFixture> { }

[Collection("Redis")]
public class CacheServiceTests
{
    private readonly RedisFixture _redis;

    public CacheServiceTests(RedisFixture redis) => _redis = redis;

    [Fact]
    public async Task SetAndGet_RoundTrip_ReturnsOriginalValue()
    {
        var multiplexer = await ConnectionMultiplexer.ConnectAsync(
            _redis.ConnectionString);
        var cache = new RedisCacheService(multiplexer);

        await cache.SetAsync("key-1", new Order { Id = 1, Total = 99m });
        var result = await cache.GetAsync<Order>("key-1");

        Assert.NotNull(result);
        Assert.Equal(99m, result.Total);
    }
}

```text

---

## Key Principles

- **Use WebApplicationFactory for API tests.** It is faster, more reliable, and more deterministic than testing against
  a deployed instance.
- **Use Testcontainers for real infrastructure.** Do not mock `DbContext` -- test against a real database to verify
  LINQ-to-SQL translation and constraint enforcement.
- **Share containers across test classes** via `ICollectionFixture` to avoid the overhead of starting a new container
  per class.
- **Choose the right isolation strategy.** Transaction rollback is fastest and simplest; use Respawn when you cannot
  control transaction boundaries.
- **Always clean up test data.** Leftover data from one test causes flaky failures in another. Use transaction rollback,
  Respawn, or fresh containers.
- **Match `Microsoft.AspNetCore.Mvc.Testing` version to TFM.** Using the wrong version causes runtime binding failures.

---

## Agent Gotchas

1. **Do not hardcode `Microsoft.AspNetCore.Mvc.Testing` versions.** The package version must match the project's target
   framework major version. Specifying e.g. `Version="8.0.0"` breaks net9.0 projects.
2. **Do not forget `InternalsVisibleTo` for the `Program` class.** Without it, `WebApplicationFactory<Program>` cannot
   access the entry point and tests fail at compile time.
3. **Do not use `EnsureCreated()` with Respawn.** `EnsureCreated()` does not track migrations. Use
   `Database.MigrateAsync()` for production schemas, or `EnsureCreated()` only for simple test schemas.
4. **Do not dispose `WebApplicationFactory` before `HttpClient`.** The factory owns the test server; disposing it
   invalidates all clients. Let xUnit manage disposal via `IClassFixture`.
5. **Do not use `localhost` ports with Testcontainers.** Testcontainers maps random host ports to container ports.
   Always use the connection string from the container object (e.g., `_container.GetConnectionString()`), never
   hardcoded ports.
6. **Do not skip Docker availability checks in CI.** Testcontainers requires a running Docker daemon. Ensure your CI
   environment has Docker available, or use conditional test skipping when Docker is unavailable.

---

## References

- [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [.NET Aspire testing](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [Respawn](https://github.com/jbogard/Respawn)
- [Testcontainers PostgreSQL module](https://dotnet.testcontainers.org/modules/postgres/)
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
