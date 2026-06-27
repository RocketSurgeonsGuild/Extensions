---
name: dotnet-testing-advanced-aspire-testing
category: testing
subcategory: integration
description: |
  Complete guide for .NET Aspire Testing integration testing framework. Use when testing .NET Aspire distributed applications, configuring AppHost tests, or migrating from Testcontainers to Aspire testing. Covers DistributedApplicationTestingBuilder, container lifecycle management, multi-service orchestration, Respawn configuration, and time testability design.
  Keywords: aspire testing, .NET Aspire, DistributedApplicationTestingBuilder, AppHost testing, distributed testing, AspireAppFixture, IAsyncLifetime, ContainerLifetime.Session, cloud-native testing, multi-service integration, Aspire.Hosting.Testing, Respawn
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'aspire, distributed-testing, cloud-native, testcontainers, integration-testing'
  related_skills: 'advanced-testcontainers-database, advanced-webapi-integration-testing, advanced-testcontainers-nosql'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-advanced-aspire-testing'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# .NET Aspire Testing Integration Testing Framework

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Create integration tests for .NET Aspire distributed applications
- Migrate from Testcontainers to .NET Aspire Testing
- Configure AppHost projects for testing
- Use DistributedApplicationTestingBuilder to establish testing environment
- Test interactions between multiple services (database, cache, API, etc.)
- Create integration testing architecture for cloud-native .NET applications

## Prerequisites

- .NET 8 SDK or higher
- Docker Desktop (WSL 2 or Hyper-V)
- AppHost project (.NET Aspire application orchestration)

## Core Concepts

### .NET Aspire Testing Positioning

**.NET Aspire Testing is a closed integration testing framework** designed specifically for distributed applications:

- Reproduce the same service architecture in tests as in production
- Use real containers instead of mock services
- Automatic container lifecycle management

### Necessity of AppHost Projects

Using .NET Aspire Testing requires creating an AppHost project:

- Define complete application architecture and container orchestration
- Tests reuse AppHost configuration to establish environment
- Without AppHost, cannot use Aspire Testing

### Differences from Testcontainers

| Feature               | .NET Aspire Testing                   | Testcontainers            |
| --------------------- | ------------------------------------- | ------------------------- |
| Design Goal           | Cloud-native distributed applications | General container testing |
| Configuration Method  | AppHost declarative definition        | Manual code configuration |
| Service Orchestration | Automatic handling                    | Manual management         |
| Learning Curve        | Higher                                | Medium                    |
| Applicable Scenarios  | Projects already using Aspire         | Traditional Web API       |

## Project Structure

````text
MyApp/
├── src/
│   ├── MyApp.Api/                    # Web API layer
│   ├── MyApp.Application/            # Application service layer
│   ├── MyApp.Domain/                 # Domain model
│   └── MyApp.Infrastructure/         # Infrastructure layer
├── MyApp.AppHost/                    # Aspire orchestration project ⭐
│   ├── MyApp.AppHost.csproj
│   └── Program.cs
└── tests/
    └── MyApp.Tests.Integration/      # Aspire Testing integration tests
        ├── MyApp.Tests.Integration.csproj
        ├── Infrastructure/
        │   ├── AspireAppFixture.cs
        │   ├── IntegrationTestCollection.cs
        │   ├── IntegrationTestBase.cs
        │   └── DatabaseManager.cs
        └── Controllers/
            └── MyControllerTests.cs
```text

## Required Packages

### AppHost Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Sdk Name="Aspire.AppHost.Sdk" Version="9.0.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <IsAspireHost>true</IsAspireHost>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.1.0" />
    <PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.1.0" />
    <PackageReference Include="Aspire.Hosting.Redis" Version="9.1.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\src\MyApp.Api\MyApp.Api.csproj" />
  </ItemGroup>
</Project>
```text

### Test Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.Testing" Version="9.1.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="Respawn" Version="6.2.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\MyApp.AppHost\MyApp.AppHost.csproj" />
  </ItemGroup>
</Project>
```text

## Container Lifecycle Management

Use `ContainerLifetime.Session` to ensure automatic cleanup of test resources:

```csharp
var postgres = builder.AddPostgres("postgres")
                     .WithLifetime(ContainerLifetime.Session);

var redis = builder.AddRedis("redis")
                  .WithLifetime(ContainerLifetime.Session);
```text

- **Session**: Automatically cleaned up after test session ends (recommended)
- **Persistent**: Containers continue running, require manual cleanup

## Waiting for Services Ready

Container startup and service readiness are two phases, requiring wait mechanism:

```csharp
private async Task WaitForPostgreSqlReadyAsync()
{
    const int maxRetries = 30;
    const int delayMs = 1000;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var connectionString = await GetConnectionStringAsync();
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            await Task.Delay(delayMs);
        }
    }
    throw new InvalidOperationException("PostgreSQL service failed to become ready");
}
```text

## Database Initialization

Aspire starts containers but does not automatically create databases:

```csharp
private async Task EnsureDatabaseExistsAsync(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var databaseName = builder.Database;
    builder.Database = "postgres"; // Connect to default database

    await using var connection = new NpgsqlConnection(builder.ToString());
    await connection.OpenAsync();

    var checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
    var dbExists = await new NpgsqlCommand(checkDbQuery, connection).ExecuteScalarAsync();

    if (dbExists == null)
    {
        await new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection)
            .ExecuteNonQueryAsync();
    }
}
```text

## Respawn Configuration

When using PostgreSQL, must specify adapter:

```csharp
_respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
{
    TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
    SchemasToInclude = new[] { "public" },
    DbAdapter = DbAdapter.Postgres  // Critical!
});
```text

## Collection Fixture Best Practices

Avoid restarting containers for each test class:

```csharp
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<AspireAppFixture>
{
}

[Collection("Integration Tests")]
public class MyControllerTests : IntegrationTestBase
{
    public MyControllerTests(AspireAppFixture fixture) : base(fixture) { }
}
```text

## Time Testability

Use `TimeProvider` to abstract time dependencies:

```csharp
// Service implementation
public class ProductService
{
    private readonly TimeProvider _timeProvider;

    public ProductService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public async Task<Product> CreateAsync(ProductCreateRequest request)
    {
        var now = _timeProvider.GetUtcNow();
        var product = new Product
        {
            CreatedAt = now,
            UpdatedAt = now
        };
        // ...
    }
}

// DI registration
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
```text

## Selection Recommendations

### Choose .NET Aspire Testing

- Project already uses .NET Aspire
- Need to test multi-service interactions
- Emphasize unified development and testing experience
- Cloud-native application architecture

### Choose Testcontainers

- Traditional .NET project
- Need fine-grained container control
- Integration with non-.NET services
- Team unfamiliar with Aspire

## Common Issues

### Endpoint Configuration Conflicts

Don't manually configure endpoints already automatically handled by Aspire:

```csharp
// ❌ Error: causes conflicts
builder.AddProject<Projects.MyApp_Api>("my-api")
       .WithHttpEndpoint(port: 8080, name: "http");

// ✅ Correct: let Aspire handle automatically
builder.AddProject<Projects.MyApp_Api>("my-api")
       .WithReference(postgresDb)
       .WithReference(redis);
```text

### Dapper Field Mapping

PostgreSQL snake_case to C# PascalCase mapping:

```csharp
// Initialize in Program.cs
DapperTypeMapping.Initialize();

// Or use SQL aliases
const string sql = @"
    SELECT id, name, price,
           created_at AS CreatedAt,
           updated_at AS UpdatedAt
    FROM products";
```text

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 24 - .NET Aspire Testing Introduction**
  - Article: https://ithelp.ithome.com.tw/articles/10377071
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day24

- **Day 25 - .NET Aspire Integration Testing: From Testcontainers to .NET Aspire Testing**
  - Article: https://ithelp.ithome.com.tw/articles/10377197
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day25

### Official Documentation

- [.NET Aspire Official Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Testing Documentation](https://learn.microsoft.com/dotnet/aspire/testing)

## Reference Resources (continued)

Please refer to example files in the same directory:

- `templates/apphost-program.cs` - AppHost orchestration definition
- `templates/aspire-app-fixture.cs` - Testing infrastructure
- `templates/integration-test-collection.cs` - Collection Fixture configuration
- `templates/integration-test-base.cs` - Test base class
- `templates/database-manager.cs` - Database manager
- `templates/controller-tests.cs` - Controller test examples
- `templates/test-project.csproj` - Test project configuration
- `templates/apphost-project.csproj` - AppHost project configuration
````
