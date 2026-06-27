---
name: dotnet-testing-datetime-testing-timeprovider
category: testing
subcategory: specialized
description: |
  Specialized skill for testing time-dependent logic using TimeProvider. Use when testing DateTime, controlling time flow, handling timezone conversions, and testing expiration logic. Covers TimeProvider abstraction, FakeTimeProvider time control, time freezing and fast-forwarding.
  Keywords: datetime, time testing, TimeProvider, FakeTimeProvider, DateTime.Now, time-dependent, cache expiration, token expiration, Microsoft.Bcl.TimeProvider, GetUtcNow, SetUtcNow, Advance, time freeze, time freezing, time fast-forward
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, TimeProvider, DateTime, time testing'
  related_skills: 'unit-test-fundamentals, nsubstitute-mocking, filesystem-testing-abstractions'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-datetime-testing-timeprovider'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# DateTime and Time-Dependent Testing Guide

## Applicable Scenarios

This skill guides how to use Microsoft.Bcl.TimeProvider to solve testing problems with time-dependent code. Through time
abstraction, make "current time" controllable, predictable, and reproducible.

### Applicable Scenarios

- **Business Hours Validation**: System determines whether to allow operations based on current time
- **Promotion Control**: Logic that activates during specific dates or time periods
- **Cache Expiration Mechanism**: Determines whether data is valid based on time
- **Scheduled Task Triggers**: Background jobs that run at scheduled times
- **Token Expiration**: Security mechanisms that are time-sensitive

### Required Packages

````xml
<!-- Production code -->
<PackageReference Include="Microsoft.Bcl.TimeProvider" Version="9.0.0" />

<!-- Test project -->
<PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" Version="9.0.0" />
```text

---

## Core Principles

### Principle One: Time Abstraction - Replace DateTime with TimeProvider

**Traditional Problem Code**:

```csharp
// ❌ Untestable - uses static time directly
public class OrderService
{
    public bool CanPlaceOrder()
    {
        var now = DateTime.Now;
        return now.Hour >= 9 && now.Hour < 17;
    }
}
```text

**Testable Refactoring**:

```csharp
// ✅ Testable - receives TimeProvider through dependency injection
public class OrderService
{
    private readonly TimeProvider _timeProvider;

    public OrderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public bool CanPlaceOrder()
    {
        var now = _timeProvider.GetLocalNow();
        return now.Hour >= 9 && now.Hour < 17;
    }
}
```text

**Dependency Injection Configuration**:

```csharp
// Program.cs - production environment uses system time
services.AddSingleton(TimeProvider.System);
services.AddScoped<OrderService>();
```text

### Principle Two: FakeTimeProvider Controls Test Time

FakeTimeProvider provides complete time control capabilities:

| Method | Purpose | When to Use |
| ------ | ------- | ----------- |
| `SetUtcNow(DateTimeOffset)` | Set UTC time | When precise UTC time is needed |
| `SetLocalTimeZone(TimeZoneInfo)` | Set local timezone | When testing timezone-related logic |
| `Advance(TimeSpan)` | Fast-forward time | When testing expiration, delay logic |
| `GetUtcNow()` | Get UTC time | Read current simulated time |
| `GetLocalNow()` | Get local time | Read local simulated time |

**Recommended Extension Method**:

```csharp
public static class FakeTimeProviderExtensions
{
    /// <summary>
    /// Sets FakeTimeProvider local time
    /// </summary>
    public static void SetLocalNow(this FakeTimeProvider fakeTimeProvider, DateTime localDateTime)
    {
        fakeTimeProvider.SetLocalTimeZone(TimeZoneInfo.Local);
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, TimeZoneInfo.Local);
        fakeTimeProvider.SetUtcNow(utcTime);
    }
}
```text

### Principle Three: Each Test Uses Independent Time Environment

```csharp
// ✅ Correct: each test creates independent FakeTimeProvider
public class OrderServiceTests
{
    [Fact]
    public void CanPlaceOrder_DuringBusinessHours_ShouldReturnTrue()
    {
        // Arrange - independent instance
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));
        var sut = new OrderService(fakeTimeProvider);

        // Act
        var result = sut.CanPlaceOrder();

        // Assert
        result.Should().BeTrue();
    }
}

// ❌ Avoid: multiple tests sharing static instance
public class BadTestClass
{
    private static readonly FakeTimeProvider SharedProvider = new(); // will interfere with each other
}
```text

---

## Advanced Time Control Techniques

### Time Freezing

When you need to verify multiple operations occur at the "same time point":

```csharp
[Fact]
public void ProcessBatch_AtFixedTimePoint_ShouldGenerateSameTimestamp()
{
    var fakeTimeProvider = new FakeTimeProvider();
    var fixedTime = new DateTime(2024, 12, 25, 10, 30, 0);
    fakeTimeProvider.SetLocalNow(fixedTime);

    var processor = new BatchProcessor(fakeTimeProvider);

    var result1 = processor.ProcessItem("Item1");
    var result2 = processor.ProcessItem("Item2");

    // Time is frozen, both operations have same timestamp
    result1.Timestamp.Should().Be(result2.Timestamp);
}
```text

### Time Fast-Forward (Advance)

Test time-sensitive logic like cache expiration, token invalidation:

```csharp
[Fact]
public void Cache_AfterExpirationTime_ShouldClearItems()
{
    var fakeTimeProvider = new FakeTimeProvider();
    fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));

    var cache = new TimedCache(fakeTimeProvider, TimeSpan.FromMinutes(5));
    cache.Set("key", "value");

    // After 3 minutes - not yet expired
    fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
    cache.Get("key").Should().Be("value");

    // After another 3 minutes (total 6 minutes) - expired
    fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
    cache.Get("key").Should().BeNull();
}
```text

> **Important**: `Advance()` is non-blocking, instantly jumps time without actually waiting.

### Time Rewind

Test historical data processing or replay scenarios:

```csharp
[Fact]
public void HistoricalDataProcessor_GoingBackInTime_ShouldProcessCorrectly()
{
    var fakeTimeProvider = new FakeTimeProvider();
    var historicalTime = new DateTime(2020, 1, 15, 9, 0, 0);
    fakeTimeProvider.SetLocalNow(historicalTime);

    var processor = new HistoricalDataProcessor(fakeTimeProvider);
    var result = processor.ProcessDataForDate(historicalTime.Date);

    result.ProcessedAt.Should().Be(historicalTime);
}
```text

---

## Practical Testing Patterns

### Pattern One: Parameterized Boundary Testing

```csharp
[Theory]
[InlineData(8, false)]   // 8 AM - before business hours
[InlineData(9, true)]    // 9 AM - business hours start
[InlineData(12, true)]   // 12 PM - during business hours
[InlineData(16, true)]   // 4 PM - during business hours
[InlineData(17, false)]  // 5 PM - business hours end
[InlineData(18, false)]  // 6 PM - after business hours
public void CanPlaceOrder_AtDifferentTimes_ShouldReturnCorrectResult(int hour, bool expected)
{
    var fakeTimeProvider = new FakeTimeProvider();
    fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, hour, 0, 0));

    var sut = new OrderService(fakeTimeProvider);

    sut.CanPlaceOrder().Should().Be(expected);
}
```text

### Pattern Two: Trading Hours Window Testing

```csharp
[Theory]
[InlineData("09:30:00", true)]   // Morning trading hours
[InlineData("12:00:00", false)]  // Lunch break
[InlineData("14:30:00", true)]   // Afternoon trading hours
[InlineData("15:30:00", false)]  // After trading ends
public void IsInTradingHours_AtDifferentTimes_ShouldReturnCorrectResult(string timeStr, bool expected)
{
    var fakeTimeProvider = new FakeTimeProvider();
    var testTime = DateTime.Today.Add(TimeSpan.Parse(timeStr));
    fakeTimeProvider.SetLocalNow(testTime);

    var sut = new TradingService(fakeTimeProvider);

    sut.IsInTradingHours().Should().Be(expected);
}
```text

### Pattern Three: Schedule Trigger Logic Testing

```csharp
[Theory]
[InlineData("2024-03-15 14:30:00", "2024-03-15 14:00:00", true)]   // Time to execute
[InlineData("2024-03-15 13:30:00", "2024-03-15 14:00:00", false)]  // Not yet time
public void ShouldExecuteJob_BasedOnTime_ShouldReturnCorrectResult(
    string currentTimeStr, string scheduledTimeStr, bool expected)
{
    var fakeTimeProvider = new FakeTimeProvider();
    fakeTimeProvider.SetLocalNow(DateTime.Parse(currentTimeStr));

    var schedule = new JobSchedule { NextExecutionTime = DateTime.Parse(scheduledTimeStr) };
    var sut = new ScheduleService(fakeTimeProvider);

    sut.ShouldExecuteJob(schedule).Should().Be(expected);
}
```text

---

## AutoFixture Integration

### FakeTimeProviderCustomization

```csharp
public class FakeTimeProviderCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Register(() => new FakeTimeProvider());
    }
}
```text

### AutoDataWithCustomization Attribute

```csharp
public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new FakeTimeProviderCustomization());
    }
}
```text

### Using Matching.DirectBaseType

```csharp
[Theory]
[AutoDataWithCustomization]
public void GetTimeBasedDiscount_OnFriday_ShouldReturnTenPercentDiscount(
    [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
    OrderService sut)
{
    // Matching.DirectBaseType tells AutoFixture:
    // When TimeProvider (base type) is needed, use FakeTimeProvider (derived type)

    var fridayTime = new DateTime(2024, 3, 15, 14, 0, 0); // Friday
    fakeTimeProvider.SetLocalNow(fridayTime);

    sut.GetTimeBasedDiscount().Should().Be("Happy Friday: 10% Discount");
}
```text

> **Key**: Must use `[Frozen(Matching.DirectBaseType)]`, otherwise AutoFixture cannot correctly inject FakeTimeProvider into constructors requiring TimeProvider.

---

## Best Practices Checklist

### ✅ Code Design

- [ ] All time-dependent classes receive `TimeProvider` through constructor
- [ ] Use `_timeProvider.GetLocalNow()` instead of `DateTime.Now`
- [ ] Use `_timeProvider.GetUtcNow()` instead of `DateTime.UtcNow`
- [ ] DI container registers `TimeProvider.System` as production implementation

### ✅ Test Design

- [ ] Each test method uses independent `FakeTimeProvider` instance
- [ ] Use `SetLocalNow()` extension method to simplify time setup
- [ ] Use `Advance()` to test time-sensitive logic (cache, expiration, delay)
- [ ] Tests cover boundary conditions (start time, end time, critical points)

### ✅ Advanced Considerations

- [ ] FakeTimeProvider is thread-safe, can be used for parallel tests
- [ ] Use `IDisposable` pattern to properly dispose FakeTimeProvider
- [ ] Use `SetLocalTimeZone()` to explicitly set timezone for timezone tests

---

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 16 - Testing Dates and Times: Replace DateTime with Microsoft.Bcl.TimeProvider**
  - Article: https://ithelp.ithome.com.tw/articles/10375821
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day16

### Official Documentation

- [TimeProvider API](https://learn.microsoft.com/dotnet/api/system.timeprovider)
- [Microsoft.Bcl.TimeProvider NuGet](https://www.nuget.org/packages/Microsoft.Bcl.TimeProvider/)
- [Microsoft.Extensions.TimeProvider.Testing NuGet](https://www.nuget.org/packages/Microsoft.Extensions.TimeProvider.Testing/)

### Related Skills

- `autofixture-basics` - AutoFixture automatic test data generation
- `nsubstitute-mocking` - Test doubles and mocking
- `autodata-xunit-integration` - xUnit and AutoFixture AutoData integration
````
