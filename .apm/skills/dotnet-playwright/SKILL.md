---
name: dotnet-playwright
category: developer-experience
subcategory: cli
description: Automates browser tests in .NET. Playwright E2E, CI browser caching, trace viewer, codegen.
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

# dotnet-playwright

Playwright for .NET: browser automation and end-to-end testing. Covers browser lifecycle management, page interactions,
assertions, CI caching of browser binaries, trace viewer for debugging failures, and codegen for rapid test scaffolding.

**Version assumptions:** Playwright 1.40+ for .NET, .NET 8.0+ baseline. Playwright supports Chromium, Firefox, and
WebKit browsers.

## Scope

- Browser lifecycle management (Chromium, Firefox, WebKit)
- Page interactions and locator-based assertions
- CI caching of browser binaries
- Trace viewer for debugging test failures
- Codegen for rapid test scaffolding

## Out of scope

- Shared UI testing patterns (page object model, selectors, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Testing strategy (when E2E vs unit vs integration) -- see [skill:dotnet-testing-strategy]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** Test project scaffolded via [skill:dotnet-add-testing] with Playwright packages referenced. Browsers
installed via `pwsh bin/Debug/net8.0/playwright.ps1 install` or `dotnet tool run playwright install`.

Cross-references: [skill:dotnet-ui-testing-core] for page object model and selector strategies,
[skill:dotnet-testing-strategy] for deciding when E2E tests are appropriate.

---

## Package Setup

````xml

<PackageReference Include="Microsoft.Playwright" Version="1.*" />
<!-- For xUnit integration: -->
<PackageReference Include="Microsoft.Playwright.Xunit" Version="1.*" />
<!-- For NUnit integration: -->
<!-- <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.*" /> -->

```text

### Installing Browsers

Playwright requires downloading browser binaries before tests can run:

```bash

# After building the test project:
pwsh bin/Debug/net8.0/playwright.ps1 install

# Or install specific browsers:
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
pwsh bin/Debug/net8.0/playwright.ps1 install firefox

# Using dotnet tool:
dotnet tool install --global Microsoft.Playwright.CLI
playwright install

```text

---

## Basic Test Structure

### With Playwright xUnit Base Class

```csharp

using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

// PageTest provides Page, Browser, BrowserContext, and Playwright properties
public class HomePageTests : PageTest
{
    [Fact]
    public async Task HomePage_Title_ContainsAppName()
    {
        await Page.GotoAsync("https://localhost:5001");

        await Expect(Page).ToHaveTitleAsync(new Regex("My App"));
    }

    [Fact]
    public async Task HomePage_NavLinks_AreVisible()
    {
        await Page.GotoAsync("https://localhost:5001");

        var nav = Page.Locator("nav");
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "Home" }))
            .ToBeVisibleAsync();
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "About" }))
            .ToBeVisibleAsync();
    }
}

```text

### Manual Setup (Without Base Class)

```csharp

public class ManualSetupTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public async ValueTask InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            Locale = "en-US"
        });
        _page = await _context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Login_ValidUser_RedirectsToDashboard()
    {
        await _page.GotoAsync("https://localhost:5001/login");

        await _page.FillAsync("[data-testid='email']", "user@example.com");
        // Use placeholder password in docs/examples
        await _page.FillAsync("[data-testid='password']", "<TEST_PASSWORD_PLACEHOLDER>");
        await _page.ClickAsync("[data-testid='login-btn']");

        await Expect(_page).ToHaveURLAsync(new Regex("/dashboard"));
    }
}

```text

---

## Locators and Interactions

### Recommended Locator Strategies

```csharp

// BEST: Role-based (accessible and semantic)
var submitBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Submit Order" });

// GOOD: Test ID (stable, explicit)
var emailInput = Page.Locator("[data-testid='email-input']");

// GOOD: Label text (user-visible, accessible)
var nameField = Page.GetByLabel("Full Name");

// GOOD: Placeholder (user-visible)
var searchBox = Page.GetByPlaceholder("Search products...");

// AVOID: CSS class (fragile, changes with styling)
var card = Page.Locator(".card-primary");

// AVOID: XPath (brittle, hard to read)
var cell = Page.Locator("//table/tbody/tr[1]/td[2]");

```text

### Common Interactions

```csharp

// Text input
await Page.FillAsync("[data-testid='name']", "Alice Johnson");

// Click
await Page.ClickAsync("[data-testid='submit']");

// Select dropdown
await Page.SelectOptionAsync("[data-testid='country']", "US");

// Checkbox / radio
await Page.CheckAsync("[data-testid='agree-terms']");

// File upload
await Page.SetInputFilesAsync("[data-testid='avatar']", "testdata/photo.jpg");

// Keyboard
await Page.Keyboard.PressAsync("Enter");
await Page.Keyboard.TypeAsync("search query");

// Hover (for dropdowns, tooltips)
await Page.HoverAsync("[data-testid='user-menu']");

```text

### Assertions (Expect API)

Playwright assertions auto-retry until the condition is met or the timeout expires:

```csharp

// Element visibility
await Expect(Page.Locator("[data-testid='success']")).ToBeVisibleAsync();
await Expect(Page.Locator("[data-testid='spinner']")).ToBeHiddenAsync();

// Text content
await Expect(Page.Locator("[data-testid='total']")).ToHaveTextAsync("$99.99");
await Expect(Page.Locator("[data-testid='status']")).ToContainTextAsync("Completed");

// Attribute
await Expect(Page.Locator("[data-testid='submit']")).ToBeEnabledAsync();
await Expect(Page.Locator("[data-testid='email']")).ToHaveValueAsync("user@example.com");

// Page-level
await Expect(Page).ToHaveURLAsync(new Regex("/orders/\\d+"));
await Expect(Page).ToHaveTitleAsync("Order Details - My App");

// Count
await Expect(Page.Locator("[data-testid='order-row']")).ToHaveCountAsync(5);

```text

---

## Network Interception

### Mocking API Responses

```csharp

[Fact]
public async Task OrderList_WithMockedApi_DisplaysOrders()
{
    // Intercept API calls and return mock data
    await Page.RouteAsync("**/api/orders", async route =>
    {
        var json = JsonSerializer.Serialize(new[]
        {
            new { Id = 1, CustomerName = "Alice", Total = 99.99 },
            new { Id = 2, CustomerName = "Bob", Total = 149.50 }
        });
        await route.FulfillAsync(new RouteFulfillOptions
        {
            Status = 200,
            ContentType = "application/json",
            Body = json
        });
    });

    await Page.GotoAsync("https://localhost:5001/orders");

    await Expect(Page.Locator("[data-testid='order-row']")).ToHaveCountAsync(2);
}

```text

### Waiting for Network Requests

```csharp

[Fact]
public async Task CreateOrder_SubmitForm_WaitsForApiResponse()
{
    await Page.GotoAsync("https://localhost:5001/orders/new");

    await Page.FillAsync("[data-testid='customer']", "Alice");
    await Page.FillAsync("[data-testid='amount']", "99.99");

    // Wait for the API call triggered by form submission
    var responseTask = Page.WaitForResponseAsync(
        response => response.Url.Contains("/api/orders") && response.Status == 201);

    await Page.ClickAsync("[data-testid='submit']");

    var response = await responseTask;
    Assert.Equal(201, response.Status);
}

```text

---

## CI Browser Caching

Downloading browser binaries on every CI run is slow (500MB+). Cache them to speed up builds.

### GitHub Actions Caching

```yaml

# .github/workflows/e2e-tests.yml
jobs:
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Build
        run: dotnet build tests/MyApp.E2E/

      - name: Cache Playwright browsers
        id: playwright-cache
        uses: actions/cache@v4
        with:
          path: ~/.cache/ms-playwright
          key: playwright-${{ runner.os }}-${{ hashFiles('tests/MyApp.E2E/MyApp.E2E.csproj') }}

      - name: Install Playwright browsers
        if: steps.playwright-cache.outputs.cache-hit != 'true'
        run: pwsh tests/MyApp.E2E/bin/Debug/net8.0/playwright.ps1 install --with-deps

      - name: Install Playwright system deps
        if: steps.playwright-cache.outputs.cache-hit == 'true'
        run: pwsh tests/MyApp.E2E/bin/Debug/net8.0/playwright.ps1 install-deps

      - name: Run E2E tests
        run: dotnet test tests/MyApp.E2E/

```powershell

### Azure DevOps Caching

```yaml

# azure-pipelines.yml
steps:
  - task: Cache@2
    inputs:
      key: 'playwright | "$(Agent.OS)" | tests/MyApp.E2E/MyApp.E2E.csproj'
      path: $(HOME)/.cache/ms-playwright
      restoreKeys: |
        playwright | "$(Agent.OS)"
      cacheHitVar: PLAYWRIGHT_CACHE_RESTORED
    displayName: Cache Playwright browsers

  - script: pwsh tests/MyApp.E2E/bin/Debug/net8.0/playwright.ps1 install --with-deps
    condition: ne(variables.PLAYWRIGHT_CACHE_RESTORED, 'true')
    displayName: Install Playwright browsers

  - script: pwsh tests/MyApp.E2E/bin/Debug/net8.0/playwright.ps1 install-deps
    condition: eq(variables.PLAYWRIGHT_CACHE_RESTORED, 'true')
    displayName: Install Playwright system deps (cached browsers)

  - script: dotnet test tests/MyApp.E2E/
    displayName: Run E2E tests

```text

### Cache Key Strategy

The cache key should include:
- **OS:** Browser binaries are platform-specific
- **Project file hash:** Playwright version determines browser versions; changing the package version invalidates the cache
- **Fallback key:** Allows partial cache restoration when the project file changes

---

## Trace Viewer

Playwright's trace viewer captures a full recording of test execution for debugging failures. Each trace includes screenshots, DOM snapshots, network logs, and console output.

### Enabling Traces

```csharp

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
