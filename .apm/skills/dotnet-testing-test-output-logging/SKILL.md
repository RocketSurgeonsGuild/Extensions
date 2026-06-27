---
name: dotnet-testing-test-output-logging
category: testing
subcategory: fundamentals
description: |
  Complete guide for xUnit test output and logging. Use when you need to implement test output, diagnostic logging, or ILogger alternatives in xUnit tests. Covers ITestOutputHelper injection, AbstractLogger pattern, structured output design. Includes XUnitLogger, CompositeLogger, performance test diagnostic tool implementations.
  Keywords: ITestOutputHelper, ILogger testing, test output xunit, test output, test logging, AbstractLogger, XUnitLogger, CompositeLogger, testOutputHelper.WriteLine, test diagnostics, logger mock, test log, structured output, Received().Log
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'xunit, ITestOutputHelper, ILogger, testing, diagnostics, logging'
  related_skills: 'unit-test-fundamentals, nsubstitute-mocking, xunit-project-setup'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-test-output-logging'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Test Output and Logging Expert Guide

This skill helps you implement high-quality test output and logging mechanisms in .NET xUnit test projects.

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Use ITestOutputHelper in xUnit tests to output diagnostic information
- Implement ILogger test alternatives (XUnitLogger)
- Create AbstractLogger or CompositeLogger patterns
- Design structured test output for debugging
- Implement performance test diagnostic tools

## Core Principles

### 1. ITestOutputHelper Usage Principles

### Correct Injection Method

- Inject `ITestOutputHelper` via constructor
- Each test class instance bound to test method
- Cannot be shared between static methods or across test methods

````csharp
public class MyTests
{
    private readonly ITestOutputHelper _output;

    public MyTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }
}
```text

### Common Mistakes

- ❌ Static access: `private static ITestOutputHelper _output`
- ❌ Using without awaiting in async tests
- ❌ Attempting to use in Dispose method

### 2. Structured Output Format Design

### Recommended Output Structure

```csharp
private void LogSection(string title)
{
    _output.WriteLine($"\n=== {title} ===");
}

private void LogKeyValue(string key, object value)
{
    _output.WriteLine($"{key}: {value}");
}

private void LogTimestamp(DateTime time)
{
    _output.WriteLine($"Execution Time: {time:yyyy-MM-dd HH:mm:ss.fff}");
}
```text

### Output Timing

- At test start: Log test setup and input data
- During execution: Log important state changes
- Before assertion: Log expected and actual values
- At test end: Log execution time and result summary

### 3. ILogger Testing Strategy

### Challenge: Extension Methods Cannot Be Directly Mocked

`ILogger.LogError()` is an extension method, NSubstitute cannot directly intercept. Need to intercept underlying `Log<TState>` method:

```csharp
// ❌ Wrong: Directly mocking extension method fails
logger.Received().LogError(Arg.Any<string>());

// ✅ Correct: Intercept underlying method
logger.Received().Log(
    LogLevel.Error,
    Arg.Any<EventId>(),
    Arg.Is<object>(o => o.ToString().Contains("expected message")),
    Arg.Any<Exception>(),
    Arg.Any<Func<object, Exception, string>>()
);
```text

### Solution: Use Abstraction Layer

Create `AbstractLogger<T>` to simplify testing:

```csharp
public abstract class AbstractLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state)
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        Log(logLevel, exception, state?.ToString() ?? string.Empty);
    }

    public abstract void Log(LogLevel logLevel, Exception ex, string information);
}
```text

### Using in Tests

```csharp
var logger = Substitute.For<AbstractLogger<MyService>>();
// Now can simply verify
logger.Received().Log(LogLevel.Error, Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("error message")));
```text

### 4. Diagnostic Tool Integration

### XUnitLogger: Direct Logs to Test Output

```csharp
public class XUnitLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _testOutputHelper;

    public XUnitLogger(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        _testOutputHelper.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] [{typeof(T).Name}] {message}");
        if (exception != null)
        {
            _testOutputHelper.WriteLine($"Exception: {exception}");
        }
    }

    // Other necessary interface implementations...
}
```text

### CompositeLogger: Support Both Verification and Output

```csharp
public class CompositeLogger<T> : ILogger<T>
{
    private readonly ILogger<T>[] _loggers;

    public CompositeLogger(params ILogger<T>[] loggers)
    {
        _loggers = loggers;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        foreach (var logger in _loggers)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    // Other interface implementations delegate to all internal loggers...
}
```text

### Usage

```csharp
// Perform both behavior verification and test output
var mockLogger = Substitute.For<AbstractLogger<MyService>>();
var xunitLogger = new XUnitLogger<MyService>(_output);
var compositeLogger = new CompositeLogger<MyService>(mockLogger, xunitLogger);

var service = new MyService(compositeLogger);
```text

## Implementation Guide

### Timing Recording in Performance Tests

```csharp
[Fact]
public async Task ProcessLargeDataSet_Performance_Test()
{
    // Arrange
    var stopwatch = Stopwatch.StartNew();
    var checkpoints = new List<(string Stage, TimeSpan Elapsed)>();

    _output.WriteLine("Starting large dataset processing...");

    // Act & Monitor
    await processor.LoadData(dataSet);
    checkpoints.Add(("Data Load", stopwatch.Elapsed));
    _output.WriteLine($"Data load complete: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

    await processor.ProcessData();
    checkpoints.Add(("Data Processing", stopwatch.Elapsed));
    _output.WriteLine($"Data processing complete: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

    stopwatch.Stop();

    // Assert & Report
    _output.WriteLine("\n=== Performance Report ===");
    foreach (var (stage, elapsed) in checkpoints)
    {
        _output.WriteLine($"{stage}: {elapsed.TotalMilliseconds:F2} ms");
    }
}
```text

### Diagnostic Test Base Class

```csharp
public abstract class DiagnosticTestBase
{
    protected readonly ITestOutputHelper Output;

    protected DiagnosticTestBase(ITestOutputHelper output)
    {
        Output = output;
    }

    protected void LogTestStart(string testName)
    {
        Output.WriteLine($"\n=== {testName} ===");
        Output.WriteLine($"Execution Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
    }

    protected void LogTestData(object data)
    {
        Output.WriteLine($"Test Data: {JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}");
    }

    protected void LogAssertionFailure(string field, object expected, object actual)
    {
        Output.WriteLine("\n=== Assertion Failure ===");
        Output.WriteLine($"Field: {field}");
        Output.WriteLine($"Expected: {expected}");
        Output.WriteLine($"Actual: {actual}");
    }
}
```text

## DO - Recommended Practices

1. **Appropriate Use of ITestOutputHelper**
   - ✅ Log important steps in complex tests
   - ✅ Adopt consistent structured output format
   - ✅ Provide diagnostic information when tests fail
   - ✅ Record timing points in performance tests

2. **Logger Testing Strategy**
   - ✅ Use abstraction layer (AbstractLogger) to simplify testing
   - ✅ Verify log level rather than full message
   - ✅ Use CompositeLogger to combine Mock with actual output
   - ✅ Ensure sensitive data is not logged

3. **Structured Output**
   - ✅ Use section headings to separate different phases
   - ✅ Include timestamps for easy tracking
   - ✅ Provide sufficient context information

## DON'T - Practices to Avoid

1. **Don't Overuse Output**
   - ❌ Avoid heavy output in every test
   - ❌ Don't log sensitive information (passwords, keys)
   - ❌ Avoid affecting test execution performance

2. **Don't Hardcode Log Verification**
   - ❌ Avoid verifying complete log messages (fragile)
   - ❌ Don't verify exact number of log calls (overspecified)
   - ❌ Avoid testing internal implementation details

3. **Don't Ignore Lifecycle**
   - ❌ Don't use ITestOutputHelper in static methods
   - ❌ Don't attempt to share instances across test methods
   - ❌ Avoid missing awaits in async tests

## Example Reference

See `templates/` directory for complete examples:

- `itestoutputhelper-example.cs` - ITestOutputHelper usage example
- `ilogger-testing-example.cs` - ILogger testing strategy example
- `diagnostic-tools.cs` - XUnitLogger and CompositeLogger implementation

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 08 - Test Output and Logging: xUnit ITestOutputHelper and ILogger**
  - Article: https://ithelp.ithome.com.tw/articles/10374711
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day08

### Official Documentation

- [xUnit Capturing Output](https://xunit.net/docs/capturing-output)

### Related Skills

- `unit-test-fundamentals` - Unit testing basics
- `xunit-project-setup` - xUnit project setup
- `nsubstitute-mocking` - Test doubles and mocking

## Testing Checklist

When implementing test output and logging, confirm the following checklist items:

- [ ] ITestOutputHelper correctly injected via constructor
- [ ] Using structured output format (sections, timestamps)
- [ ] Logger tests use abstraction layer or CompositeLogger
- [ ] Verifying log level rather than full message
- [ ] Performance tests include timing point recording
- [ ] No sensitive information leaked in output
- [ ] Async tests properly await log completion
- [ ] Sufficient diagnostic information when tests fail

## Reference Resources (continued)

See example files in same directory:

- [templates/itestoutputhelper-example.cs](templates/itestoutputhelper-example.cs) - ITestOutputHelper usage example
- [templates/ilogger-testing-example.cs](templates/ilogger-testing-example.cs) - ILogger testing example
- [templates/diagnostic-tools.cs](templates/diagnostic-tools.cs) - Diagnostic tool implementation
````
