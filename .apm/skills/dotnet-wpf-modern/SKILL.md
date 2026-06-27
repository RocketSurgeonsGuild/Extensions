---
name: dotnet-wpf-modern
category: ui-frameworks
subcategory: wpf
description: Builds WPF on .NET 8+. Host builder, MVVM Toolkit, Fluent theme, performance, modern C# patterns.
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

# dotnet-wpf-modern

WPF on .NET 8+: Host builder and dependency injection, MVVM with CommunityToolkit.Mvvm source generators,
hardware-accelerated rendering improvements, modern C# patterns (records, primary constructors, pattern matching),
Fluent theme (.NET 9+), system theme detection, and what changed from .NET Framework WPF.

**Version assumptions:** .NET 8.0+ baseline (current LTS). TFM `net8.0-windows`. .NET 9 features (Fluent theme)
explicitly marked.

## Scope

- WPF .NET 8+ project setup (SDK-style)
- Host builder and dependency injection
- MVVM with CommunityToolkit.Mvvm source generators
- Fluent theme (.NET 9+) and system theme detection
- Hardware-accelerated rendering improvements
- Modern C# patterns (records, primary constructors, pattern matching)

## Out of scope

- WPF .NET Framework patterns (legacy)
- Migration guidance -- see [skill:dotnet-wpf-migration]
- Desktop testing -- see [skill:dotnet-ui-testing-core]
- General Native AOT patterns -- see [skill:dotnet-native-aot]
- UI framework selection -- see [skill:dotnet-ui-chooser]

Cross-references: [skill:dotnet-ui-testing-core] for desktop testing, [skill:dotnet-winui] for WinUI 3 patterns,
[skill:dotnet-wpf-migration] for migration guidance, [skill:dotnet-native-aot] for general AOT,
[skill:dotnet-ui-chooser] for framework selection, [skill:dotnet-accessibility] for accessibility patterns
(AutomationProperties, AutomationPeer, UI Automation).

---

## .NET 8+ Differences

WPF on .NET 8+ is a significant modernization from .NET Framework WPF. The project format, DI pattern, language
features, and runtime behavior have all changed.

### New Project Template

````xml

<!-- MyWpfApp.csproj (SDK-style) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.*" />
  </ItemGroup>
</Project>

```text

**Key differences from .NET Framework WPF:**
- SDK-style `.csproj` (no `packages.config`, no `AssemblyInfo.cs`)
- Nullable reference types enabled by default
- Implicit usings enabled
- NuGet `PackageReference` format (not `packages.config`)
- No `App.config` for DI -- use Host builder
- `dotnet publish` produces a single deployment artifact
- Side-by-side .NET installation (no machine-wide framework dependency)

### Host Builder Pattern

Modern WPF apps use the generic host for dependency injection, configuration, and logging -- replacing the legacy `ServiceLocator` or manual DI approaches.

```csharp

// App.xaml.cs
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IProductService, ProductService>();
                services.AddSingleton<ISettingsService, SettingsService>();

                // HTTP client
                services.AddHttpClient("api", client =>
                {
                    client.BaseAddress = new Uri(
                        context.Configuration["ApiBaseUrl"] ?? "https://api.example.com");
                });

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<ProductListViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Windows and pages
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        var app = (App)Application.Current;
        return app._host.Services.GetRequiredService<T>();
    }
}

```text

---

## MVVM Toolkit

CommunityToolkit.Mvvm (Microsoft MVVM Toolkit) is the recommended MVVM framework for modern WPF. It uses source generators to eliminate boilerplate.

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
    private ObservableCollection<Product> _products = [];

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadProductsAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var items = await _productService.GetProductsAsync(ct);
            Products = new ObservableCollection<Product>(items);
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
        Products = new ObservableCollection<Product>(results);
    }

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchTerm);
}

```text

### XAML Binding with MVVM Toolkit

```xml

<Window x:Class="MyApp.Views.ProductListWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:vm="clr-namespace:MyApp.ViewModels"
        d:DataContext="{d:DesignInstance vm:ProductListViewModel}">

    <DockPanel>
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="16">
            <TextBox Text="{Binding SearchTerm, UpdateSourceTrigger=PropertyChanged}"
                     Width="300" Margin="0,0,8,0" />
            <Button Content="Search" Command="{Binding SearchCommand}" />
        </StackPanel>

        <ListBox ItemsSource="{Binding Products}" Margin="16">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal" Margin="4">
                        <TextBlock Text="{Binding Name}" FontWeight="Bold" Margin="0,0,12,0" />
                        <TextBlock Text="{Binding Price, StringFormat='{}{0:C}'}" Foreground="Gray" />
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </DockPanel>
</Window>

```text

**Key source generator attributes:**
- `[ObservableProperty]` -- generates property with `INotifyPropertyChanged` from a backing field
- `[RelayCommand]` -- generates `ICommand` from a method (supports async, cancellation, `CanExecute`)
- `[NotifyPropertyChangedFor]` -- raises `PropertyChanged` for dependent properties
- `[NotifyCanExecuteChangedFor]` -- re-evaluates command `CanExecute` when property changes

---

## Performance

WPF on .NET 8+ delivers significant performance improvements over .NET Framework WPF.

### Hardware-Accelerated Rendering

- **DirectX 11 rendering path** is the default on .NET 8+ (up from DirectX 9 on .NET Framework)
- **GPU-accelerated text rendering** improves text clarity and reduces CPU usage for text-heavy UIs
- **Reduced GC pressure** from runtime improvements (dynamic PGO, on-stack replacement)

### Startup Time

- **ReadyToRun (R2R)** -- pre-compiled assemblies reduce JIT overhead at startup
- **Tiered compilation** -- fast startup with progressive optimization
- **Trimming readiness** -- `.NET 8+` WPF supports IL trimming for smaller deployment size

```xml

<!-- Enable trimming for smaller deployment -->
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>partial</TrimMode>
  <!-- WPF apps need partial trim mode due to reflection usage -->
</PropertyGroup>

```text

**Trimming caveat:** WPF relies heavily on XAML reflection for data binding and resource resolution. Use `TrimMode=partial` (not `full`) and test thoroughly. Compiled bindings and `x:Type` references are safer than string-based bindings for trimming.

### Memory and GC

- **Frozen object heap** (.NET 8) -- static strings and singleton allocations placed on non-collected heap segments
- **Dynamic PGO** -- runtime profiles guide JIT optimizations for hot paths
- **Reduced working set** -- .NET 8 runtime uses less baseline memory than .NET Framework CLR

### Expected Improvements

WPF on .NET 8 delivers measurable improvements over .NET Framework 4.8 across key metrics. Exact numbers depend on workload, hardware, and application complexity -- always benchmark your own scenarios:

- **Cold startup** -- significantly faster due to ReadyToRun, tiered compilation, and reduced framework initialization overhead
- **UI virtualization** -- improved rendering pipeline and GC reduce time for large ItemsControls (ListBox, DataGrid)
- **GC pauses** -- shorter and less frequent Gen2 collections from .NET 8 GC improvements (Dynamic PGO, frozen object heap, pinned object heap)
- **Memory footprint** -- lower baseline working set compared to .NET Framework CLR

---

## Modern C#

.NET 8+ WPF projects can use the latest C# language features. These patterns reduce boilerplate and improve code clarity.

### Records for Data Models

```csharp

// Immutable data models
public record Product(string Name, decimal Price, string Category);

// Records with computed properties
public record ProductViewModel(Product Product)
{
    public string DisplayPrice => Product.Price.ToString("C");
    public string Summary => $"{Product.Name} - {DisplayPrice}";
}

```text

### Primary Constructors in Services

```csharp

// Service with primary constructor (C# 12)
public class ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    : IProductService
{
    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct)
    {
        logger.LogInformation("Fetching products");
        var response = await httpClient.GetAsync("/products", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Product>>(ct) ?? [];
    }
}

```json

### Pattern Matching in Converters

```csharp

// Modern converter using pattern matching (C# 11+)
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            OrderStatus.Pending => Brushes.Orange,
            OrderStatus.Processing => Brushes.Blue,
            OrderStatus.Shipped => Brushes.Green,
            OrderStatus.Cancelled => Brushes.Red,
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

```text

### Collection Expressions

```csharp

// C# 12 collection expressions
[ObservableProperty]
private ObservableCollection<Product> _products = [];

// In methods
List<string> categories = ["Electronics", "Clothing", "Books"];

```text

---

## Theming

### Fluent Theme (.NET 9+)

.NET 9 introduces the Fluent theme for WPF, providing modern Windows 11-style visuals. It applies rounded corners, updated control templates, and Mica/Acrylic backdrop support.

```xml

<!-- App.xaml: enable Fluent theme (.NET 9+) via ThemeMode property -->
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             ThemeMode="System"
             StartupUri="MainWindow.xaml">
</Application>

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
