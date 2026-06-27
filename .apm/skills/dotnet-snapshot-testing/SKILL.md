---
name: dotnet-snapshot-testing
category: testing
subcategory: assertions
description: Verifies complex outputs with Verify. API responses, scrubbing non-deterministic values.
license: MIT
targets: ['*']
tags: [testing, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for testing tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-snapshot-testing

Snapshot (approval) testing with the Verify library for .NET. Covers verifying API responses, serialized objects,
rendered emails, and other complex outputs by comparing them against approved baseline files. Includes scrubbing and
filtering patterns to handle non-deterministic values (dates, GUIDs, timestamps), custom converters for domain-specific
types, and strategies for organizing and reviewing snapshot files.

**Version assumptions:** Verify 20.x+ (.NET 8.0+ baseline). Examples use the `Verify.Xunit` integration package;
equivalent packages exist for NUnit (`Verify.NUnit`) and MSTest (`Verify.MSTest`). Verify auto-discovers the test
framework from the referenced package.

## Scope

- Verify library setup and snapshot lifecycle
- Scrubbing and filtering non-deterministic values (dates, GUIDs)
- Custom converters for domain-specific types
- Organizing and reviewing snapshot files
- Integration with xUnit, NUnit, and MSTest

## Out of scope

- Test project scaffolding (creating projects, package references) -- see [skill:dotnet-add-testing]
- Testing strategy and test type decisions -- see [skill:dotnet-testing-strategy]
- Integration test infrastructure (WebApplicationFactory, Testcontainers) -- see [skill:dotnet-integration-testing]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with Verify packages referenced. .NET
8.0+ baseline required.

Cross-references: [skill:dotnet-testing-strategy] for deciding when snapshot tests are appropriate,
[skill:dotnet-integration-testing] for combining Verify with WebApplicationFactory and Testcontainers.

---

## Setup

### Packages

````xml

<PackageReference Include="Verify.Xunit" Version="20.*" />
<!-- For HTTP response verification -->
<PackageReference Include="Verify.Http" Version="6.*" />

```xml

### Module Initializer

Verify requires a one-time initialization per test assembly. Place this in a file at the root of your test project:

```csharp

// ModuleInitializer.cs
using System.Runtime.CompilerServices;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifySourceGenerators.Initialize();
}

```text

### Source Control

Add to `.gitignore`:

```gitignore

# Verify received files (test failures)
*.received.*

```text

Add to `.gitattributes` so verified files diff cleanly:

```gitattributes

*.verified.txt text eol=lf
*.verified.xml text eol=lf
*.verified.json text eol=lf

```json

---

## Basic Usage

### Verifying Objects

Verify serializes the object to JSON and compares against a `.verified.txt` file:

```csharp

[UsesVerify]
public class OrderSerializationTests
{
    [Fact]
    public Task Serialize_CompletedOrder_MatchesSnapshot()
    {
        var order = new Order
        {
            Id = 1,
            CustomerId = "cust-123",
            Status = OrderStatus.Completed,
            Items =
            [
                new OrderItem("SKU-001", Quantity: 2, UnitPrice: 29.99m),
                new OrderItem("SKU-002", Quantity: 1, UnitPrice: 49.99m)
            ],
            Total = 109.97m
        };

        return Verify(order);
    }
}

```text

First run creates `OrderSerializationTests.Serialize_CompletedOrder_MatchesSnapshot.verified.txt`:

```txt

{
  Id: 1,
  CustomerId: cust-123,
  Status: Completed,
  Items: [
    {
      Sku: SKU-001,
      Quantity: 2,
      UnitPrice: 29.99
    },
    {
      Sku: SKU-002,
      Quantity: 1,
      UnitPrice: 49.99
    }
  ],
  Total: 109.97
}

```text

### Verifying Strings and Streams

```csharp

[Fact]
public Task RenderInvoice_MatchesExpectedHtml()
{
    var html = invoiceRenderer.Render(order);
    return Verify(html, extension: "html");
}

[Fact]
public Task ExportReport_MatchesExpectedXml()
{
    var stream = reportExporter.Export(report);
    return Verify(stream, extension: "xml");
}

```xml

---

## Scrubbing and Filtering

Non-deterministic values (dates, GUIDs, auto-incremented IDs) change between test runs. Scrubbing replaces them with stable placeholders so snapshots remain comparable.

### Built-In Scrubbers

Verify includes scrubbers for common non-deterministic types that are active by default:

```csharp

[Fact]
public Task CreateOrder_ScrubsNonDeterministicValues()
{
    var order = new Order
    {
        Id = Guid.NewGuid(),          // Scrubbed to Guid_1
        CreatedAt = DateTime.UtcNow,  // Scrubbed to DateTime_1
        TrackingNumber = Guid.NewGuid().ToString() // Scrubbed to Guid_2
    };

    return Verify(order);
}

```text

Produces stable output:

```txt

{
  Id: Guid_1,
  CreatedAt: DateTime_1,
  TrackingNumber: Guid_2
}

```text

### Custom Scrubbers

When built-in scrubbing is not sufficient, add custom scrubbers:

```csharp

[Fact]
public Task AuditLog_ScrubsTimestampsAndMachineNames()
{
    var log = auditService.GetRecentEntries();

    return Verify(log)
        .ScrubLinesWithReplace(line =>
            Regex.Replace(line, @"Machine:\s+\w+", "Machine: Scrubbed"))
        .ScrubLinesContaining("CorrelationId:");
}

```text

### Ignoring Members

Exclude specific properties from verification:

```csharp

[Fact]
public Task OrderSnapshot_IgnoresVolatileFields()
{
    var order = orderService.CreateOrder(request);

    return Verify(order)
        .IgnoreMember("CreatedAt")
        .IgnoreMember("UpdatedAt")
        .IgnoreMember("ETag");
}

```text

Or ignore by type across all verifications:

```csharp

// In ModuleInitializer
[ModuleInitializer]
public static void Init()
{
    VerifierSettings.IgnoreMembersWithType<DateTime>();
    VerifierSettings.IgnoreMembersWithType<DateTimeOffset>();
}

```text

### Scrubbing Inline Values

Replace specific patterns in the serialized output:

```csharp

[Fact]
public Task ApiResponse_ScrubsTokens()
{
    var response = authService.GenerateTokenResponse(user);

    return Verify(response)
        .ScrubLinesWithReplace(line =>
            Regex.Replace(line, @"Bearer [A-Za-z0-9\-._~+/]+=*", "Bearer {scrubbed}"));
}

```text

---

## Verifying HTTP Responses

Verify HTTP responses from WebApplicationFactory integration tests to lock down API contracts.

### Setup

```xml

<PackageReference Include="Verify.Http" Version="6.*" />

```xml

### Verifying Full HTTP Responses

```csharp

[UsesVerify]
public class OrdersApiSnapshotTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiSnapshotTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_ResponseMatchesSnapshot()
    {
        var response = await _client.GetAsync("/api/orders");

        await Verify(response);
    }
}

```text

The verified file captures status code, headers, and body:

```txt

{
  Status: 200 OK,
  Headers: {
    Content-Type: application/json; charset=utf-8
  },
  Body: [
    {
      Id: 1,
      CustomerId: cust-123,
      Status: Pending,
      Total: 109.97
    }
  ]
}

```text

### Verifying Specific Response Parts

```csharp

[Fact]
public async Task CreateOrder_VerifyResponseBody()
{
    var response = await _client.PostAsJsonAsync("/api/orders", request);
    var body = await response.Content.ReadFromJsonAsync<OrderDto>();

    await Verify(body)
        .IgnoreMember("Id")
        .IgnoreMember("CreatedAt");
}

```text

---

## Verifying Rendered Emails

Snapshot-test email templates by verifying the rendered HTML output:

```csharp

[UsesVerify]
public class EmailTemplateTests
{
    private readonly EmailRenderer _renderer = new();

    [Fact]
    public Task OrderConfirmation_MatchesSnapshot()
    {
        var model = new OrderConfirmationModel
        {
            CustomerName = "Alice Johnson",
            OrderNumber = "ORD-001",
            Items =
            [
                new("Widget A", Quantity: 2, Price: 29.99m),
                new("Widget B", Quantity: 1, Price: 49.99m)
            ],
            Total = 109.97m
        };


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
