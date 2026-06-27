---
name: dotnet-uno-platform
category: ui-frameworks
subcategory: uno
description: Builds Uno Platform cross-platform apps. Extensions, MVUX, Toolkit controls, Hot Reload.
license: MIT
targets: ['*']
tags: [ui, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for ui tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-uno-platform

Uno Platform core development: Extensions ecosystem (Navigation, DI, Configuration, Serialization, Localization,
Logging, HTTP, Authentication), MVUX reactive pattern, Toolkit controls, Theme resources (Material/Cupertino/Fluent),
Hot Reload, and single-project structure. Covers Uno Platform 5.x+ on .NET 8.0+ baseline.

## Scope

- Uno Platform single-project structure with conditional TFMs
- Extensions ecosystem (Navigation, DI, Configuration, Serialization, HTTP, Auth)
- MVUX reactive pattern
- Toolkit controls and theme resources (Material/Cupertino/Fluent)
- Hot Reload

## Out of scope

- Per-target deployment (WASM, iOS, Android, Desktop) -- see [skill:dotnet-uno-targets]
- MCP server integration for live documentation -- see [skill:dotnet-uno-mcp]
- Uno Platform testing -- see [skill:dotnet-uno-testing]
- General serialization patterns -- see [skill:dotnet-serialization]
- AOT/trimming for WASM -- see [skill:dotnet-aot-wasm]
- UI framework selection decision tree -- see [skill:dotnet-ui-chooser]

Cross-references: [skill:dotnet-uno-targets] for per-target deployment, [skill:dotnet-uno-mcp] for MCP integration,
[skill:dotnet-uno-testing] for testing patterns, [skill:dotnet-serialization] for serialization depth,
[skill:dotnet-aot-wasm] for WASM AOT, [skill:dotnet-ui-chooser] for framework selection, [skill:dotnet-accessibility]
for accessibility patterns (AutomationProperties, ARIA mapping on WASM).

---

## Single-Project Structure

Uno Platform 5.x uses a single-project structure with conditional TFMs for multi-targeting. One `.csproj` targets all
platforms via multi-targeting.

````xml

<!-- MyApp.csproj -->
<Project Sdk="Uno.Sdk">
  <PropertyGroup>
    <TargetFrameworks>
      net8.0-browserwasm;
      net8.0-ios;
      net8.0-android;
      net8.0-maccatalyst;
      net8.0-windows10.0.19041;
      net8.0-desktop
    </TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UnoFeatures>
      Extensions;
      Toolkit;
      Material;
      MVUX;
      Navigation;
      Configuration;
      Hosting;
      Http;
      Localization;
      Logging;
      LoggingSerilog;
      Serialization;
      Authentication;
      AuthenticationOidc
    </UnoFeatures>
  </PropertyGroup>
</Project>

```text

The `UnoFeatures` MSBuild property controls which Uno Extensions and theming packages are included. The Uno SDK resolves these features to the correct NuGet packages automatically.

### Project Layout

```text

MyApp/
  MyApp/
    App.xaml / App.xaml.cs        # Application entry, resource dictionaries
    MainPage.xaml / .xaml.cs      # Initial page
    Presentation/                 # ViewModels or MVUX Models
    Views/                        # XAML pages
    Services/                     # Service interfaces and implementations
    Strings/                      # Localization resources (.resw)
      en/Resources.resw
    Assets/                       # Images, fonts, icons
    appsettings.json              # Configuration (Extensions.Configuration)
    Platforms/                    # Platform-specific code (conditional compilation)
      Android/
      iOS/
      Wasm/
      Desktop/
  MyApp.Tests/                   # Unit tests (shared logic)

```text

---

## Uno Extensions

Uno Extensions provide opinionated infrastructure on top of the platform. All modules are registered through the host builder pattern.

### Host Builder Setup

```csharp

// App.xaml.cs
public App()
{
    this.InitializeComponent();

    Host = UnoHost
        .CreateDefaultBuilder()
        .UseConfiguration(configure: configBuilder =>
            configBuilder.EmbeddedSource<App>()
                         .Section<AppConfig>())
        .UseLocalization()
        .UseNavigation(RegisterRoutes)
        .UseSerilog(loggerConfiguration: config =>
            config.WriteTo.Debug())
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<IProductService, ProductService>();
        })
        .Build();
}

```text

### Navigation

**Package:** `Uno.Extensions.Navigation`

Region-based navigation with route maps, deep linking, and type-safe parameter passing. Navigation is driven declaratively from XAML or imperatively from code.

```csharp

// Route registration
private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
{
    views.Register(
        new ViewMap(ViewModel: typeof(ShellModel)),
        new ViewMap<MainPage, MainModel>(),
        new ViewMap<ProductDetailPage, ProductDetailModel>(),
        new DataViewMap<ProductDetailPage, ProductDetailModel, ProductEntity>()
    );

    routes.Register(
        new RouteMap("", View: views.FindByViewModel<ShellModel>(),
            Nested: new RouteMap[]
            {
                new("Main", View: views.FindByViewModel<MainModel>()),
                new("ProductDetail", View: views.FindByViewModel<ProductDetailModel>())
            })
    );
}

```text

```xml

<!-- XAML-based navigation using attached properties -->
<Button Content="View Product"
        uen:Navigation.Request="ProductDetail"
        uen:Navigation.Data="{Binding SelectedProduct}" />

```text

**Key concepts:** Region-based navigation attaches navigation behavior to visual regions (Frame, NavigationView, TabBar). Route maps define the navigation graph. Deep linking maps URLs to routes for WASM.

### Dependency Injection

**Package:** `Uno.Extensions.Hosting`

Uses Microsoft.Extensions.Hosting under the hood. Host builder pattern with service registration, keyed services, and scoped lifetimes.

```csharp

.ConfigureServices((context, services) =>
{
    // Standard DI registration
    services.AddSingleton<IAuthService, AuthService>();
    services.AddTransient<IOrderService, OrderService>();

    // Keyed services (.NET 8+)
    services.AddKeyedSingleton<ICache>("memory", new MemoryCache());
    services.AddKeyedSingleton<ICache>("distributed", new RedisCache());
})

```text

### Configuration

**Package:** `Uno.Extensions.Configuration`

Loads configuration from `appsettings.json` (embedded resource), environment-specific overrides, and runtime writeable options.

```json

// appsettings.json
{
  "AppConfig": {
    "ApiBaseUrl": "https://api.example.com",
    "MaxRetries": 3
  }
}

```text

```csharp

// Binding to strongly-typed options
.UseConfiguration(configure: configBuilder =>
    configBuilder
        .EmbeddedSource<App>()
        .Section<AppConfig>())

// AppConfig.cs
public record AppConfig
{
    public string ApiBaseUrl { get; init; } = "";
    public int MaxRetries { get; init; } = 3;
}

```text

### Serialization

**Package:** `Uno.Extensions.Serialization`

Integrates System.Text.Json with source generators for AOT compatibility. Configures JSON serialization across the Extensions ecosystem.

```csharp

.UseSerialization(configure: serializerBuilder =>
    serializerBuilder
        .AddJsonTypeInfo(AppJsonContext.Default.ProductDto)
        .AddJsonTypeInfo(AppJsonContext.Default.OrderDto))

```json

For general serialization patterns and AOT source-gen depth, see [skill:dotnet-serialization].

### Localization

**Package:** `Uno.Extensions.Localization`

Resource-based localization using `.resw` files with runtime culture switching.

```csharp

.UseLocalization()

```csharp

```xml

<!-- Strings/en/Resources.resw -->
<!-- name: MainPage_Title.Text, value: Welcome -->
<!-- name: MainPage_LoginButton.Content, value: Log In -->

<!-- XAML: use x:Uid for automatic resource binding -->
<TextBlock x:Uid="MainPage_Title" />
<Button x:Uid="MainPage_LoginButton" />

```text

Culture switching at runtime:

```csharp

// Switch culture programmatically
var localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
await localizationService.SetCurrentCultureAsync(new CultureInfo("fr-FR"));

```csharp

### Logging

**Package:** `Uno.Extensions.Logging`

Integrates with Microsoft.Extensions.Logging. Serilog integration for platform-specific sinks.

```csharp

.UseSerilog(loggerConfiguration: config =>
    config
        .MinimumLevel.Information()
        .WriteTo.Debug()
        .WriteTo.Console())

```text

Platform-specific sinks: Debug output for desktop, browser console for WASM, platform logcat for Android, NSLog for iOS.

### HTTP

**Package:** `Uno.Extensions.Http`

HTTP client integration with endpoint configuration. Supports Refit for typed API clients and Kiota for OpenAPI-generated clients.

```csharp

.UseHttp(configure: (context, services) =>
    services
        .AddRefitClient<IProductApi>(context,
            configure: builder => builder
                .ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri("https://api.example.com"))))

```text

```csharp

// Refit interface
public interface IProductApi
{
    [Get("/products")]
    Task<List<ProductDto>> GetProductsAsync(CancellationToken ct = default);

    [Get("/products/{id}")]
    Task<ProductDto> GetProductByIdAsync(int id, CancellationToken ct = default);
}

```text

### Authentication

**Package:** `Uno.Extensions.Authentication`

OIDC, custom auth providers, and token management. Integrates with navigation for login/logout flows.

```csharp

.UseAuthentication(auth =>
    auth.AddOidc(oidc =>
    {
        oidc.Authority = "https://login.example.com";
        oidc.ClientId = "my-app";
        oidc.Scope = "openid profile email";
    }))

```text

Token management is automatic: tokens are stored securely per platform (Keychain on iOS/macOS, KeyStore on Android, Credential Manager on Windows, browser storage on WASM) and refreshed transparently.

---

## MVUX (Model-View-Update-eXtended)

MVUX is Uno's recommended reactive pattern, distinct from MVVM. It uses immutable records, Feeds, and States to model data flow declaratively. Source generators produce bindable proxies from plain model classes.

### Core Concepts

| Concept | Purpose | MVVM Equivalent |
|---------|---------|-----------------|
| **Model** | Immutable record defining UI state | ViewModel |
| **Feed** | Async data source (loading/data/error states) | ObservableCollection + loading flag |
| **State** | Mutable reactive state with change tracking | INotifyPropertyChanged property |
| **ListFeed** | Feed specialized for collections | ObservableCollection |
| **Command** | Auto-generated from public async methods | ICommand |

### Model Example

```csharp

// ProductModel.cs -- MVUX model (source generators produce the bindable proxy)
public partial record ProductModel(IProductService ProductService)
{
    // Feed: async data source with loading/error/data states
    public IFeed<IImmutableList<ProductDto>> Products => Feed
        .Async(async ct => await ProductService.GetProductsAsync(ct));

    // State: mutable reactive value
    public IState<string> SearchTerm => State<string>.Value(this, () => "");

    // ListFeed with selection support
    public IListFeed<ProductDto> FilteredProducts => SearchTerm

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
