---
name: dotnet-blazor-patterns
description: Architects Blazor apps. Hosting models, render modes, routing, streaming, prerender.
license: MIT
targets: ['*']
category: ui-frameworks
subcategory: blazor
tags:
  - platforms
  - dotnet
  - skill
  - blazor
  - web
version: '1.0.0'
author: 'dotnet-agent-harness'
invocable: true
related_skills:
  - dotnet-blazor-components
  - dotnet-blazor-auth
  - dotnet-blazor-testing
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for blazor tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-blazor-patterns

Blazor hosting models, render modes, project setup, routing, enhanced navigation, streaming rendering, and AOT-safe
patterns. Covers all five hosting models (InteractiveServer, InteractiveWebAssembly, InteractiveAuto, Static SSR,
Hybrid) with trade-off analysis for each.

## Scope

- Blazor Web App project setup and configuration
- Hosting model selection (Server, WASM, Auto, SSR, Hybrid)
- Render mode configuration (global, per-page, per-component)
- Routing and enhanced navigation
- Streaming rendering and prerendering
- AOT-safe Blazor patterns

## Out of scope

- Component architecture (lifecycle, state, JS interop) -- see [skill:dotnet-blazor-components]
- Authentication across hosting models -- see [skill:dotnet-blazor-auth]
- bUnit component testing -- see [skill:dotnet-blazor-testing]
- Standalone SignalR patterns -- see [skill:dotnet-realtime-communication]
- Browser-based E2E testing -- see [skill:dotnet-playwright]
- UI framework selection decision tree -- see [skill:dotnet-ui-chooser]

Cross-references: [skill:dotnet-blazor-components] for component architecture, [skill:dotnet-blazor-auth] for
authentication, [skill:dotnet-blazor-testing] for bUnit testing, [skill:dotnet-realtime-communication] for standalone
SignalR, [skill:dotnet-playwright] for E2E testing, [skill:dotnet-ui-chooser] for framework selection,
[skill:dotnet-accessibility] for accessibility patterns (ARIA, keyboard nav, screen readers).

---

## Hosting Models & Render Modes

Blazor Web App (.NET 8+) is the default project template, replacing the separate Blazor Server and Blazor WebAssembly
templates. Render modes can be set globally, per-page, or per-component.

### Render Mode Overview

| Render Mode            | Attribute                            | Interactivity                                 | Connection                   | Best For                                                       |
| ---------------------- | ------------------------------------ | --------------------------------------------- | ---------------------------- | -------------------------------------------------------------- |
| Static SSR             | (none / default)                     | None -- server renders HTML, no interactivity | HTTP request only            | Content pages, SEO, forms with minimal interactivity           |
| InteractiveServer      | `@rendermode InteractiveServer`      | Full                                          | SignalR circuit              | Low-latency interactivity, full server access, small user base |
| InteractiveWebAssembly | `@rendermode InteractiveWebAssembly` | Full (after download)                         | None (runs in browser)       | Offline-capable, large user base, reduced server load          |
| InteractiveAuto        | `@rendermode InteractiveAuto`        | Full                                          | SignalR initially, then WASM | Best of both -- immediate interactivity, eventual client-side  |
| Blazor Hybrid          | `BlazorWebView` in MAUI/WPF/WinForms | Full (native)                                 | None (runs in-process)       | Desktop/mobile apps with web UI, native API access             |

### Per-Mode Trade-offs

| Concern              | Static SSR   | InteractiveServer   | InteractiveWebAssembly    | InteractiveAuto     | Hybrid          |
| -------------------- | ------------ | ------------------- | ------------------------- | ------------------- | --------------- |
| First load           | Fast         | Fast                | Slow (WASM download)      | Fast (Server first) | Instant (local) |
| Server resources     | Minimal      | Per-user circuit    | None after download       | Circuit then none   | None            |
| Offline support      | No           | No                  | Yes                       | Partial             | Yes             |
| Full .NET API access | Yes (server) | Yes (server)        | Limited (browser sandbox) | Varies by phase     | Yes (native)    |
| Scalability          | High         | Limited by circuits | High                      | High (after WASM)   | N/A (local)     |
| SEO                  | Yes          | Prerender           | Prerender                 | Prerender           | N/A             |

### Setting Render Modes

**Global (App.razor):**

````razor

<!-- Sets default render mode for all pages -->
<Routes @rendermode="InteractiveServer" />

```text

**Per-page:**

```razor

@page "/dashboard"
@rendermode InteractiveServer

<h1>Dashboard</h1>

```text

**Per-component:**

```razor

<Counter @rendermode="InteractiveWebAssembly" />

```text

**Gotcha:** Without an explicit render mode boundary, a child component cannot request a more interactive render mode
than its parent. However, interactive islands are supported: you can place an `@rendermode` attribute on a component
embedded in a Static SSR page to create a render mode boundary, enabling interactive children under otherwise static
content.

---

## Project Setup

### Blazor Web App (Default Template)

```bash

# Creates a Blazor Web App with InteractiveServer render mode
dotnet new blazor -n MyApp

# With specific interactivity options
dotnet new blazor -n MyApp --interactivity Auto    # InteractiveAuto
dotnet new blazor -n MyApp --interactivity WebAssembly  # InteractiveWebAssembly
dotnet new blazor -n MyApp --interactivity Server  # InteractiveServer (default)
dotnet new blazor -n MyApp --interactivity None    # Static SSR only

```text

### Blazor Web App Project Structure

```text

MyApp/
  MyApp/                     # Server project
    Program.cs               # Host builder, services, middleware
    Components/
      App.razor              # Root component (sets global render mode)
      Routes.razor           # Router component
      Layout/
        MainLayout.razor     # Main layout
      Pages/
        Home.razor            # Static SSR by default
        Counter.razor         # Can set per-page render mode
  MyApp.Client/              # Client project (only if WASM or Auto)
    Pages/
      Counter.razor           # Components that run in browser
    Program.cs                # WASM entry point

```csharp

When using InteractiveAuto or InteractiveWebAssembly, components that must run in the browser go in the `.Client`
project. Components in the server project run on the server only.

### Blazor Hybrid Setup (MAUI)

```xml

<!-- .csproj for MAUI Blazor Hybrid -->
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
  </PropertyGroup>
</Project>

```text

```csharp

// MainPage.xaml.cs hosts BlazorWebView
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
}

```text

```xml

<!-- MainPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:b="clr-namespace:Microsoft.AspNetCore.Components.WebView.Maui;assembly=Microsoft.AspNetCore.Components.WebView.Maui">
    <b:BlazorWebView HostPage="wwwroot/index.html">
        <b:BlazorWebView.RootComponents>
            <b:RootComponent Selector="#app" ComponentType="{x:Type local:Routes}" />
        </b:BlazorWebView.RootComponents>
    </b:BlazorWebView>
</ContentPage>

```text

---

## Routing

### Basic Routing

```razor

@page "/products"
@page "/products/{Category}"

<h1>Products</h1>
@if (!string.IsNullOrEmpty(Category))
{
    <p>Category: @Category</p>
}

@code {
    [Parameter]
    public string? Category { get; set; }
}

```text

### Route Constraints

```razor

@page "/products/{Id:int}"
@page "/orders/{Date:datetime}"
@page "/search/{Query:minlength(3)}"

@code {
    [Parameter] public int Id { get; set; }
    [Parameter] public DateTime Date { get; set; }
    [Parameter] public string Query { get; set; } = "";
}

```text

### Query String Parameters

```razor

@page "/search"

@code {
    [SupplyParameterFromQuery]
    public string? Term { get; set; }

    [SupplyParameterFromQuery(Name = "page")]
    public int CurrentPage { get; set; } = 1;
}

```text

### NavigationManager

```csharp

@inject NavigationManager Navigation

// Programmatic navigation
Navigation.NavigateTo("/products/electronics");

// With query string
Navigation.NavigateTo("/search?term=keyboard&page=2");

// Force full page reload (bypasses enhanced navigation)
Navigation.NavigateTo("/external-page", forceLoad: true);

```text

---

## Enhanced Navigation (.NET 8+)

Enhanced navigation intercepts link clicks and form submissions to update only the changed DOM content, preserving page
state and avoiding full page reloads. This applies to Static SSR and prerendered pages.

### How It Works

1. User clicks a link within the Blazor app
2. Blazor intercepts the navigation
3. A fetch request loads the new page content
4. Blazor patches the DOM with only the differences
5. Scroll position and focus state are preserved

### Opting Out

```razor

<!-- Disable enhanced navigation for a specific link -->
<a href="/legacy-page" data-enhance-nav="false">Legacy Page</a>

<!-- Disable enhanced form handling for a specific form -->
<form method="post" data-enhance="false">
    ...
</form>

```text

**Gotcha:** Enhanced navigation may interfere with third-party JavaScript libraries that expect full page loads. Use
`data-enhance-nav="false"` on links that navigate to pages with JS that initializes on `DOMContentLoaded`.

---

## Streaming Rendering (.NET 8+)

Streaming rendering sends initial HTML immediately (with placeholder content), then streams updates as async operations
complete. Useful for pages with slow data sources.

```razor

@page "/dashboard"
@attribute [StreamRendering]

<h1>Dashboard</h1>

@if (orders is null)
{
    <p>Loading orders...</p>
}
else
{
    <table>
        @foreach (var order in orders)
        {
            <tr><td>@order.Id</td><td>@order.Total</td></tr>
        }
    </table>
}

@code {
    private List<OrderDto>? orders;

    protected override async Task OnInitializedAsync()
    {
        // Initial HTML sent immediately with "Loading orders..."
        // Updated HTML streamed when this completes
        orders = await OrderService.GetRecentOrdersAsync();
    }
}

```text

**Behavior per render mode:**

- **Static SSR:** Streaming rendering sends the initial response, then patches the DOM via chunked transfer encoding.
  The page is not interactive.
- **InteractiveServer/WebAssembly/Auto:** Streaming rendering is less impactful because components re-render
  automatically after async operations. The `[StreamRendering]` attribute primarily benefits the prerender phase.

---

## AOT-Safe Patterns

When targeting Blazor WebAssembly with Native AOT (ahead-of-time compilation) or IL trimming, avoid patterns that rely
on runtime reflection.

### Source-Generator-First Serialization

```csharp

// CORRECT: Source-generated JSON serialization (AOT-compatible)
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonContext : JsonSerializerContext { }

// Register in Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

// Usage in HttpClient calls
var products = await Http.GetFromJsonAsync<List<ProductDto>>(
    "/api/products",
    AppJsonContext.Default.ListProductDto);

```json

```csharp

// WRONG: Reflection-based serialization (fails under AOT/trimming)
var products = await Http.GetFromJsonAsync<List<ProductDto>>("/api/products");

```csharp

### Trim-Safe JS Interop

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
