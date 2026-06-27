---
name: dotnet-testing-advanced-xunit-upgrade-guide
category: testing
subcategory: fundamentals
description: |
  Complete guide for upgrading xUnit 2.9.x to 3.x. Use when upgrading xUnit v2 to v3 or understanding xUnit v3 new features and breaking changes. Covers package updates, async void fixes, IAsyncLifetime adjustments. Includes new features: Assert.Skip, Explicit Tests, Matrix Theory, Assembly Fixtures.
  Keywords: xunit upgrade, xunit v3, xunit 3.x, xunit migration, xunit upgrade, xunit.v3, OutputType Exe, IAsyncLifetime v3, Assert.Skip, SkipUnless, SkipWhen, Explicit attribute, MatrixTheoryData, AssemblyFixture, breaking changes
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'xunit, upgrade, migration, v3, breaking-changes, testing-framework'
  related_skills: 'xunit-project-setup, unit-test-fundamentals'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-advanced-xunit-upgrade-guide'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# xUnit Upgrade Guide: From 2.9.x to 3.x

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Upgrade existing xUnit 2.x test projects to xUnit 3.x
- Evaluate the scope of impact for xUnit upgrade
- Resolve compilation errors during xUnit upgrade
- Use xUnit 3.x new features to improve tests

## Core Concepts

### Package Naming Changes

xUnit v3 adopts a new package naming strategy:

| v1~v2 Package Name          | v3 Package Name                     | Description         |
| --------------------------- | ----------------------------------- | ------------------- |
| `xunit`                     | `xunit.v3`                          | Main test framework |
| `xunit.assert`              | `xunit.v3.assert`                   | Assertion library   |
| `xunit.core`                | `xunit.v3.core`                     | Core components     |
| `xunit.abstractions`        | (removed)                           | No longer needed    |
| `xunit.runner.visualstudio` | `xunit.runner.visualstudio` (3.x.y) | Test runner         |

**Important**: Use `xunit.v3` package name, not `xunit`.

### Minimum Runtime Requirements

xUnit 3.x strict requirements:

- **.NET Framework 4.7.2+** or
- **.NET 8.0+** (recommended)

**Unsupported versions**:

- .NET Core 3.1
- .NET 5, 6, 7

---

## Breaking Changes List

### 1. Test Project Becomes Executable

````xml
<!-- xUnit 2.x (Library) -->
<PropertyGroup>
  <OutputType>Library</OutputType>
</PropertyGroup>

<!-- xUnit 3.x (Exe) - Must change -->
<PropertyGroup>
  <OutputType>Exe</OutputType>
</PropertyGroup>
```text

### 2. async void Tests No Longer Supported

```csharp
// ❌ xUnit 2.x - fails in 3.x
[Fact]
public async void TestSomeAsyncFunction()
{
    var result = await SomeAsyncMethod();
    Assert.True(result);
}

// ✅ xUnit 3.x - correct way
[Fact]
public async Task TestSomeAsyncFunction()
{
    var result = await SomeAsyncMethod();
    Assert.True(result);
}
```text

### 3. IAsyncLifetime Changes

In xUnit 3.x, `IAsyncLifetime` inherits `IAsyncDisposable`. If implementing both `IAsyncLifetime` and `IDisposable`, only `DisposeAsync` will be called, not `Dispose`.

```csharp
// ⚠️ Pattern to note
public class MyTestClass : IAsyncLifetime, IDisposable
{
    public async Task InitializeAsync() { /* ... */ }
    public async Task DisposeAsync() { /* will be called */ }
    public void Dispose() { /* will NOT be called in 3.x */ }
}

// ✅ Recommended: put all cleanup logic in DisposeAsync
public class MyTestClass : IAsyncLifetime
{
    public async Task InitializeAsync() { /* initialization */ }
    public async Task DisposeAsync() { /* all cleanup logic */ }
}
```text

### 4. SkippableFact/SkippableTheory Removed

```csharp
// ❌ xUnit 2.x - removed
[SkippableFact]
public void SkippableTest()
{
    Skip.If(someCondition, "Skip reason");
    // test logic
}

// ✅ xUnit 3.x - use Assert.Skip
[Fact]
public void SkippableTest()
{
    if (someCondition)
    {
        Assert.Skip("Skip reason");
    }
    // test logic
}
```text

### 5. SDK-style Projects Only

Check if project file starts with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
```text

If traditional format, must convert to SDK-style first.

---

## Upgrade Steps

### Step 1: Create Upgrade Branch

```bash
git checkout -b feature/upgrade-xunit-v3
```text

### Step 2: Update Project Files

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- xUnit v3 packages -->
    <PackageReference Include="xunit.v3" Version="3.0.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />

    <!-- Common helper packages -->
    <PackageReference Include="AwesomeAssertions" Version="8.1.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
  </ItemGroup>
</Project>
```text

### Step 3: Fix async void Tests

Use IDE search:

```regex
async\s+void.*\[(Fact|Theory)\]
```text

Change all `async void` to `async Task`.

### Step 4: Update using Statements

```csharp
// Remove (no longer needed)
// using Xunit.Abstractions;

// Keep
using Xunit;
```text

### Step 5: Compile and Test

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test --verbosity normal
```text

---

## xUnit 3.x New Features

### Dynamic Skip Tests

**Declarative (SkipUnless/SkipWhen)**:

```csharp
[Fact(SkipUnless = nameof(IsWindowsEnvironment),
      Skip = "This test only runs on Windows")]
public void OnlyRunOnWindowsTest()
{
    // test logic
}

public static bool IsWindowsEnvironment =>
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
```text

**Imperative (Assert.Skip)**:

```csharp
[Fact]
public void SkipBasedOnEnvironmentVariableTest()
{
    var enableTests = Environment.GetEnvironmentVariable("ENABLE_INTEGRATION_TESTS");

    if (string.IsNullOrEmpty(enableTests) || enableTests.ToLower() != "true")
    {
        Assert.Skip("Integration tests disabled. Set ENABLE_INTEGRATION_TESTS=true to run");
    }

    // test logic...
}
```text

### Explicit Tests

```csharp
[Fact(Explicit = true)]
public void ExpensiveIntegrationTest()
{
    // This test won't run by default unless explicitly requested
    // Suitable for performance tests, long-running tests
}
```text

### [Test] Attribute

```csharp
// All three ways have same functionality
[Test]
public void UsingTestAttributeTest() { Assert.True(true); }

[Fact]
public void UsingFactAttributeTest() { Assert.True(true); }
```text

### Matrix Theory Data

```csharp
public static TheoryData<int, string> TestData =>
    new MatrixTheoryData<int, string>(
        [1, 2, 3],                    // number data
        ["Hello", "World", "Test"]    // string data
    );
    // This generates 3×3=9 test cases

[Theory]
[MemberData(nameof(TestData))]
public void MatrixTestExample(int number, string text)
{
    number.Should().BePositive();
    text.Should().NotBeNullOrEmpty();
}
```text

### Assembly Fixtures

```csharp
public class DatabaseAssemblyFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        // create test database
        ConnectionString = await CreateTestDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        // cleanup test database
        await DropTestDatabaseAsync();
    }
}

// Register Assembly Fixture
[assembly: AssemblyFixture(typeof(DatabaseAssemblyFixture))]

// Use in tests
public class UserServiceTests
{
    private readonly DatabaseAssemblyFixture _dbFixture;

    public UserServiceTests(DatabaseAssemblyFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public void Test1() { /* use _dbFixture.ConnectionString */ }
}
```text

### Test Pipeline Startup

```csharp
public class TestPipelineStartup : ITestPipelineStartup
{
    public async Task ConfigureAsync(ITestPipelineBuilder builder,
                                     CancellationToken cancellationToken)
    {
        // global initialization logic
        Console.WriteLine("Initializing test environment...");
        await InitializeDatabaseAsync();
    }
}

// Register
[assembly: TestPipelineStartup(typeof(TestPipelineStartup))]
```text

---

## xunit.runner.json Configuration

```json
{
  "$schema": "https://xunit.net/schema/v3/xunit.runner.schema.json",
  "parallelAlgorithm": "conservative",
  "maxParallelThreads": 4,
  "diagnosticMessages": true,
  "internalDiagnosticMessages": false,
  "methodDisplay": "classAndMethod",
  "preEnumerateTheories": true,
  "stopOnFail": false
}
```text

---

## Test Report Formats

xUnit 3.x supports multiple report formats:

```bash
# Generate CTRF format report
dotnet run -- -ctrf results.json

# Generate TRX format report
dotnet run -- -trx results.trx

# Generate XML format report
dotnet run -- -xml results.xml

# Generate multiple format reports
dotnet run -- -xml results.xml -ctrf results.json -trx results.trx
```text

---

## Common Issues and Solutions

### Issue 1: xunit.abstractions not found

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
