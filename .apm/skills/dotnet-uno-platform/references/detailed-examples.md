    public IListFeed<ProductDto> FilteredProducts => SearchTerm
        .SelectAsync(async (term, ct) =>
            await ProductService.SearchProductsAsync(term, ct))
        .AsListFeed();

    // Command: auto-generated from async method signature
    public async ValueTask AddProduct(CancellationToken ct)
    {
        var term = await SearchTerm;
        await ProductService.AddProductAsync(term, ct);
    }
}

```text

```xml

<!-- ProductPage.xaml -- binds to generated proxy -->
<Page x:Class="MyApp.Views.ProductPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <StackPanel>
        <TextBox Text="{Binding SearchTerm, Mode=TwoWay}" />

        <FeedView Source="{Binding FilteredProducts}">
            <FeedView.ValueTemplate>
                <DataTemplate>
                    <ListView ItemsSource="{Binding Data}">
                        <ListView.ItemTemplate>
                            <DataTemplate>
                                <TextBlock Text="{Binding Name}" />
                            </DataTemplate>
                        </ListView.ItemTemplate>
                    </ListView>
                </DataTemplate>
            </FeedView.ValueTemplate>
            <FeedView.ProgressTemplate>
                <DataTemplate>
                    <ProgressRing IsActive="True" />
                </DataTemplate>
            </FeedView.ProgressTemplate>
            <FeedView.ErrorTemplate>
                <DataTemplate>
                    <TextBlock Text="Error loading products" Foreground="Red" />
                </DataTemplate>
            </FeedView.ErrorTemplate>
        </FeedView>

        <Button Content="Add Product" Command="{Binding AddProduct}" />
    </StackPanel>
</Page>

```bash

### MVUX vs MVVM

| Concern | MVUX | MVVM |
|---------|------|------|
| Model definition | Immutable `record` types | Mutable classes with `INotifyPropertyChanged` |
| Data loading | `IFeed<T>` with built-in loading/error states | Manual loading flags and try/catch |
| Collections | `IListFeed<T>` with immutable snapshots | `ObservableCollection<T>` with mutation |
| Commands | Auto-generated from `async` methods | `ICommand` implementations (RelayCommand) |
| State changes | `IState<T>` with explicit update semantics | Property setters firing `PropertyChanged` |
| Boilerplate | Minimal (source generators) | Significant (base classes, attributes) |

**When to use MVUX:** New Uno Platform projects, especially those with async data sources and complex loading states. MVUX eliminates most boilerplate and handles loading/error states declaratively.

**When to use MVVM:** Projects migrating from existing WPF/UWP/WinUI codebases, teams familiar with MVVM patterns, or projects using CommunityToolkit.Mvvm.

---

## Uno Toolkit Controls

The Uno Toolkit provides cross-platform controls and helpers beyond stock WinUI controls. Enabled via `UnoFeatures` with `Toolkit`.

### Key Controls

| Control | Purpose |
|---------|---------|
| `AutoLayout` | Flexbox-like layout with spacing, padding, and alignment |
| `Card` / `CardContentControl` | Material-style card surfaces with elevation |
| `Chip` / `ChipGroup` | Filter chips, action chips, selection chips |
| `Divider` | Horizontal/vertical separator lines |
| `DrawerControl` | Side drawer (hamburger menu) |
| `LoadingView` | Loading state wrapper with skeleton/shimmer |
| `NavigationBar` | Cross-platform navigation bar |
| `ResponsiveView` | Adaptive layout based on screen width breakpoints |
| `SafeArea` | Insets for notches, status bars, navigation bars |
| `ShadowContainer` | Cross-platform drop shadows via `ThemeShadow` |
| `TabBar` | Bottom or top tab navigation |
| `ZoomContentControl` | Pinch-to-zoom container |

### Toolkit Helpers

| Helper | Purpose |
|--------|---------|
| `CommandExtensions` | Attach commands to any control (not just Button) |
| `ItemsRepeaterExtensions` | Selection and command support for ItemsRepeater |
| `InputExtensions` | Auto-focus, return key command, input scope |
| `ResponsiveMarkupExtensions` | Responsive values in XAML markup (e.g., `Responsive.Narrow`) |
| `StatusBarExtensions` | Control status bar appearance per-platform |
| `AncestorBinding` | Bind to ancestor DataContext in templates |

### AutoLayout Example

```xml

<!-- Vertical stack with spacing, padding, and alignment -->
<utu:AutoLayout Spacing="16" Padding="24"
                PrimaryAxisAlignment="Start"
                CounterAxisAlignment="Stretch">

    <TextBlock Text="Product List"
               Style="{StaticResource HeadlineMedium}" />

    <utu:AutoLayout Spacing="8" Orientation="Horizontal">
        <TextBox PlaceholderText="Search..."
                 utu:AutoLayout.PrimaryLength="*" />
        <Button Content="Search"
                utu:AutoLayout.CounterAlignment="Center" />
    </utu:AutoLayout>

    <ListView ItemsSource="{Binding Products}"
              utu:AutoLayout.PrimaryLength="*" />
</utu:AutoLayout>

```text

---

## Theme Resources

Uno supports Material, Cupertino, and Fluent design systems as theme packages. Themes provide consistent colors, typography, elevation, and control styles across all platforms.

### Theme Configuration

```xml

<!-- UnoFeatures in .csproj -->
<UnoFeatures>Material</UnoFeatures>   <!-- or Cupertino, or both -->

```csharp

```xml

<!-- App.xaml -- theme resource dictionaries -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Material theme resources -->
            <MaterialTheme />

            <!-- Optional: color palette override -->
            <ResourceDictionary Source="ms-appx:///Themes/ColorPaletteOverride.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>

```text

### Color Customization

Override Material theme colors through `ColorPaletteOverride.xaml`:

```xml

<!-- Themes/ColorPaletteOverride.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Color x:Key="PrimaryColor">#6750A4</Color>
    <Color x:Key="SecondaryColor">#625B71</Color>
    <Color x:Key="TertiaryColor">#7D5260</Color>
    <Color x:Key="ErrorColor">#B3261E</Color>
</ResourceDictionary>

```text

### Typography

Use existing TextBlock styles from the theme system. Never set explicit font sizes -- use the Material type scale:

```xml

<TextBlock Text="Headline" Style="{StaticResource HeadlineMedium}" />
<TextBlock Text="Body text" Style="{StaticResource BodyLarge}" />
<TextBlock Text="Caption" Style="{StaticResource LabelSmall}" />

```xml

| Style | Typical Use |
|-------|------------|
| `DisplayLarge/Medium/Small` | Hero text, splash screens |
| `HeadlineLarge/Medium/Small` | Page titles, section headers |
| `TitleLarge/Medium/Small` | Card titles, dialog titles |
| `BodyLarge/Medium/Small` | Paragraph text, descriptions |
| `LabelLarge/Medium/Small` | Button labels, captions, metadata |

### ThemeService

Switch between light and dark themes programmatically:

```csharp

var themeService = serviceProvider.GetRequiredService<IThemeService>();
await themeService.SetThemeAsync(AppTheme.Dark);
var currentTheme = themeService.Theme;

```csharp

---

## Hot Reload

Uno Platform provides Hot Reload across all targets via its custom implementation. Changes to XAML and C# code-behind are reflected without restarting the app.

### Supported Changes

| Change Type | Hot Reload Support |
|-------------|-------------------|
| XAML layout/styling | Full reload, instant |
| C# code-behind (method bodies) | Supported via MetadataUpdateHandler |
| New properties/methods | Requires rebuild |
| Resource dictionary changes | Full reload |
| Navigation route changes | Requires rebuild |

### Enabling Hot Reload

```bash

# Set environment variable before dotnet run
export DOTNET_MODIFIABLE_ASSEMBLIES=debug

# Run with Hot Reload
dotnet run -f net8.0-desktop --project MyApp/MyApp.csproj

```csharp

Hot Reload is automatically configured by Visual Studio and VS Code (with Uno extension). For CLI usage, set `DOTNET_MODIFIABLE_ASSEMBLIES=debug` before running.

**Gotcha:** Hot Reload does not support adding new types, changing inheritance hierarchies, or modifying `UnoFeatures`. These require a full rebuild.

---

## Agent Gotchas

1. **Do not confuse MVUX with MVVM.** MVUX uses immutable records, Feeds, and States -- not `INotifyPropertyChanged`. Do not add `ObservableProperty` attributes to MVUX models.
2. **Do not hardcode NuGet package versions for Uno Extensions.** The `UnoFeatures` MSBuild property resolves packages automatically via the Uno SDK. Adding explicit `PackageReference` items for Extensions can cause version conflicts.
3. **Do not use `{Binding StringFormat=...}`** in Uno XAML. It is a WPF-only feature. Use converters or multiple `<Run>` elements for formatted text.
4. **Do not use `x:Static` or `{x:Reference}` in bindings.** These are WPF-only markup extensions not available in WinUI/Uno.
5. **Do not set explicit font sizes or weights.** Use the theme's TextBlock styles (e.g., `HeadlineMedium`, `BodyLarge`) to maintain design system consistency.
6. **Do not use hardcoded hex colors.** Always reference theme resources (`PrimaryColor`, `SecondaryColor`) or semantic brushes to maintain theme compatibility.
7. **Do not use `AppBarButton` outside a `CommandBar`.** Use regular `Button` with icon content for standalone icon buttons.
8. **Do not forget `x:Uid` for localization.** Every user-visible string should use `x:Uid` referencing `.resw` resources, not hardcoded text.

---

## Prerequisites

- .NET 8.0+ (Uno Platform 5.x baseline)
- Uno SDK (`Uno.Sdk` project SDK)
- Platform workloads as needed: `dotnet workload install ios android maccatalyst wasm-tools`
- Visual Studio 2022+ or VS Code with Uno Platform extension

---

## References

- [Uno Platform Documentation](https://platform.uno/docs/)
- [Uno Extensions Overview](https://platform.uno/docs/articles/external/uno.extensions/)
- [MVUX Pattern](https://platform.uno/docs/articles/external/uno.extensions/doc/Overview/Mvux/Overview.html)
- [Uno Toolkit](https://platform.uno/docs/articles/external/uno.toolkit.ui/)
- [Uno Themes](https://platform.uno/docs/articles/external/uno.themes/)
- [Uno SDK Features](https://platform.uno/docs/articles/features/uno-sdk.html)
````

## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**

- **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**

```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
