---
name: dotnet-xunit
description: Authors xUnit v3 tests -- Facts, Theories, fixtures, parallelism, IAsyncLifetime.
license: MIT
targets: ['*']
category: testing
subcategory: frameworks
tags:
  - testing
  - dotnet
  - skill
  - xunit
  - frameworks
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-testing-unit-test-fundamentals
  - dotnet-testing-advanced-xunit-upgrade-guide
  - dotnet-testing-xunit-project-setup
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
codexcli:
  short-description: '.NET skill guidance for xunit tasks'
geminicli: {}
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
antigravity: {}
---

# dotnet-xunit

xUnit v3 testing framework features for .NET. Covers `[Fact]` and `[Theory]` attributes, test fixtures (`IClassFixture`,
`ICollectionFixture`), parallel execution configuration, `IAsyncLifetime` for async setup/teardown, custom assertions,
and xUnit analyzers. Includes v2 compatibility notes where behavior differs.

**Version assumptions:** xUnit v3 primary (.NET 8.0+ baseline). Where v3 behavior differs from v2, compatibility notes
are provided inline. xUnit v2 remains widely used; many projects will encounter both versions during migration.

## Scope

- [Fact] and [Theory] test attributes and data sources
- Test fixtures (IClassFixture, ICollectionFixture) and shared context
- Parallel execution configuration and collection ordering
- IAsyncLifetime for async setup/teardown
- xUnit analyzers and custom assertions
- xUnit v3 migration from v2 (TheoryDataRow, ValueTask lifecycle)

## Out of scope

- Test project scaffolding -- see [skill:dotnet-add-testing]
- Testing strategy and test type decisions -- see [skill:dotnet-testing-strategy]
- Integration testing patterns (WebApplicationFactory, Testcontainers) -- see [skill:dotnet-integration-testing]
- Snapshot testing with Verify -- see [skill:dotnet-snapshot-testing]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with xUnit packages referenced. Run
[skill:dotnet-version-detection] to confirm .NET 8.0+ baseline for xUnit v3 support.

Cross-references: [skill:dotnet-testing-strategy] for deciding what to test and how, [skill:dotnet-integration-testing]
for combining xUnit with WebApplicationFactory and Testcontainers.

---

## xUnit v3 vs v2: Key Changes

| Feature                 | xUnit v2                                      | xUnit v3                                                                                |
| ----------------------- | --------------------------------------------- | --------------------------------------------------------------------------------------- |
| **Package**             | `xunit` (2.x)                                 | `xunit.v3`                                                                              |
| **Runner**              | `xunit.runner.visualstudio`                   | `xunit.runner.visualstudio` (3.x)                                                       |
| **Async lifecycle**     | `IAsyncLifetime`                              | `IAsyncLifetime` (now returns `ValueTask`)                                              |
| **Assert package**      | Bundled                                       | Separate `xunit.v3.assert` (or `xunit.v3.assert.source` for extensibility)              |
| **Parallelism default** | Per-collection                                | Per-collection (same, but configurable per-assembly)                                    |
| **Timeout**             | `Timeout` property on `[Fact]` and `[Theory]` | `Timeout` property on `[Fact]` and `[Theory]` (unchanged)                               |
| **Test output**         | `ITestOutputHelper`                           | `ITestOutputHelper` (unchanged)                                                         |
| **`[ClassData]`**       | Returns `IEnumerable<object[]>`               | Returns `IEnumerable<TheoryDataRow<T>>` (strongly typed)                                |
| **`[MemberData]`**      | Returns `IEnumerable<object[]>`               | Supports `TheoryData<T>` and `TheoryDataRow<T>`                                         |
| **Assertion messages**  | Optional string parameter on Assert methods   | Removed in favor of custom assertions (v3.0); use `Assert.Fail()` for explicit messages |

**v2 compatibility note:** If migrating from v2, replace `xunit` package with `xunit.v3`. Most `[Fact]` and `[Theory]`
tests work without changes. The primary migration effort is in `IAsyncLifetime` (return type changes to `ValueTask`),
`[ClassData]` (strongly typed row format), and removed assertion message parameters.

---

## Facts and Theories

### `[Fact]` -- Single Test Case

Use `[Fact]` for tests with no parameters:

````csharp

public class DiscountCalculatorTests
{
    [Fact]
    public void Apply_NegativePercentage_ThrowsArgumentOutOfRangeException()
    {
        var calculator = new DiscountCalculator();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Apply(100m, percentage: -5));

        Assert.Equal("percentage", ex.ParamName);
    }

    [Fact]
    public async Task ApplyAsync_ValidDiscount_ReturnsDiscountedPrice()
    {
        var calculator = new DiscountCalculator();

        var result = await calculator.ApplyAsync(100m, percentage: 15);

        Assert.Equal(85m, result);
    }
}

```text

### `[Theory]` -- Parameterized Tests

Use `[Theory]` to run the same test logic with different inputs.

#### `[InlineData]`

Best for simple value types:

```csharp

[Theory]
[InlineData(100, 10, 90)]      // 10% off 100 = 90
[InlineData(200, 25, 150)]     // 25% off 200 = 150
[InlineData(50, 0, 50)]        // 0% off = no change
[InlineData(100, 100, 0)]      // 100% off = 0
public void Apply_VariousInputs_ReturnsExpectedPrice(
    decimal price, decimal percentage, decimal expected)
{
    var calculator = new DiscountCalculator();

    var result = calculator.Apply(price, percentage);

    Assert.Equal(expected, result);
}

```text

#### `[MemberData]` with `TheoryData<T>`

Best for complex data or shared datasets:

```csharp

public class OrderValidatorTests
{
    public static TheoryData<Order, bool> ValidationCases => new()
    {
        { new Order { Items = [new("SKU-1", 1)], CustomerId = "C1" }, true },
        { new Order { Items = [], CustomerId = "C1" }, false },              // no items
        { new Order { Items = [new("SKU-1", 1)], CustomerId = "" }, false }, // no customer
    };

    [Theory]
    [MemberData(nameof(ValidationCases))]
    public void IsValid_VariousOrders_ReturnsExpected(Order order, bool expected)
    {
        var validator = new OrderValidator();

        var result = validator.IsValid(order);

        Assert.Equal(expected, result);
    }
}

```text

#### `[ClassData]`

Best for data shared across multiple test classes:

```csharp

// xUnit v3: use TheoryDataRow<T> for strongly-typed rows
public class CurrencyConversionData : IEnumerable<TheoryDataRow<string, string, decimal>>
{
    public IEnumerator<TheoryDataRow<string, string, decimal>> GetEnumerator()
    {
        yield return new("USD", "EUR", 0.92m);
        yield return new("GBP", "USD", 1.27m);
        yield return new("EUR", "GBP", 0.86m);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// xUnit v2 compatibility: v2 uses IEnumerable<object[]> instead of TheoryDataRow<T>
// public class CurrencyConversionData : IEnumerable<object[]>
// {
//     public IEnumerator<object[]> GetEnumerator()
//     {
//         yield return new object[] { "USD", "EUR", 0.92m };
//     }
//     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
// }

[Theory]
[ClassData(typeof(CurrencyConversionData))]
public void Convert_KnownPairs_ReturnsExpectedRate(
    string from, string to, decimal expectedRate)
{
    var converter = new CurrencyConverter();

    var rate = converter.GetRate(from, to);

    Assert.Equal(expectedRate, rate, precision: 2);
}

```text

---

## Fixtures: Shared Setup and Teardown

Fixtures provide shared, expensive resources across tests while maintaining test isolation.

### `IClassFixture<T>` -- Shared Per Test Class

Use when multiple tests in the same class share an expensive resource (database connection, configuration):

```csharp

public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = "";

    public ValueTask InitializeAsync()
    {
        // xUnit v3: returns ValueTask (v2 returns Task)
        ConnectionString = $"Host=localhost;Database=test_{Guid.NewGuid():N}";
        // Create database, run migrations, etc.
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // xUnit v3: returns ValueTask (v2 returns Task)
        // Drop database
        return ValueTask.CompletedTask;
    }
}

public class OrderRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _db;

    public OrderRepositoryTests(DatabaseFixture db)
    {
        _db = db;
        // Each test gets the shared database fixture
    }

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOrder()
    {
        var repo = new OrderRepository(_db.ConnectionString);

        var result = await repo.GetByIdAsync(KnownOrderId);

        Assert.NotNull(result);
    }
}

```text

**v2 compatibility note:** In xUnit v2, `IAsyncLifetime.InitializeAsync()` and `DisposeAsync()` return `Task`. In v3, they return `ValueTask`. When migrating, change the return types accordingly.

### `ICollectionFixture<T>` -- Shared Across Test Classes

Use when multiple test classes need the same expensive resource:

```csharp

// 1. Define the collection
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code -- it is a marker for the collection
}

// 2. Use in test classes
[Collection("Database")]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _db;

    public OrderRepositoryTests(DatabaseFixture db)
    {
        _db = db;
    }

    [Fact]
    public async Task Insert_ValidOrder_Persists()
    {
        // Uses the shared database fixture
    }
}

[Collection("Database")]
public class CustomerRepositoryTests
{
    private readonly DatabaseFixture _db;

    public CustomerRepositoryTests(DatabaseFixture db)
    {
        _db = db;
    }
}

```text

### `IAsyncLifetime` on Test Classes

For per-test async setup/teardown without a shared fixture:

```csharp

public class FileProcessorTests : IAsyncLifetime
{
    private string _tempDir = "";

    public ValueTask InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Process_CsvFile_ExtractsRecords()
    {
        var filePath = Path.Combine(_tempDir, "data.csv");
        await File.WriteAllTextAsync(filePath, "Name,Age\nAlice,30\nBob,25");

        var processor = new FileProcessor();
        var records = await processor.ProcessAsync(filePath);

        Assert.Equal(2, records.Count);
    }
}

```text

---

## Parallel Execution

### Default Behavior

xUnit runs test classes within a collection sequentially but runs different collections in parallel. Each test class without an explicit `[Collection]` attribute is its own implicit collection, so by default test classes run in parallel.

### Controlling Parallelism

#### Disable Parallelism for Specific Tests

Place tests that share mutable state in the same collection:

```csharp

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection { }

[Collection("Sequential")]
public class StatefulServiceTests
{
    // These tests run sequentially within this collection
}

```text

#### Assembly-Level Configuration

Create `xunit.runner.json` in the test project root:

```json

{
    "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
    "parallelizeAssembly": false,
    "parallelizeTestCollections": true,
    "maxParallelThreads": 4
}

```text


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
