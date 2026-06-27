</Application>

```xml

Or in code-behind:

```csharp

// App.xaml.cs: set theme programmatically (.NET 9+)
Application.Current.ThemeMode = ThemeMode.System; // or ThemeMode.Light / ThemeMode.Dark

// Per-window theming is also supported
mainWindow.ThemeMode = ThemeMode.Dark;

```text

**ThemeMode values:**
- `None` -- classic WPF look (no Fluent styling)
- `Light` -- Fluent theme with light colors
- `Dark` -- Fluent theme with dark colors
- `System` -- follow Windows system light/dark theme setting

**Fluent theme includes:**
- Rounded corners on buttons, text boxes, and list items
- Updated color palette aligned with Windows 11 design language
- Mica and Acrylic backdrop support (Windows 11)
- Accent color integration with Windows system settings
- Dark/light mode following system theme

### System Theme Detection

Detect and respond to the Windows system light/dark theme:

```csharp

// Detect system theme
public static bool IsDarkTheme()
{
    using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
    var value = key?.GetValue("AppsUseLightTheme");
    return value is int i && i == 0;
}

// Listen for theme changes
SystemEvents.UserPreferenceChanged += (sender, args) =>
{
    if (args.Category == UserPreferenceCategory.General)
    {
        // Theme may have changed; re-read and apply
        ApplyTheme(IsDarkTheme() ? AppTheme.Dark : AppTheme.Light);
    }
};

```text

### Custom Themes

For pre-.NET 9 apps or custom branding, use resource dictionaries:

```xml

<!-- Themes/DarkTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="WindowBackground" Color="#1E1E1E" />
    <SolidColorBrush x:Key="TextForeground" Color="#FFFFFF" />
    <SolidColorBrush x:Key="AccentBrush" Color="#0078D7" />
</ResourceDictionary>

```text

```csharp

// Switch themes at runtime
public void ApplyTheme(AppTheme theme)
{
    var themeUri = theme switch
    {
        AppTheme.Dark => new Uri("Themes/DarkTheme.xaml", UriKind.Relative),
        AppTheme.Light => new Uri("Themes/LightTheme.xaml", UriKind.Relative),
        _ => throw new ArgumentOutOfRangeException(nameof(theme))
    };

    Application.Current.Resources.MergedDictionaries.Clear();
    Application.Current.Resources.MergedDictionaries.Add(
        new ResourceDictionary { Source = themeUri });
}

```text

---

## Agent Gotchas

1. **Do not use .NET Framework WPF patterns in .NET 8+ projects.** Avoid `App.config` for DI (use Host builder), `packages.config` (use `PackageReference`), `ServiceLocator` pattern (use constructor injection), and `AssemblyInfo.cs` (use `<PropertyGroup>` properties).
2. **Do not use deprecated WPF APIs.** `BitmapEffect` (replaced by `Effect`/`ShaderEffect`), `DrawingContext.PushEffect` (removed), and `VisualBrush` tile modes with hardware acceleration disabled are obsolete.
3. **Do not mix `{Binding}` and manual `INotifyPropertyChanged` when using MVVM Toolkit.** Use `[ObservableProperty]` source generators consistently. Mixing approaches causes subtle binding update bugs.
4. **Do not use `Dispatcher.Invoke` from async code.** In async methods, `await` automatically marshals back to the UI thread (the default `ConfigureAwait(true)` behavior). `Dispatcher.Invoke`/`BeginInvoke` is still appropriate from non-async contexts (timers, COM callbacks, native interop).
5. **Do not set `TrimMode=full` for WPF apps.** WPF uses XAML reflection extensively. Use `TrimMode=partial` and test all views after trimming to catch missing types.
6. **Do not forget the Host builder lifecycle.** Call `_host.StartAsync()` in `OnStartup` and `_host.StopAsync()` in `OnExit`. Forgetting lifecycle management causes DI-registered `IHostedService` instances to never start or stop.
7. **Do not hardcode colors when using Fluent theme.** Reference theme resources (`{DynamicResource SystemAccentColor}`) to maintain compatibility with light/dark mode and system accent color changes.

---

## Prerequisites

- .NET 8.0+ with Windows desktop workload
- TFM: `net8.0-windows` (no Windows SDK version needed for WPF)
- Visual Studio 2022+, VS Code with C# Dev Kit, or JetBrains Rider
- For Fluent theme: .NET 9+

---

## References

- [WPF on .NET Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [WPF Fluent Theme (.NET 9)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90)
- [Microsoft.Extensions.Hosting](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [WPF Performance](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance)
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
