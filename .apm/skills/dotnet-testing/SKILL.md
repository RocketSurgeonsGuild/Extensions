---
name: dotnet-testing
category: testing
subcategory: fundamentals
complexity: beginner
description: 'Complete .NET testing guide covering unit, integration, and advanced testing patterns with xUnit'
tags: ['testing', 'xunit', 'unit-test', 'integration-test', 'advanced-testing', 'beginner', 'intermediate', 'advanced']
invocable: true
related:
  - dotnet-assertions
  - dotnet-mocking
  - dotnet-test-data
  - dotnet-performance
---

# .NET Testing Fundamentals

Complete guide for .NET testing with xUnit, covering unit tests, integration tests, and advanced patterns.

## Quick Start

**Install xUnit:**
```bash
dotnet new xunit -n MyProject.Tests
cd MyProject.Tests
dotnet add reference ../MyProject/MyProject.csproj
dotnet test
```

**Basic test:**
```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.Equal(5, result);
    }
}
```

## Testing Principles (FIRST)

| Principle | Meaning | Application |
|-----------|---------|-------------|
| **F**ast | Tests run quickly | Isolate units, use in-memory databases |
| **I**ndependent | No test dependencies | Each test creates its own state |
| **R**epeatable | Same results every time | No shared state, no external dependencies |
| **S**elf-validating | Boolean pass/fail | Clear assertions, no manual verification |
| **T**imely | Write tests first | TDD, or at least with implementation |

## Test Structure (AAA Pattern)

```csharp
[Fact]
public void Withdraw_ValidAmount_UpdatesBalance()
{
    // Arrange - Set up the test
    var account = new BankAccount(100);
    
    // Act - Execute the code under test
    account.Withdraw(50);
    
    // Assert - Verify expectations
    Assert.Equal(50, account.Balance);
}
```

## Project Setup

### Test Project Structure

```
src/
├── MyApp/
│   └── MyApp.csproj
tests/
├── MyApp.UnitTests/
│   └── MyApp.UnitTests.csproj
├── MyApp.IntegrationTests/
│   └── MyApp.IntegrationTests.csproj
└── MyApp.Tests.sln
```

### Required Packages

```xml
<!-- Unit Tests -->
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />

<!-- Assertions (optional but recommended) -->
<PackageReference Include="AwesomeAssertions" Version="8.0.0" />

<!-- Mocking -->
<PackageReference Include="NSubstitute" Version="5.3.0" />

<!-- Test Data -->
<PackageReference Include="AutoFixture" Version="4.18.1" />
<PackageReference Include="Bogus" Version="35.6.1" />
```

### xUnit Configuration

**xunit.runner.json:**
```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4,
  "methodDisplay": "classAndMethod"
}
```

## Test Types

### Unit Tests (Fact)

```csharp
public class OrderTests
{
    [Fact]
    public void AddItem_IncreasesItemCount()
    {
        var order = new Order();
        
        order.AddItem(new OrderItem { Product = "Book", Quantity = 1 });
        
        Assert.Single(order.Items);
    }
}
```

### Parameterized Tests (Theory)

```csharp
[Theory]
[InlineData(1, 1, 2)]
[InlineData(2, 3, 5)]
[InlineData(-1, 1, 0)]
public void Add_VariousInputs_ReturnsExpected(int a, int b, int expected)
{
    var calculator = new Calculator();
    
    var result = calculator.Add(a, b);
    
    Assert.Equal(expected, result);
}
```

### Async Tests

```csharp
[Fact]
public async Task GetUserAsync_ExistingId_ReturnsUser()
{
    var service = new UserService();
    
    var user = await service.GetUserAsync(1);
    
    Assert.NotNull(user);
    Assert.Equal(1, user.Id);
}
```

## Integration Testing

### WebApplicationFactory

```csharp
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient _client;
    protected readonly WebApplicationFactory<Program> _factory;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real services with test doubles
                services.Replace(ServiceDescriptor.Singleton<IDatabaseService, TestDatabaseService>());
            });
        });
        
        _client = _factory.CreateClient();
    }
}

public class UsersApiTests : IntegrationTestBase
{
    public UsersApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/users");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Testcontainers (Database Integration)

```csharp
public class DatabaseIntegrationTest : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer;
    private ApplicationDbContext _dbContext;

    public DatabaseIntegrationTest()
    {
        _sqlServer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_sqlServer.GetConnectionString())
            .Options;
            
        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }

    [Fact]
    public async Task AddUser_SavesToDatabase()
    {
        var user = new User { Name = "Test" };
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var saved = await _dbContext.Users.FirstAsync();
        saved.Name.Should().Be("Test");
    }
}
```

## Advanced Patterns

### Test Data Builders

```csharp
public class OrderBuilder
{
    private readonly Order _order = new();

    public OrderBuilder WithCustomer(string customer)
    {
        _order.Customer = customer;
        return this;
    }

    public OrderBuilder WithItem(string product, int quantity)
    {
        _order.Items.Add(new OrderItem { Product = product, Quantity = quantity });
        return this;
    }

    public Order Build() => _order;
}

// Usage:
var order = new OrderBuilder()
    .WithCustomer("Acme Corp")
    .WithItem("Widget", 10)
    .Build();
```

### Fixtures (Shared Setup)

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; }

    public async Task InitializeAsync()
    {
        // Setup shared database
    }

    public async Task DisposeAsync()
    {
        // Cleanup
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database")]
public class TestsUsingDatabase
{
    public TestsUsingDatabase(DatabaseFixture fixture) { }
}
```

### Time Testing with TimeProvider

```csharp
public class TimeTests
{
    private readonly FakeTimeProvider _timeProvider = new();

    [Fact]
    public void Token_Expired_ReturnsTrue()
    {
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(1)
        );
        
        _timeProvider.SetUtcNow(DateTime.UtcNow.AddHours(2));
        
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, _timeProvider).Should().BeFalse();
    }
}
```

## Test Naming Conventions

**Three-part naming:**
```csharp
// Method_Scenario_Expected
public void Withdraw_InsufficientFunds_ThrowsException()
public void AddItem_NewProduct_IncreasesCount()
public void GetUser_NonExistentId_ReturnsNull()
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific project
dotnet test tests/MyApp.UnitTests

# Run with filter
dotnet test --filter "FullyQualifiedName~UserServiceTests"

# Run with verbosity
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "UserServiceTests.GetUserAsync_ReturnsUser"

# Run in parallel with N workers
dotnet test --parallel -- N
```

## Assertions

**Basic xUnit:**
```csharp
Assert.Equal(expected, actual);
Assert.True(condition);
Assert.Throws<Exception>(() => action());
Assert.Contains(expectedItem, collection);
```

**AwesomeAssertions (Recommended):**
```csharp
actual.Should().Be(expected);
actual.Should().BeTrue();
actual.Should().Contain(expected);
action.Should().Throw<Exception>();
actual.Should().BeEquivalentTo(expected);
```

## Common Pitfalls

**DON'T:**
- Share state between tests
- Test multiple things in one test
- Depend on test execution order
- Use Thread.Sleep for async
- Test private methods directly

**DO:**
- Keep tests isolated
- One concept per test
- Use descriptive names
- Use async/await properly
- Test behavior, not implementation

## Related Skills

- **dotnet-assertions** - Assertion patterns with AwesomeAssertions
- **dotnet-mocking** - NSubstitute for test doubles
- **dotnet-test-data** - AutoFixture and Bogus for data generation
- **dotnet-performance** - BenchmarkDotNet for performance testing
