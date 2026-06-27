
<Page FlowDirection="RightToLeft">
  <!-- All children inherit RTL layout -->
</Page>

```xml

For Uno Extensions and x:Uid binding, see [skill:dotnet-uno-platform].

**WPF:** `FlowDirection` property on `FrameworkElement`:

```xml

<Window FlowDirection="RightToLeft">
  <!-- All children inherit RTL layout -->
</Window>

```xml

For WPF on modern .NET patterns, see [skill:dotnet-wpf-modern].

---

## Pluralization

### The Problem

Simple string interpolation fails for pluralization across languages:

```csharp

// WRONG: English-only, breaks in languages with complex plural rules
$"You have {count} item{(count != 1 ? "s" : "")}"

```csharp

Languages like Arabic have six plural forms (zero, one, two, few, many, other). Polish distinguishes "few" from "many"
based on number ranges.

### ICU MessageFormat (MessageFormat.NET)

CLDR-compliant pluralization using ICU plural categories. Recommended for internationalization-first projects.

```csharp

// Package: jeffijoe/messageformat.net (v5.0+, ships CLDR pluralizers)
var formatter = new MessageFormatter();

string pattern = "{count, plural, " +
    "=0 {No items}" +
    "one {# item}" +
    "other {# items}}";

formatter.Format(pattern, new { count = 0 });  // "No items"
formatter.Format(pattern, new { count = 1 });  // "1 item"
formatter.Format(pattern, new { count = 42 }); // "42 items"

```text

### SmartFormat.NET

Flexible text templating with built-in pluralization. Good for projects wanting maximum flexibility.

```csharp

// Package: axuno/SmartFormat (v3.6.1+)
using SmartFormat;

Smart.Format("{count:plural:No items|# item|# items}",
    new { count = 0 });  // "No items"
Smart.Format("{count:plural:No items|# item|# items}",
    new { count = 1 });  // "1 item"
Smart.Format("{count:plural:No items|# item|# items}",
    new { count = 5 });  // "5 items"

```text

### Choosing a Pluralization Engine

| Engine             | CLDR Compliance        | API Style                    | Best For                                      |
| ------------------ | ---------------------- | ---------------------------- | --------------------------------------------- |
| MessageFormat.NET  | Full (CLDR categories) | ICU pattern strings          | Multi-locale apps needing standard compliance |
| SmartFormat.NET    | Partial (extensible)   | .NET format string extension | Flexible templating with pluralization        |
| Manual conditional | None                   | `string.Format` + branching  | Simple English-only dual forms                |

---

## UI Framework Integration

### Blazor Localization

Blazor supports `IStringLocalizer` only -- `IHtmlLocalizer` and `IViewLocalizer` are not available.

**Component injection:**

```razor

@inject IStringLocalizer<MyComponent> Loc

<h1>@Loc["Welcome"]</h1>
<p>@Loc["ItemCount", items.Count]</p>

```text

**Culture configuration by render mode:**

| Render Mode  | Culture Source                                                                       |
| ------------ | ------------------------------------------------------------------------------------ |
| Server / SSR | `RequestLocalizationMiddleware` (server-side)                                        |
| WebAssembly  | `CultureInfo.DefaultThreadCurrentCulture` + Blazor start option `applicationCulture` |
| Auto         | Both -- server middleware for initial load, WASM culture for client-side             |

**WASM globalization data:**

```xml

<!-- Required for full ICU data in Blazor WASM -->
<PropertyGroup>
  <BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>
</PropertyGroup>

```text

Without this property, Blazor WASM loads only a subset of ICU data. For minimal download size, use
`InvariantGlobalization=true` (disables localization entirely).

**Dynamic culture switching:**

```csharp

// CultureSelector component pattern:
// 1. Store selected culture in browser local storage
// 2. Set culture cookie via controller redirect (server-side)
// 3. Read cookie in RequestLocalizationMiddleware

```text

For deep Blazor component patterns (lifecycle, state management, JS interop), see [skill:dotnet-blazor-components].

### MAUI Localization

MAUI uses `.resx` files with strongly-typed generated properties.

**Resource setup:**

```text

Resources/
  Strings/
    AppResources.resx              # Default (neutral) culture
    AppResources.fr-FR.resx        # French
    AppResources.ja-JP.resx        # Japanese

```text

**XAML binding:**

```xml

<!-- Import namespace -->
<ContentPage xmlns:strings="clr-namespace:MyApp.Resources.Strings">

  <!-- Use x:Static for strongly-typed access -->
  <Label Text="{x:Static strings:AppResources.Welcome}" />
  <Button Text="{x:Static strings:AppResources.LoginButton}" />
</ContentPage>

```text

**Code access:**

```csharp

string welcome = AppResources.Welcome;

```csharp

**Platform requirements:**

- iOS/Mac Catalyst: Add `CFBundleLocalizations` to `Info.plist`
- Windows: Add `<Resource Language="...">` entries to `Package.appxmanifest`
- All platforms: Set `<NeutralLanguage>en-US</NeutralLanguage>` in csproj

For deep MAUI development patterns (controls, navigation, platform APIs), see [skill:dotnet-maui-development].

### Uno Platform Localization

Uno uses `.resw` files (Windows resource format) with `x:Uid` for automatic XAML resource binding.

**Resource structure:**

```text

Strings/
  en/Resources.resw
  fr-FR/Resources.resw
  ja-JP/Resources.resw

```text

**Registration:**

```csharp

// In Host builder configuration
.UseLocalization()

```csharp

**XAML binding with x:Uid:**

```xml

<!-- x:Uid maps to resource keys: "MainPage_Title.Text", "LoginButton.Content" -->
<TextBlock x:Uid="MainPage_Title" />
<Button x:Uid="LoginButton" />

```xml

**Runtime culture switching:**

```csharp

var localizationService = serviceProvider
    .GetRequiredService<ILocalizationService>();
await localizationService.SetCurrentCultureAsync(
    new CultureInfo("fr-FR"));
// Note: XAML x:Uid bindings retain old culture until app restart

```text

**Known limitation:** `x:Uid`-based localization keeps the old culture until app restart, even after calling
`SetCurrentCultureAsync`. Code-based `IStringLocalizer` updates immediately.

For Uno Extensions ecosystem configuration and MVUX patterns, see [skill:dotnet-uno-platform].

### WPF Localization

**Recommended approach for .NET 8+:** `.resx` files with `DynamicResource` binding for runtime locale switching. Avoid
LocBaml (works only on .NET Framework).

**Resource dictionary approach:**

```xml

<!-- Resources/Strings.en-US.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:sys="clr-namespace:System;assembly=System.Runtime">
  <sys:String x:Key="Welcome">Welcome</sys:String>
  <sys:String x:Key="LoginButton">Log In</sys:String>
</ResourceDictionary>

```xml

```xml

<!-- MainWindow.xaml -->
<TextBlock Text="{DynamicResource Welcome}" />
<Button Content="{DynamicResource LoginButton}" />

```xml

**Runtime locale switching:**

```csharp

// Swap resource dictionary at runtime
var dict = new ResourceDictionary
{
    Source = new Uri($"Resources/Strings.{cultureName}.xaml",
                     UriKind.Relative)
};
Application.Current.Resources.MergedDictionaries.Clear();
Application.Current.Resources.MergedDictionaries.Add(dict);

```text

**ResX approach (simpler, works on all .NET versions):**

```csharp

// Standard .resx with generated class
string welcome = Strings.Welcome;

// Runtime switch
Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
// Re-read after culture change
string welcomeFr = Strings.Welcome; // Now returns French

```text

**Community options:**

- **WPF Localization Extensions** -- RESX files with XAML markup extensions for declarative localization
- **LocBamlCore** (h3xds1nz) -- unofficial port supporting .NET 9, for BAML localization on modern .NET

For WPF Host builder, MVVM Toolkit, and theming patterns, see [skill:dotnet-wpf-modern].

---

## Agent Gotchas

1. **Do not use `IHtmlLocalizer` or `IViewLocalizer` in Blazor.** These are MVC-only features. Use `IStringLocalizer<T>`
   in Blazor components.
2. **Do not rely on `CultureInfo.CurrentCulture` thread defaults in server code.** Always pass explicit `CultureInfo` to
   formatting methods. Server thread culture may not match the request culture.
3. **Do not hardcode plural forms.** English "singular/plural" does not work for Arabic (6 forms), Polish, or other
   languages. Use MessageFormat.NET or SmartFormat.NET for proper CLDR pluralization.
4. **Do not use LocBaml for WPF on .NET 8+.** LocBaml is a .NET Framework-only sample tool. Use `.resx` files or
   resource dictionaries for modern WPF.
5. **Do not forget `BlazorWebAssemblyLoadAllGlobalizationData` for Blazor WASM.** Without it, only partial ICU data is
   loaded, causing incorrect date/number formatting for many cultures.
6. **Do not add translation keys absent from the default `.resx` file.** The default resource is the single source of
   truth; satellite assemblies must be a subset.
7. **Do not use `ResourceManager` directly in AOT/trimmed apps.** It relies on reflection. Use a source generator
   (ResXGenerator) for compile-time resource access.
8. **Do not forget platform-specific setup for MAUI.** iOS/Mac Catalyst need `CFBundleLocalizations` in `Info.plist`;
   Windows needs `Resource Language` entries.

---
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
