</Project>

```text

This ensures all projects in the solution share the same polyfill setup. Individual projects set their own `<TargetFrameworks>`.

---

## API Compatibility Validation

When publishing a NuGet package that targets multiple TFMs, validate that the public API surface is consistent and that you have not accidentally broken consumers.

### EnablePackageValidation

Package validation runs automatically during `dotnet pack` and checks:
- **Baseline validation:** Compares the current package against a previous version to detect breaking changes.
- **Compatible framework validation:** Ensures APIs available on one TFM are available on all compatible TFMs.

```xml

<PropertyGroup>
  <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
  <!-- Enable package validation during pack -->
  <EnablePackageValidation>true</EnablePackageValidation>
  <!-- Compare against last published version for breaking change detection -->
  <PackageValidationBaselineVersion>1.2.0</PackageValidationBaselineVersion>
</PropertyGroup>

```text

### API Compatibility Workflow

**Step 1: Enable validation in .csproj**

```xml

<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
</PropertyGroup>

```xml

**Step 2: Set baseline version (for existing packages)**

```xml

<PropertyGroup>
  <!-- The last published stable version to compare against -->
  <PackageValidationBaselineVersion>2.0.0</PackageValidationBaselineVersion>
</PropertyGroup>

```text

**Step 3: Pack and check**

```bash

# Pack triggers validation automatically
dotnet pack --configuration Release

# Success: no output about compatibility issues
# Failure: error messages listing incompatible API changes

```text

**Step 4: Interpret results**

| Result | Meaning | Action |
|--------|---------|--------|
| Clean pack | All TFMs expose compatible API surfaces; no breaking changes from baseline | Ship |
| `CP0001` | Missing type on a compatible TFM | Add the type to the TFM or use `#if` to exclude it from the public API |
| `CP0002` | Missing member on a compatible TFM | Add the member or suppress if intentional |
| `CP0003` | Breaking change from baseline version | Bump major version or revert the change |
| `PKV004` | Compatible TFM has different API surface | Ensure conditional APIs are intentional |

### Suppressing Known Differences

For intentional API differences between TFMs, use a suppression file. Package validation can generate one when suppression generation is enabled:

```bash

# Build with suppression-file generation enabled
dotnet pack /p:ApiCompatGenerateSuppressionFile=true
# Creates CompatibilitySuppressions.xml in the project directory

```bash

The generated `CompatibilitySuppressions.xml` contains targeted suppressions:

```xml

<?xml version="1.0" encoding="utf-8"?>
<Suppressions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
              xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Suppression>
    <DiagnosticId>CP0002</DiagnosticId>
    <Target>M:MyLib.PerformanceHelper.CountVowels(System.ReadOnlySpan{System.Char})</Target>
    <Left>lib/net8.0/MyLib.dll</Left>
    <Right>lib/net10.0/MyLib.dll</Right>
  </Suppression>
</Suppressions>

```text

Reference the suppression file in .csproj (automatic when file is at project root):

```xml

<PropertyGroup>
  <!-- Explicit path if suppression file is not at project root -->
  <ApiCompatSuppressionFile>CompatibilitySuppressions.xml</ApiCompatSuppressionFile>
</PropertyGroup>

```xml

**Prefer targeted suppression files over blanket `<NoWarn>$(NoWarn);CP0002</NoWarn>`** -- blanket suppression hides real issues. Commit the suppression file to source control so reviewers can see intentional API differences.

### ApiCompat Standalone Tool

For CI pipelines that validate without packing:

```bash

# Install as a global tool
dotnet tool install -g Microsoft.DotNet.ApiCompat.Tool

# Global tool invocation (after install -g)
apicompat --left-assembly bin/Release/net8.0/MyLib.dll \
          --right-assembly bin/Release/net10.0/MyLib.dll

# Or install as a local tool (preferred for CI reproducibility)
dotnet new tool-manifest   # if .config/dotnet-tools.json doesn't exist
dotnet tool install Microsoft.DotNet.ApiCompat.Tool

# Local tool invocation
dotnet tool run apicompat --left-assembly bin/Release/net8.0/MyLib.dll \
                          --right-assembly bin/Release/net10.0/MyLib.dll

```text

---

## Agent Gotchas

1. **Do not use `#if` for language feature polyfills.** If the gap is a compiler attribute or syntax feature (e.g., `required`, `init`, `SetsRequiredMembers`), use PolySharp. `#if` blocks for language features create unnecessary code duplication and maintenance burden.

1. **Do not omit `<PrivateAssets>all</PrivateAssets>` on polyfill packages.** PolySharp and SimonCropp/Polyfill are source generators meant for compile time only. Without `PrivateAssets=all`, the polyfill types leak into your package's dependency graph and can conflict with consumers' own polyfills.

1. **Do not hardcode TFM versions in conditional compilation.** Use `NET10_0_OR_GREATER`-style range symbols instead of `NET10_0` exact symbols. Exact symbols break when a new TFM is added (e.g., net11.0 would skip the net10.0-specific path). Range symbols automatically include future TFMs.

1. **Do not set `<LangVersion>` per TFM.** Set it once to the highest version needed across all TFMs (e.g., `<LangVersion>14</LangVersion>`). PolySharp and Polyfill handle the backporting. Per-TFM LangVersion causes confusing syntax errors.

1. **Do not skip `EnablePackageValidation` for multi-targeted NuGet packages.** Without it, you can accidentally expose different API surfaces on different TFMs, causing consumer build failures when they switch TFMs.

1. **Do not use `$(TargetFramework)` string equality for range checks in MSBuild conditions.** Use `$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net10.0'))` for forward-compatible range checks. String equality (e.g., `== 'net10.0'`) misses net11.0 and higher.

1. **Do not re-implement TFM detection.** This skill consumes the structured output from [skill:dotnet-version-detection]. Never parse `.csproj` files to determine TFMs -- use the detection skill's output (TFM, C# version, SDK version, warnings).

1. **Do not assume polyfills cover runtime behavior.** PolySharp and Polyfill provide compile-time stubs and source-generated implementations. Features that require runtime changes (e.g., runtime-async, GC improvements, JIT optimizations) cannot be polyfilled -- use `#if` for these.

1. **Do not use version-specific TFM globs for platform targets.** Use `net*-android` pattern matching (version-agnostic) instead of `net10.0-android` in documentation and tooling to avoid false negatives when users target different .NET versions.

---

## Prerequisites

- .NET 8.0+ SDK (multi-targeting requires the highest targeted SDK installed)
- `PolySharp` NuGet package (for language feature polyfills)
- `Polyfill` NuGet package by Simon Cropp (for BCL API polyfills)
- `Microsoft.DotNet.ApiCompat.Tool` (optional, for standalone API compatibility checks)
- Output from [skill:dotnet-version-detection] (TFM, C# version, SDK version)

---

## References

> **Last verified: 2026-02-12**

- [PolySharp - Source Generator for Polyfill Attributes](https://github.com/Sergio0694/PolySharp)
- [SimonCropp/Polyfill - Source-Only BCL Polyfills](https://github.com/SimonCropp/Polyfill)
- [.NET Target Framework Monikers](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [Multi-Targeting in .NET](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting)
- [C# Preprocessor Directives](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives)
- [Package Validation Overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview)
- [API Compatibility Overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/overview)
- [MSBuild Target Framework Properties](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#targetframework)
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
