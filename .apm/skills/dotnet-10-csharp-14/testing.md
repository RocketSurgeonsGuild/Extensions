# Testing in .NET 10

WebApplicationFactory, integration tests, and authentication testing.

## WebApplicationFactory

### Basic Setup

````csharp
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```text

### Integration Test Class

```csharp
public class UsersEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public UsersEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        // Arrange
        var expected = new UserDto(1, "John", "john@example.com");
        await SeedDatabaseAsync(expected);

        // Act
        var response = await _client.GetAsync("/api/users/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().BeEquivalentTo(expected);
    }

    private async Task SeedDatabaseAsync(UserDto user)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(new User(user.Id, user.Name, user.Email));
        await context.SaveChangesAsync();
    }
}
```text

### With Custom Configuration

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Mock external services
            services.AddSingleton<IEmailService, MockEmailService>();

            // Replace configuration
            services.Configure<DatabaseOptions>(options =>
            {
                options.ConnectionString = "DataSource=:memory:";
            });
        });
    }
}
```text

## Authentication Testing

### Test Auth Handler

```csharp
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("permissions", "user:read")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```text

### Configure Test Authentication

```csharp
public class AuthenticatedWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace auth with test auth
            services.AddAuthentication(defaultScheme: "Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });
        });
    }
}
```text

### Test with Auth

```csharp
public class ProtectedEndpointTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProtectedEndpointTests(AuthenticatedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsSuccess_WhenAuthenticated()
    {
        var response = await _client.GetAsync("/api/protected");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_ReturnsForbidden_WhenNotAdmin()
    {
        var response = await _client.GetAsync("/api/admin");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```text

## Custom Claims Tests

```csharp
public class RoleBasedTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RoleBasedTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("User", "/api/users", HttpStatusCode.OK)]
    [InlineData("User", "/api/admin", HttpStatusCode.Forbidden)]
    [InlineData("Admin", "/api/admin", HttpStatusCode.OK)]
    public async Task RoleBasedAccess_ReturnsExpectedStatus(
        string role, string endpoint, HttpStatusCode expected)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options =>
                        {
                            options.Claims = new[] { new Claim(ClaimTypes.Role, role) };
                        });
            });
        }).CreateClient();

        var response = await client.GetAsync(endpoint);
        response.StatusCode.Should().Be(expected);
    }
}
```text

## Database Testing

### In-Memory Database

```csharp
[Fact]
public async Task CreateUser_AddsUserToDatabase()
{
    // Arrange
    await using var context = new ApplicationDbContext(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    var service = new UserService(context);

    // Act
    await service.CreateAsync(new CreateUserRequest("John", "john@example.com"));

    // Assert
    var user = await context.Users.FirstOrDefaultAsync();
    user.Should().NotBeNull();
    user.Name.Should().Be("John");
}
```text

### Test Containers (Real Database)

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    public string ConnectionString => _container.GetConnectionString();

    public DatabaseFixture()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_SavesToRealDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();

        var repository = new UserRepository(context);
        var user = new User(1, "John", "john@example.com");

        await repository.CreateAsync(user);

        var saved = await repository.GetByIdAsync(1);
        saved.Should().NotBeNull();
    }
}
```text

## HttpClient Testing

### Mock HttpClient

```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _handler(request, cancellationToken);
    }
}

[Fact]
public async Task ExternalApiClient_ReturnsData()
{
    // Arrange
    var handler = new MockHttpMessageHandler((request, ct) =>
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new { id = 1, name = "Test" })
        });
    });

    var client = new HttpClient(handler);
    var apiClient = new ExternalApiClient(client);

    // Act
    var result = await apiClient.GetDataAsync();

    // Assert
    result.Name.Should().Be("Test");
}
```text

## Minimal API Testing

```csharp
public class EndpointTests
{
    [Fact]
    public async Task GetUser_ReturnsCorrectResponse()
    {
        // Arrange
        var userService = Substitute.For<IUserService>();
        userService.GetAsync(1).Returns(new User(1, "John", "john@example.com"));

        // Build minimal app
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(userService);
        var app = builder.Build();

        app.MapGet("/users/{id:int}", async (int id, IUserService svc) =>
        {
            var user = await svc.GetAsync(id);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        // Test
        await app.StartAsync();
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        var response = await client.GetAsync("/users/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```text

## Test Organization

### Project Structure

```text
tests/
├── IntegrationTests/
│   ├── ApiWebApplicationFactory.cs
│   ├── UsersEndpointTests.cs
│   └── OrdersEndpointTests.cs
├── UnitTests/
│   ├── Services/
│   │   └── UserServiceTests.cs
│   └── Repositories/
│       └── UserRepositoryTests.cs
└── Shared/
    ├── TestAuthHandler.cs
    └── MockData.cs
```text

### xUnit Conventions

```csharp
public class UserServiceTests
{
    private readonly IUserService _service;
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserServiceTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UserService>>();
        _service = new UserService(_repository, _logger);
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        // Arrange
        var expected = new User(1, "John", "john@example.com");
        _repository.GetByIdAsync(1).Returns(expected);

        // Act
        var result = await _service.GetAsync(1);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetUser_Throws_WhenIdInvalid(int id)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAsync(id));
    }

    [Fact]
    public async Task CreateUser_CallsRepository_WithCorrectData()
    {
        // Arrange
        var request = new CreateUserRequest("John", "john@example.com");

        // Act
        await _service.CreateAsync(request);

        // Assert
        await _repository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Name == "John" && u.Email == "john@example.com"));
    }
}
```text

## Best Practices

### ✅ DO

- Use WebApplicationFactory for integration tests
- Use in-memory database for unit tests
- Test with real auth handler (not disabled)
- Mock external dependencies
- Clean up test data
- Use Theory for parameterized tests

### ❌ DON'T

- Test implementation details
- Share database between tests
- Mock things you own
- Test multiple things in one test
- Skip test isolation
````
