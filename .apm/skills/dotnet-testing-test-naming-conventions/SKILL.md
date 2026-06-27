---
name: dotnet-testing-test-naming-conventions
category: testing
subcategory: fundamentals
description: |
  Test naming conventions and best practices specialized skill. Used when naming test methods, improving test readability, and establishing naming standards. Covers three-part naming method, Chinese naming recommendations, test class naming, etc.
  Keywords: test naming, test naming, naming conventions, naming conventions, three-part naming, three-part naming, method_scenario_expected, method_scenario_expected, how to name tests, test readability, naming best practices, test report, test documentation
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, naming conventions, test naming, readability'
  related_skills: 'unit-test-fundamentals, test-output-logging, xunit-project-setup'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-test-naming-conventions'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# .NET Test Naming Conventions Guide

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Name test methods or test classes
- Review and improve existing test naming
- Ensure test report readability
- Establish team-consistent test naming standards

## Test Method Naming Conventions

### Standard Format

Use underscore-separated three-part naming:

````text
[MethodUnderTest]_[TestScenario/InputConditions]_[ExpectedBehavior/Result]
```text

### Section Explanations

| Section                  | Description                     | Examples                                       |
| --------------------- | ------------------------ | ------------------------------------------ |
| **MethodUnderTest**    | Name of the method being tested       | `Add`, `ProcessOrder`, `IsValidEmail`      |
| **TestScenario/InputConditions** | Describe test preconditions or input | `Input1And2`, `InputNull`, `InputValidOrder`     |
| **ExpectedBehavior/Result**     | Describe expected output or behavior     | `ShouldReturn3`, `ShouldThrowException`, `ShouldReturnTrue` |

## Naming Examples Comparison Table

### Good Naming vs Bad Naming

| Bad Naming | Good Naming                                         | Reason                       |
| ------------- | --------------------------------------------------- | -------------------------- |
| `TestAdd`     | `Add_WithInput1And2_ShouldReturn3`                              | Clearly describes test scenario and expected result |
| `Test1`       | `Add_WithNegativeAndPositiveNumbers_ShouldReturnCorrectResult`                 | Meaningful description               |
| `EmailTest`   | `IsValidEmail_WithValidEmail_ShouldReturnTrue`             | Complete three-part naming           |
| `OrderTest`   | `ProcessOrder_WithNullInput_ShouldThrowArgumentNullException` | Clear exception scenario             |

## Practical Examples

### Basic Calculation Tests

```csharp
// Normal path test
[Fact]
public void Add_WithInput1And2_ShouldReturn3()

// Boundary condition test
[Fact]
public void Add_WithInput0And0_ShouldReturn0()

// Negative number test
[Fact]
public void Add_WithNegativeAndPositiveNumbers_ShouldReturnCorrectResult()
```text

### Validation Logic Tests

```csharp
// Valid input test
[Fact]
public void IsValidEmail_WithValidEmail_ShouldReturnTrue()

// Invalid input - null
[Fact]
public void IsValidEmail_WithNullValue_ShouldReturnFalse()

// Invalid input - empty string
[Fact]
public void IsValidEmail_WithEmptyString_ShouldReturnFalse()

// Invalid input - wrong format
[Fact]
public void IsValidEmail_WithInvalidEmailFormat_ShouldReturnFalse()
```text

### Business Logic Tests

```csharp
// Process flow test
[Fact]
public void ProcessOrder_WithValidOrder_ShouldReturnProcessedOrder()

// Exception handling test
[Fact]
public void ProcessOrder_WithNullInput_ShouldThrowArgumentNullException()

// Formatting test
[Fact]
public void GetOrderNumber_WithValidOrder_ShouldReturnFormattedOrderNumber()
```text

### Calculation Logic Tests

```csharp
// Normal calculation
[Fact]
public void Calculate_WithInput100And10PercentDiscount_ShouldReturn90()

// Invalid input - negative number
[Fact]
public void Calculate_WithNegativePrice_ShouldThrowArgumentException()

// Boundary value test
[Fact]
public void Calculate_WithInput0Price_ShouldHandleNormally()

// Tax calculation
[Fact]
public void CalculateWithTax_WithInput100And5PercentTax_ShouldReturn105()
```text

### State Change Tests

```csharp
// Initial state test
[Fact]
public void Increment_StartingFrom0_ShouldReturn1()

// Continuous operation test
[Fact]
public void Increment_StartingFrom0Twice_ShouldReturn2()

// Reset test
[Fact]
public void Reset_FromAnyValue_ShouldReturn0()
```text

## Test Class Naming Conventions

### Standard Format (continued)

```text
[ClassUnderTest]Tests
```text

### Examples

| Class Under Test        | Test Class Name           |
| ----------------- | ---------------------- |
| `Calculator`      | `CalculatorTests`      |
| `OrderService`    | `OrderServiceTests`    |
| `EmailHelper`     | `EmailHelperTests`     |
| `PriceCalculator` | `PriceCalculatorTests` |

### Class Structure Template

```csharp
namespace MyProject.Tests;

/// <summary>
/// class CalculatorTests - Calculator test class
/// </summary>
public class CalculatorTests
{
    private readonly Calculator _calculator;

    public CalculatorTests()
    {
        _calculator = new Calculator();
    }

    //---------------------------------------------------------------------------------------------
    // Add method tests

    [Fact]
    public void Add_WithInput1And2_ShouldReturn3()
    {
        // ...
    }

    //---------------------------------------------------------------------------------------------
    // Divide method tests

    [Fact]
    public void Divide_WithInput10And2_ShouldReturn5()
    {
        // ...
    }
}
```text

## Parameterized Test Naming

Naming conventions when using `[Theory]`:

```csharp
// Use "Various" to indicate multiple test data
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
public void Add_WithVariousNumberCombinations_ShouldReturnCorrectResult(int a, int b, int expected)

// Use "Valid" to indicate positive tests
[Theory]
[InlineData("test@example.com")]
[InlineData("user.name@domain.org")]
public void IsValidEmail_WithValidEmailFormats_ShouldReturnTrue(string validEmail)

// Use "Invalid" to indicate negative tests
[Theory]
[InlineData("invalid-email")]
[InlineData("@example.com")]
public void IsValidEmail_WithInvalidEmailFormats_ShouldReturnFalse(string invalidEmail)
```text

## Common Scenario Vocabulary

### Input Condition Vocabulary

| Vocabulary        | Usage Scenario             |
| ----------- | -------------------- |
| `With`      | General input parameters         |
| `Given`      | Given-When-Then style |
| `When`        | Event trigger             |
| `StartingFrom` | Initial state description         |

### Expected Result Vocabulary

| Vocabulary         | Usage Scenario |
| ------------ | -------- |
| `ShouldReturn`     | Has return value |
| `ShouldThrow`     | Expected exception |
| `ShouldBe`       | State validation |
| `ShouldContain`     | Collection validation |
| `ShouldHandleNormally` | Boundary conditions |

## Naming Checklist

When naming test methods, please confirm:

- [ ] Use three-part naming `Method_Scenario_Expected`
- [ ] Scenario description is clear and explicit
- [ ] Expected result is specific and verifiable
- [ ] Use Chinese to increase readability
- [ ] Avoid vague vocabulary like `Test1`, `TestMethod`
- [ ] Parameterized tests use vocabulary like "Various", "Valid", "Invalid"

## Test Report Readability

Good naming makes test reports more readable:

```text
CalculatorTests
   Add_WithInput1And2_ShouldReturn3
   Add_WithNegativeAndPositiveNumbers_ShouldReturnCorrectResult
   Divide_WithInput10And0_ShouldThrowDivideByZeroException

EmailHelperTests
   IsValidEmail_WithValidEmail_ShouldReturnTrue
   IsValidEmail_WithNullValue_ShouldReturnFalse
```text

## Reference Resources

Please refer to example files in the same directory:

- [templates/naming-convention-examples.cs](templates/naming-convention-examples.cs) - Complete naming convention examples

### Original Articles

This skill content is extracted from "Old School Software Engineer's Testing Practice - 30 Day Challenge" series:

- **Day 01 - Old School Engineer's Testing Enlightenment**
  - Ironman Article: https://ithelp.ithome.com.tw/articles/10373888
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day01
````
