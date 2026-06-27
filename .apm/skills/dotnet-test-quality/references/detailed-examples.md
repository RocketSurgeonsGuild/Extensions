```text

Mutation Score = Killed / (Killed + Survived + NoCoverage) * 100

```text

A mutation score of 80%+ indicates a strong test suite. Below 60% suggests significant gaps.

### Example: Identifying Test Gaps

Given this production code:

```csharp

public class PricingService
{
    public decimal CalculateDiscount(decimal price, CustomerTier tier) =>
        tier switch
        {
            CustomerTier.Bronze => price * 0.05m,
            CustomerTier.Silver => price * 0.10m,
            CustomerTier.Gold => price * 0.15m,
            CustomerTier.Platinum => price * 0.20m,
            _ => 0m
        };
}

```text

If tests only verify `Gold` tier, Stryker generates mutations like:
- Replace `0.05m` with `0.06m` (survived -- no Bronze test)
- Replace `0.10m` with `0.11m` (survived -- no Silver test)
- Replace `0.15m` with `0.16m` (killed -- Gold test catches this)
- Replace `0.20m` with `0.21m` (survived -- no Platinum test)
- Replace `0m` with `1m` (survived -- no default test)

The HTML report highlights each surviving mutation with the exact code change, guiding where to add tests.

### Stryker Thresholds

```json

{
  "thresholds": {
    "high": 80,   // Green: mutation score >= 80%
    "low": 60,    // Yellow: 60% <= mutation score < 80%
    "break": 50   // Red: mutation score < 50% -> exit code 1
  }
}

```text

The `break` threshold causes Stryker to return a non-zero exit code, useful for CI gates.

---

## Flaky Test Detection

Flaky tests pass and fail intermittently without code changes. They erode trust in the test suite and slow development.

### Common Causes

| Cause | Symptom | Fix |
|-------|---------|-----|
| **Shared mutable state** | Tests fail when run in specific order | Use proper test isolation (see [skill:dotnet-xunit] for fixtures) |
| **Time-dependent logic** | Tests fail near midnight or at specific times | Inject `TimeProvider` (or `ISystemClock`) instead of using `DateTime.Now` |
| **Race conditions** | Tests fail intermittently under parallel execution | Use `ICollectionFixture` for shared resources; avoid shared static state |
| **External dependencies** | Tests fail when network/services unavailable | Mock external calls; use Testcontainers for infrastructure |
| **Port conflicts** | Tests fail when another process uses the same port | Use dynamic port allocation (WebApplicationFactory handles this) |
| **File system contention** | Tests fail under parallel execution | Use unique temp directories per test (see [skill:dotnet-xunit] `IAsyncLifetime` patterns) |

### Detecting Flaky Tests

#### Repeated Runs

```bash

# Run tests multiple times to surface flakiness
for i in $(seq 1 10); do
  dotnet test --logger "trx;LogFileName=run-$i.trx" || echo "Run $i failed"
done

```text

#### xUnit Conditional Skip

**xUnit v3** has built-in conditional skip via `Skip` on `[Fact]`:

```csharp

// xUnit v3 — built-in conditional skip
[Fact(Skip = "Requires external service")]
public async Task ExternalApi_ReturnsData()
{
    var result = await _client.GetDataAsync();
    Assert.NotEmpty(result);
}

// xUnit v3 — runtime skip via Assert.Skip
[Fact]
public async Task ExternalApi_ReturnsData()
{
    if (!await IsServiceAvailable())
        Assert.Skip("External service unavailable");

    var result = await _client.GetDataAsync();
    Assert.NotEmpty(result);
}

```text

### Time-Dependent Tests

Replace `DateTime.Now`/`DateTime.UtcNow` with .NET 8's `TimeProvider`:

```csharp

// Production code
public class SubscriptionService(TimeProvider timeProvider)
{
    public bool IsExpired(Subscription sub)
    {
        var now = timeProvider.GetUtcNow();
        return sub.ExpiresAt < now;
    }
}

// Test code
[Fact]
public void IsExpired_PastExpiry_ReturnsTrue()
{
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero));

    var service = new SubscriptionService(fakeTime);
    var sub = new Subscription
    {
        ExpiresAt = new DateTimeOffset(2025, 6, 14, 0, 0, 0, TimeSpan.Zero)
    };

    Assert.True(service.IsExpired(sub));
}

[Fact]
public void IsExpired_FutureExpiry_ReturnsFalse()
{
    var fakeTime = new FakeTimeProvider(
        new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero));

    var service = new SubscriptionService(fakeTime);
    var sub = new Subscription
    {
        ExpiresAt = new DateTimeOffset(2025, 6, 16, 0, 0, 0, TimeSpan.Zero)
    };

    Assert.False(service.IsExpired(sub));
}

```text

**Note:** `FakeTimeProvider` is available in `Microsoft.Extensions.TimeProvider.Testing` (NuGet).

### Quarantine Strategy

When a flaky test cannot be fixed immediately:

```csharp

// Mark as skipped with a tracking issue
[Fact(Skip = "Flaky: tracking in #1234 -- race condition in event handler")]
public async Task EventHandler_ConcurrentEvents_ProcessesAll()
{
    // ...
}

```text

Do not delete flaky tests. Skip them with an issue reference and fix them systematically.

---

## Key Principles

- **Coverage is a lagging indicator, not a target.** High coverage does not guarantee good tests. A test suite with 90% coverage can still miss critical bugs if the assertions are weak.
- **Use CRAP scores to prioritize.** Focus testing effort on methods with high complexity and low coverage rather than chasing overall coverage percentage.
- **Run mutation testing on critical paths.** Mutation testing is computationally expensive. Focus on business-critical code (pricing, authentication, data validation) rather than running it on the entire codebase.
- **Fix flaky tests immediately or quarantine them.** A flaky test that remains in the suite trains developers to ignore failures, undermining the entire test suite's value.
- **Measure trends, not snapshots.** Track coverage and mutation scores over time. A declining trend indicates test quality erosion even if absolute numbers look acceptable.
- **Exclude generated code from coverage.** Migrations, generated clients, and scaffolded code inflate or deflate coverage numbers without reflecting actual test quality.

---

## Agent Gotchas

1. **Do not confuse `coverlet.collector` with `coverlet.msbuild`.** The `coverlet.collector` package uses the `--collect:"XPlat Code Coverage"` CLI flag. The `coverlet.msbuild` package uses `/p:CollectCoverage=true` MSBuild properties. Do not mix flags across packages -- they are independent integration points.
2. **Do not hardcode coverage result paths.** The GUID in `TestResults/<guid>/coverage.cobertura.xml` changes every run. Always use glob patterns (`**/TestResults/*/coverage.cobertura.xml`) when referencing coverage output files.
3. **Do not set coverage thresholds too high initially.** Starting with 90%+ thresholds on an existing project blocks all PRs. Begin with the current baseline and increase incrementally (e.g., 5% per quarter).
4. **Do not run Stryker.NET on the entire solution for CI.** Mutation testing is CPU-intensive. In CI, limit mutations to changed files (`--since:main`) or critical paths. Reserve full runs for nightly builds.
5. **Do not ignore survived mutations in trivial code.** While some survived mutations are in code that does not warrant testing (logging, `ToString()`), review each one. Configure `ignore-mutations` in `stryker-config.json` for categories you have consciously decided not to test.
6. **Do not use `[ExcludeFromCodeCoverage]` as a blanket fix for low coverage.** This attribute hides the problem rather than solving it. Use it only for genuinely untestable code (platform interop, generated code) and ensure the reason is documented.

---

## References

- [Coverlet GitHub repository](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator GitHub repository](https://github.com/danielpalme/ReportGenerator)
- [ReportGenerator usage guide](https://github.com/danielpalme/ReportGenerator/wiki)
- [Stryker.NET documentation](https://stryker-mutator.io/docs/stryker-net/introduction/)
- [Stryker.NET configuration](https://stryker-mutator.io/docs/stryker-net/configuration/)
- [TimeProvider in .NET 8](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider)
- [Microsoft.Extensions.TimeProvider.Testing](https://www.nuget.org/packages/Microsoft.Extensions.TimeProvider.Testing)
- [CRAP metric explanation](https://testing.googleblog.com/2011/02/this-code-is-crap.html)
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
