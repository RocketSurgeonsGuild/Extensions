---
name: dotnet-testing-advanced-webapi-integration-testing
category: testing
subcategory: integration
description: |
  Complete guide for ASP.NET Core Web API integration testing. Use when performing integration testing on Web API endpoints or validating ProblemDetails error format. Covers WebApplicationFactory, IExceptionHandler, Testcontainers multi-container orchestration, Flurl URL construction, and AwesomeAssertions HTTP validation.
  Keywords: webapi integration testing, WebApplicationFactory, asp.net core integration test, webapi integration test, IExceptionHandler, ProblemDetails, ValidationProblemDetails, AwesomeAssertions, Flurl, Respawn, Be201Created, Be400BadRequest, multi-container testing, Collection Fixture, global exception handling
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'webapi, integration-testing, testcontainers, aspnetcore, clean-architecture'
  related_skills: 'advanced-aspnet-integration-testing, advanced-testcontainers-database, advanced-aspire-testing'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-advanced-webapi-integration-testing'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Web API Integration Testing

## Applicable Scenarios

**Skill Level**: Advanced  
**Prerequisites**: xUnit basics, ASP.NET Core basics, Testcontainers basics, Clean Architecture  
**Estimated Learning Time**: 60-90 minutes

## Learning Objectives

After completing this skill, you will be able to:

1. Establish complete Web API integration testing architecture
2. Implement modern exception handling using `IExceptionHandler`
3. Validate standard `ProblemDetails` and `ValidationProblemDetails` format
4. Use Flurl to simplify URL construction for HTTP testing
5. Use AwesomeAssertions for precise HTTP response validation
6. Establish multi-container (PostgreSQL + Redis) testing environment

## Core Concepts

### IExceptionHandler - Modern Exception Handling

The `IExceptionHandler` interface introduced in ASP.NET Core 8+ provides a more elegant error handling approach than
traditional middleware:

````csharp
/// <summary>
/// Global exception handler
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Resource Not Found",
                Status = 404,
                Detail = exception.Message
            },
            ArgumentException => new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Invalid Parameters",
                Status = 400,
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred"
            }
        };
    }
}
```text

### ProblemDetails Standard Format

RFC 7807 defined unified error response format:

| Field | Description |
| ----- | ----------- |
| `type` | URI for problem type |
| `title` | Short error description |
| `status` | HTTP status code |
| `detail` | Detailed error explanation |
| `instance` | URI of problem occurrence |

### ValidationProblemDetails - Validation Error Specific

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Input data contains validation errors",
  "errors": {
    "Name": ["Product name cannot be empty"],
    "Price": ["Product price must be greater than 0"]
  }
}
```text

### FluentValidation Exception Handler

FluentValidation exception handler implements the `IExceptionHandler` interface, specifically handling `ValidationException` and converting validation errors to standard `ValidationProblemDetails` format response. Handlers execute in registration order, and specific handlers (like FluentValidation) must be registered before global handlers.

> 📖 Complete implementation code please refer to [references/exception-handler-details.md](references/exception-handler-details.md)

## Integration Testing Infrastructure

### TestWebApplicationFactory

```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private FakeTimeProvider? _timeProvider;

    public PostgreSqlContainer PostgresContainer => _postgresContainer
        ?? throw new InvalidOperationException("PostgreSQL container has not been initialized");

    public RedisContainer RedisContainer => _redisContainer
        ?? throw new InvalidOperationException("Redis container has not been initialized");

    public FakeTimeProvider TimeProvider => _timeProvider
        ?? throw new InvalidOperationException("TimeProvider has not been initialized");

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = PostgresContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = RedisContainer.GetConnectionString(),
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace TimeProvider
            services.Remove(services.Single(d => d.ServiceType == typeof(TimeProvider)));
            services.AddSingleton<TimeProvider>(TimeProvider);
        });

        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        if (_postgresContainer != null) await _postgresContainer.DisposeAsync();
        if (_redisContainer != null) await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
```text

### Collection Fixture Pattern

```csharp
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}
```text

### Test Base Class

```csharp
[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;
    protected readonly DatabaseManager DatabaseManager;
    protected readonly IFlurlClient FlurlClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        DatabaseManager = new DatabaseManager(factory.PostgresContainer.GetConnectionString());
        FlurlClient = new FlurlClient(HttpClient);
    }

    public virtual async Task InitializeAsync()
    {
        await DatabaseManager.InitializeDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
        FlurlClient.Dispose();
    }

    protected void ResetTime()
    {
        Factory.TimeProvider.SetUtcNow(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    protected void AdvanceTime(TimeSpan timeSpan)
    {
        Factory.TimeProvider.Advance(timeSpan);
    }
}
```text

## Flurl Simplifies URL Construction

Flurl provides fluent API for building complex URLs:

```csharp
// Traditional approach
var url = $"/products?pageSize={pageSize}&page={page}&keyword={keyword}";

// Using Flurl
var url = "/products"
    .SetQueryParam("pageSize", 5)
    .SetQueryParam("page", 2)
    .SetQueryParam("keyword", "special");
```text

## Testing Examples

### Successful Product Creation Test

```csharp
[Fact]
public async Task CreateProduct_WithValidData_ShouldCreateProductSuccessfully()
{
    // Arrange
    var request = new ProductCreateRequest { Name = "New Product", Price = 299.99m };

    // Act
    var response = await HttpClient.PostAsJsonAsync("/products", request);

    // Assert
    response.Should().Be201Created()
        .And.Satisfy<ProductResponse>(product =>
        {
            product.Id.Should().NotBeEmpty();
            product.Name.Should().Be("New Product");
            product.Price.Should().Be(299.99m);
        });
}
```text

### Validation Error Test

```csharp
[Fact]
public async Task CreateProduct_WhenProductNameIsEmpty_ShouldReturn400BadRequest()
{
    // Arrange
    var invalidRequest = new ProductCreateRequest { Name = "", Price = 100.00m };

    // Act
    var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

    // Assert
    response.Should().Be400BadRequest()
        .And.Satisfy<ValidationProblemDetails>(problem =>
        {
            problem.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
            problem.Title.Should().Be("One or more validation errors occurred.");
            problem.Errors.Should().ContainKey("Name");
            problem.Errors["Name"].Should().Contain("Product name cannot be empty");
        });
}
```text

### Resource Not Found Test

```csharp
[Fact]
public async Task GetById_WhenProductDoesNotExist_ShouldReturn404WithProblemDetails()
{
    // Arrange
    var nonExistentId = Guid.NewGuid();

    // Act
    var response = await HttpClient.GetAsync($"/Products/{nonExistentId}");

    // Assert
    response.Should().Be404NotFound()
        .And.Satisfy<ProblemDetails>(problem =>
        {
            problem.Type.Should().Be("https://httpstatuses.com/404");
            problem.Title.Should().Be("Product does not exist");
            problem.Status.Should().Be(404);
        });
}
```text

### Pagination Query Test

```csharp
[Fact]
public async Task GetProducts_WithPaginationParameters_ShouldReturnCorrectPagedResult()
{
    // Arrange
    await TestHelpers.SeedProductsAsync(DatabaseManager, 15);

    // Act - Use Flurl to construct QueryString
    var url = "/products"
        .SetQueryParam("pageSize", 5)
        .SetQueryParam("page", 2);

    var response = await HttpClient.GetAsync(url);

    // Assert
    response.Should().Be200Ok()
        .And.Satisfy<PagedResult<ProductResponse>>(result =>
        {
            result.Total.Should().Be(15);
            result.PageSize.Should().Be(5);
            result.Page.Should().Be(2);
            result.Items.Should().HaveCount(5);
        });
}
```text

## Data Management Strategy

### TestHelpers Design

```csharp
public static class TestHelpers
{
    public static ProductCreateRequest CreateProductRequest(
        string name = "Test Product",
        decimal price = 100.00m)
    {
        return new ProductCreateRequest { Name = name, Price = price };
    }

    public static async Task SeedProductsAsync(DatabaseManager dbManager, int count)
    {
        var tasks = Enumerable.Range(1, count)
            .Select(i => SeedSpecificProductAsync(dbManager, $"Product {i:D2}", i * 10.0m));
        await Task.WhenAll(tasks);
    }
}
```text

### SQL Script Externalization

```text
tests/Integration/
└── SqlScripts/
    └── Tables/
        └── CreateProductsTable.sql
```text

## Best Practices

### 1. Test Structure Design

- **Single Responsibility**: Each test focuses on one specific scenario
- **3A Pattern**: Clear separation of Arrange, Act, Assert
- **Clear Naming**: Method name expresses test intent

### 2. Error Handling Validation

- **ValidationProblemDetails**: Validate error response format
- **ProblemDetails**: Validate business exception response
- **HTTP Status Code**: Confirm correct status code

### 3. Performance Considerations

- **Container Sharing**: Use Collection Fixture
- **Data Cleanup**: Clean data after tests, don't recreate containers
- **Parallel Execution**: Ensure test independence

## Dependency Packages

```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="AwesomeAssertions" Version="9.1.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="4.0.0" />
<PackageReference Include="Testcontainers.Redis" Version="4.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="Flurl" Version="4.0.0" />
<PackageReference Include="Respawn" Version="6.2.1" />
```text

## Project Structure

```text
src/
├── Api/                          # Web API layer
├── Application/                  # Application service layer
├── Domain/                       # Domain model
└── Infrastructure/               # Infrastructure layer
tests/
└── Integration/
    ├── Fixtures/
    │   ├── TestWebApplicationFactory.cs
    │   ├── IntegrationTestCollection.cs
    │   └── IntegrationTestBase.cs
    ├── Handlers/
    │   ├── GlobalExceptionHandler.cs
    │   └── FluentValidationExceptionHandler.cs
    ├── Helpers/
    │   ├── DatabaseManager.cs
    │   └── TestHelpers.cs
    ├── SqlScripts/
    │   └── Tables/
    └── Controllers/
        └── ProductsControllerTests.cs
```text

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 23 - Integration Testing in Practice: Web API Service Integration Testing**
  - Article: https://ithelp.ithome.com.tw/articles/10376873
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day23

### Official Documentation

- [ASP.NET Core Integration Testing](https://docs.microsoft.com/aspnet/core/test/integration-tests)
- [IExceptionHandler Documentation](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
- [ProblemDetails RFC 7807](https://tools.ietf.org/html/rfc7807)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [AwesomeAssertions](https://awesomeassertions.org/)
- [Flurl HTTP Client](https://flurl.dev/)
- [Respawn](https://github.com/jbogard/Respawn)
````
