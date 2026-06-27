---
name: dotnet-testing-advanced-testcontainers-database
category: testing
subcategory: integration
description: |
  Specialized skill for containerized database testing using Testcontainers. Use when testing real database behavior, using SQL Server/PostgreSQL/MySQL containers, testing EF Core/Dapper. Covers container startup, database migrations, test isolation, container sharing.
  Keywords: testcontainers, container testing, database testing, MsSqlContainer, PostgreSqlContainer, MySqlContainer, EF Core testing, Dapper testing, Testcontainers.MsSql, Testcontainers.PostgreSql, GetConnectionString, IAsyncLifetime, CollectionFixture
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, Testcontainers, database, SQL Server, PostgreSQL'
  related_skills: 'advanced-testcontainers-nosql, advanced-aspnet-integration-testing, advanced-aspire-testing'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-advanced-testcontainers-database'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Testcontainers Database Integration Testing Guide

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Need to test real database behavior (transactions, concurrency, stored procedures, etc.)
- EF Core InMemory database cannot meet testing requirements
- Create containerized testing environment for PostgreSQL or MSSQL
- Use Collection Fixture pattern to share container instances
- Test data access layer for both EF Core and Dapper
- Need SQL script externalization strategy

## EF Core InMemory Limitations

Before choosing a testing strategy, must understand the significant limitations of EF Core InMemory database:

### 1. Transaction Behavior and Database Locking

- **Does not support database transactions**: `SaveChanges()` immediately saves data, cannot Rollback
- **No database locking mechanism**: Cannot simulate behavior under concurrency scenarios

### 2. LINQ Query Differences

- **Query translation differences**: Some LINQ queries (complex GroupBy, JOIN, custom functions) work in InMemory but
  may fail when translated to SQL
- **Case Sensitivity**: InMemory defaults to case-insensitive, but real databases depend on collation rules
- **Performance simulation insufficient**: Cannot simulate real database performance bottlenecks or index issues

### 3. Database-Specific Features

InMemory mode cannot test:

- Stored Procedures and Triggers
- Views
- Foreign Key Constraints, Check Constraints
- Data type precision (decimal, datetime, etc.)
- Concurrency Tokens (RowVersion, Timestamp)

**Conclusion**: When needing to validate complex transaction logic, concurrency handling, or database-specific behavior,
should use Testcontainers for integration testing.

## Testcontainers Core Concepts

### What is Testcontainers?

Testcontainers is a testing library providing lightweight, easy-to-use API to start Docker containers, specifically for
integration testing.

### Core Advantages

1. **Real Environment Testing**: Use real databases, test actual SQL syntax and database constraints
2. **Environment Consistency**: Ensure test environment uses same service versions as production
3. **Clean Test Environment**: Each test has independent clean environment, containers auto-cleanup
4. **Simplified Development Environment**: Developers only need Docker, no need to install various services

## Required Packages

````xml
<ItemGroup>
  <!-- Test frameworks -->
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="xunit" Version="2.9.3" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.9.3" />
  <PackageReference Include="AwesomeAssertions" Version="9.1.0" />

  <!-- Testcontainers core packages -->
  <PackageReference Include="Testcontainers" Version="3.10.0" />

  <!-- Database containers -->
  <PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
  <PackageReference Include="Testcontainers.MsSql" Version="3.10.0" />

  <!-- Entity Framework Core -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />

  <!-- Dapper (optional) -->
  <PackageReference Include="Dapper" Version="2.1.35" />
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
</ItemGroup>
```text

> **Important**: Use `Microsoft.Data.SqlClient` instead of the old `System.Data.SqlClient` for better performance and security.

## Environment Requirements

### Docker Desktop Configuration

- Windows 10 version 2004 or later
- WSL 2 feature enabled
- 8GB RAM (16GB+ recommended)
- 64GB available disk space

### Recommended Docker Desktop Resources Settings

- Memory: 6GB (50-75% of system memory)
- CPUs: 4 cores
- Swap: 2GB
- Disk image size: 64GB

## Basic Container Operation Patterns

### PostgreSQL Container

```csharp
public class PostgreSqlTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private UserDbContext _dbContext = null!;

    public PostgreSqlTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(5432, true)  // Auto-assign host port
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new UserDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
```text

### SQL Server Container

```csharp
public class SqlServerTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    private UserDbContext _dbContext = null!;

    public SqlServerTests()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _dbContext = new UserDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}
```text

## Collection Fixture Pattern: Container Sharing

### Why Container Sharing?

In large projects, creating new containers for each test class encounters serious performance bottlenecks:

- **Traditional approach**: Each test class starts a container. If there are 3 test classes, total time is approximately `3 × 10 seconds = 30 seconds`
- **Collection Fixture**: All test classes share the same container. Total time is only approximately `1 × 10 seconds = 10 seconds`

### Test execution time reduced by approximately 67%

### Collection Fixture Implementation

```csharp
/// <summary>
/// MSSQL container Collection Fixture
/// </summary>
public class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public SqlServerContainerFixture()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test123456!")
            .WithCleanUp(true)
            .Build();
    }

    public static string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // Wait for container to fully start
        await Task.Delay(2000);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// Define test collection
/// </summary>
[CollectionDefinition(nameof(SqlServerCollectionFixture))]
public class SqlServerCollectionFixture : ICollectionFixture<SqlServerContainerFixture>
{
    // This class is just for defining Collection, no implementation needed
}
```text

### Test Class Integration

```csharp
[Collection(nameof(SqlServerCollectionFixture))]
public class EfCoreTests : IDisposable
{
    private readonly ECommerceDbContext _dbContext;

    public EfCoreTests(ITestOutputHelper testOutputHelper)
    {
        var connectionString = SqlServerContainerFixture.ConnectionString;

        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(connectionString)
            .EnableSensitiveDataLogging()
            .LogTo(testOutputHelper.WriteLine, LogLevel.Information)
            .Options;

        _dbContext = new ECommerceDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // Clean up data in foreign key constraint order
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM OrderItems");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Products");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Categories");
        _dbContext.Dispose();
    }
}
```text

## SQL Script Externalization Strategy

### Why Externalize SQL Scripts?

- **Separation of Concerns**: C# code focuses on test logic, SQL scripts focus on database structure
- **Maintainability**: When modifying database structure, only edit `.sql` files
- **Readability**: C# code becomes cleaner
- **Tool Support**: SQL files get editor syntax highlighting and formatting support
- **Version Control Friendly**: SQL changes can be clearly tracked in version control

### Folder Structure

```text
tests/DatabaseTesting.Tests/
├── SqlScripts/
│   ├── Tables/
│   │   ├── CreateCategoriesTable.sql
│   │   ├── CreateProductsTable.sql
│   │   ├── CreateOrdersTable.sql
│   │   └── CreateOrderItemsTable.sql
│   └── StoredProcedures/
│       └── GetProductSalesReport.sql
```text

### .csproj Configuration

```xml
<ItemGroup>
  <Content Include="SqlScripts\**\*.sql">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```text

### Script Loading Implementation

```csharp
private void EnsureTablesExist()
{
    var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
    if (!Directory.Exists(scriptDirectory)) return;

    // Execute table creation scripts in dependency order
    var orderedScripts = new[]
    {
        "Tables/CreateCategoriesTable.sql",
        "Tables/CreateProductsTable.sql",
        "Tables/CreateOrdersTable.sql",
        "Tables/CreateOrderItemsTable.sql"
    };

    foreach (var scriptPath in orderedScripts)
    {
        var fullPath = Path.Combine(scriptDirectory, scriptPath);
        if (File.Exists(fullPath))
        {
            var script = File.ReadAllText(fullPath);
            _dbContext.Database.ExecuteSqlRaw(script);
        }
    }
}
```text

## Wait Strategy Best Practices

### Built-in Wait Strategy

```csharp
// Wait for specific port to be available
var postgres = new PostgreSqlBuilder()
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilPortIsAvailable(5432))
    .Build();

// Wait for log message to appear
var sqlServer = new MsSqlBuilder()
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilPortIsAvailable(1433)
        .UntilMessageIsLogged("SQL Server is now ready for client connections"))
    .Build();
```text

## EF Core Advanced Feature Testing

Covers Include/ThenInclude multi-level relationship queries, AsSplitQuery to avoid Cartesian product, N+1 query problem validation, AsNoTracking read-only query optimization, and other complete testing examples.

> 📖 Complete code examples please refer to [references/orm-advanced-testing.md](references/orm-advanced-testing.md#ef-core-advanced-feature-testing)

## Dapper Advanced Feature Testing

Covers basic CRUD test class setup, QueryMultiple one-to-many relationship handling, DynamicParameters dynamic query construction, and other complete testing examples.

> 📖 Complete code examples please refer to [references/orm-advanced-testing.md](references/orm-advanced-testing.md#dapper-advanced-feature-testing)

## Repository Pattern Design Principles

### Interface Segregation Principle (ISP) Application

```csharp
/// <summary>
/// Basic CRUD operation interface
/// </summary>
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

/// <summary>
/// EF Core specific advanced features interface
/// </summary>
public interface IProductByEFCoreRepository
{
    Task<Product?> GetProductWithCategoryAndTagsAsync(int productId);
    Task<IEnumerable<Product>> GetProductsByCategoryWithSplitQueryAsync(int categoryId);
    Task<int> BatchUpdateProductPricesAsync(int categoryId, decimal priceMultiplier);
    Task<IEnumerable<Product>> GetProductsWithNoTrackingAsync(decimal minPrice);
}

/// <summary>
/// Dapper specific advanced features interface
/// </summary>
public interface IProductByDapperRepository
{
    Task<Product?> GetProductWithTagsAsync(int productId);
    Task<IEnumerable<Product>> SearchProductsAsync(int? categoryId, decimal? minPrice, bool? isActive);
    Task<IEnumerable<ProductSalesReport>> GetProductSalesReportAsync(decimal minPrice);
}
```text

### Design Advantages

1. **Single Responsibility Principle (SRP)**: Each interface focuses on specific responsibilities
2. **Interface Segregation Principle (ISP)**: Consumers only depend on interfaces they need
3. **Dependency Inversion Principle (DIP)**: High-level modules depend on abstractions not concrete implementations
4. **Test Isolation**: Can perform precise testing for specific features

## Common Issues Handling

### Docker Container Startup Failure

```bash
# Check if port is occupied
netstat -an | findstr :5432

# Clean up unused images
docker system prune -a
```text

### Out of Memory Issues

- Adjust Docker Desktop memory configuration
- Limit number of simultaneously running containers
- Use Collection Fixture to share containers

### Test Data Isolation

```csharp
public void Dispose()
{
    // Clean up data in foreign key constraint order
    _dbContext.Database.ExecuteSqlRaw("DELETE FROM OrderItems");
    _dbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
    _dbContext.Database.ExecuteSqlRaw("DELETE FROM Products");
    _dbContext.Database.ExecuteSqlRaw("DELETE FROM Categories");
    _dbContext.Dispose();
}
```text

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 20 - Testcontainers Introduction: Using Docker to Set Up Testing Environment**
  - Article: https://ithelp.ithome.com.tw/articles/10376401
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day20

- **Day 21 - Testcontainers Integration Testing: MSSQL + EF Core and Dapper Basic Applications**
  - Article: https://ithelp.ithome.com.tw/articles/10376524
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day21

### Official Documentation

- [Testcontainers Official Website](https://testcontainers.com/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [Testcontainers for .NET / Modules](https://dotnet.testcontainers.org/modules/)
````
