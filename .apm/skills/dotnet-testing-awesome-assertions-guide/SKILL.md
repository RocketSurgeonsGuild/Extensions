---
name: dotnet-testing-awesome-assertions-guide
category: testing
subcategory: assertions
description: |
  Using AwesomeAssertions for fluent and readable test assertion skill. Used when writing clear assertions, comparing objects, validating collections, handling complex comparisons. Covers Should(), BeEquivalentTo(), Contain(), ThrowAsync() and complete API.
  Keywords: assertions, awesome assertions, fluent assertions, assertion, fluent assertion, Should(), Be(), BeEquivalentTo, Contain, ThrowAsync, NotBeNull, object comparison, collection validation, exception assertion, AwesomeAssertions, FluentAssertions, fluent syntax
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, AwesomeAssertions, FluentAssertions, assertions'
  related_skills: 'complex-object-comparison, fluentvalidation-testing, unit-test-fundamentals'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-awesome-assertions-guide'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# AwesomeAssertions Fluent Assertion Guide

This skill provides a complete guide for writing high-quality test assertions using AwesomeAssertions, covering basic
syntax, advanced techniques, and best practices.

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Write clear, highly readable test assertions
- Compare complex objects or collection contents
- Verify exception throwing and messages
- Use fluent syntax (Should/Be/Contain) for test validation
- Replace native Assert with AwesomeAssertions

## About AwesomeAssertions

**AwesomeAssertions** is a community fork version of FluentAssertions, using **Apache 2.0** license, completely free
with no commercial usage restrictions.

### Core Features

- Fully Free: Apache 2.0 license, suitable for commercial projects
- Fluent Syntax: Supports natural language style method chaining
- Rich Assertions: Covers objects, collections, strings, numbers, exceptions, and various other types
- Excellent Error Messages: Provides detailed and easy-to-understand failure information
- High Performance: Optimized implementation ensures test execution efficiency
- Extensible: Supports custom Assertion methods

### Relationship with FluentAssertions

AwesomeAssertions is a community fork of FluentAssertions, main differences:

| Item                  | FluentAssertions                    | AwesomeAssertions            |
| --------------------- | ----------------------------------- | ---------------------------- |
| **License**           | Commercial projects require payment | Apache 2.0 (completely free) |
| **Namespace**         | `FluentAssertions`                  | `AwesomeAssertions`          |
| **API Compatibility** | Original                            | Highly compatible            |
| **Community Support** | Official maintenance                | Community maintenance        |

---

## Installation and Setup

### NuGet Package Installation

````bash
# .NET CLI
dotnet add package AwesomeAssertions

# Package Manager Console
Install-Package AwesomeAssertions
```text

### csproj Setup (Recommended)

```xml
<ItemGroup>
  <PackageReference Include="AwesomeAssertions" Version="9.1.0" PrivateAssets="all" />
</ItemGroup>
```text

### Namespace Import

```csharp
using AwesomeAssertions;
using Xunit;
```text

---

## Core Assertions Syntax

All Assertions start with `.Should()`, combined with fluent method chaining.

| Category | Common Methods | Description |
|------|----------|------|
| **Object** | `NotBeNull()`, `BeOfType<T>()`, `BeEquivalentTo()` | Null, type, equality checks |
| **String** | `Contain()`, `StartWith()`, `MatchRegex()`, `BeEquivalentTo()` | Content, patterns, case-insensitive comparison |
| **Number** | `BeGreaterThan()`, `BeInRange()`, `BeApproximately()` | Comparison, range, floating point precision |
| **Collection** | `HaveCount()`, `Contain()`, `BeEquivalentTo()`, `AllSatisfy()` | Count, content, order, conditions |
| **Exception** | `Throw<T>()`, `NotThrow()`, `WithMessage()`, `WithInnerException()` | Exception types, messages, nested exceptions |
| **Async** | `ThrowAsync<T>()`, `CompleteWithinAsync()` | Async exceptions and completion validation |

> Full syntax examples and code please refer to [references/core-assertions-syntax.md](references/core-assertions-syntax.md)

---

## Advanced Techniques: Complex Object Comparison

Use `BeEquivalentTo()` with `options` for deep object comparison:

- **Exclude properties**: `options.Excluding(u => u.Id)` — exclude auto-generated fields
- **Dynamic exclusion**: `options.Excluding(ctx => ctx.Path.EndsWith("At"))` — exclude by pattern
- **Circular references**: `options.IgnoringCyclicReferences().WithMaxRecursionDepth(10)`

---

## Advanced Techniques: Custom Assertions Extension

Create domain-specific extension methods, like `product.Should().BeValidProduct()`, and reusable exclusion extensions like `ExcludingAuditFields()`.

Refer to [templates/custom-assertions-template.cs](templates/custom-assertions-template.cs) for complete implementation.

> Full examples please refer to [references/complex-object-assertions.md](references/complex-object-assertions.md)

---

## Performance Optimization Strategies

- **Large data**: First use `HaveCount()` for quick count check, then sample validation (avoid full `BeEquivalentTo`)
- **Selective comparison**: Use anonymous objects + `ExcludingMissingMembers()` to only validate key properties

```csharp
// Selective property comparison — only validate key fields
order.Should().BeEquivalentTo(new
{
    CustomerId = 123,
    TotalAmount = 999.99m,
    Status = "Pending"
}, options => options.ExcludingMissingMembers());
```text

---

## Best Practices and Team Standards

### Test Naming Conventions

Follow `Method_Scenario_ExpectedResult` pattern (e.g., `CreateUser_WithValidEmail_ShouldReturnEnabledUser`).

### Error Message Optimization

Add `because` string in assertions to provide clear failure context:

```csharp
result.IsSuccess.Should().BeFalse("because negative payment amounts are not allowed");
```text

### AssertionScope Usage

Use `AssertionScope` to collect multiple failure messages, display all problems at once:

```csharp
using (new AssertionScope())
{
    user.Should().NotBeNull("User creation should not fail");
    user.Id.Should().BeGreaterThan(0, "User should have valid ID");
    user.Email.Should().NotBeNullOrEmpty("Email is required");
}
```text

---

## Common Scenarios and Solutions

| Scenario | Key Technique |
|------|----------|
| API response validation | `BeEquivalentTo()` + `Including()` selective comparison |
| Database entity validation | `BeEquivalentTo()` + `Excluding()` exclude auto-generated fields |
| Event validation | Subscribe to capture events then validate properties one by one |

> Full code examples please refer to [references/common-scenarios.md](references/common-scenarios.md)

---

## Troubleshooting

### Problem 1: BeEquivalentTo fails but objects look the same

**Reason**: May contain auto-generated fields or timestamps

**Solution**:

```csharp
// Exclude dynamic fields
actual.Should().BeEquivalentTo(expected, options => options
    .Excluding(x => x.Id)
    .Excluding(x => x.CreatedAt)
    .Excluding(x => x.UpdatedAt)
);
```text

### Problem 2: Collection order different causes failure

**Reason**: Collection order is different

**Solution**:

```csharp
// Use BeEquivalentTo ignore order
actual.Should().BeEquivalentTo(expected); // Does not check order

// Or explicitly specify need to check order
actual.Should().Equal(expected); // Checks order
```text

### Problem 3: Floating point comparison fails

**Reason**: Floating point precision issues

**Solution**:

```csharp
// Use precision tolerance
actualValue.Should().BeApproximately(expectedValue, 0.001);
```text

---

## When to Use This Skill

### Applicable Scenarios

Write unit tests or integration tests
Need to validate complex object structures
Compare API responses or database entities
Need clear failure messages
Establish domain-specific testing standards

### Not Applicable Scenarios

Performance testing (use dedicated benchmarking tools)
Load testing (use K6, JMeter, etc.)
UI testing (use Playwright, Selenium)

---

## Integration with Other Skills

### Integration with unit-test-fundamentals

First use `unit-test-fundamentals` to establish test structure, then use this skill to write assertions:

```csharp
[Fact]
public void Calculator_Add_TwoPositiveNumbers_ShouldReturnSum()
{
    // Arrange - follow 3A Pattern
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(2, 3);

    // Assert - use AwesomeAssertions
    result.Should().Be(5);
}
```text

### Integration with test-naming-conventions

Use `test-naming-conventions` naming conventions, combined with this skill's assertions:

```csharp
[Fact]
public void CreateUser_WithValidData_ShouldReturnEnabledUser()
{
    var user = userService.CreateUser("test@example.com");

    user.Should().NotBeNull()
        .And.BeOfType<User>();
    user.IsActive.Should().BeTrue();
}
```text

### Integration with xunit-project-setup

Install and use AwesomeAssertions in projects created with `xunit-project-setup`.

---

## Reference Resources

### Original Articles

This skill content is extracted from "Old School Software Engineer's Testing Practice - 30 Day Challenge" series:

- **Day 04 - AwesomeAssertions Basic Application and Practical Techniques**
  - Ironman Article: https://ithelp.ithome.com.tw/articles/10374188
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day04

- **Day 05 - AwesomeAssertions Advanced Techniques and Complex Scenario Applications**
  - Ironman Article: https://ithelp.ithome.com.tw/articles/10374425
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day05

### Official Resources

- **AwesomeAssertions GitHub**: https://github.com/AwesomeAssertions/AwesomeAssertions
- **AwesomeAssertions Official Documentation**: https://awesomeassertions.org/

### Related Articles

- **Fluent Assertions License Change Discussion**: https://www.dotblogs.com.tw/mrkt/2025/04/19/152408

---

## Summary

AwesomeAssertions provides powerful and readable assertion syntax, an important tool for writing high-quality tests. Through:

1. **Fluent Syntax**: Makes test code more readable
2. **Rich Assertions**: Covers various data types
3. **Custom Extensions**: Establish domain-specific assertions
4. **Performance Optimization**: Handle large data scenarios
5. **Completely Free**: Apache 2.0 license with no commercial restrictions

Remember: Good assertions not only validate results but clearly express expected behavior and provide useful diagnostic information when failing.

Refer to [templates/assertion-examples.cs](templates/assertion-examples.cs) for more practical examples.
````
