---
name: dotnet-testing-code-coverage-analysis
category: testing
subcategory: fundamentals
description: |
  Complete guide for code coverage analysis. Use when you need to analyze code coverage, generate coverage reports, or configure CI/CD coverage checks. Covers Coverlet configuration, report generation, metric interpretation, and cyclomatic complexity integration. Includes Fine Code Coverage, VS Code built-in tools, and best practices.
  Keywords: code coverage, coverage analysis, coverage report, Coverlet, Fine Code Coverage, dotnet-coverage, ReportGenerator, line coverage, branch coverage, cyclomatic complexity, runsettings, cobertura
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'code-coverage, coverlet, testing-metrics, quality, ci-cd, analysis'
  related_skills: 'unit-test-fundamentals, xunit-project-setup'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-code-coverage-analysis'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Code Coverage Analysis Guide

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Configure and execute code coverage analysis
- Configure Coverlet or other coverage tools
- Generate and interpret coverage reports
- View coverage in Visual Studio or VS Code
- Evaluate test completeness and quality
- Combine complexity metrics to formulate testing strategies

## Code Coverage Core Concepts

### Definition

**Code Coverage** is a measurement metric used to count how much code is actually executed during test execution.

### Correct Understanding

### Actual Value of Code Coverage

1. **Find Testing Blind Spots**: Quickly identify code not tested
2. **Evaluate Test Completeness**: Check if important logic is tested
3. **Assist Refactoring Decisions**: Understand which areas need more attention
4. **Increase Testing Confidence**: Confirm critical paths are validated

### Common Misconceptions (Must Avoid)

❌ **Wrong Understanding:**

- 100% coverage means no bugs
- Higher coverage numbers are always better
- Can use coverage as KPI

✅ **Correct Understanding:**

- Code coverage is just a reminder tool, telling you which code is not tested
- **Focus on test effectiveness, not coverage numbers**
- Helps determine if more test cases are needed
- **Should never be used as KPI**

> ⚠️ **Warning**: When Code Coverage is used as KPI, developers write tests without Asserts just to boost numbers,
> completely losing the meaning of testing.

## .NET Project Coverage Tool Selection

### 1. Visual Studio Enterprise (Enterprise Edition Only)

### Pros

- Built-in integration, no additional configuration needed
- Complete UI support
- Real-time results display

### Limitations

- **Only Enterprise version has this feature**
- Professional and Community versions not supported

### 2. Fine Code Coverage (Recommended Free Option)

### Pros: (continued)

- Completely free
- Integrated in Visual Studio
- Real-time coverage display
- Direct editor marking

**Installation:** Visual Studio Extensions Manager → Search "Fine Code Coverage" → Install

**Required Configuration:** Tools → Options → Fine Code Coverage → Enable: `True`, Editor Colouring Line Highlighting:
`True`

### 3. .NET CLI Tools

### Usage Scenarios

- CI/CD pipeline integration
- Command-line automation
- Cross-platform development

### Installation and Usage

````powershell
# Install tool
dotnet tool install -g dotnet-coverage

# Execute tests and generate report
dotnet-coverage collect dotnet test

# Or use Coverlet (recommended)
dotnet test --collect:"XPlat Code Coverage"
```text

### 4. VS Code Built-in Test Coverage

### Pros: (continued)

- Cross-platform support
- Built-in feature, no extension needed
- Integrated test management

### Usage

1. Install C# Dev Kit extension
2. Open Test Explorer (beaker icon)
3. Click "Run Coverage Test"
4. View results: Test Coverage view, editor display, file explorer display

## Executing Coverage Analysis

### Method 1: Using .NET CLI (Recommended for CI/CD)

```powershell
# Execute tests and collect coverage
dotnet test --collect:"XPlat Code Coverage"

# Specify output format
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate multiple format reports
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat="cobertura;opencover;json"
```text

### Method 2: Using Fine Code Coverage

1. Execute tests in Visual Studio
2. View → Other Windows → Fine Code Coverage
3. Coverage report automatically displayed

### Method 3: Using VS Code

1. Open Test Explorer
2. Click "Run Coverage Test" icon
3. View coverage results:
   - **Test Coverage** view: Tree structure
   - **Editor display**: Green/red marking
   - **File Explorer**: Percentage display

## Configuring Coverlet

### Configuring in csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test framework packages -->
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />

    <!-- Coverage collector -->
    <PackageReference Include="coverlet.collector" Version="6.0.3">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```text

### Using runsettings File

See `templates/runsettings-template.xml` file for advanced coverage configuration:

- Exclude specific files or namespaces
- Set coverage thresholds
- Custom report formats

### Usage: (continued)

```powershell
dotnet test --settings coverage.runsettings
```text

## Interpreting Coverage Reports

### Color Marking Explanation

In code editor:

- **Green**: Covered by tests
- **Yellow**: Partially covered (some branches not tested)
- **Red**: Not covered

### Coverage Metrics

1. **Line Coverage**
   - Executed code lines / Total code lines
   - Most basic metric

2. **Branch Coverage**
   - Executed branches / Total branches
   - More accurate than line coverage
   - Ensures all branches like if/else, switch are tested

3. **Method Coverage**
   - Executed methods / Total methods

### Report Interpretation Strategy

1. **Prioritize Red Areas**
   - Code completely not tested
   - May be critical business logic

2. **Check Yellow Areas**
   - Confirm all conditional branches are tested
   - Pay special attention to if/else, try/catch, etc.

3. **Evaluate Necessity**
   - Simple getters/setters may not need testing
   - Auto-generated code can be excluded
   - Focus on business logic and complex calculations

## Combining Complexity Metrics

### Cyclomatic Complexity

### Definition

Number of independent logic paths in code

### Relationship with Test Cases

- Cyclomatic complexity = Minimum number of test cases needed
- Each if, for, while, case, &&, || increases complexity

### Example

```csharp
public int Max(int[] array)
{
    if (array == null || array.Length == 0)  // +2 (null check + length check)
    {
        throw new ArgumentException("array must not be empty.");
    }

    int max = array[0];

    for (int i = 1; i < array.Length; i++)  // +1 (loop)
    {
        if (array[i] > max)  // +1 (condition check)
        {
            max = array[i];
        }
    }

    return max;  // +1 (method itself)
}
// Total complexity = 5
```text

### Testing Strategy

Cyclomatic complexity is 5, need at least 5 test cases:

1. Pass null → Test `array == null`
2. Pass empty array → Test `array.Length == 0`
3. Single element → Doesn't enter loop
4. Max at beginning → Loop doesn't update max
5. Max in middle → Loop updates max

### Visual Studio Extensions

### CodeMaintainability

- Display maintainability metrics
- Calculate cyclomatic complexity
- Evaluate code quality

### CodeMaid

- Spade feature: Visualize code structure
- Display complexity of each method
- Help identify code needing refactoring

## Improving Coverage Strategy

### 1. Gradual Improvement

```text
Current coverage → Identify key modules → Supplement tests → Reach target value → Continuous monitoring
```text

### Recommended Process

1. **Phase 1**: Cover core business logic (target 60-70%)
2. **Phase 2**: Supplement boundary condition tests (target 70-80%)
3. **Phase 3**: Handle exception scenarios (target 80-85%)
4. **Maintenance Phase**: New features must have tests

### 2. Priority Ranking

### High Priority (Must Test)

- Business logic core
- Financial calculations
- Data validation
- Permission control
- Exception handling

### Medium Priority (Recommended to Test)

- Data transformation
- Formatting logic
- Query logic

### Low Priority (Optional Testing)

- Simple getters/setters
- DTO classes
- Auto-generated code

### 3. Exclude Unnecessary Code

Exclude in code or runsettings:

```csharp
// Use attribute to exclude
[ExcludeFromCodeCoverage]
public class GeneratedCode
{
    // ...
}
```text

## Practical Recommendations and Best Practices

### Test Case Quantity Decision

1. **Based on Requirement Analysis**
   - List method use cases
   - Identify boundary conditions and exception cases
   - Consider various scenarios of business logic

2. **Reference Complexity Metrics**
   - Cyclomatic complexity provides test case lower bound
   - High complexity methods need more tests
   - Consider refactoring to reduce complexity

3. **Balance Coverage and Quality**
   - Don't use 100% coverage as sole target
   - Focus on critical business logic
   - Ensure actual value of tests

### Testing Strategy

### Four Test Types

1. **Boundary Testing**: Test upper and lower limits of input values
2. **Exception Testing**: Verify error handling logic
3. **Main Flow Testing**: Cover normal business processes
4. **Condition Branch Testing**: Ensure all branches are tested

### Continuous Improvement

1. **Regular Review Reports**
   - Check coverage changes before each commit
   - Review coverage during Pull Request

2. **Identify Risk Areas**
   - Focus on uncovered critical code
   - Prioritize high complexity untested areas

3. **Gradual Improvement**
   - Gradually improve test coverage of important modules
   - New features must include tests


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
