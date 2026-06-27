---
name: dotnet-testing-test-data
category: testing
subcategory: test-data
complexity: intermediate
description: 'Generate test data with AutoFixture, Bogus, and builder patterns for .NET testing'
tags: ['testing', 'test-data', 'autofixture', 'bogus', 'faker', 'builder-pattern', 'autodata', 'intermediate']
invocable: true
related:
  - dotnet-testing
  - dotnet-testing-mocking
---

# Test Data Generation

Generate realistic, anonymous, and complex test data for .NET unit and integration tests.

## Quick Start

**Install packages:**
```bash
dotnet add package AutoFixture
dotnet add package AutoFixture.Xunit2
dotnet add package Bogus
```

## AutoFixture Basics

**Auto-generate test objects:**
```csharp
[Fact]
public void UserService_CreatesUser_SavesToDatabase()
{
    // Arrange
    var fixture = new Fixture();
    var user = fixture.Create<User>();  // Anonymous user with auto-populated data
    var mockDb = Substitute.For<IDatabase>();
    var service = new UserService(mockDb);
    
    // Act
    service.CreateUser(user);
    
    // Assert
    mockDb.Received().Save(Arg.Is<User>(u => u.Id == user.Id));
}
```

**AutoData with xUnit:**
```csharp
public class UserTests
{
    [Theory, AutoData]
    public void ValidateUser_ValidData_ReturnsTrue(User user)
    {
        // AutoFixture creates user automatically
        var result = UserValidator.Validate(user);
        
        result.IsValid.Should().BeTrue();
    }
}
```

## Bogus for Realistic Data

**Generate fake but realistic data:**
```csharp
var faker = new Faker<User>()
    .RuleFor(u => u.FirstName, f => f.Name.FirstName())
    .RuleFor(u => u.LastName, f => f.Name.LastName())
    .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
    .RuleFor(u => u.Age, f => f.Random.Int(18, 65))
    .RuleFor(u => u.Address, f => f.Address.FullAddress());

var user = faker.Generate();
// user.Email: "john.smith@example.com"
// user.Age: 42
```

**Generate collections:**
```csharp
var users = faker.Generate(100);  // 100 realistic users
var orders = new Faker<Order>()
    .RuleFor(o => o.Product, f => f.Commerce.ProductName())
    .RuleFor(o => o.Price, f => f.Random.Decimal(10, 500))
    .Generate(50);
```

## Test Data Builder Pattern

**Fluent builders for complex objects:**
```csharp
public class OrderBuilder
{
    private readonly Order _order = new();
    private readonly Fixture _fixture = new();

    public OrderBuilder WithCustomer(string customer)
    {
        _order.Customer = customer;
        return this;
    }

    public OrderBuilder WithItem(string product, int quantity, decimal price)
    {
        _order.Items.Add(new OrderItem 
        { 
            Product = product, 
            Quantity = quantity,
            Price = price 
        });
        return this;
    }

    public OrderBuilder WithDefaultValues()
    {
        _order.Id = _fixture.Create<int>();
        _order.CreatedAt = DateTime.UtcNow;
        return this;
    }

    public Order Build() => _order;
}

// Usage:
var order = new OrderBuilder()
    .WithCustomer("Acme Corp")
    .WithItem("Widget", 10, 29.99m)
    .WithItem("Gadget", 5, 49.99m)
    .WithDefaultValues()
    .Build();
```

## Combining AutoFixture + Bogus

**Hybrid approach for realistic anonymous data:**
```csharp
public class HybridFixture : Fixture
{
    public HybridFixture()
    {
        Customize<User>(c => c
            .With(u => u.FirstName, () => new Faker().Name.FirstName())
            .With(u => u.LastName, () => new Faker().Name.LastName())
            .With(u => u.Email, (User u) => new Faker().Internet.Email(u.FirstName, u.LastName)));
    }
}

// Usage:
var fixture = new HybridFixture();
var user = fixture.Create<User>();
// user.FirstName: "Alice" (realistic)
// user.Id: 84723 (anonymous)
```

## NSubstitute + AutoFixture Integration

**Auto-mocking with AutoData:**
```csharp
public class OrderServiceTests
{
    [Theory, AutoNSubstituteData]
    public void PlaceOrder_CallsPaymentGateway(
        Order order,
        [Frozen] IPaymentGateway paymentGateway,
        OrderService sut)
    {
        // AutoFixture creates order and mocks paymentGateway
        // [Frozen] ensures same instance used throughout test
        
        sut.PlaceOrder(order);
        
        paymentGateway.Received().Process(order.Total);
    }
}

// Custom AutoData attribute:
public class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute() 
        : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
    { }
}
```

## Builder Composition

**Compose builders for complex scenarios:**
```csharp
public static class AUser
{
    public static UserBuilder WithName(string name) => new UserBuilder().WithName(name);
    public static UserBuilder WithEmail(string email) => new UserBuilder().WithEmail(email);
    public static UserBuilder Default() => new UserBuilder().WithDefaultValues();
}

// Fluent semantic API:
var user = AUser.WithName("John Doe")
    .WithEmail("john@example.com")
    .WithRole("Admin")
    .Build();
```

## Best Practices

**DO:**
- Use AutoFixture for anonymous test data
- Use Bogus for realistic domain data
- Use builders for complex object construction
- Keep test data creation close to test
- Name builders semantically (AUser, AnOrder)

**DON'T:**
- Share mutable test data between tests
- Use hardcoded values that could break
- Create test data far from test (separate files)
- Over-specify data that doesn't matter to test

## When to Use What

| Tool | Best For | Example |
|------|----------|---------|
| **AutoFixture** | Anonymous objects | `fixture.Create<User>()` |
| **Bogus** | Realistic fake data | Customer names, addresses |
| **Builder** | Complex construction | Multi-step object setup |
| **AutoData** | Parameterized tests | `[Theory, AutoData]` |

## Related Skills

- **dotnet-testing** - Testing fundamentals
- **dotnet-testing-mocking** - NSubstitute for test doubles
