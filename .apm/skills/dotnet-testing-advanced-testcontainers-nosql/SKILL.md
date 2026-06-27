---
name: dotnet-testing-advanced-testcontainers-nosql
category: testing
subcategory: integration
description: |
  Complete guide for Testcontainers NoSQL integration testing. Use when containerized integration testing for MongoDB or Redis. Covers MongoDB document operations, Redis five data structures, Collection Fixture pattern. Includes BSON serialization, index performance testing, data isolation strategy, and container lifecycle management.
  Keywords: testcontainers mongodb, testcontainers redis, mongodb integration test, redis integration test, nosql testing, MongoDbContainer, RedisContainer, IMongoDatabase, IConnectionMultiplexer, BSON serialization, BsonDocument, document model testing, cache testing, Collection Fixture
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'testcontainers, mongodb, redis, nosql, integration-testing, bson'
  related_skills: 'advanced-testcontainers-database, advanced-aspire-testing, advanced-webapi-integration-testing'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-advanced-testcontainers-nosql'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Testcontainers NoSQL Integration Testing Guide

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Use Testcontainers to test MongoDB document operations
- Use Testcontainers to test Redis cache services
- Create MongoDB Collection Fixture for sharing containers
- Create Redis Collection Fixture for sharing containers
- Test MongoDB BSON serialization and complex document structures
- Test MongoDB index performance and uniqueness constraints
- Test Redis five data structures (String, Hash, List, Set, Sorted Set)
- Implement data isolation strategy for NoSQL databases

## Core Concepts

### NoSQL Testing Challenges

NoSQL database testing has significant differences from relational database testing:

1. **Document Model Complexity**: MongoDB supports nested objects, arrays, dictionaries, and other complex structures
2. **No Fixed Schema**: Need to validate data structure consistency through testing
3. **Diverse Data Structures**: Redis has five main data structures, each with different use cases
4. **Serialization Handling**: BSON (MongoDB) and JSON (Redis) serialization behavior needs validation

### Testcontainers Advantages

- **Real Environment Simulation**: Uses actual MongoDB 7.0 and Redis 7.2 containers
- **Consistent Testing**: Test results directly reflect production environment behavior
- **Isolation Guarantee**: Each test environment is completely independent
- **Performance Validation**: Can perform real index performance testing

## Environment Requirements

### Required Packages

````xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- MongoDB related packages -->
    <PackageReference Include="MongoDB.Driver" Version="3.0.0" />
    <PackageReference Include="MongoDB.Bson" Version="3.0.0" />

    <!-- Redis related packages -->
    <PackageReference Include="StackExchange.Redis" Version="2.8.16" />

    <!-- Testcontainers -->
    <PackageReference Include="Testcontainers" Version="4.0.0" />
    <PackageReference Include="Testcontainers.MongoDb" Version="4.0.0" />
    <PackageReference Include="Testcontainers.Redis" Version="4.0.0" />

    <!-- Test frameworks -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="AwesomeAssertions" Version="9.1.0" />

    <!-- JSON serialization and time testing -->
    <PackageReference Include="System.Text.Json" Version="9.0.0" />
    <PackageReference Include="Microsoft.Bcl.TimeProvider" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" Version="9.0.0" />
  </ItemGroup>
</Project>
```text

### Package Version Notes

| Package | Version | Purpose |
| ------- | ------- | ------- |
| MongoDB.Driver | 3.0.0 | MongoDB official driver, supports latest features |
| MongoDB.Bson | 3.0.0 | BSON serialization handling |
| StackExchange.Redis | 2.8.16 | Redis client, supports Redis 7.x |
| Testcontainers.MongoDb | 4.0.0 | MongoDB container management |
| Testcontainers.Redis | 4.0.0 | Redis container management |

---

## MongoDB Containerized Testing

Covers MongoDB Container Fixture creation, complex document model design (nested objects, arrays, dictionaries), BSON serialization testing, CRUD operation testing (including optimistic locking), and index performance and uniqueness constraint testing. Uses Collection Fixture pattern to share containers, saving 80%+ test time.

> 📖 Complete code examples please refer to [MongoDB Containerized Testing Detailed Guide](references/mongodb-testing.md)

---

## Redis Containerized Testing

Covers Redis Container Fixture creation, cache model design (CacheItem generic wrapper, UserSession, RecentView, LeaderboardEntry), and complete testing examples for Redis five data structures (String, Hash, List, Set, Sorted Set), including TTL expiration testing and data isolation strategy.

> 📖 Complete code examples please refer to [Redis Containerized Testing Detailed Guide](references/redis-testing.md)

---

## Best Practices

### 1. Collection Fixture Pattern

Use Collection Fixture to share containers, avoid restarting containers for each test:

```csharp
// Define collection
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbContainerFixture> { }

// Use collection
[Collection("MongoDb Collection")]
public class MyMongoTests
{
    public MyMongoTests(MongoDbContainerFixture fixture)
    {
        // Use shared container
    }
}
```text

### 2. Data Isolation Strategy

Ensure tests don't interfere with each other:

```csharp
// MongoDB: use unique Email/Username
var user = new UserDocument
{
    Username = $"testuser_{Guid.NewGuid():N}",
    Email = $"test_{Guid.NewGuid():N}@example.com"
};

// Redis: use unique Key prefix
var testId = Guid.NewGuid().ToString("N")[..8];
var key = $"test:{testId}:mykey";
```text

### 3. Cleanup Strategy

```csharp
// MongoDB: cleanup after tests
await fixture.ClearDatabaseAsync();

// Redis: use KeyDelete instead of FLUSHDB (avoid permission issues)
var keys = server.Keys(database.Database);
if (keys.Any())
{
    await database.KeyDeleteAsync(keys.ToArray());
}
```text

### 4. Performance Considerations

| Strategy | Description |
| -------- | ----------- |
| Collection Fixture | Container only starts once, saves 80%+ time |
| Data Isolation | Use unique Key/ID instead of clearing database |
| Batch Operations | Use InsertManyAsync, SetMultipleStringAsync |
| Index Creation | Create indexes during Fixture initialization |

---

## Common Issues

### Redis FLUSHDB Permission Issue

Some Redis container images don't enable admin mode by default:

```csharp
// ❌ Error: may fail
await server.FlushDatabaseAsync();

// ✅ Correct: use KeyDelete
var keys = server.Keys(database.Database);
if (keys.Any())
{
    await database.KeyDeleteAsync(keys.ToArray());
}
```text

### MongoDB Unique Index Duplicate Insert

```csharp
// Use unique Email during testing to avoid conflicts
var uniqueEmail = $"test_{Guid.NewGuid():N}@example.com";
```text

### Container Startup Timeout

```csharp
// Increase wait time
_container = new MongoDbBuilder()
    .WithImage("mongo:7.0")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilPortIsAvailable(27017))
    .Build();
```text

---

## Related Skills

- [testcontainers-database](../testcontainers-database/SKILL.md) - PostgreSQL/MSSQL containerized testing
- [aspnet-integration-testing](../aspnet-integration-testing/SKILL.md) - ASP.NET Core integration testing
- [nsubstitute-mocking](../../dotnet-testing/nsubstitute-mocking/SKILL.md) - Test doubles and Mock

---

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 22 - Testcontainers Integration Testing: MongoDB and Redis from Basic to Advanced**
  - Article: https://ithelp.ithome.com.tw/articles/10376740
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day22

### Official Documentation

- [Testcontainers Official Website](https://testcontainers.com/)
- [.NET Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [MongoDB.Driver Official Documentation](https://www.mongodb.com/docs/drivers/csharp/)
- [StackExchange.Redis Official Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [xUnit Collection Fixtures](https://xunit.net/docs/shared-context#collection-fixture)
````
