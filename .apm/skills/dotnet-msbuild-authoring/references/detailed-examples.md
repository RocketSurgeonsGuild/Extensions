  <!-- Normalize path separators -->
  <SafePath>$([MSBuild]::NormalizePath('$(MSBuildProjectDirectory)', '..', 'shared'))</SafePath>
</PropertyGroup>

```text

### Useful MSBuild Properties

| Property | Value |
|---|---|
| `$(MSBuildProjectDirectory)` | Directory containing the current `.csproj` |
| `$(MSBuildThisFileDirectory)` | Directory containing the current `.props`/`.targets` file |
| `$(MSBuildProjectName)` | Project name without extension |
| `$(IntermediateOutputPath)` | `obj/` output path |
| `$(OutputPath)` | `bin/` output path |
| `$(TargetFramework)` | Current TFM (e.g., `net10.0`) |
| `$(TargetFrameworks)` | Multi-TFM list (e.g., `net8.0;net10.0`) |
| `$(Configuration)` | `Debug` or `Release` |
| `$(SolutionDir)` | Solution directory (only set when building from solution) |

**Use `$(MSBuildThisFileDirectory)` in `.props`/`.targets` files**, not `$(MSBuildProjectDirectory)`. The former resolves to the file's own location, which is correct when the file is imported from a NuGet package or a different directory.

---

## Directory.Build.props/targets Advanced Patterns

Basic Directory.Build layout is covered in [skill:dotnet-project-structure]. This section covers advanced patterns for multi-repo and monorepo scenarios.

### Import Chain with GetPathOfFileAbove

In monorepos with nested directories, each level can define its own `Directory.Build.props` that chains to the parent:

```xml

repo/
  Directory.Build.props            (repo-wide defaults)
  src/
    Directory.Build.props          (src-specific overrides)
    MyApp/
      MyApp.csproj
  tests/
    Directory.Build.props          (test-specific overrides)
    MyApp.Tests/
      MyApp.Tests.csproj

```csharp

```xml

<!-- src/Directory.Build.props -->
<Project>
  <!-- Chain to parent Directory.Build.props (with existence guard) -->
  <PropertyGroup>
    <_ParentBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)..'))</_ParentBuildProps>
  </PropertyGroup>
  <Import Project="$(_ParentBuildProps)" Condition="'$(_ParentBuildProps)' != ''" />

  <PropertyGroup>
    <!-- Source-specific overrides -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>

```text

```xml

<!-- tests/Directory.Build.props -->
<Project>
  <!-- Chain to parent Directory.Build.props (with existence guard) -->
  <PropertyGroup>
    <_ParentBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)..'))</_ParentBuildProps>
  </PropertyGroup>
  <Import Project="$(_ParentBuildProps)" Condition="'$(_ParentBuildProps)' != ''" />

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
</Project>

```text

### Condition Guards

Prevent property values from being overridden by child imports:

```xml

<!-- repo/Directory.Build.props -->
<Project>
  <PropertyGroup>
    <!-- Defaults: can be overridden by child Directory.Build.props or project -->
    <TreatWarningsAsErrors Condition="'$(TreatWarningsAsErrors)' == ''">true</TreatWarningsAsErrors>
    <Nullable Condition="'$(Nullable)' == ''">enable</Nullable>
    <ImplicitUsings Condition="'$(ImplicitUsings)' == ''">enable</ImplicitUsings>
  </PropertyGroup>
</Project>

```text

**Rule:** Properties in `.props` files should use the `Condition="'$(Prop)' == ''"` guard so that inner `.props` files and project-level properties can override them. Properties you want to enforce unconditionally belong in `Directory.Build.targets` (which evaluates last).

### Preventing Double Imports

When multiple `Directory.Build.props` files chain upward, a shared import could be evaluated twice. Use a sentinel property to guard against this:

```xml

<!-- shared/Common.props -->
<Project>
  <!-- Guard: only evaluate once -->
  <PropertyGroup Condition="'$(_CommonPropsImported)' != 'true'">
    <_CommonPropsImported>true</_CommonPropsImported>
    <Authors>My Company</Authors>
    <Company>My Company</Company>
    <Copyright>Copyright (c) My Company. All rights reserved.</Copyright>
  </PropertyGroup>
</Project>

```text

The sentinel and content properties must be in the **same** `PropertyGroup` with the `!= 'true'` condition. Putting content in a separate block with `== 'true'` does not prevent re-evaluation -- it runs on every import because the sentinel is already set.

A cleaner approach uses `Condition` on the `<Import>` element:

```xml

<!-- Only import if not already imported -->
<Import Project="$(SharedPropsPath)"
        Condition="'$(_CommonPropsImported)' != 'true' AND Exists('$(SharedPropsPath)')" />

```xml

### Enforcing Settings in Directory.Build.targets

Properties set in `.targets` files cannot be overridden by project-level `PropertyGroup` elements because they evaluate after the project body:

```xml

<!-- Directory.Build.targets -->
<Project>
  <!-- Enforced: projects cannot override these -->
  <PropertyGroup>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
  </PropertyGroup>

  <!-- Conditional enforcement: only for src projects -->
  <PropertyGroup Condition="'$(IsTestProject)' != 'true' AND '$(IsPackable)' != 'false'">
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>

```text

---

## Agent Gotchas

1. **Unquoted condition comparisons.** Always quote both sides: `'$(Prop)' == 'value'`. Unquoted `$(Prop) == value` fails silently when the property is empty or contains spaces.

1. **Using `$(MSBuildProjectDirectory)` in shared `.props`/`.targets` files.** This resolves to the importing project's directory, not the file's own directory. Use `$(MSBuildThisFileDirectory)` to reference paths relative to the `.props`/`.targets` file itself.

1. **Setting properties in `.targets` and expecting project overrides.** Properties in `.targets` evaluate after the project body and override project-level values. If a property should be overridable, set it in `.props` with a `Condition="'$(Prop)' == ''"` guard.

1. **Adding `<Compile Include="**/*.cs" />` in SDK-style projects.** SDK-style projects auto-include all `*.cs` files. Explicit inclusion causes `NETSDK1022` duplicate items. Use `Remove` then `Include` for conditional scenarios.

1. **Missing `Outputs` on targets with `Inputs`.** A target with `Inputs` but no `Outputs` runs every build. Always pair them for incremental behavior.

1. **Using `$(SolutionDir)` in `.props`/`.targets` files.** This property is only set when building through a solution file. Command-line `dotnet build MyProject.csproj` leaves it empty. Use `$([MSBuild]::GetPathOfFileAbove('*.sln', '$(MSBuildProjectDirectory)'))` or pass paths relative to `$(MSBuildThisFileDirectory)`.

1. **Putting items in `PropertyGroup` or properties in `ItemGroup`.** Items (using `Include=`) must be in `<ItemGroup>`. Properties (using element value) must be in `<PropertyGroup>`. Mixing them produces silent evaluation failures.

1. **Forgetting `Condition` guard on parent import chain.** `GetPathOfFileAbove` returns empty string when no file is found. The `<Import>` must have `Condition="Exists('$(ResolvedPath)')"` or the build fails with a file-not-found error.

---

## References

- [MSBuild Reference](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-reference)
- [MSBuild Targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-targets)
- [MSBuild Items](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-items)
- [MSBuild Conditions](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-conditions)
- [MSBuild Property Functions](https://learn.microsoft.com/en-us/visualstudio/msbuild/property-functions)
- [MSBuild Well-Known Item Metadata](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-well-known-item-metadata)
- [Customize Your Build](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-your-build)
- [Directory.Build.props and Directory.Build.targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)
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
