---
name: dotnet-testing-complex-object-comparison
category: testing
subcategory: assertions
description: |
  Specialized skill for handling complex object comparison and deep validation. Use when you need to compare deep objects, exclude specific properties, handle circular references, or validate DTOs/Entities. Covers BeEquivalentTo, Excluding, Including, custom comparison rules, etc.
  Keywords: object comparison, deep comparison, BeEquivalentTo, DTO comparison, Entity validation, excluding properties, circular reference, Excluding, Including, ExcludingNestedObjects, RespectingRuntimeTypes, WithStrictOrdering, ignore timestamp, exclude timestamp
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, object comparison, BeEquivalentTo, AwesomeAssertions'
  related_skills: 'awesome-assertions-guide, autofixture-basics, test-data-builder-pattern'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-complex-object-comparison'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# Complex Object Comparison Guide

## Applicable Scenarios

This skill focuses on complex object comparison scenarios in .NET testing, using AwesomeAssertions' `BeEquivalentTo` API
to handle various advanced comparison needs.

## Core Usage Scenarios

### 1. Deep Object Structure Comparison (Object Graph Comparison)

When comparing complex objects containing multi-layer nested properties:

````csharp
[Fact]
public void ComplexObject_Deep_Structure_Comparison_Should_Match()
{
    var expected = new Order
    {
        Id = 1,
        Customer = new Customer
        {
            Name = "John Doe",
            Address = new Address
            {
                Street = "123 Main St",
                City = "Seattle",
                ZipCode = "98101"
            }
        },
        Items = new[]
        {
            new OrderItem { ProductName = "Laptop", Quantity = 1, Price = 999.99m },
            new OrderItem { ProductName = "Mouse", Quantity = 2, Price = 29.99m }
        }
    };

    var actual = orderService.GetOrder(1);

    // Deep object comparison
    actual.Should().BeEquivalentTo(expected);
}
```text

### 2. Circular Reference Handling (Circular Reference Handling)

Handling cases where objects have circular references:

```csharp
[Fact]
public void TreeStructure_Circular_Reference_Should_Handle_Correctly()
{
    // Create tree structure with parent-child bidirectional references
    var parent = new TreeNode { Value = "Root" };
    var child1 = new TreeNode { Value = "Child1", Parent = parent };
    var child2 = new TreeNode { Value = "Child2", Parent = parent };
    parent.Children = new[] { child1, child2 };

    var actualTree = treeService.GetTree("Root");

    // Handle circular references
    actualTree.Should().BeEquivalentTo(parent, options =>
        options.IgnoringCyclicReferences()
               .WithMaxRecursionDepth(10)
    );
}
```text

### 3-6. Advanced Comparison Patterns

FluentAssertions provides various advanced comparison patterns: dynamic field exclusion (excluding timestamps, auto-generated fields), nested object field exclusion, performance-optimized comparison for large data (selective property comparison, sampling validation strategies), and strict/loose ordering control.

> For complete code examples, see [references/detailed-comparison-patterns.md](references/detailed-comparison-patterns.md)

## Comparison Options Quick Reference

| Option Method                | Purpose              | Applicable Scenario                  |
| ---------------------------- | -------------------- | ------------------------------------ |
| `Excluding(x => x.Property)` | Exclude specific property | Exclude timestamps, auto-generated fields |
| `Including(x => x.Property)` | Include only specific property | Key property validation           |
| `IgnoringCyclicReferences()` | Ignore circular references | Tree structures, bidirectional associations |
| `WithMaxRecursionDepth(n)`   | Limit recursion depth | Deep nested structures              |
| `WithStrictOrdering()`       | Strict ordering comparison | When array/collection order matters |
| `WithoutStrictOrdering()`    | Loose ordering comparison | When array/collection order doesn't matter |
| `WithTracing()`              | Enable tracing       | Debugging complex comparison failures |

## Common Comparison Patterns and Solutions

### Pattern 1: Entity Framework Entity Comparison

```csharp
[Fact]
public void EFEntity_Database_Entity_Should_Exclude_Navigation_Properties()
{
    var expected = new Product { Id = 1, Name = "Laptop", Price = 999 };
    var actual = dbContext.Products.Find(1);

    actual.Should().BeEquivalentTo(expected, options =>
        options.ExcludingMissingMembers()  // Exclude EF tracking properties
               .Excluding(p => p.CreatedAt)
               .Excluding(p => p.UpdatedAt)
    );
}
```text

### Pattern 2: API Response Comparison

```csharp
[Fact]
public void ApiResponse_JSON_Deserialization_Should_Ignore_Extra_Fields()
{
    var expected = new UserDto
    {
        Id = 1,
        Username = "john_doe"
    };

    var response = await httpClient.GetAsync("/api/users/1");
    var actual = await response.Content.ReadFromJsonAsync<UserDto>();

    actual.Should().BeEquivalentTo(expected, options =>
        options.ExcludingMissingMembers()  // Ignore API extra fields
    );
}
```text

### Pattern 3: Test Data Builder Comparison

```csharp
[Fact]
public void Builder_Test_Data_Should_Match_Expected_Structure()
{
    var expected = new OrderBuilder()
        .WithId(1)
        .WithCustomer("John Doe")
        .WithItems(3)
        .Build();

    var actual = orderService.CreateOrder(orderRequest);

    actual.Should().BeEquivalentTo(expected, options =>
        options.Excluding(o => o.OrderNumber)  // System generated
               .Excluding(o => o.CreatedAt)
    );
}
```text

## Error Message Optimization

### Providing Meaningful Error Messages

```csharp
[Fact]
public void Comparison_Error_Message_Should_Clearly_Explain_Differences()
{
    var expected = new User { Name = "John", Age = 30 };
    var actual = userService.GetUser(1);

    // Use because parameter to provide context
    actual.Should().BeEquivalentTo(expected, options =>
        options.Excluding(u => u.Id)
               .Because("ID is system-generated and should not be included in comparison")
    );
}
```text

### Using AssertionScope for Batch Validation

```csharp
[Fact]
public void MultipleComparisons_Batch_Validation_Should_Show_All_Failures()
{
    var users = userService.GetAllUsers();

    using (new AssertionScope())
    {
        foreach (var user in users)
        {
            user.Id.Should().BeGreaterThan(0);
            user.Name.Should().NotBeNullOrEmpty();
            user.Email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        }
    }
    // All failures reported together, rather than stopping at first failure
}
```text

## Integration with Other Skills

This skill can be combined with the following:

- **awesome-assertions-guide**: Basic assertion syntax and common APIs
- **autofixture-data-generation**: Automatically generate test data
- **test-data-builder-pattern**: Build complex test objects
- **unit-test-fundamentals**: Unit testing basics and 3A pattern

## Best Practice Recommendations

### ✅ Recommended Practices

1. **Prefer Property Exclusion over Inclusion**: Unless only validating a few properties, using `Excluding` is clearer
2. **Create Reusable Exclusion Extension Methods**: Avoid repeating exclusion logic in each test
3. **Set Reasonable Strategies for Large Data Comparison**: Balance performance and validation completeness
4. **Use AssertionScope for Batch Validation**: See all failure reasons at once
5. **Provide Meaningful because Descriptions**: Help future maintainers understand test intent

### ❌ Practices to Avoid

1. **Avoid Over-reliance on Complete Object Comparison**: Consider only validating key properties
2. **Avoid Ignoring Circular Reference Issues**: Use `IgnoringCyclicReferences()` to explicitly handle
3. **Avoid Repeating Exclusion Logic in Each Test**: Extract as extension methods
4. **Avoid Full Deep Comparison for Large Data**: Use sampling or key property validation

## Troubleshooting

### Q1: BeEquivalentTo Performance is Slow?

**A:** Use the following strategies to optimize:

- Use `Including` to only compare key properties
- Use sampling validation for large data
- Use `WithMaxRecursionDepth` to limit recursion depth
- Consider using `AssertKeyPropertiesOnly` for quick comparison of key fields

### Q2: How to Handle StackOverflowException?

**A:** Usually caused by circular references:

```csharp
options.IgnoringCyclicReferences()
       .WithMaxRecursionDepth(10)
```text

### Q3: How to Exclude All Time-Related Fields?

**A:** Use path pattern matching:

```csharp
options.Excluding(ctx => ctx.Path.EndsWith("At"))
       .Excluding(ctx => ctx.Path.EndsWith("Time"))
       .Excluding(ctx => ctx.Path.Contains("Timestamp"))
```text

### Q4: Comparison Fails but Can't See the Difference?

**A:** Enable detailed tracing:

```csharp
options.WithTracing()  // Generate detailed comparison trace information
```text

## Template Files Reference

This skill provides the following template files:

- `templates/comparison-patterns.cs`: Common comparison pattern examples
- `templates/exclusion-strategies.cs`: Field exclusion strategies and extension methods

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 05 - AwesomeAssertions Advanced Techniques and Complex Scenario Applications**
  - Article: https://ithelp.ithome.com.tw/articles/10374425
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day05

### Official Documentation

- [AwesomeAssertions GitHub](https://github.com/AwesomeAssertions/AwesomeAssertions)
- [AwesomeAssertions Documentation](https://awesomeassertions.org/)

### Related Skills

- `awesome-assertions-guide` - AwesomeAssertions basics and advanced usage
- `unit-test-fundamentals` - Unit testing basics
````
