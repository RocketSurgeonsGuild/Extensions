---
name: dotnet-csproj-reading
description: Reads and modifies SDK-style .csproj files. PropertyGroup, ItemGroup, CPM, props.
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
category: fundamentals
subcategory: coding-standards
---

# dotnet-csproj-reading

Teaches agents to read and safely modify SDK-style .csproj files. Covers project structure, PropertyGroup conventions,
ItemGroup patterns, conditional expressions, Directory.Build.props/.targets, and central package management
(Directory.Packages.props). Each subsection provides annotated XML examples and common modification patterns.

## Scope

- SDK-style .csproj structure and SDK attribute conventions
- PropertyGroup and ItemGroup reading and modification
- Conditional expressions and TFM-based conditions
- Directory.Build.props/.targets and Central Package Management

## Out of scope

- Project organization and SDK selection -- see [skill:dotnet-project-structure]
- Build error interpretation -- see [skill:dotnet-build-analysis]
- Common agent coding mistakes -- see [skill:dotnet-agent-gotchas]

## Prerequisites

.NET 8.0+ SDK. SDK-style projects only (legacy .csproj format is not covered). MSBuild (included with .NET SDK).

Cross-references: [skill:dotnet-project-structure] for project organization and SDK selection,
[skill:dotnet-build-analysis] for interpreting build errors from project misconfiguration, [skill:dotnet-agent-gotchas]
for common project structure mistakes agents make.

---

## Subsection 1: SDK-Style Project Structure

SDK-style projects use a `<Project Sdk="...">` declaration that imports hundreds of default targets and props.
Understanding what the SDK provides implicitly is essential to avoid redundant or conflicting declarations.

### Annotated XML Example

````xml

<!-- The Sdk attribute imports default props at the top and targets at the bottom -->
<!-- This single line replaces dozens of Import statements from legacy .csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <!--
    Common SDK values:
    - Microsoft.NET.Sdk           -> Console apps, libraries, class libraries
    - Microsoft.NET.Sdk.Web       -> ASP.NET Core (adds Kestrel, MVC, Razor, shared framework)
    - Microsoft.NET.Sdk.Worker    -> Background worker services
    - Microsoft.NET.Sdk.Razor     -> Razor class libraries
    - Microsoft.NET.Sdk.BlazorWebAssembly -> Blazor WASM apps
  -->

  <!-- SDK-style projects auto-include all *.cs files via default globs -->
  <!-- No need to list individual .cs files in <Compile Include="..."> -->
  <!-- Default globs: **/*.cs for Compile, **/*.resx for EmbeddedResource -->

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

</Project>

```text

### Common Modification Patterns

**Changing SDK type** -- when an agent creates a web project with the wrong SDK:

```xml

<!-- WRONG: console SDK for a web project -->
<Project Sdk="Microsoft.NET.Sdk">

<!-- CORRECT: Web SDK includes ASP.NET Core shared framework -->
<Project Sdk="Microsoft.NET.Sdk.Web">

```text

**Disabling default globs** -- rare, but needed when migrating from legacy format or when explicit file control is
required:

```xml

<PropertyGroup>
  <!-- Disable automatic inclusion of *.cs files -->
  <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  <!-- Disable all default items (Compile, EmbeddedResource, Content) -->
  <EnableDefaultItems>false</EnableDefaultItems>
</PropertyGroup>

```text

**Verifying which SDK a project uses:**

```bash

# Check the first line of the .csproj for the Sdk attribute
head -1 src/MyApp/MyApp.csproj
# Output: <Project Sdk="Microsoft.NET.Sdk.Web">

```bash

---

## Subsection 2: PropertyGroup Conventions

PropertyGroup elements contain scalar MSBuild properties. The most important properties control the target framework,
language features, and output type.

### Annotated XML Example

```xml

<PropertyGroup>
  <!-- Target Framework Moniker (TFM) -- determines runtime and API surface -->
  <!-- Use the latest LTS or STS release; prefer the repo's existing TFM. -->
  <TargetFramework>net9.0</TargetFramework>

  <!-- For multi-targeting, use plural form (see Subsection 4) -->
  <!-- <TargetFrameworks>net8.0;net9.0</TargetFrameworks> -->

  <!-- Enable nullable reference types (recommended for all new projects) -->
  <Nullable>enable</Nullable>

  <!-- Enable implicit global usings (System, System.Linq, etc.) -->
  <ImplicitUsings>enable</ImplicitUsings>

  <!-- Output type: Exe for apps, omit or Library for libraries -->
  <OutputType>Exe</OutputType>
  <!-- Omitting OutputType defaults to Library (produces .dll) -->

  <!-- Root namespace -- defaults to project name if omitted -->
  <RootNamespace>MyApp.Api</RootNamespace>

  <!-- Assembly name -- defaults to project name if omitted -->
  <AssemblyName>MyApp.Api</AssemblyName>

  <!-- Language version -- usually omitted (SDK sets default for TFM) -->
  <!-- Only set explicitly when using preview features -->
  <LangVersion>preview</LangVersion>
</PropertyGroup>

```text

### Common Modification Patterns

**Enabling nullable for an existing project:**

```xml

<!-- Add to the main PropertyGroup -->
<Nullable>enable</Nullable>
<!-- This enables nullable warnings project-wide. Existing code will produce warnings. -->
<!-- To adopt incrementally, use #nullable enable in individual files instead. -->

```text

**Setting output type for a console app:**

```xml

<!-- Required for executable projects; without this, dotnet run fails -->
<OutputType>Exe</OutputType>

```xml

**Adding TreatWarningsAsErrors (recommended for CI parity):**

```xml

<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <!-- Enable unconditionally -- do NOT use CI-only conditions -->
</PropertyGroup>

```text

---

## Subsection 3: ItemGroup Patterns

ItemGroup elements contain collections: package references, project references, file inclusions, and other build items.
Understanding the three main item types prevents common agent mistakes.

### Annotated XML Example

```xml

<ItemGroup>
  <!-- PackageReference: NuGet package dependency -->
  <!-- Version attribute is required unless using central package management -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />

  <!-- PrivateAssets="All" prevents the dependency from flowing to consumers -->
  <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />

  <!-- IncludeAssets controls which assets from the package are used -->
  <PackageReference Include="Nerdbank.GitVersioning" Version="3.7.115"
                    PrivateAssets="All" IncludeAssets="runtime;build;native;analyzers" />
</ItemGroup>

<ItemGroup>
  <!-- ProjectReference: reference to another project in the solution -->
  <!-- Use forward slashes for cross-platform compatibility -->
  <ProjectReference Include="../MyApp.Core/MyApp.Core.csproj" />

  <!-- Set PrivateAssets to prevent transitive exposure to consumers -->
  <ProjectReference Include="../MyApp.Internal/MyApp.Internal.csproj"
                    PrivateAssets="All" />
</ItemGroup>

<ItemGroup>
  <!-- None: files included in the project but not compiled -->
  <!-- CopyToOutputDirectory controls deployment behavior -->
  <None Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />

  <!-- Content: files that are part of the published output -->
  <Content Include="wwwroot/**" CopyToOutputDirectory="PreserveNewest" />

  <!-- EmbeddedResource: files compiled into the assembly -->
  <EmbeddedResource Include="Resources/**/*.resx" />
</ItemGroup>

```text

### Common Modification Patterns

**Adding a NuGet package:**

```bash

# Prefer CLI to avoid formatting issues
dotnet add src/MyApp/MyApp.csproj package Microsoft.EntityFrameworkCore --version 9.0.0

```bash

```xml

<!-- Or add manually -- ensure Version is specified -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />

```xml

**Adding a project reference:**

```bash

# CLI ensures correct relative path
dotnet add src/MyApp.Api/MyApp.Api.csproj reference src/MyApp.Core/MyApp.Core.csproj

```bash

```xml

<!-- Verify path actually resolves to an existing .csproj -->
<ProjectReference Include="../MyApp.Core/MyApp.Core.csproj" />

```csharp

**Including non-compiled files in output:**

```xml

<!-- Copy config files to output on build -->
<None Update="config/*.json" CopyToOutputDirectory="PreserveNewest" />
<!-- Note: Update (not Include) when the file is already matched by default globs -->

```json

---

## Subsection 4: Condition Expressions and Multi-Targeting

MSBuild conditions enable TFM-specific properties, platform-specific package references, and build configuration logic.
Understanding condition syntax prevents broken multi-targeting builds.

### Annotated XML Example

```xml

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Multi-targeting: builds the project once per TFM -->
    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
    <!-- Note the plural 'TargetFrameworks' (not 'TargetFramework') -->
  </PropertyGroup>

  <!-- TFM-conditional property: only applies to net9.0 builds -->
  <PropertyGroup Condition="'$(TargetFramework)' == 'net9.0'">
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <!-- TFM-conditional package reference: only include on specific TFMs -->
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Backport.System.Threading.Lock" Version="2.0.5" />
    <!-- System.Threading.Lock is built-in on net9.0+; this polyfill enables it on net8.0 -->
  </ItemGroup>

  <!-- Configuration-conditional items -->
  <ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
  </ItemGroup>

  <!-- Platform-conditional items for MAUI/Uno -->
  <ItemGroup Condition="$(TargetFramework.StartsWith('net9.0-android'))">
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.15.0.1" />
  </ItemGroup>

  <!-- Boolean conditions -->
  <PropertyGroup Condition="'$(CI)' == 'true'">
    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
  </PropertyGroup>

</Project>

```text

### Common Modification Patterns

**Adding a TFM:**

```xml

<!-- Change singular to plural and add new TFM -->
<!-- Before: -->
<TargetFramework>net8.0</TargetFramework>
<!-- After: -->
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>

```text

**Using version-agnostic TFM patterns for platform detection:**

```xml

<!-- CORRECT: version-agnostic glob handles net8.0-android, net9.0-android, etc. -->
<ItemGroup Condition="$(TargetFramework.Contains('-android'))">
  <AndroidResource Include="Resources/**" />
</ItemGroup>

<!-- WRONG: hardcoded version misses other TFMs -->
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0-android'">

```text

**Condition syntax reference:**

| Expression                     | Meaning                        |
| ------------------------------ | ------------------------------ |
| `'$(Prop)' == 'value'`         | Exact match (case-insensitive) |
| `'$(Prop)' != 'value'`         | Not equal                      |
| `$(Prop.StartsWith('prefix'))` | String starts with             |
| `$(Prop.Contains('sub'))`      | String contains                |
| `'$(Prop)' == ''`              | Property is empty/not set      |
| `Exists('path')`               | File or directory exists       |

---

## Subsection 5: Directory.Build.props and Directory.Build.targets

These files centralize shared build configuration. MSBuild automatically imports `Directory.Build.props` (before the
project) and `Directory.Build.targets` (after the project) from the current directory and all parent directories up to
the filesystem root.

### Annotated XML: Directory.Build.props

```xml

<!-- Directory.Build.props: imported BEFORE the project file -->
<!-- Use for properties that projects inherit but can override -->
<!-- Place at solution root to apply to all projects -->
<Project>

  <PropertyGroup>
    <!-- Common properties for all projects in the solution -->
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
