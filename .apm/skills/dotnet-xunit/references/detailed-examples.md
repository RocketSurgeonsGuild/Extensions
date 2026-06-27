
Ensure it is copied to output:

```xml

<ItemGroup>
  <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>

```json

**v2 compatibility note:** In v2, configuration was via `xunit.runner.json` or assembly attributes. v3 retains `xunit.runner.json` support with the same property names.

---

## Test Output

### `ITestOutputHelper`

Capture diagnostic output that appears in test results:

```csharp

public class DiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public DiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ProcessBatch_LargeDataset_CompletesWithinTimeout()
    {
        var sw = Stopwatch.StartNew();

        var result = await processor.ProcessBatchAsync(largeDataset);

        sw.Stop();
        _output.WriteLine($"Processed {result.Count} items in {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5));
    }
}

```text

### Integrating with `ILogger`

Bridge xUnit output to `Microsoft.Extensions.Logging` for integration tests:

```csharp

// NuGet: Microsoft.Extensions.Logging (for LoggerFactory)
// + a logging provider that writes to ITestOutputHelper
// Common approach: use a simple adapter
public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public XunitLoggerProvider(ITestOutputHelper output) => _output = output;

    public ILogger CreateLogger(string categoryName) =>
        new XunitLogger(_output, categoryName);

    public void Dispose() { }
}

public class XunitLogger(ITestOutputHelper output, string category) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        output.WriteLine($"[{logLevel}] {category}: {formatter(state, exception)}");
        if (exception is not null)
            output.WriteLine(exception.ToString());
    }
}

```text

---

## Custom Assertions

### Extending Assert with Custom Methods

Create domain-specific assertions for cleaner test code:

```csharp

public static class OrderAssert
{
    public static void HasStatus(Order order, OrderStatus expected)
    {
        Assert.NotNull(order);
        if (order.Status != expected)
        {
            throw Xunit.Sdk.EqualException.ForMismatchedValues(
                expected, order.Status);
        }
    }

    public static void ContainsItem(Order order, string sku, int quantity)
    {
        Assert.NotNull(order);
        var item = Assert.Single(order.Items, i => i.Sku == sku);
        Assert.Equal(quantity, item.Quantity);
    }
}

// Usage
[Fact]
public void Complete_ValidOrder_SetsCompletedStatus()
{
    var order = new Order();
    order.Complete();

    OrderAssert.HasStatus(order, OrderStatus.Completed);
}

```text

### Using `Assert.Multiple` (xUnit v3)

Group related assertions so all are evaluated even if one fails:

```csharp

[Fact]
public void CreateOrder_ValidRequest_SetsAllProperties()
{
    var order = OrderFactory.Create(request);

    Assert.Multiple(
        () => Assert.Equal("cust-123", order.CustomerId),
        () => Assert.Equal(OrderStatus.Pending, order.Status),
        () => Assert.NotEqual(Guid.Empty, order.Id),
        () => Assert.NotEmpty(order.Items)
    );
}

```text

**v2 compatibility note:** `Assert.Multiple` is new in xUnit v3. In v2, use separate assertions -- the test stops at the first failure.

---

## xUnit Analyzers

The `xunit.analyzers` package (included with xUnit v3) catches common test authoring mistakes at compile time.

### Important Rules

| Rule | Description | Severity |
|------|-------------|----------|
| `xUnit1004` | Test methods should not be skipped | Info |
| `xUnit1012` | Null should not be used for value type parameters | Warning |
| `xUnit1025` | `InlineData` should be unique within a `Theory` | Warning |
| `xUnit2000` | Constants and literals should be the expected argument | Warning |
| `xUnit2002` | Do not use null check on value type | Warning |
| `xUnit2007` | Do not use `typeof` expression to check type | Warning |
| `xUnit2013` | Do not use equality check to check collection size | Warning |
| `xUnit2017` | Do not use `Contains()` to check if value exists in a set | Warning |

### Suppressing Specific Rules

In `.editorconfig` for test projects:

```ini

[tests/**.cs]
# Allow skipped tests during development
dotnet_diagnostic.xUnit1004.severity = suggestion

```csharp

---

## Key Principles

- **One fact per `[Fact]`, one concept per `[Theory]`.** If a `[Theory]` tests fundamentally different scenarios, split into separate `[Fact]` methods.
- **Use `IClassFixture` for expensive shared resources** within a single test class. Use `ICollectionFixture` when multiple classes share the same resource.
- **Do not disable parallelism globally.** Instead, group tests that share mutable state into named collections.
- **Use `IAsyncLifetime` for async setup/teardown** instead of constructors and `IDisposable`. Constructors cannot be async, and `IDisposable.Dispose()` does not await.
- **Keep test data close to the test.** Prefer `[InlineData]` for simple cases. Use `[MemberData]` or `[ClassData]` only when data is complex or shared.
- **Enable xUnit analyzers** in all test projects. They catch common mistakes that lead to false-passing or flaky tests.

---

## Agent Gotchas

1. **Do not use constructor-injected `ITestOutputHelper` in static methods.** `ITestOutputHelper` is per-test-instance; store it in an instance field, not a static one.
2. **Do not forget to make fixture classes `public`.** xUnit requires fixture types to be public with a public parameterless constructor (or `IAsyncLifetime`). Non-public fixtures cause silent failures.
3. **Do not mix `[Fact]` and `[Theory]` on the same method.** A method is either a fact or a theory, not both.
4. **Do not return `void` from async test methods.** Return `Task` or `ValueTask`. `async void` tests report false success because xUnit cannot observe the async completion.
5. **Do not use `[Collection]` without a matching `[CollectionDefinition]`.** An unmatched collection name silently creates an implicit collection with default behavior, defeating the purpose.

---

## References

- [xUnit Documentation](https://xunit.net/)
- [xUnit v3 migration guide](https://xunit.net/docs/getting-started/v3/migration)
- [xUnit analyzers](https://xunit.net/xunit.analyzers/rules/)
- [Shared context in xUnit](https://xunit.net/docs/shared-context)
- [Configuring xUnit with JSON](https://xunit.net/docs/configuration-files)
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
