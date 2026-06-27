---
name: dotnet-localization
category: developer-experience
subcategory: project
description: Localizes .NET apps. .resx resources, IStringLocalizer, source generators, pluralization, RTL.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for csharp tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-localization

Comprehensive .NET internationalization and localization: .resx resource files and satellite assemblies, modern
alternatives (JSON resources, source generators for AOT), IStringLocalizer patterns, date/number/currency formatting
with CultureInfo, RTL layout support, pluralization engines, and per-framework localization integration for Blazor,
MAUI, Uno Platform, and WPF.

**Version assumptions:** .NET 8.0+ baseline. IStringLocalizer stable since .NET Core 1.0; localization APIs stable since
.NET 5. .NET 9+ features explicitly marked.

## Scope

- .resx resource files, satellite assemblies, culture fallback chains
- IStringLocalizer patterns and DI registration
- Modern alternatives: JSON resources, source generators for AOT
- Date/number/currency formatting with CultureInfo
- RTL layout support per framework (Blazor, MAUI, Uno, WPF)
- Pluralization engines (MessageFormat.NET, SmartFormat.NET)

## Out of scope

- Deep Blazor component patterns -- see [skill:dotnet-blazor-components]
- Deep MAUI development patterns -- see [skill:dotnet-maui-development]
- Uno Platform project structure and Extensions ecosystem -- see [skill:dotnet-uno-platform]
- WPF Host builder and MVVM patterns -- see [skill:dotnet-wpf-modern]
- Source generator authoring (Roslyn API) -- see [skill:dotnet-csharp-source-generators]

Cross-references: [skill:dotnet-blazor-components] for Blazor component lifecycle, [skill:dotnet-maui-development] for
MAUI app structure, [skill:dotnet-uno-platform] for Uno Extensions and x:Uid, [skill:dotnet-wpf-modern] for WPF on
modern .NET.

---

## .resx Resource Files

### Overview

Resource files (`.resx`) are the standard .NET localization format. They compile into satellite assemblies resolved by
`ResourceManager` with automatic culture fallback.

### Culture Fallback Chain

Resources resolve in order of specificity, falling back until a match is found:

````text

sr-Cyrl-RS.resx -> sr-Cyrl.resx -> sr.resx -> Resources.resx (default/neutral)

```text

The default `.resx` file (no culture suffix) is the single source of truth. Translation files must not contain keys
absent from the default file.

### Project Setup

```xml

<!-- MyApp.csproj -->
<PropertyGroup>
  <NeutralLanguage>en-US</NeutralLanguage>
</PropertyGroup>

<ItemGroup>
  <!-- Default resources -->
  <EmbeddedResource Include="Resources\Messages.resx" />
  <!-- Culture-specific resources -->
  <EmbeddedResource Include="Resources\Messages.fr-FR.resx" />
  <EmbeddedResource Include="Resources\Messages.de-DE.resx" />
</ItemGroup>

```text

### Resource File Structure

```xml

<!-- Resources/Messages.resx (default/neutral) -->
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Welcome" xml:space="preserve">
    <value>Welcome to the application</value>
    <comment>Shown on the home page</comment>
  </data>
  <data name="ItemCount" xml:space="preserve">
    <value>You have {0} item(s)</value>
    <comment>{0} = number of items</comment>
  </data>
</root>

```text

### Accessing Resources

```csharp

// Via generated strongly-typed class (ResXFileCodeGenerator custom tool)
string welcome = Messages.Welcome;

// Via ResourceManager directly
var rm = new ResourceManager("MyApp.Resources.Messages",
    typeof(Messages).Assembly);
string welcome = rm.GetString("Welcome", CultureInfo.CurrentUICulture);

```text

---

## Modern Alternatives

### JSON-Based Resources

Lightweight alternative for projects already using JSON for configuration. Libraries provide `IStringLocalizer`
implementations backed by JSON files.

```json

// Resources/en-US.json
{
  "Welcome": "Welcome to the application",
  "ItemCount": "You have {0} item(s)"
}

```text

**Libraries:**

- `Senlin.Mo.Localization` -- JSON-backed `IStringLocalizer`
- `Embedded.Json.Localization` -- embedded JSON resources

JSON resources are popular in ASP.NET Core but lack the built-in tooling support (Visual Studio designer, satellite
assembly compilation) of `.resx`.

### Source Generators for AOT Compatibility

Traditional `.resx` with `ResourceManager` uses reflection at runtime, which is problematic for Native AOT and trimming.
Source generators eliminate runtime reflection by generating strongly-typed accessor classes at compile time.

**Recommended source generators:**

| Generator                        | Description                                                                | AOT-Safe                                                           |
| -------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| ResXGenerator (ycanardeau)       | Strongly-typed classes with `IStringLocalizer` support and DI registration | Yes                                                                |
| VocaDb.ResXFileCodeGenerator     | Original strongly-typed `.resx` source generator                           | Yes                                                                |
| Built-in `ResXFileCodeGenerator` | Visual Studio custom tool (not a Roslyn source generator)                  | No -- generates static properties but still uses `ResourceManager` |

```xml

<!-- Using ResXGenerator -->
<ItemGroup>
  <PackageReference Include="ResXGenerator" Version="1.*"
                    PrivateAssets="all" />
</ItemGroup>

```text

```csharp

// Generated at compile time -- no runtime reflection
string welcome = Messages.Welcome;

// With DI registration (ResXGenerator)
services.AddResXLocalization();

```text

**Recommendation:** Use `.resx` files as the resource format (broadest tooling support) with a source generator for
AOT/trimming scenarios. Use JSON resources only for lightweight or config-heavy projects.

---

## IStringLocalizer Patterns

### Registration

```csharp

var builder = WebApplication.CreateBuilder(args);

// Register localization services
builder.Services.AddLocalization(options =>
    options.ResourcesPath = "Resources");

var app = builder.Build();

// Configure request localization middleware
var supportedCultures = new[] { "en-US", "fr-FR", "de-DE", "ja-JP" };
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(supportedCultures[0])
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

```text

### IStringLocalizer<T>

The primary localization interface. Injectable via DI. Use everywhere: services, controllers, Blazor components,
middleware.

```csharp

public class OrderService
{
    private readonly IStringLocalizer<OrderService> _localizer;

    public OrderService(IStringLocalizer<OrderService> localizer)
    {
        _localizer = localizer;
    }

    public string GetConfirmation(int orderId)
    {
        // Indexer returns LocalizedString with implicit string conversion
        return _localizer["OrderConfirmed", orderId];
        // Resolves: "Order {0} confirmed" with orderId substituted
    }

    public bool IsTranslated(string key)
    {
        LocalizedString result = _localizer[key];
        return !result.ResourceNotFound;
    }
}

```text

### IViewLocalizer (MVC Razor Views Only)

Auto-resolves resource files matching the view path. Not supported in Blazor.

```cshtml

@* Views/Home/Index.cshtml *@
@inject IViewLocalizer Localizer

<h1>@Localizer["Welcome"]</h1>
<p>@Localizer["ItemCount", Model.Count]</p>

```text

Resource file location: `Resources/Views/Home/Index.en-US.resx`

### IHtmlLocalizer (MVC Only)

HTML-aware variant that HTML-encodes format arguments but preserves HTML in the resource string itself. Not supported in
Blazor.

```cshtml

@inject IHtmlLocalizer<SharedResource> HtmlLocalizer

@* Resource: "Read our <a href='/terms'>terms</a>, {0}" *@
@* {0} is HTML-encoded, the <a> tag is preserved *@
<p>@HtmlLocalizer["TermsNotice", Model.UserName]</p>

```text

### When to Use Each

| Interface             | Scope              | HTML-Safe       | Blazor | MVC |
| --------------------- | ------------------ | --------------- | ------ | --- |
| `IStringLocalizer<T>` | Everywhere         | No (plain text) | Yes    | Yes |
| `IViewLocalizer`      | View-local strings | No              | **No** | Yes |
| `IHtmlLocalizer<T>`   | HTML in resources  | Yes             | **No** | Yes |

### Namespace Resolution

If resource lookup fails, check namespace alignment. `IStringLocalizer<T>` resolves resources using the full type name
of `T` relative to the `ResourcesPath`. Use `RootNamespaceAttribute` to fix namespace/assembly mismatches:

```csharp

[assembly: RootNamespace("MyApp")]

```csharp

---

## Date, Number, and Currency Formatting

### CultureInfo

`CultureInfo` is the central class for culture-specific formatting. Two distinct properties control behavior:

- `CultureInfo.CurrentCulture` -- controls **formatting** (dates, numbers, currency)
- `CultureInfo.CurrentUICulture` -- controls **resource lookup** (which `.resx` file)

```csharp

// Always pass explicit CultureInfo -- never rely on thread defaults in server code
var date = DateTime.Now.ToString("D", new CultureInfo("fr-FR"));
// "vendredi 14 fevrier 2026"

var price = 1234.56m.ToString("C", new CultureInfo("de-DE"));
// "1.234,56 EUR" (uses NumberFormatInfo.CurrencySymbol)

var number = 1234567.89.ToString("N2", new CultureInfo("ja-JP"));
// "1,234,567.89"

```text

### Server-Side Best Practices

```csharp

// Use useUserOverride: false in server scenarios to avoid
// picking up user-customized formats
var culture = new CultureInfo("en-US", useUserOverride: false);

// Set culture per-request (ASP.NET Core middleware handles this)
CultureInfo.CurrentCulture = culture;
CultureInfo.CurrentUICulture = culture;

```text

### Format Specifiers

| Specifier | Type       | Example (en-US)           | Example (de-DE)           |
| --------- | ---------- | ------------------------- | ------------------------- |
| `"d"`     | Short date | 2/14/2026                 | 14.02.2026                |
| `"D"`     | Long date  | Friday, February 14, 2026 | Freitag, 14. Februar 2026 |
| `"C"`     | Currency   | $1,234.56                 | 1.234,56 EUR              |
| `"N2"`    | Number     | 1,234.57                  | 1.234,57                  |
| `"P1"`    | Percent    | 85.5%                     | 85,5 %                    |

---

## RTL Support

### Detecting RTL Cultures

```csharp

bool isRtl = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
// true for: ar-*, he-*, fa-*, ur-*, etc.

```csharp

### Per-Framework RTL Patterns

**Blazor:** No native `FlowDirection` -- use CSS `dir` attribute:

```javascript

// wwwroot/js/app.js
window.setDocumentDirection = dir => (document.documentElement.dir = dir);

```javascript

```csharp

// Set via named JS function (avoid eval -- causes CSP unsafe-eval violations)
await JSRuntime.InvokeVoidAsync("setDocumentDirection",
    isRtl ? "rtl" : "ltr");

```csharp

For deep Blazor component patterns, see [skill:dotnet-blazor-components].

**MAUI:** `FlowDirection` property on `VisualElement` and `Window`:

```csharp

// Set at window level -- cascades to all children
window.FlowDirection = isRtl
    ? FlowDirection.RightToLeft
    : FlowDirection.LeftToRight;

```text

Android requires `android:supportsRtl="true"` in AndroidManifest.xml (set by default in MAUI). For deep MAUI patterns,
see [skill:dotnet-maui-development].

**Uno Platform:** Inherits WinUI `FlowDirection` model:

```xml


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
