---
name: dotnet-multi-targeting
category: developer-experience
subcategory: project
description: Targets multiple TFMs via polyfills and conditional compilation. PolySharp, API compat.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-multi-targeting

Comprehensive guide for .NET multi-targeting strategies with a polyfill-first approach. This skill consumes the
structured output from [skill:dotnet-version-detection] (TFM, C# version, preview flags) and provides actionable
guidance on backporting language features, handling runtime gaps, and validating API compatibility across target
frameworks.

## Scope

- Decision matrix: polyfill vs conditional compilation
- PolySharp for compiler-synthesized polyfills
- SimonCropp/Polyfill for BCL API backporting
- Conditional compilation with TFM-based preprocessor symbols
- Multi-targeting .csproj patterns and TFM-specific source files
- API compatibility validation (EnablePackageValidation, ApiCompat tool)

## Out of scope

- TFM detection logic -- see [skill:dotnet-version-detection]
- Version upgrade lane selection -- see [skill:dotnet-version-upgrade]
- Platform-specific UI frameworks (MAUI, Blazor) -- see respective framework skills
- Cloud deployment configuration

Cross-references: [skill:dotnet-version-detection] for TFM resolution and version matrix, [skill:dotnet-version-upgrade]
for upgrade lane guidance and migration strategies.

---

## Decision Matrix: Polyfill vs Conditional Compilation

Use this matrix to select the correct strategy for each type of gap between your highest and lowest TFMs.

| Gap Type                    | Strategy                                                | When to Use                                                            | Example                                                                                   |
| --------------------------- | ------------------------------------------------------- | ---------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Language/syntax feature     | Polyfill (PolySharp)                                    | Compiler needs attribute/type stubs to emit newer syntax on older TFMs | `required` modifier, `init` properties, `SetsRequiredMembers` on net8.0                   |
| BCL API addition            | Polyfill (SimonCropp/Polyfill) if available, else `#if` | A newer BCL type or method is missing on older TFMs                    | `System.Threading.Lock` on net8.0, `Index`/`Range` on netstandard2.0                      |
| Runtime behavior difference | Conditional compilation (`#if`) or adapter pattern      | Behavior differs at runtime regardless of compilation                  | Runtime-async (net11.0 only), different GC modes, `SearchValues<T>` runtime optimizations |
| Platform API divergence     | Conditional compilation with `[SupportedOSPlatform]`    | API exists only on specific OS targets                                 | Windows Registry APIs, Android-specific intents, iOS keychain                             |

**Decision flow:**

1. Can a compile-time polyfill satisfy the gap? Use PolySharp or SimonCropp/Polyfill.
2. Is the gap a missing BCL API with no polyfill available? Use `#if` with TFM-specific code.
3. Is the gap a runtime behavior difference? Use `#if` or the adapter pattern to isolate divergent code paths.
4. Is the gap platform-specific? Use `#if` with `[SupportedOSPlatform]` attributes.

---

## PolySharp (Compiler-Synthesized Polyfills)

PolySharp is a source generator that synthesizes the attribute and type stubs the C# compiler needs to emit newer
language features when targeting older TFMs. It operates entirely at compile time -- no runtime dependencies are added.

### What PolySharp Provides

- `required` modifier support (C# 11+)
- `init` property accessors (C# 9+)
- `SetsRequiredMembers` attribute
- `CompilerFeatureRequired` attribute
- `IsExternalInit` type
- `CallerArgumentExpression` attribute
- `StackTraceHidden` attribute
- `UnscopedRef` attribute
- `InterpolatedStringHandler` attributes
- `ModuleInitializer` attribute
- Index and Range support types

### Setup

````xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
    <!-- Use the highest C# version across all TFMs -->
    <LangVersion>14</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- PolySharp is a source generator; it adds no runtime dependency -->
    <PackageReference Include="PolySharp" Version="1.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>

```text

### How It Works

PolySharp detects which polyfill types are missing for the current TFM and generates source for only those types. On net10.0, where `required` is natively supported, the generator emits nothing -- zero overhead.

```csharp

// This compiles on net8.0 WITH PolySharp installed,
// because PolySharp generates the required CompilerFeatureRequired
// and IsExternalInit types that the compiler needs.
public class UserProfile
{
    public required string DisplayName { get; init; }
    public required string Email { get; init; }
    public string? Bio { get; set; }
}

```text

### PolySharp Limitations

- PolySharp provides **compiler stubs only**. It does not backport runtime behavior.
- Features that require runtime support (e.g., runtime-async, `SearchValues<T>` hardware acceleration) cannot be polyfilled.
- If a feature needs both a compiler attribute AND a BCL API (e.g., collection expressions with `Span<T>` overloads), you may need both PolySharp and SimonCropp/Polyfill.

---

## SimonCropp/Polyfill (BCL API Polyfills)

SimonCropp/Polyfill provides source-generated implementations of newer BCL APIs for older TFMs. Unlike PolySharp (which provides compiler attribute stubs), Polyfill provides actual method and type implementations.

### What Polyfill Provides

Key polyfilled APIs (non-exhaustive):

- `System.Threading.Lock` (C# 13 / net9.0+)
- `String.Contains(char)`, `String.Contains(string, StringComparison)`
- `String.ReplaceLineEndings()`
- `HashCode` struct
- `SkipLocalsInit` attribute
- `TaskCompletionSource` (non-generic)
- `Stream.ReadExactly`, `Stream.ReadAtLeast`
- `Memory<T>` and `Span<T>` extensions
- `IReadOnlySet<T>` interface
- Various LINQ additions (`TryGetNonEnumeratedCount`, `DistinctBy`, `Chunk`, etc.)

### Setup

```xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
    <LangVersion>14</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- Polyfill is a source generator; no runtime dependency -->
    <PackageReference Include="Polyfill" Version="7.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>

```text

### Usage Example

```csharp

// System.Threading.Lock is a net9.0+ type.
// With Polyfill installed, this compiles on net8.0.
public class ThrottledProcessor
{
    private readonly Lock _lock = new();

    public void Process(string item)
    {
        lock (_lock)
        {
            // Lock provides better diagnostics than object-based locking
            Console.WriteLine($"Processing: {item}");
        }
    }
}

```text

### Combining PolySharp and Polyfill

For maximum compatibility, use both packages together. They are complementary and do not conflict:

```xml

<ItemGroup>
  <!-- PolySharp: compiler attribute stubs (required, init, etc.) -->
  <PackageReference Include="PolySharp" Version="1.*">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>

  <!-- Polyfill: BCL API implementations (Lock, LINQ additions, etc.) -->
  <PackageReference Include="Polyfill" Version="7.*">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>

```text

With both installed, you get full language feature support (PolySharp) **and** BCL API backporting (Polyfill) on older TFMs.

---

## Conditional Compilation

Use conditional compilation (`#if`) when the gap is a runtime behavior difference or a platform API that cannot be polyfilled at compile time.

### TFM-Based Conditionals

The compiler defines preprocessor symbols for each TFM. Use `NET8_0_OR_GREATER`-style symbols (available since .NET 5) for version range checks:

```csharp

public static class PerformanceHelper
{
#if NET10_0_OR_GREATER
    // net10.0+ has optimized SearchValues with hardware acceleration
    private static readonly SearchValues<char> s_vowels =
        SearchValues.Create("aeiouAEIOU");

    public static int CountVowels(ReadOnlySpan<char> text)
        => text.Count(s_vowels);
#else
    // Fallback for net8.0: manual loop
    public static int CountVowels(ReadOnlySpan<char> text)
    {
        int count = 0;
        foreach (char c in text)
        {
            if ("aeiouAEIOU".Contains(c))
                count++;
        }
        return count;
    }
#endif
}

```text

### Available Preprocessor Symbols

| Symbol | True When |
|--------|-----------|
| `NET8_0` | Exactly net8.0 |
| `NET8_0_OR_GREATER` | net8.0 or any higher version |
| `NET9_0_OR_GREATER` | net9.0 or any higher version |
| `NET10_0_OR_GREATER` | net10.0 or any higher version |
| `NET11_0_OR_GREATER` | net11.0 or any higher version |
| `NETSTANDARD2_0` | Exactly netstandard2.0 |
| `NETSTANDARD2_0_OR_GREATER` | netstandard2.0 or higher |

### When #if Is Correct

1. **Runtime behavior gap:** The API exists on both TFMs but behaves differently at runtime (e.g., `GC.Collect` modes, `HttpClient` connection pooling behavior).
2. **No polyfill available:** The BCL API is not covered by SimonCropp/Polyfill and cannot be stubbed.
3. **Performance-critical path:** You want to use a TFM-specific optimized API path (e.g., `SearchValues<T>`, `FrozenDictionary<K,V>`).
4. **Platform API:** The API is available only on a specific OS platform target.

### When #if Is Wrong

- **Language syntax feature** (e.g., `required`, `init`): Use PolySharp instead.
- **Missing BCL method** that has a polyfill (e.g., `System.Threading.Lock`): Use SimonCropp/Polyfill instead.
- Wrapping entire files in `#if` blocks -- use TFM-specific source files instead (see below).

---

## Multi-Targeting .csproj Patterns

### Basic Multi-Targeting

```xml

<PropertyGroup>
  <!-- Semicolon-delimited list of TFMs -->
  <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
  <!-- Use the highest C# version to access all language features -->
  <LangVersion>14</LangVersion>
</PropertyGroup>

```csharp

### Conditional Package References

Some packages are only needed on specific TFMs:

```xml

<ItemGroup>
  <!-- Polyfill packages: only needed on older TFMs, but safe to reference
       unconditionally because they emit nothing when features are native -->
  <PackageReference Include="PolySharp" Version="1.*">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>

  <!-- TFM-conditional package: only available/needed on specific TFMs -->
  <PackageReference Include="System.Text.Json" Version="9.*"
                    Condition="'$(TargetFramework)' == 'net8.0'" />
</ItemGroup>

```json

### TFM-Specific Source Files

For large blocks of TFM-specific code, use dedicated source files instead of `#if` blocks:

```xml

<ItemGroup>
  <!-- SDK-style projects auto-include all *.cs files. Remove TFM-specific
       directories first to avoid NETSDK1022 duplicate compile items. -->
  <Compile Remove="Compatibility\**\*.cs" />

  <!-- Then conditionally include only the files for the current TFM -->
  <Compile Include="Compatibility\Net8\**\*.cs"
           Condition="'$(TargetFramework)' == 'net8.0'" />
  <Compile Include="Compatibility\Net10\**\*.cs"
           Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net10.0'))" />
</ItemGroup>

```csharp

Directory structure:

```text

MyLibrary/
  Compatibility/
    Net8/
      SearchValuesCompat.cs
    Net10/
      SearchValuesNative.cs
  Services/
    TextAnalyzer.cs          # shared code, references interface
  MyLibrary.csproj

```csharp

### Platform-Specific TFMs

For projects targeting platform-specific TFMs (MAUI, Uno):

```xml

<PropertyGroup>
  <!-- Use version-agnostic platform globs where possible -->
  <TargetFrameworks>net10.0;net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0</TargetFrameworks>
</PropertyGroup>

<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
  <PackageReference Include="Xamarin.AndroidX.Core" Version="1.*" />
</ItemGroup>

```text

### Shared Properties via Directory.Build.props

For multi-project solutions, centralize multi-targeting configuration:

```xml

<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <LangVersion>14</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="PolySharp" Version="1.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Polyfill" Version="7.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
