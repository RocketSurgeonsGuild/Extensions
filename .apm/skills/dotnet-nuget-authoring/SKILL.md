---
name: dotnet-nuget-authoring
category: developer-experience
subcategory: nuget
description: Creates NuGet packages. SDK-style csproj, source generators, multi-TFM, symbols, signing.
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

# dotnet-nuget-authoring

NuGet package authoring for .NET library authors: SDK-style `.csproj` package properties (`PackageId`, `PackageTags`,
`PackageReadmeFile`, `PackageLicenseExpression`), source generator NuGet packaging with `analyzers/dotnet/cs/` folder
layout and `buildTransitive` targets, multi-TFM packages, symbol packages (snupkg) with deterministic builds, package
signing (author signing with certificates, repository signing), package validation (`EnablePackageValidation`,
`Microsoft.DotNet.ApiCompat.Task` for API compatibility), and NuGet versioning strategies (SemVer 2.0, pre-release
suffixes, NBGV integration).

**Version assumptions:** .NET 8.0+ baseline. NuGet client bundled with .NET 8+ SDK. `Microsoft.DotNet.ApiCompat.Task`
8.0+ for API compatibility validation.

## Scope

- SDK-style csproj package properties and metadata
- Source generator NuGet packaging with analyzers folder layout
- Multi-TFM packages and symbol packages (snupkg)
- Package signing (author and repository signing)
- Package validation (EnablePackageValidation, API compatibility)
- NuGet versioning strategies (SemVer 2.0, NBGV)

## Out of scope

- Central Package Management, SourceLink, nuget.config -- see [skill:dotnet-project-structure]
- CI/CD NuGet push workflows -- see [skill:dotnet-gha-publish] and [skill:dotnet-ado-publish]
- CLI tool packaging and distribution -- see [skill:dotnet-cli-packaging]
- Roslyn analyzer authoring -- see [skill:dotnet-roslyn-analyzers]
- Release lifecycle and NBGV setup -- see [skill:dotnet-release-management]

Cross-references: [skill:dotnet-project-structure] for CPM, SourceLink, nuget.config, [skill:dotnet-gha-publish] for CI
NuGet push workflows, [skill:dotnet-ado-publish] for ADO NuGet push workflows, [skill:dotnet-cli-packaging] for CLI tool
distribution formats, [skill:dotnet-csharp-source-generators] for Roslyn source generator authoring,
[skill:dotnet-release-management] for release lifecycle and NBGV setup, [skill:dotnet-roslyn-analyzers] for Roslyn
analyzer authoring.

---

## SDK-Style Package Properties

Every NuGet package starts with MSBuild properties in the `.csproj`. SDK-style projects produce NuGet packages with
`dotnet pack` -- no `.nuspec` file required.

### Essential Package Metadata

````xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>MyCompany.Widgets</PackageId>
    <Version>1.0.0</Version>
    <Authors>My Company</Authors>
    <Description>A library for managing widgets with fluent API support.</Description>
    <PackageTags>widgets;fluent;dotnet</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/mycompany/widgets</PackageProjectUrl>
    <RepositoryUrl>https://github.com/mycompany/widgets</RepositoryUrl>
    <RepositoryType>git</RepositoryType>

    <!-- README displayed on nuget.org package page -->
    <PackageReadmeFile>README.md</PackageReadmeFile>

    <!-- Package icon (128x128 PNG recommended) -->
    <PackageIcon>icon.png</PackageIcon>

    <!-- Generate XML docs for IntelliSense -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- Deterministic builds for reproducibility -->
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  </PropertyGroup>

  <!-- Include README and icon in the package -->
  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
    <None Include="icon.png" Pack="true" PackagePath="\" />
  </ItemGroup>
</Project>

```markdown

### Property Reference

| Property | Purpose | Example |
|----------|---------|---------|
| `PackageId` | Unique package identifier on nuget.org | `MyCompany.Widgets` |
| `Version` | SemVer 2.0 version | `1.2.3-beta.1` |
| `Authors` | Comma-separated author names | `Jane Doe, My Company` |
| `Description` | Package description for nuget.org | `Fluent widget management library` |
| `PackageTags` | Semicolon-separated search tags | `widgets;fluent;dotnet` |
| `PackageLicenseExpression` | SPDX license identifier | `MIT`, `Apache-2.0` |
| `PackageLicenseFile` | License file (alternative to expression) | `LICENSE.txt` |
| `PackageReadmeFile` | Markdown readme displayed on nuget.org | `README.md` |
| `PackageIcon` | Package icon filename | `icon.png` |
| `PackageProjectUrl` | Project homepage URL | `https://github.com/mycompany/widgets` |
| `PackageReleaseNotes` | Release notes for this version | `Added widget caching support` |
| `Copyright` | Copyright statement | `Copyright 2024 My Company` |
| `RepositoryUrl` | Source repository URL | `https://github.com/mycompany/widgets` |
| `RepositoryType` | Repository type | `git` |

### Directory.Build.props for Shared Metadata

For multi-project repos, set common properties in `Directory.Build.props`:

```xml

<!-- Directory.Build.props (repo root) -->
<Project>
  <PropertyGroup>
    <Authors>My Company</Authors>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/mycompany/widgets</PackageProjectUrl>
    <RepositoryUrl>https://github.com/mycompany/widgets</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Copyright>Copyright 2024 My Company</Copyright>
  </PropertyGroup>
</Project>

```text

Individual `.csproj` files then only set package-specific properties (`PackageId`, `Description`, `PackageTags`).

---

## Source Generator NuGet Packaging

Source generators and analyzers require a specific NuGet package layout. The generator DLL must be placed in the `analyzers/dotnet/cs/` folder, not the `lib/` folder. For Roslyn source generator authoring (IIncrementalGenerator, syntax/semantic analysis), see [skill:dotnet-csharp-source-generators]. This section covers NuGet *packaging* of generators only.

### Project Setup for Source Generator Package

```xml

<!-- MyCompany.Generators.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>

    <!-- Package metadata -->
    <PackageId>MyCompany.Generators</PackageId>
    <Description>Source generators for widget auto-registration.</Description>

    <!-- Do NOT include generator DLL in lib/ folder -->
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>

    <!-- Generator must target netstandard2.0 for Roslyn host compat -->
    <IsPackable>true</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
  </ItemGroup>

  <!-- Place generator DLL in analyzers folder -->
  <ItemGroup>
    <None Include="$(OutputPath)$(AssemblyName).dll"
          Pack="true"
          PackagePath="analyzers/dotnet/cs"
          Visible="false" />
  </ItemGroup>
</Project>

```text

### Adding Build Props/Targets

When a source generator needs to set MSBuild properties in consuming projects, use the `buildTransitive` folder:

```xml

<!-- build/MyCompany.Generators.props -->
<Project>
  <PropertyGroup>
    <MyCompanyGeneratorsEnabled>true</MyCompanyGeneratorsEnabled>
  </PropertyGroup>
  <ItemGroup>
    <!-- Example: add additional files for generator to consume -->
    <CompilerVisibleProperty Include="MyCompanyGeneratorsEnabled" />
  </ItemGroup>
</Project>

```text

Include `buildTransitive` content in the package:

```xml

<!-- In the .csproj -->
<ItemGroup>
  <!-- buildTransitive ensures props/targets flow through transitive dependencies -->
  <None Include="build\MyCompany.Generators.props"
        Pack="true"
        PackagePath="buildTransitive\MyCompany.Generators.props" />
  <None Include="build\MyCompany.Generators.targets"
        Pack="true"
        PackagePath="buildTransitive\MyCompany.Generators.targets" />
</ItemGroup>

```text

### Multi-Target Analyzer Package (Analyzer + Library)

When shipping both an analyzer and a runtime library in the same package:

```xml

<!-- MyCompany.Widgets.csproj (ships both runtime lib + analyzer) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;netstandard2.0</TargetFrameworks>
    <PackageId>MyCompany.Widgets</PackageId>
  </PropertyGroup>

  <!-- Reference generator project, but suppress its output from lib/ -->
  <ItemGroup>
    <ProjectReference Include="..\MyCompany.Widgets.Generators\MyCompany.Widgets.Generators.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>
</Project>

```text

### NuGet Package Folder Layout

```text

MyCompany.Generators.1.0.0.nupkg
  analyzers/
    dotnet/
      cs/
        MyCompany.Generators.dll          <-- generator/analyzer assembly
  buildTransitive/
    MyCompany.Generators.props            <-- auto-imported MSBuild props
    MyCompany.Generators.targets          <-- auto-imported MSBuild targets
  lib/
    netstandard2.0/
      _._                                <-- empty marker (no runtime lib)

```text

---

## Multi-TFM Packages

Multi-targeting produces a single NuGet package with assemblies for each target framework. Consumers automatically get the best-matching assembly.

### When to Multi-Target

| Scenario | Approach |
|----------|----------|
| Library works on net8.0 only | Single TFM: `<TargetFramework>net8.0</TargetFramework>` |
| Library needs netstandard2.0 + net8.0 APIs | Multi-TFM: `<TargetFrameworks>netstandard2.0;net8.0</TargetFrameworks>` |
| Library uses net9.0-specific APIs (e.g., `SearchValues`) | Multi-TFM with polyfills or conditional code |
| Library targets .NET Framework consumers | Include `net472` or `netstandard2.0` TFM |

### Multi-TFM Configuration

```xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net8.0;net9.0</TargetFrameworks>
  </PropertyGroup>

  <!-- API differences per TFM -->
  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
    <PackageReference Include="System.Memory" Version="4.6.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.5" />
  </ItemGroup>
</Project>

```json

### Conditional Compilation

```csharp

public static class StringExtensions
{
    public static bool ContainsIgnoreCase(this string source, string value)
    {
#if NET8_0_OR_GREATER
        return source.Contains(value, StringComparison.OrdinalIgnoreCase);
#else
        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
#endif
    }
}

```text

### NuGet Package Folder Layout (Multi-TFM)

```text

MyCompany.Widgets.1.0.0.nupkg
  lib/
    netstandard2.0/
      MyCompany.Widgets.dll
    net8.0/
      MyCompany.Widgets.dll
    net9.0/
      MyCompany.Widgets.dll

```text

---

## Symbol Packages and Deterministic Builds

Symbol packages (`.snupkg`) enable source-level debugging for package consumers via the NuGet symbol server.

### Enabling Symbol Packages

```xml

<PropertyGroup>
  <!-- Generate .snupkg alongside .nupkg -->
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>

  <!-- Deterministic builds (required for reproducible packages) -->
  <Deterministic>true</Deterministic>
  <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>

  <!-- Embed source in PDB for debugging without source server -->
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
</PropertyGroup>

```text

The `snupkg` is pushed alongside the `nupkg` automatically when using `dotnet nuget push`:

```bash

# Push both .nupkg and .snupkg to nuget.org
dotnet nuget push "bin/Release/*.nupkg" \
  --source https://api.nuget.org/v3/index.json \
  --api-key "$NUGET_API_KEY"

```json

**SourceLink integration:** For source-level debugging with links to the actual source repository, configure SourceLink in your project. See [skill:dotnet-project-structure] for SourceLink setup -- do not duplicate that configuration here.

### Embedded PDB Alternative

For packages where a separate symbol package is undesirable:

```xml

<PropertyGroup>
  <DebugType>embedded</DebugType>
</PropertyGroup>

```xml

This embeds the PDB directly in the assembly DLL. The tradeoff is larger package size but simpler distribution.

---

## Package Signing

NuGet supports author signing (proving package origin) and repository signing (proving it came from a specific feed).

### Author Signing with a Certificate

```bash

# Sign a package with a PFX certificate
dotnet nuget sign "MyCompany.Widgets.1.0.0.nupkg" \
  --certificate-path ./signing-cert.pfx \
  --certificate-password "$CERT_PASSWORD" \
  --timestamper http://timestamp.digicert.com

# Sign with a certificate from the certificate store (Windows)

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
