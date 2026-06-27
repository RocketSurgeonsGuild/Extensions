---
name: dotnet-blazor-testing
category: testing
subcategory: fundamentals
description: Tests Blazor components. bUnit rendering, events, cascading params, JS interop mocking.
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

# dotnet-blazor-testing

bUnit testing for Blazor components. Covers component rendering and markup assertions, event handling, cascading
parameters and cascading values, JavaScript interop mocking, and async component lifecycle testing. bUnit provides an
in-memory Blazor renderer that executes components without a browser.

**Version assumptions:** .NET 8.0+ baseline, bUnit 1.x (stable). Examples use the latest bUnit APIs. bUnit supports both
Blazor Server and Blazor WebAssembly components.

## Scope

- bUnit component rendering and markup assertions
- Event handling and user interaction simulation
- Cascading parameters and cascading values
- JavaScript interop mocking
- Async component lifecycle testing

## Out of scope

- Browser-based E2E testing of Blazor apps -- see [skill:dotnet-playwright]
- Shared UI testing patterns (page object model, selectors, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** A Blazor test project scaffolded via [skill:dotnet-add-testing] with bUnit packages referenced. The
component under test must be in a referenced Blazor project.

Cross-references: [skill:dotnet-ui-testing-core] for shared UI testing patterns (POM, selectors, wait strategies),
[skill:dotnet-xunit] for xUnit fixtures and test organization, [skill:dotnet-blazor-patterns] for hosting models and
render modes, [skill:dotnet-blazor-components] for component architecture and state management.

---

## Package Setup

````xml

<PackageReference Include="bunit" Version="1.*" />
<!-- bUnit depends on xunit internally; ensure compatible xUnit version -->

```xml

bUnit test classes inherit from `TestContext` (or use it via composition):

```csharp

using Bunit;
using Xunit;

// Inheritance approach (less boilerplate)
public class CounterTests : TestContext
{
    [Fact]
    public void Counter_InitialRender_ShowsZero()
    {
        var cut = RenderComponent<Counter>();

        cut.Find("[data-testid='count']").MarkupMatches("<span data-testid=\"count\">0</span>");
    }
}

// Composition approach (more flexibility)
public class CounterCompositionTests : IDisposable
{
    private readonly TestContext _ctx = new();

    [Fact]
    public void Counter_InitialRender_ShowsZero()
    {
        var cut = _ctx.RenderComponent<Counter>();
        Assert.Equal("0", cut.Find("[data-testid='count']").TextContent);
    }

    public void Dispose() => _ctx.Dispose();
}

```text

---

## Component Rendering

### Basic Rendering and Markup Assertions

```csharp

public class AlertTests : TestContext
{
    [Fact]
    public void Alert_WithMessage_RendersCorrectMarkup()
    {
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(p => p.Message, "Order saved successfully")
            .Add(p => p.Severity, AlertSeverity.Success));

        // Assert on text content
        Assert.Contains("Order saved successfully", cut.Markup);

        // Assert on specific elements
        var alert = cut.Find("[data-testid='alert']");
        Assert.Contains("success", alert.ClassList);
    }

    [Fact]
    public void Alert_Dismissed_RendersNothing()
    {
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(p => p.Message, "Info")
            .Add(p => p.IsDismissed, true));

        Assert.Empty(cut.Markup.Trim());
    }
}

```text

### Rendering with Child Content

```csharp

[Fact]
public void Card_WithChildContent_RendersChildren()
{
    var cut = RenderComponent<Card>(parameters => parameters
        .AddChildContent("<p>Card body content</p>"));

    cut.Find("p").MarkupMatches("<p>Card body content</p>");
}

[Fact]
public void Card_WithRenderFragment_RendersTemplate()
{
    var cut = RenderComponent<Card>(parameters => parameters
        .Add(p => p.Header, builder =>
        {
            builder.OpenElement(0, "h2");
            builder.AddContent(1, "Card Title");
            builder.CloseElement();
        })
        .AddChildContent("<p>Body</p>"));

    cut.Find("h2").MarkupMatches("<h2>Card Title</h2>");
}

```text

### Rendering with Dependency Injection

Register services before rendering components that depend on them:

```csharp

public class OrderListTests : TestContext
{
    [Fact]
    public async Task OrderList_OnLoad_DisplaysOrders()
    {
        // Register mock service
        var mockService = Substitute.For<IOrderService>();
        mockService.GetOrdersAsync().Returns(
        [
            new OrderDto { Id = 1, CustomerName = "Alice", Total = 99.99m },
            new OrderDto { Id = 2, CustomerName = "Bob", Total = 149.50m }
        ]);
        Services.AddSingleton(mockService);

        // Render component -- DI resolves IOrderService automatically
        var cut = RenderComponent<OrderList>();

        // Wait for async data loading
        cut.WaitForState(() => cut.FindAll("[data-testid='order-row']").Count == 2);

        var rows = cut.FindAll("[data-testid='order-row']");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Alice", rows[0].TextContent);
    }
}

```text

---

## Event Handling

### Click Events

```csharp

[Fact]
public void Counter_ClickIncrement_IncreasesCount()
{
    var cut = RenderComponent<Counter>();

    cut.Find("[data-testid='increment-btn']").Click();

    Assert.Equal("1", cut.Find("[data-testid='count']").TextContent);
}

[Fact]
public void Counter_MultipleClicks_AccumulatesCount()
{
    var cut = RenderComponent<Counter>();

    var button = cut.Find("[data-testid='increment-btn']");
    button.Click();
    button.Click();
    button.Click();

    Assert.Equal("3", cut.Find("[data-testid='count']").TextContent);
}

```text

### Form Input Events

```csharp

[Fact]
public void SearchBox_TypeText_UpdatesResults()
{
    Services.AddSingleton(Substitute.For<ISearchService>());
    var cut = RenderComponent<SearchBox>();

    var input = cut.Find("[data-testid='search-input']");
    input.Input("wireless keyboard");

    Assert.Equal("wireless keyboard", cut.Instance.SearchTerm);
}

[Fact]
public async Task LoginForm_SubmitValid_CallsAuthService()
{
    var authService = Substitute.For<IAuthService>();
    authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>())
        .Returns(new AuthResult { Success = true });
    Services.AddSingleton(authService);

    var cut = RenderComponent<LoginForm>();

    cut.Find("[data-testid='email']").Change("user@example.com");
    // Use placeholder password in examples/tests in docs
    cut.Find("[data-testid='password']").Change("<TEST_PASSWORD_PLACEHOLDER>");
    cut.Find("[data-testid='login-form']").Submit();

    // Wait for async submission
    cut.WaitForState(() => cut.Instance.IsAuthenticated);

    await authService.Received(1).LoginAsync("user@example.com", "<TEST_PASSWORD_PLACEHOLDER>");
}

```text

### EventCallback Parameters

```csharp

[Fact]
public void DeleteButton_Click_InvokesOnDeleteCallback()
{
    var deletedId = 0;
    var cut = RenderComponent<DeleteButton>(parameters => parameters
        .Add(p => p.ItemId, 42)
        .Add(p => p.OnDelete, EventCallback.Factory.Create<int>(
            this, id => deletedId = id)));

    cut.Find("[data-testid='delete-btn']").Click();

    Assert.Equal(42, deletedId);
}

```text

---

## Cascading Parameters

### CascadingValue Setup

```csharp

[Fact]
public void ThemedButton_WithDarkTheme_AppliesDarkClass()
{
    var theme = new AppTheme { Mode = ThemeMode.Dark, PrimaryColor = "#1a1a2e" };

    var cut = RenderComponent<ThemedButton>(parameters => parameters
        .Add(p => p.Label, "Save")
        .AddCascadingValue(theme));

    var button = cut.Find("button");
    Assert.Contains("dark-theme", button.ClassList);
}

[Fact]
public void UserDisplay_WithCascadedAuthState_ShowsUserName()
{
    var authState = new AuthenticationState(
        new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "Alice"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "TestAuth")));

    var cut = RenderComponent<UserDisplay>(parameters => parameters
        .AddCascadingValue(Task.FromResult(authState)));

    Assert.Contains("Alice", cut.Find("[data-testid='user-name']").TextContent);
}

```text

### Named Cascading Values

```csharp

[Fact]
public void LayoutComponent_ReceivesNamedCascadingValues()
{
    var cut = RenderComponent<DashboardWidget>(parameters => parameters
        .AddCascadingValue("PageTitle", "Dashboard")
        .AddCascadingValue("SidebarCollapsed", true));

    Assert.Contains("Dashboard", cut.Find("[data-testid='widget-title']").TextContent);
}

```text

---

## JavaScript Interop Mocking

Blazor components that call JavaScript via `IJSRuntime` require mock setup in bUnit. bUnit provides a built-in JS
interop mock.

### Basic JSInterop Setup

```csharp

public class ClipboardButtonTests : TestContext
{
    [Fact]
    public void CopyButton_Click_InvokesClipboardAPI()
    {
        // Set up JS interop mock -- bUnit's JSInterop is available via this.JSInterop
        JSInterop.SetupVoid("navigator.clipboard.writeText", "Hello, World!");

        var cut = RenderComponent<CopyButton>(parameters => parameters
            .Add(p => p.TextToCopy, "Hello, World!"));

        cut.Find("[data-testid='copy-btn']").Click();

        // Verify the JS call was made
        JSInterop.VerifyInvoke("navigator.clipboard.writeText", calledTimes: 1);
    }
}

```text

### JSInterop with Return Values

```csharp

[Fact]
public void GeoLocation_OnLoad_DisplaysCoordinates()
{
    // Mock JS call that returns a value
    var location = new { Latitude = 47.6062, Longitude = -122.3321 };
    JSInterop.Setup<object>("getGeoLocation").SetResult(location);

    var cut = RenderComponent<LocationDisplay>();

    cut.WaitForState(() => cut.Find("[data-testid='coordinates']").TextContent.Contains("47.6"));
    Assert.Contains("47.6062", cut.Find("[data-testid='coordinates']").TextContent);
}

```text

### Catch-All JSInterop Mode


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
