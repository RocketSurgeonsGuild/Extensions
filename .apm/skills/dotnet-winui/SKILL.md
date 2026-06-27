---
name: dotnet-winui
category: ui-frameworks
subcategory: winui
description: Builds WinUI 3 desktop apps. Windows App SDK, XAML patterns, MSIX/unpackaged, UWP migration.
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

# dotnet-winui

WinUI 3 / Windows App SDK development: project setup with `UseWinUI` and Windows 10 TFM, XAML patterns with compiled
bindings (`x:Bind`) and deferred loading (`x:Load`), MVVM with CommunityToolkit.Mvvm, MSIX and unpackaged deployment
modes, Windows integration (lifecycle, notifications, widgets), UWP migration guidance, and common agent pitfalls.

**Version assumptions:** .NET 8.0+ baseline. Windows App SDK 1.6+ (current stable). TFM `net8.0-windows10.0.19041.0`.
.NET 9 features explicitly marked.

## Scope

- WinUI 3 project setup (UseWinUI, Windows 10 TFM)
- XAML patterns (x:Bind compiled bindings, x:Load deferred loading)
- MVVM with CommunityToolkit.Mvvm
- MSIX and unpackaged deployment modes
- Windows integration (lifecycle, notifications, widgets)
- UWP migration guidance

## Out of scope

- Desktop UI testing (Appium, WinAppDriver) -- see [skill:dotnet-ui-testing-core]
- General Native AOT patterns -- see [skill:dotnet-native-aot]
- UI framework selection decision tree -- see [skill:dotnet-ui-chooser]
- WPF patterns -- see [skill:dotnet-wpf-modern]

Cross-references: [skill:dotnet-ui-testing-core] for desktop testing, [skill:dotnet-wpf-modern] for WPF patterns,
[skill:dotnet-wpf-migration] for migration guidance, [skill:dotnet-native-aot] for general AOT,
[skill:dotnet-ui-chooser] for framework selection, [skill:dotnet-native-interop] for general P/Invoke patterns (CsWin32
generates P/Invoke declarations), [skill:dotnet-accessibility] for accessibility patterns (AutomationProperties,
AutomationPeer, UI Automation).

---

## Project Setup

WinUI 3 uses the Windows App SDK (formerly Project Reunion) as its runtime and API layer. Projects target a Windows 10
version-specific TFM.

````xml

<!-- MyWinUIApp.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <UseWinUI>true</UseWinUI>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- Windows App SDK version (auto-referenced via UseWinUI) -->
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
    <PackageReference Include="CommunityToolkit.WinUI.Controls.SettingsControls" Version="8.*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.*" />
  </ItemGroup>
</Project>

```text

### Project Layout

```text

MyWinUIApp/
  App.xaml / App.xaml.cs        # Application entry, resource dictionaries
  MainWindow.xaml / .xaml.cs    # Main window
  ViewModels/                   # MVVM ViewModels
  Views/                        # XAML pages (for Frame navigation)
  Models/                       # Data models
  Services/                     # Service interfaces and implementations
  Assets/                       # Images, icons
  Package.appxmanifest          # MSIX manifest (packaged mode)
  Properties/
    launchSettings.json

```json

### Host Builder Pattern

Modern WinUI apps use the generic host for dependency injection and service configuration:

```csharp

// App.xaml.cs
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        this.InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IProductService, ProductService>();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<ProductDetailViewModel>();

                // Views
                services.AddTransient<MainPage>();
                services.AddTransient<ProductDetailPage>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Closed += async (_, _) =>
        {
            await _host.StopAsync();
            _host.Dispose();
        };
        mainWindow.Activate();
    }

    public static T GetService<T>() where T : class
    {
        var app = (App)Application.Current;
        return app._host.Services.GetRequiredService<T>();
    }
}

```text

### TFM Requirements

The `net8.0-windows10.0.19041.0` TFM specifies:
- **.NET 8.0** -- the runtime version
- **Windows 10 build 19041** (version 2004) -- the minimum Windows SDK version

Windows App SDK features may require higher SDK versions:
- **Widgets (Windows 11):** `net8.0-windows10.0.22000.0` (Windows 11 build 22000)
- **Mica backdrop:** `net8.0-windows10.0.22000.0`
- **Snap layouts integration:** `net8.0-windows10.0.22000.0`

---

## XAML Patterns

WinUI 3 XAML is distinct from UWP XAML. The root namespace is `Microsoft.UI.Xaml`, not `Windows.UI.Xaml`.

### Compiled Bindings (x:Bind)

`x:Bind` provides compile-time type checking and better performance than `{Binding}`. It resolves properties relative to the code-behind class (not the `DataContext`).

```xml

<Page x:Class="MyApp.Views.ProductListPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:vm="using:MyApp.ViewModels">

    <Page.Resources>
        <!-- x:Bind resolves against code-behind, so expose ViewModel as property -->
    </Page.Resources>

    <StackPanel Padding="16" Spacing="12">
        <TextBox Text="{x:Bind ViewModel.SearchTerm, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
        <Button Content="Search" Command="{x:Bind ViewModel.SearchCommand}" />

        <ListView ItemsSource="{x:Bind ViewModel.Products, Mode=OneWay}"
                  SelectionMode="Single">
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="vm:ProductViewModel">
                    <StackPanel Orientation="Horizontal" Spacing="12" Padding="8">
                        <Image Source="{x:Bind ImageUrl}" Height="60" Width="60" />
                        <StackPanel>
                            <TextBlock Text="{x:Bind Name}" Style="{StaticResource BodyStrongTextBlockStyle}" />
                            <TextBlock Text="{x:Bind Price}" Style="{StaticResource CaptionTextBlockStyle}" />
                        </StackPanel>
                    </StackPanel>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
    </StackPanel>
</Page>

```text

```csharp

// Code-behind: expose ViewModel property for x:Bind
public sealed partial class ProductListPage : Page
{
    public ProductListViewModel ViewModel { get; }

    public ProductListPage()
    {
        ViewModel = App.GetService<ProductListViewModel>();
        this.InitializeComponent();
    }
}

```text

**Key differences from `{Binding}`:**
- `x:Bind` is resolved at compile time (type-safe, faster)
- Default mode is `OneTime` (not `OneWay` like `{Binding}`)
- Resolves against the code-behind class, not `DataContext`
- Requires `x:DataType` in `DataTemplate` items

### Deferred Loading (x:Load)

Use `x:Load` to defer element creation until needed, reducing initial page load time:

```xml

<StackPanel>
    <TextBlock Text="Always visible" />

    <!-- This panel is not created until ShowDetails is true -->
    <StackPanel x:Load="{x:Bind ViewModel.ShowDetails, Mode=OneWay}" x:Name="DetailsPanel">
        <TextBlock Text="Detail content loaded on demand" />
        <ListView ItemsSource="{x:Bind ViewModel.DetailItems, Mode=OneWay}" />
    </StackPanel>
</StackPanel>

```text

**When to use `x:Load`:** Heavy UI sections (complex lists, settings panels, detail views) that are not immediately visible. The element is created when the bound property becomes `true` and destroyed when it becomes `false`.

### NavigationView Pattern

WinUI apps typically use `NavigationView` with a `Frame` for page navigation:

```xml

<!-- MainWindow.xaml -->
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <NavigationView x:Name="NavView"
                    IsBackButtonVisible="Collapsed"
                    SelectionChanged="NavView_SelectionChanged">
        <NavigationView.MenuItems>
            <NavigationViewItem Content="Home" Tag="home" Icon="Home" />
            <NavigationViewItem Content="Products" Tag="products" Icon="Shop" />
            <NavigationViewItem Content="Settings" Tag="settings" Icon="Setting" />
        </NavigationView.MenuItems>

        <Frame x:Name="ContentFrame" />
    </NavigationView>
</Window>

```text

---

## MVVM

WinUI 3 integrates with CommunityToolkit.Mvvm (the same MVVM Toolkit used by MAUI). Source generators eliminate boilerplate for properties and commands.

```csharp

// ViewModels/ProductListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ProductListViewModel : ObservableObject
{
    private readonly IProductService _productService;

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchTerm = "";

    [ObservableProperty]
    private ObservableCollection<ProductViewModel> _products = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _showDetails;

    [RelayCommand]
    private async Task LoadProductsAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var items = await _productService.GetProductsAsync(ct);
            Products = new ObservableCollection<ProductViewModel>(
                items.Select(p => new ProductViewModel(p)));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync(CancellationToken ct)
    {
        var results = await _productService.SearchAsync(SearchTerm, ct);
        Products = new ObservableCollection<ProductViewModel>(
            results.Select(p => new ProductViewModel(p)));
    }

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchTerm);
}

```text

**Key source generator attributes:**
- `[ObservableProperty]` -- generates property with `INotifyPropertyChanged` from a backing field
- `[RelayCommand]` -- generates `ICommand` from a method (supports async, cancellation, `CanExecute`)
- `[NotifyPropertyChangedFor]` -- raises `PropertyChanged` for dependent properties
- `[NotifyCanExecuteChangedFor]` -- re-evaluates command `CanExecute` when property changes

---

## Packaging

WinUI 3 supports two deployment models: MSIX packaged and unpackaged. The choice affects app identity, capabilities, and distribution.

### MSIX Packaged Deployment

MSIX is the default packaging model. It provides app identity, clean install/uninstall, automatic updates, and access to full Windows integration APIs.

```xml

<!-- Package.appxmanifest declares app identity and capabilities -->
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10">

  <Identity Name="MyApp" Publisher="CN=Contoso" Version="1.0.0.0" />

  <Applications>
    <Application Id="App"
                 Executable="$targetnametoken$.exe"
                 EntryPoint="$targetentrypoint$">
      <uap:VisualElements DisplayName="My App"
                          Description="WinUI 3 application"
                          BackgroundColor="transparent"
                          Square150x150Logo="Assets\Square150x150Logo.png"
                          Square44x44Logo="Assets\Square44x44Logo.png" />
    </Application>
  </Applications>

  <Capabilities>
    <Capability Name="internetClient" />
  </Capabilities>
</Package>

```text

```bash

# Build MSIX package
dotnet publish -c Release -r win-x64

```bash

### Unpackaged Deployment

Unpackaged mode removes MSIX requirements. The app runs as a standard Win32 executable without app identity.

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
