---
name: dotnet-testing-fluentvalidation-testing
category: testing
subcategory: fundamentals
description: |
  Specialized skill for testing FluentValidation validators. Use when you need to create tests for Validator classes, validate business rules, or test error messages. Covers complete FluentValidation.TestHelper usage, ShouldHaveValidationErrorFor, async validation, cross-field logic, etc.
  Keywords: validator, validation, fluentvalidation, validation testing, UserValidator, CreateOrderValidator, TestHelper, ShouldHaveValidationErrorFor, ShouldNotHaveValidationErrorFor, TestValidate, TestValidateAsync, testing validators, validating business rules
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, FluentValidation, validator, validation'
  related_skills: 'awesome-assertions-guide, nsubstitute-mocking, unit-test-fundamentals'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-fluentvalidation-testing'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# FluentValidation Validator Testing Guide

## Applicable Scenarios

This skill focuses on testing data validation logic using FluentValidation.TestHelper, covering basic validation,
complex business rules, async validation, and testing best practices.

## Why Test Validators?

Validators are the first line of defense for applications, testing validators can:

1. **Ensure Data Integrity** - Prevent invalid data from entering the system
2. **Document Business Rules** - Tests serve as living documentation, clearly showing business rules
3. **Security Protection** - Prevent malicious or inappropriate data input
4. **Refactoring Safety Net** - Provide protection when business rules change
5. **Cross-Field Logic Validation** - Ensure complex logic works correctly

## Prerequisites

### Package Installation

```xml
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.TestHelper" Version="11.11.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="Microsoft.Extensions.Time.Testing" Version="9.0.0" />
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="AwesomeAssertions" Version="9.1.0" />
```

### Basic using Directives

```csharp
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;
using AwesomeAssertions;
```

## Core Testing Patterns

This section covers 7 core testing patterns, each including validator definitions and complete test examples.

> For complete code examples, see [references/core-test-patterns.md](references/core-test-patterns.md)

- **Pattern 1: Basic Field Validation** — Using `TestValidate` + `ShouldHaveValidationErrorFor` /
  `ShouldNotHaveValidationErrorFor` to test single field rules
- **Pattern 2: Parameterized Tests** — Using `[Theory]` + `[InlineData]` to test multiple invalid/valid input
  combinations
- **Pattern 3: Cross-Field Validation** — Password confirmation, custom `Must()` rules, and other multi-field related
  validations
- **Pattern 4: Time-Dependent Validation** — Injecting `TimeProvider`, using `FakeTimeProvider` to control time for
  testing
- **Pattern 5: Conditional Validation** — Using `.When()` for optional field validation, testing conditional trigger and
  skip scenarios
- **Pattern 6: Async Validation** — `MustAsync` + `TestValidateAsync`, using NSubstitute Mock for external services
- **Pattern 7: Collection Validation** — Validating collections non-empty and element validity

### Quick Example: Basic Field Validation

```csharp
public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact]
    public void Validate_Blank_Username_Should_Validation_Fail()
    {
        var result = _validator.TestValidate(
            new UserRegistrationRequest { Username = "" });

        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username cannot be null or blank");
    }
}
```

## FluentValidation.TestHelper Core API

### Test Methods

| Method                     | Purpose                  | Example                                       |
| -------------------------- | ------------------------ | --------------------------------------------- |
| `TestValidate(model)`      | Execute sync validation  | `_validator.TestValidate(request)`            |
| `TestValidateAsync(model)` | Execute async validation | `await _validator.TestValidateAsync(request)` |

### Assertion Methods

| Method                                             | Purpose                               | Example                                                |
| -------------------------------------------------- | ------------------------------------- | ------------------------------------------------------ |
| `ShouldHaveValidationErrorFor(x => x.Property)`    | Assert property should have error     | `result.ShouldHaveValidationErrorFor(x => x.Username)` |
| `ShouldNotHaveValidationErrorFor(x => x.Property)` | Assert property should not have error | `result.ShouldNotHaveValidationErrorFor(x => x.Email)` |
| `ShouldNotHaveAnyValidationErrors()`               | Assert entire object has no errors    | `result.ShouldNotHaveAnyValidationErrors()`            |

### Error Message Validation

| Method                     | Purpose                        | Example                                         |
| -------------------------- | ------------------------------ | ----------------------------------------------- |
| `WithErrorMessage(string)` | Validate error message content | `.WithErrorMessage("Username cannot be empty")` |
| `WithErrorCode(string)`    | Validate error code            | `.WithErrorCode("NOT_EMPTY")`                   |

## Testing Best Practices

### ✅ Recommended Practices

1. **Use Parameterized Tests** - Use Theory to test multiple input combinations
2. **Test Boundary Values** - Pay special attention to boundary conditions
3. **Control Time** - Use FakeTimeProvider for time-dependent scenarios
4. **Mock External Dependencies** - Use NSubstitute to isolate external services
5. **Create Helper Methods** - Uniformly manage test data
6. **Clear Test Naming** - Use `Method_Scenario_ExpectedResult` format
7. **Test Error Messages** - Ensure users see correct error messages

### ❌ Practices to Avoid

1. **Avoid Using DateTime.Now** - Causes unstable tests
2. **Avoid Overly Coupled Tests** - Each test should only validate one rule
3. **Avoid Hardcoded Test Data** - Use helper methods to create
4. **Avoid Ignoring Boundary Conditions** - Boundary values are where errors most easily occur
5. **Avoid Skipping Error Message Validation** - Error messages are part of user experience

## Common Testing Scenarios

### Scenario 1: Email Format Validation

```csharp
[Theory]
[InlineData("", "Email cannot be null or blank")]
[InlineData("invalid", "Email format is incorrect")]
[InlineData("@example.com", "Email format is incorrect")]
public void Validate_Invalid_Email_Should_Validation_Fail(string email, string expectedError)
{
    var request = new UserRegistrationRequest { Email = email };
    var result = _validator.TestValidate(request);
    result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(expectedError);
}
```

### Scenario 2: Age Range Validation

```csharp
[Theory]
[InlineData(17, "Age must be greater than or equal to 18")]
[InlineData(121, "Age must be less than or equal to 120")]
public void Validate_Invalid_Age_Should_Validation_Fail(int age, string expectedError)
{
    var request = new UserRegistrationRequest { Age = age };
    var result = _validator.TestValidate(request);
    result.ShouldHaveValidationErrorFor(x => x.Age).WithErrorMessage(expectedError);
}
```

### Scenario 3: Required Field Validation

```csharp
[Fact]
public void Validate_Not_Agree_To_Terms_Should_Validation_Fail()
{
    var request = new UserRegistrationRequest { AgreeToTerms = false };
    var result = _validator.TestValidate(request);
    result.ShouldHaveValidationErrorFor(x => x.AgreeToTerms)
          .WithErrorMessage("Must agree to terms of use");
}
```

## Testing Helper Tools

### Test Data Builder

```csharp
public static class TestDataBuilder
{
    public static UserRegistrationRequest CreateValidRequest()
    {
         return new UserRegistrationRequest
         {
             Username = "testuser123",
             Email = "test@example.com",
             Password = "<DB_PASSWORD_PLACEHOLDER>",
             ConfirmPassword = "<DB_PASSWORD_PLACEHOLDER>",
             BirthDate = new DateTime(1990, 1, 1),
             Age = 34,
             PhoneNumber = "0912345678",
             Roles = new List<string> { "User" },
            AgreeToTerms = true
        };
    }

    public static UserRegistrationRequest WithUsername(this UserRegistrationRequest request, string username)
    {
        request.Username = username;
        return request;
    }

    public static UserRegistrationRequest WithEmail(this UserRegistrationRequest request, string email)
    {
        request.Email = email;
        return request;
    }
}

// Usage example
var request = TestDataBuilder.CreateValidRequest()
                            .WithUsername("newuser")
                            .WithEmail("new@example.com");
```

## Integration with Other Skills

This skill can be combined with:

- **unit-test-fundamentals**: Unit testing basics and 3A pattern
- **test-naming-conventions**: Test naming conventions
- **nsubstitute-mocking**: Mocking external service dependencies
- **test-data-builder-pattern**: Building complex test data
- **datetime-testing-timeprovider**: Time-dependent testing

## Troubleshooting

### Q1: How to Test Validators Requiring Database Queries?

**A:** Use Mock to isolate database dependencies:

```csharp
_mockUserService.IsUsernameAvailableAsync("username")
                .Returns(Task.FromResult(false));
```

### Q2: How to Handle Time-Related Validation?

**A:** Use FakeTimeProvider to control time:

```csharp
_fakeTimeProvider.SetUtcNow(new DateTime(2024, 1, 1));
```

### Q3: How to Test Complex Cross-Field Validation?

**A:** Test each condition separately, ensuring complete coverage:

```csharp
// Test birthday passed scenario
// Test birthday not yet arrived scenario
// Test boundary date
```

### Q4: How Much Should Be Tested?

**A:** Focus on testing:

- At least one test for each validation rule
- Boundary values and special cases
- Error message correctness
- All combinations of cross-field logic

## Template Files Reference

This skill provides the following template files:

- `templates/validator-test-template.cs`: Complete validator test example
- `templates/async-validator-examples.cs`: Async validation examples

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article
series:

- **Day 18 - Validation Testing: FluentValidation Test Extensions**
  - Article: https://ithelp.ithome.com.tw/articles/10376147
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day18

### Official Documentation

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [FluentValidation.TestHelper](https://docs.fluentvalidation.net/en/latest/testing.html)
- [FluentValidation GitHub](https://github.com/FluentValidation/FluentValidation)

### Related Skills

- `unit-test-fundamentals` - Unit testing basics
- `nsubstitute-mocking` - Test doubles and mocking
