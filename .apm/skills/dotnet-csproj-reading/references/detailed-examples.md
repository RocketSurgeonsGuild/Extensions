    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

    <!-- Deterministic builds for CI -->
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  </PropertyGroup>

  <PropertyGroup>
    <!-- Package metadata for all libraries -->
    <Authors>MyCompany</Authors>
    <Company>MyCompany</Company>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

</Project>

```text

### Annotated XML: Directory.Build.targets

```xml

<!-- Directory.Build.targets: imported AFTER the project file -->
<!-- Use for targets, items, and properties that depend on project-level values -->
<!-- Place at solution root alongside Directory.Build.props -->
<Project>

  <!-- Add analyzers to all projects (after project props are set) -->
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0"
                      PrivateAssets="All" IncludeAssets="analyzers" />
  </ItemGroup>

  <!-- Conditional item that depends on project-set properties -->
  <ItemGroup Condition="'$(IsTestProject)' == 'true'">
    <PackageReference Include="coverlet.collector" Version="8.0.0"
                      PrivateAssets="All" />
  </ItemGroup>

  <!-- Custom target that runs after build -->
  <Target Name="PrintBuildInfo" AfterTargets="Build">
    <Message Importance="high" Text="Built $(AssemblyName) for $(TargetFramework)" />
  </Target>

</Project>

```text

### Common Modification Patterns

**Hierarchy and override behavior:**

```text

repo-root/
  Directory.Build.props     <-- applies to ALL projects
  src/
    Directory.Build.props   <-- applies to src/ projects only
    MyApp.Api/
      MyApp.Api.csproj      <-- inherits from src/ props (NOT repo-root/)

```csharp

MSBuild imports the nearest `Directory.Build.props` found walking upward. If a nested `Directory.Build.props` exists, it
shadows the parent. To chain both, the nested file must explicitly import the parent:

```xml

<!-- src/Directory.Build.props -- import parent first, then override -->
<Project>
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />

  <PropertyGroup>
    <!-- Override or extend parent properties for src/ projects -->
    <RootNamespace>MyApp.$(MSBuildProjectName)</RootNamespace>
  </PropertyGroup>
</Project>

```text

**When to use .props vs .targets:**

| Use .props for                          | Use .targets for                                   |
| --------------------------------------- | -------------------------------------------------- |
| Property defaults (TFM, nullable, etc.) | Items that depend on project properties            |
| Package metadata (authors, license)     | Custom build targets (AfterTargets, BeforeTargets) |
| Properties projects can override        | Analyzer packages added to all projects            |

---

## Subsection 6: Directory.Packages.props (Central Package Management)

Central Package Management (CPM) centralizes NuGet package versions in a single `Directory.Packages.props` file.
Individual projects reference packages without specifying versions.

### Annotated XML Example

```xml

<!-- Directory.Packages.props: place at solution root -->
<Project>

  <PropertyGroup>
    <!-- Enable Central Package Management -->
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- PackageVersion defines the version centrally -->
    <!-- Projects use PackageReference WITHOUT Version attribute -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageVersion Include="Serilog.AspNetCore" Version="8.0.3" />

    <!-- Test packages -->
    <PackageVersion Include="xunit.v3" Version="3.2.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
  </ItemGroup>

</Project>

```text

**Project file with CPM enabled:**

```xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- No Version attribute -- version comes from Directory.Packages.props -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
  </ItemGroup>
</Project>

```text

### Common Modification Patterns

**Enabling CPM in an existing solution:**

1. Create `Directory.Packages.props` at the solution root with
   `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`.
2. Move all `Version` attributes from `PackageReference` items into `PackageVersion` entries in the central file.
3. Remove `Version` from all `PackageReference` items in individual `.csproj` files.

```bash

# Find all PackageReference entries with Version attributes
grep -rn 'PackageReference Include=.*Version=' --include="*.csproj" src/

```bash

**Overriding a version in a specific project** (escape hatch):

```xml

<!-- In the individual .csproj -- use VersionOverride when a project needs a different version -->
<PackageReference Include="Microsoft.EntityFrameworkCore" VersionOverride="8.0.11" />

```csharp

**Hierarchical resolution:** `Directory.Packages.props` resolves upward from the project directory, the same as
`Directory.Build.props`. In monorepos, place the central file at the repo root. Sub-directories can have their own
`Directory.Packages.props` -- the nearest one wins.

**Migrating from per-project versions:**

```bash

# List all unique packages and versions across the solution
dotnet list src/MyApp.sln package --format json
# Use this output to build the central PackageVersion list

```bash

---

## Slopwatch Anti-Patterns

These patterns in project files indicate an agent is hiding problems rather than fixing them. See
[skill:dotnet-slopwatch] for the automated quality gate that detects these patterns.

### NoWarn in .csproj

```xml

<!-- RED FLAG: blanket warning suppression in project file -->
<PropertyGroup>
  <NoWarn>CS8600;CS8602;CS8604;IL2026;IL2046;IL3050</NoWarn>
</PropertyGroup>

```text

`<NoWarn>` in the project file suppresses warnings for the entire project, making issues invisible. This is worse than
`#pragma` because it has no scope boundary and cannot be audited per-file.

**Fix:** Remove `<NoWarn>` entries and fix the underlying issues. For warnings that genuinely do not apply project-wide,
configure severity in `.editorconfig` instead:

```ini

# .editorconfig -- preferred over <NoWarn> for controlled suppression
[*.cs]
dotnet_diagnostic.CA2007.severity = none  # No SynchronizationContext in ASP.NET Core

```csharp

### Suppressed Analyzers in Directory.Build.props

```xml

<!-- RED FLAG: disabling analyzers for all projects via shared props -->
<PropertyGroup>
  <NoWarn>$(NoWarn);CA1062;CA1822;CA2007</NoWarn>
  <!-- OR -->
  <RunAnalyzers>false</RunAnalyzers>
  <!-- OR -->
  <EnableNETAnalyzers>false</EnableNETAnalyzers>
</PropertyGroup>

```text

Disabling analyzers in `Directory.Build.props` silences them across every project in the solution, including new
projects added later. Agents sometimes do this to achieve a clean build quickly.

**Fix:** Keep analyzers enabled globally. Address warnings per-project or per-file. If a specific rule category does not
apply (e.g., CA2007 in ASP.NET Core apps), suppress it in `.editorconfig` at the appropriate scope with a comment
explaining why.

---

## Cross-References

- [skill:dotnet-project-structure] -- SDK selection, project organization, solution layout
- [skill:dotnet-build-analysis] -- interpreting build errors caused by project misconfiguration
- [skill:dotnet-agent-gotchas] -- common project structure mistakes agents make (wrong SDK, broken refs)



## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**
- ✅ **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- ✅ **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- ✅ **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**
```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
## References

- [MSBuild Project SDK](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- [MSBuild Reference](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild)
- [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management)
- [Directory.Build.props/targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)
- [MSBuild Conditions](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-conditions)
- [SDK-style Project Format](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
````