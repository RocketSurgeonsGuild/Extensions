---
name: dotnet-testing-private-internal-testing
category: testing
subcategory: specialized
description: |
  Guide for Private and Internal member testing strategies. Use when you need to test private or internal members, configure InternalsVisibleTo, or evaluate testability design. Covers design-first thinking, reflection testing, strategy pattern refactoring, AbstractLogger pattern, and decision frameworks.
  Keywords: private method testing, internal testing, InternalsVisibleTo, reflection testing, GetMethod BindingFlags, Meziantou.MSBuild.InternalsVisibleTo, testability design, strategy pattern refactoring, testability
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: 'private-testing, internal-testing, InternalsVisibleTo, reflection, testability, design'
  related_skills: 'nsubstitute-mocking, unit-test-fundamentals, test-naming-conventions'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-private-internal-testing'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Private and Internal Member Testing Strategy Guide

This skill helps you properly handle testing of private and internal members in .NET testing, emphasizing design-first
testing thinking.

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Test private or internal methods and properties
- Configure InternalsVisibleTo to access internal members
- Evaluate whether to test private methods or refactor design
- Use Reflection to access private members
- Improve code testability design

## Core Principles: Design-First Thinking

### Golden Rule

### Good design naturally has good testability. If you find yourself frequently needing to test private methods, the design is likely problematic

### Signs of Design Problems

When you want to test private methods, first check for these signs:

- ❌ Private methods over 10 lines with complex logic
- ❌ Private methods contain important business rules
- ❌ Private methods difficult to test indirectly through public methods
- ❌ Class has multiple responsibilities

### Solution: Refactor Rather Than Test

````csharp
// ❌ Problematic design
public class OrderProcessor
{
    public OrderResult ProcessOrder(Order order)
    {
        // Uses multiple complex private methods
        var discount = CalculateDiscount(order); // 20 lines logic
        var tax = CalculateTax(order, discount);  // 15 lines logic
        // ...
    }

    private decimal CalculateDiscount(Order order) { /* complex logic */ }
    private decimal CalculateTax(Order order, decimal discount) { /* complex logic */ }
}

// ✅ Improved design: Separation of concerns
public class OrderProcessor
{
    private readonly IDiscountCalculator _discountCalculator;
    private readonly ITaxCalculator _taxCalculator;

    public OrderProcessor(
        IDiscountCalculator discountCalculator,
        ITaxCalculator taxCalculator)
    {
        _discountCalculator = discountCalculator;
        _taxCalculator = taxCalculator;
    }

    public OrderResult ProcessOrder(Order order)
    {
        var discount = _discountCalculator.Calculate(order);
        var tax = _taxCalculator.Calculate(order, discount);
        // ...
    }
}

// Now each calculator can be tested independently
public class DiscountCalculator : IDiscountCalculator
{
    public decimal Calculate(Order order)
    {
        // Complex logic is now public method, easy to test
    }
}
```text

## Internal Member Testing Strategy

### When to Test Internal Members

### Appropriate Scenarios

- ✅ Framework or class library development
- ✅ Complex internal algorithm validation
- ✅ Performance-critical internal components
- ✅ Security-related internal logic

### Inappropriate Scenarios

- ❌ Application layer business logic (should be public)
- ❌ Simple helper methods
- ❌ Logic that can be tested indirectly through public API

### Method 1: Using InternalsVisibleTo Attribute

Most direct method, suitable for simple cases:

```csharp
// In main project AssemblyInfo.cs or any class file
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YourProject.Tests")]
[assembly: InternalsVisibleTo("YourProject.IntegrationTests")]
```text

### Pros

- Simple and direct
- No additional packages needed

### Cons

- Requires hardcoded assembly names
- For signed assemblies, need to include public key

### Method 2: Configuring in csproj

Configure via MSBuild properties:

```xml
<!-- YourProject.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>$(AssemblyName).Tests</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>
</Project>
```text

### Pros: (continued)

- Can use MSBuild variables
- Centralized management

### Method 3: Using Meziantou.MSBuild.InternalsVisibleTo (Recommended)

For complex projects, recommend using this NuGet package:

```xml
<!-- YourProject.csproj -->
<ItemGroup>
  <PackageReference Include="Meziantou.MSBuild.InternalsVisibleTo" Version="1.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>

<ItemGroup>
  <InternalsVisibleTo Include="$(AssemblyName).Tests" />
  <InternalsVisibleTo Include="$(AssemblyName).IntegrationTests" />
  <InternalsVisibleTo Include="DynamicProxyGenAssembly2" Key="0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7" />
</ItemGroup>
```text

### Pros: (continued)

- Automatically handles public keys for signed assemblies
- Supports DynamicProxyGenAssembly2 (NSubstitute/Moq)
- High readability

### Reference Resources

- [Declaring InternalsVisibleTo in the csproj - Meziantou's blog](https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm)
- [GitHub - meziantou/Meziantou.MSBuild.InternalsVisibleTo](https://github.com/meziantou/Meziantou.MSBuild.InternalsVisibleTo)

### Internal Testing Risk Assessment

| Assessment Aspect | Risk Level | Description                              |
| :---------------- | :--------- | :--------------------------------------- |
| Encapsulation Break | Medium     | Increases test dependency on internal implementation |
| Refactoring Resistance | High    | Changing internal members affects tests  |
| Maintenance Cost | Medium      | Need to maintain both production and test code |
| Design Quality | Low         | If overused, may indicate design problems |

## Private Method Testing Techniques

Covers decision tree (whether to test private methods), reflection testing of private instance and static methods, `ReflectionTestHelper` helper class encapsulation, and risks and best practices of reflection testing.

> For complete code examples and technical details, see [references/private-method-testing.md](references/private-method-testing.md)

## Test-Friendly Design Patterns

### Strategy Pattern to Improve Testability

Refactor complex private logic into strategy pattern:

#### Before Refactoring: Hard to Test Design

```csharp
public class PricingService
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = CalculateDiscount(customer, product); // private method
        var tax = CalculateTax(product, customer.Location);   // private method
        return basePrice - discount + tax;
    }

    private decimal CalculateDiscount(Customer customer, Product product)
    {
        // 20 lines complex discount calculation logic
    }

    private decimal CalculateTax(Product product, Location location)
    {
        // 15 lines complex tax calculation
    }
}
```text

#### After Refactoring: Using Strategy Pattern

```csharp
// Strategy interface
public interface IDiscountStrategy
{
    decimal Calculate(Customer customer, Product product);
}

public interface ITaxStrategy
{
    decimal Calculate(Product product, Location location);
}

// Concrete strategy implementations
public class StandardDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Customer customer, Product product)
    {
        // Discount logic is now public method, easy to test
        if (customer.IsVIP)
            return product.BasePrice * 0.1m;

        return 0;
    }
}

public class TaiwanTaxStrategy : ITaxStrategy
{
    public decimal Calculate(Product product, Location location)
    {
        // Tax logic is now public method, easy to test
        return product.BasePrice * 0.05m;
    }
}

// Improved service
public class PricingService
{
    private readonly IDiscountStrategy _discountStrategy;
    private readonly ITaxStrategy _taxStrategy;

    public PricingService(
        IDiscountStrategy discountStrategy,
        ITaxStrategy taxStrategy)
    {
        _discountStrategy = discountStrategy;
        _taxStrategy = taxStrategy;
    }

    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = _discountStrategy.Calculate(customer, product);
        var tax = _taxStrategy.Calculate(product, customer.Location);
        return basePrice - discount + tax;
    }
}
```text

### Pros: (continued)

- Each strategy can be tested independently
- Follows Open/Closed Principle
- Easy to extend new strategies
- Reduces dependency on reflection

### Partial Mock

Sometimes need to mock partial behavior of a class:

```csharp
// Class needing partial mock
public class DataProcessor
{
    public ProcessResult Process(string input)
    {
        var validated = ValidateInput(input);
        if (!validated)
            return ProcessResult.InvalidInput();

        var data = TransformData(input);
        var saved = SaveData(data); // Want to mock this method to avoid actual database operations

        return saved
            ? ProcessResult.Success()
            : ProcessResult.Failed();
    }

    protected virtual bool SaveData(string data)
    {
        // Actual database operation
        return true;
    }

    private bool ValidateInput(string input) => !string.IsNullOrEmpty(input);
    private string TransformData(string input) => input.ToUpper();
}

// Test subclass
public class TestableDataProcessor : DataProcessor
{
    protected override bool SaveData(string data)
    {
        // Mock implementation, avoid actual database operations
        return true;
    }
}

// Test
[Fact]
public void Process_Using_Partial_Mock_Should_Process_Successfully()
{
    // Arrange
    var processor = new TestableDataProcessor();

    // Act
    var result = processor.Process("test");

    // Assert
    result.Success.Should().BeTrue();
}
```text

## Practical Decision Framework

### Three-Level Risk Assessment

#### Level 1: Design Quality Assessment

### Question: Is this a design problem or a testing problem?

- Are private methods too complex? (> 10 lines)
- Does the class have multiple responsibilities?
- Can it be extracted as an independent class?

### Recommended Action

- Prioritize refactoring (extract class, strategy pattern)
- Improve testability through improved design

#### Level 2: Maintenance Cost Assessment

### Question: Will testing become a hindrance to refactoring?

- Does the test depend on implementation details?
- Will the test need significant modification when refactoring?
- Is it difficult to locate problems when tests fail?

### Recommended Action: (continued)

- If maintenance cost is high, reconsider testing strategy
- Consider integration testing through public API

#### Level 3: Value Output Assessment

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
