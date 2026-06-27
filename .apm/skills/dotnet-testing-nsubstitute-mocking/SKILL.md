---
name: dotnet-testing-nsubstitute-mocking
category: testing
subcategory: mocking
description: |
  Using NSubstitute to create test doubles (Mock, Stub, Spy) specialized skill. Used when isolating external dependencies, simulating interface behavior, and validating method calls. Covers Substitute.For, Returns, Received, Throws and complete guidance.
  Keywords: mock, stub, spy, nsubstitute, mock, test double, test double, IRepository, IService, Substitute.For, Returns, Received, Throws, Arg.Any, Arg.Is, isolate dependencies, simulate external services, dependency injection testing
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, NSubstitute, mock, stub, test double'
  related_skills: 'autofixture-nsubstitute-integration, unit-test-fundamentals, private-internal-testing'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-nsubstitute-mocking'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# NSubstitute Test Double Guide

## Applicable Scenarios

This skill focuses on creating and managing test doubles using NSubstitute, covering the five types of Test Doubles,
dependency isolation strategies, behavior setup and validation best practices.

## Why Do We Need Test Doubles?

Real-world code usually depends on external resources, which make tests:

1. **Slow** - Need actual database operations, file systems, networks
2. **Unstable** - External service failures cause test failures
3. **Difficult to Repeat** - Time, random numbers cause inconsistent results
4. **Environment Dependent** - Need specific external environment setup
5. **Development Blocking** - Must wait for external systems to be ready

Test doubles allow us to isolate these dependencies and focus on testing business logic.

## Prerequisites

### Package Installation

````xml
<PackageReference Include="NSubstitute" Version="5.3.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="AwesomeAssertions" Version="9.1.0" />
```text

### Basic using Directives

```csharp
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
```text

## Test Double Five Types

According to Gerard Meszaros's definition in "xUnit Test Patterns", test doubles are divided into five types:

| Type | Purpose | NSubstitute Equivalent |
|------|------|-------------------|
| **Dummy** | Fill objects, only satisfy method signatures | `Substitute.For<T>()` without setting any behavior |
| **Stub** | Provide default return values, set test scenarios | `.Returns(value)` |
| **Fake** | Simplified implementation with real logic | Manual interface implementation (like `FakeUserRepository`) |
| **Spy** | Record calls, verify afterwards | `.Received()` validation |
| **Mock** | Default expected interactions, test fails if not satisfied | `.Received(n)` strict validation |

> Full code examples for each type please refer to [references/test-double-types.md](references/test-double-types.md)

## NSubstitute Core Functions

### Basic Substitution Syntax

```csharp
// Create interface substitute
var substitute = Substitute.For<IUserRepository>();

// Create class substitute (needs virtual members)
var classSubstitute = Substitute.For<BaseService>();

// Create multiple interface substitute
var multiSubstitute = Substitute.For<IService, IDisposable>();
```text

### Return Value Setup

#### Basic Return Values

```csharp
// Exact parameter matching
_repository.GetById(1).Returns(new User { Id = 1, Name = "John" });

// Any parameter matching
_service.Process(Arg.Any<string>()).Returns("processed");

// Return sequence values
_generator.GetNext().Returns(1, 2, 3, 4, 5);
```text

#### Conditional Return Values

```csharp
// Use delegate to calculate return value
_calculator.Add(Arg.Any<int>(), Arg.Any<int>())
           .Returns(x => (int)x[0] + (int)x[1]);

// Condition matching
_service.Process(Arg.Is<string>(x => x.StartsWith("test")))
        .Returns("test-result");
```text

#### Throw Exceptions

```csharp
// Synchronous method throws exception
_service.RiskyOperation()
        .Throws(new InvalidOperationException("Something went wrong"));

// Async method throws exception
_service.RiskyOperationAsync()
        .Throws(new InvalidOperationException("Async operation failed"));
```text

### Argument Matchers

```csharp
// Any value
_service.Process(Arg.Any<string>()).Returns("result");

// Specific condition
_service.Process(Arg.Is<string>(x => x.Length > 5)).Returns("long-result");

// Argument capture
string capturedArg = null;
_service.Process(Arg.Do<string>(x => capturedArg = x)).Returns("result");
_service.Process("test");
capturedArg.Should().Be("test");

// Argument check
_service.Process(Arg.Is<string>(x =>
{
    x.Should().StartWith("prefix");
    return true;
})).Returns("result");
```text

### Call Verification

```csharp
// Verify was called (at least once)
_service.Received().Process("test");

// Verify call count
_service.Received(2).Process(Arg.Any<string>());

// Verify was not called
_service.DidNotReceive().Delete(Arg.Any<int>());

// Verify any argument call
_service.ReceivedWithAnyArgs().Process(default);

// Verify call order
Received.InOrder(() =>
{
    _service.Start();
    _service.Process();
    _service.Stop();
});
```text

## Practical Patterns

Covers five common NSubstitute practical patterns, including complete code examples:

| Pattern | Description |
|------|------|
| Pattern 1: Dependency Injection and Test Setup | FileBackupService complete example, including constructor injection and SUT setup |
| Pattern 2: Mock vs Stub Differences | Stub focuses on state return values vs Mock focuses on interaction behavior validation |
| Pattern 3: Async Method Testing | `Returns(Task.FromResult(...))` and `.Throws()` patterns |
| Pattern 4: ILogger Validation | Validate underlying `Log` method bypassing extension method limitations |
| Pattern 5: Complex Setup Management | Base test class managing shared Substitute setups |

> Full code examples please refer to [references/practical-patterns.md](references/practical-patterns.md)

## Advanced Argument Matching Techniques

### Complex Object Matching

```csharp
[Fact]
public void CreateOrder_CreateOrder_ShouldStoreCorrectOrderData()
{
    var repository = Substitute.For<IOrderRepository>();
    var service = new OrderService(repository);

    service.CreateOrder("Product A", 5, 100);

    // Verify object properties
    repository.Received(1).Save(Arg.Is<Order>(o =>
        o.ProductName == "Product A" &&
        o.Quantity == 5 &&
        o.Price == 100));
}
```text

### Argument Capture and Validation

```csharp
[Fact]
public void RegisterUser_RegisterUser_ShouldGenerateCorrectHashPassword()
{
    var repository = Substitute.For<IUserRepository>();
    var service = new UserService(repository);

    User capturedUser = null;
    repository.Save(Arg.Do<User>(u => capturedUser = u));

    service.RegisterUser("john@example.com", "password123");

    capturedUser.Should().NotBeNull();
    capturedUser.Email.Should().Be("john@example.com");
    capturedUser.PasswordHash.Should().NotBe("password123"); // Should be hashed
    capturedUser.PasswordHash.Length.Should().BeGreaterThan(20);
}
```text

## Common Pitfalls and Best Practices

### Recommended Practices

1. **Target interfaces rather than implementations for Substitutes**

    ```csharp
    // Correct: target interface
    var repository = Substitute.For<IUserRepository>();

    // Wrong: target concrete class (unless has virtual members)
    var repository = Substitute.For<UserRepository>();
```text

2. **Use meaningful test data**

    ```csharp
    // Correct: clearly express intent
    var user = new User { Id = 123, Name = "John Doe", Email = "john@example.com" };

    // Wrong: meaningless data
    var user = new User { Id = 1, Name = "test", Email = "a@b.c" };
```text

3. **Avoid over-verification**

    ```csharp
    // Correct: only verify important behaviors
    _emailService.Received(1).SendWelcomeEmail(Arg.Any<string>());

    // Wrong: verify all internal implementation details
    _repository.Received(1).GetById(123);
    _repository.Received(1).Update(Arg.Any<User>());
    _validator.Received(1).Validate(Arg.Any<User>());
```text

4. **Clear distinction between Mock and Stub**

    ```csharp
    // Correct: Stub for setting scenarios, Mock for validating behaviors
    var stubRepository = Substitute.For<IUserRepository>(); // Stub
    var mockLogger = Substitute.For<ILogger>(); // Mock

    stubRepository.GetById(123).Returns(user);
    service.ProcessUser(123);
    mockLogger.Received(1).LogInformation(Arg.Any<string>());
```text

### Practices to Avoid

1. **Avoid simulating value types**

    ```csharp
    // Wrong: DateTime is value type
    var badDate = Substitute.For<DateTime>();

    // Correct: abstract time provider
    var dateTimeProvider = Substitute.For<IDateTimeProvider>();
    dateTimeProvider.Now.Returns(new DateTime(2024, 1, 1));
```text

2. **Avoid tight coupling between tests and implementations**

    ```csharp
    // Wrong: test implementation details
    _repository.Received(1).Query(Arg.Any<string>());
    _repository.Received(1).Filter(Arg.Any<Expression<Func<User, bool>>>());

    // Correct: test behavior results
    var users = service.GetActiveUsers();
    users.Should().HaveCount(2);
```text

3. **Avoid overly complex setups**

    ```csharp
    // Wrong: too many Substitutes (may violate SRP)
    var sub1 = Substitute.For<IService1>();
    var sub2 = Substitute.For<IService2>();
    var sub3 = Substitute.For<IService3>();
    var sub4 = Substitute.For<IService4>();

    // Correct: reconsider class responsibilities
    // Consider whether violating single responsibility principle, needs refactoring
```text

## Identifying Dependencies to Substitute

### Should Substitute

- External API calls (IHttpClient, IApiClient)
- Database operations (IRepository, IDbContext)
- File system operations (IFileSystem)
- Network communication (IEmailService, IMessageQueue)
- Time dependencies (IDateTimeProvider, TimeProvider)
- Random number generation (IRandom)
- Expensive calculations (IComplexCalculator)
- Logging services (ILogger<T>)

### Should Not Substitute

- Value objects (DateTime, string, int)
- Simple data transfer objects (DTO)
- Pure function tools (like AutoMapper's IMapper, consider using real instance)
- Framework core classes (unless explicitly needed)

## Troubleshooting

### Q1: How to test classes without interfaces?

**A:** Ensure members to be simulated are virtual:

```csharp
public class BaseService
{
    public virtual string GetData() => "real data";
}

var substitute = Substitute.For<BaseService>();
substitute.GetData().Returns("test data");
```text

### Q2: How to verify method call order?

**A:** Use Received.InOrder():

```csharp
Received.InOrder(() =>
{
    _service.Start();
    _service.Process();
    _service.Stop();
});
```text

### Q3: How to handle out parameters?

**A:** Use Returns() with delegate:

```csharp
_service.TryGetValue("key", out Arg.Any<string>())
        .Returns(x =>
        {
            x[1] = "value";
            return true;
        });
```text

### Q4: NSubstitute vs Moq which to choose?

**A:** NSubstitute advantages:

- More concise and intuitive syntax
- Gentle learning curve
- No privacy controversies
- Sufficient for most testing scenarios

Choose NSubstitute, unless:

- Project already uses Moq
- Need Moq-specific advanced features
- Team already familiar with Moq syntax

## Integration with Other Skills

This skill can be combined with the following skills:

- **unit-test-fundamentals**: Unit test fundamentals and 3A pattern
- **dependency-injection-testing**: Dependency injection testing strategies
- **test-naming-conventions**: Test naming conventions
- **test-output-logging**: ITestOutputHelper and ILogger integration
- **datetime-testing-timeprovider**: TimeProvider abstraction for time dependencies
- **filesystem-testing-abstractions**: File system dependency abstraction

## Template Files Reference

This skill provides the following template files:

- `templates/mock-patterns.cs`: Complete Mock/Stub/Spy pattern examples
- `templates/verification-examples.cs`: Behavior verification and argument matching examples
- `references/practical-patterns.md`: Five practical patterns with complete code
- `references/test-double-types.md`: Test Double five types detailed examples

## Reference Resources

### Original Articles

This skill content is extracted from "Old School Software Engineer's Testing Practice - 30 Day Challenge" series:

- **Day 07 - Dependency Substitution Entry: Using NSubstitute**
  - Ironman Article: https://ithelp.ithome.com.tw/articles/10374593
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day07

### NSubstitute Official

- [NSubstitute Official Website](https://nsubstitute.github.io/)
- [NSubstitute GitHub](https://github.com/nsubstitute/NSubstitute)
- [NSubstitute NuGet](https://www.nuget.org/packages/NSubstitute/)

### Test Double Theory

- [XUnit Test Patterns](http://xunitpatterns.com/Test%20Double.html)
- [Martin Fowler - Test Double](https://martinfowler.com/bliki/TestDouble.html)
````
