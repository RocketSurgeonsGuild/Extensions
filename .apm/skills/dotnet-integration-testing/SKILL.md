---
name: dotnet-integration-testing
description: Tests with real infrastructure. WebApplicationFactory, Testcontainers, Aspire, fixtures.
license: MIT
targets: ['*']
category: testing
subcategory: integration
tags:
  - testing
  - dotnet
  - skill
  - integration
  - testcontainers
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-testing-advanced-aspnet-integration-testing
  - dotnet-testing-advanced-testcontainers-database
  - dotnet-testing-advanced-aspire-testing
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
codexcli:
  short-description: '.NET skill guidance for testing tasks'
geminicli: {}
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
antigravity: {}
---

# dotnet-integration-testing

Integration testing patterns for .NET applications using WebApplicationFactory, Testcontainers, and .NET Aspire testing.
Covers in-process API testing, disposable infrastructure via containers, database fixture management, and test isolation
strategies.

**Version assumptions:** .NET 8.0+ baseline, Testcontainers 3.x+, .NET Aspire 9.0+. Package versions for
`Microsoft.AspNetCore.Mvc.Testing` must match the project's target framework major version (e.g., 8.x for net8.0, 9.x
for net9.0, 10.x for net10.0). Examples below use Testcontainers 4.x APIs; the patterns apply equally to 3.x with minor
namespace differences.

## Scope

- In-process API testing with WebApplicationFactory
- Disposable infrastructure via Testcontainers
- .NET Aspire distributed application testing
- Database fixture management and test isolation
- Authentication and authorization test setup

## Out of scope

- Test project scaffolding (creating projects, package references) -- see [skill:dotnet-add-testing]
- Testing strategy and test type selection -- see [skill:dotnet-testing-strategy]
- Snapshot testing for verifying API response structures -- see [skill:dotnet-snapshot-testing]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with integration test packages
referenced. Docker daemon running (required by Testcontainers). Run [skill:dotnet-version-detection] to confirm .NET
8.0+ baseline.

Cross-references: [skill:dotnet-testing-strategy] for deciding when integration tests are appropriate,
[skill:dotnet-xunit] for xUnit fixtures and parallel execution configuration, [skill:dotnet-snapshot-testing] for
verifying API response structures with Verify.

---

## WebApplicationFactory

`WebApplicationFactory<TEntryPoint>` creates an in-process test server for ASP.NET Core applications. Tests send HTTP
requests without network overhead, exercising the full middleware pipeline, routing, model binding, and serialization.

### Package

````xml

<!-- Version must match target framework: 8.x for net8.0, 9.x for net9.0, etc. -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />

```xml

### Basic Usage

```csharp

public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_ReturnsOkWithJsonArray()
    {
        var response = await _client.GetAsync("/api/orders");

        response.EnsureSuccessStatusCode();
        var orders = await response.Content
            .ReadFromJsonAsync<List<OrderDto>>();
        Assert.NotNull(orders);
    }

    [Fact]
    public async Task CreateOrder_ValidPayload_Returns201()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = "cust-123",
            Items = [new("SKU-001", Quantity: 2)]
        };

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }
}

```text

**Important:** The `Program` class must be accessible to the test project. Either make it public or add an
`InternalsVisibleTo` attribute:

```csharp

// In the API project (e.g., Program.cs or a separate file)
[assembly: InternalsVisibleTo("MyApp.Api.IntegrationTests")]

```csharp

Or in the csproj:

```xml

<ItemGroup>
  <InternalsVisibleTo Include="MyApp.Api.IntegrationTests" />
</ItemGroup>

```xml

### Customizing the Test Server

Override services, configuration, or middleware using `WebApplicationFactory<T>.WithWebHostBuilder`:

```csharp

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    // Provide connection string from test fixture (e.g., Testcontainers)
    public string ConnectionString { get; set; } = "";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                ["Features:EnableNewCheckout"] = "true"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace real services with test doubles
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender, FakeEmailSender>();

            // Replace database context with test database
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(ConnectionString));
        });
    }
}

```text

### Authenticated Requests

Test authenticated endpoints by configuring an authentication handler:

```csharp

public class AuthenticatedWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });
        });
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

```text

---

## Testcontainers

Testcontainers spins up real infrastructure (databases, message brokers, caches) in Docker containers for tests. Each
test run gets a fresh, disposable environment.

### Packages

```xml

<PackageReference Include="Testcontainers" Version="4.*" />
<!-- Database-specific modules -->
<PackageReference Include="Testcontainers.PostgreSql" Version="4.*" />
<PackageReference Include="Testcontainers.MsSql" Version="4.*" />
<PackageReference Include="Testcontainers.Redis" Version="4.*" />

```text

### PostgreSQL Example

```csharp

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }

[Collection("Postgres")]
public class OrderRepositoryTests
{
    private readonly PostgresFixture _postgres;

    public OrderRepositoryTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Insert_ValidOrder_CanBeRetrieved()
    {
        await using var context = CreateContext(_postgres.ConnectionString);
        await context.Database.EnsureCreatedAsync();

        var order = new Order { CustomerId = "cust-1", Total = 99.99m };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(99.99m, retrieved.Total);
    }

    private static AppDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AppDbContext(options);
    }
}

```text

### SQL Server Example

```csharp

public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

```text

### Combining WebApplicationFactory with Testcontainers

The most common pattern: use Testcontainers for the database and WebApplicationFactory for the API:

```csharp

public class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}

public class OrdersApiIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly ApiTestFactory _factory;

    public OrdersApiIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAndRetrieveOrder_RoundTrip()
    {
        // Ensure schema exists
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/orders",
            new { CustomerId = "cust-1", Items = new[] { new { Sku = "SKU-1", Quantity = 2 } } });
        createResponse.EnsureSuccessStatusCode();
        var location = createResponse.Headers.Location!.ToString();

        // Retrieve
        var getResponse = await _client.GetAsync(location);
        getResponse.EnsureSuccessStatusCode();
        var order = await getResponse.Content.ReadFromJsonAsync<OrderDto>();


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
