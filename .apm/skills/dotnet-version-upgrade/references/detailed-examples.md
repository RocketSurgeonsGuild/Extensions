- `SYSLIB0XXX`: Obsolete system API diagnostics (e.g., `SYSLIB0011` for BinaryFormatter)

### API Diff Tools

For library authors, use API compatibility tools to validate public surface changes:

```bash

# Package validation detects breaking changes against a baseline
dotnet pack /p:EnablePackageValidation=true /p:PackageValidationBaselineVersion=1.0.0

# Standalone API comparison
dotnet tool install -g Microsoft.DotNet.ApiCompat.Tool
apicompat --left-assembly bin/Release/net8.0/MyLib.dll \
          --right-assembly bin/Release/net10.0/MyLib.dll

```text

See [skill:dotnet-multi-targeting] for detailed API compatibility validation workflows including suppression files and CI integration.

---

## Package Update Strategies

### dotnet-outdated

The `dotnet-outdated` tool provides a comprehensive view of package staleness:

```bash

# Install
dotnet tool install -g dotnet-outdated-tool

# Show all outdated packages with current vs latest versions
dotnet outdated

# Lock major version, show only minor/patch updates (safer incremental upgrades)
dotnet outdated --version-lock major

# Auto-upgrade with version locking to avoid unexpected major bumps
dotnet outdated --upgrade --version-lock major

# Output as JSON for CI integration
dotnet outdated --output-format json

```json

### Central Package Management

For solutions using Central Package Management (`Directory.Packages.props`), update versions centrally:

```xml

<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Update all Microsoft.Extensions.* packages to match TFM -->
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="10.*" />
    <PackageVersion Include="Microsoft.Extensions.Http" Version="10.*" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="10.*" />
    <!-- Third-party packages: check compatibility before upgrading -->
    <PackageVersion Include="Serilog" Version="4.*" />
  </ItemGroup>
</Project>

```text

`Directory.Packages.props` resolves hierarchically upward from the project directory. In monorepo structures, verify that nested `Directory.Packages.props` files are not shadowing the root-level configuration.

### ASP.NET Core Shared Framework Packages

ASP.NET Core shared framework packages must align their major version with the target TFM. Two valid approaches:

```xml

<ItemGroup>
  <!-- Option A: floating version — auto-resolves latest patch, convenient for upgrades -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />

  <!-- Option B: pinned version — deterministic CI builds, update explicitly -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.1" />
</ItemGroup>

```text

Pinned versions are recommended for deterministic CI; floating versions are useful during exploratory upgrades. Either way, the **major version must match the TFM** (e.g., `10.x` for `net10.0`).

---

## Agent Gotchas

1. **Do not skip .NET 9 without validating ecosystem compatibility.** While LTS-to-LTS (net8.0 -> net10.0) is the recommended default, some third-party packages may only support net9.0 as an intermediate step. Check package compatibility before selecting a lane.

1. **Do not run production workloads on preview SDKs.** .NET 11 preview APIs are unstable and will change between preview releases. Isolate experimental work in separate branches or projects with pinned preview SDK versions.

1. **Do not assume .NET 9 STS has 12-month support.** STS lifecycle is 18 months from GA. .NET 9 GA was November 2024, so end-of-support is May 2026. Always calculate from actual GA date, not release year.

1. **Ensure ASP.NET shared framework package major versions match the TFM.** Packages like `Microsoft.AspNetCore.Mvc.Testing` must have their major version aligned with the project TFM (e.g., `10.x` for `net10.0`). Pin exact versions for deterministic CI or float with wildcards (e.g., `10.*`) during exploratory upgrades.

1. **Do not re-implement TFM detection.** This skill consumes the structured output from [skill:dotnet-version-detection]. Never parse `.csproj` files to determine the current version -- use the detection skill's output (TFM, C# version, SDK version, warnings).

1. **Do not treat `dotnet-outdated --upgrade` as a complete solution.** It updates package versions but does not handle breaking API changes within those packages. Always build, test, and review changelogs after upgrading packages.

1. **Do not use `"rollForward": "latestMajor"` with preview SDKs.** This can silently advance to a different preview version with breaking changes. Use `"rollForward": "disable"` for preview SDKs to maintain reproducible builds.

1. **Do not forget `Directory.Packages.props` hierarchy.** In monorepo structures, nested `Directory.Packages.props` files shadow parent-level configurations. When upgrading, search upward from the project directory to find all `Directory.Packages.props` files that may affect package resolution.

1. **Do not ignore `SYSLIB` diagnostic codes during upgrade.** These system-level obsolete warnings (e.g., `SYSLIB0011` for BinaryFormatter, `SYSLIB0014` for WebRequest) indicate APIs that will throw at runtime on newer TFMs, not just compile-time warnings.

---

## Prerequisites

- .NET SDK for the target TFM installed (e.g., .NET 10 SDK for net10.0 upgrade)
- `dotnet-outdated-tool` (for package staleness detection): `dotnet tool install -g dotnet-outdated-tool`
- `upgrade-assistant` (optional, for automated migration): `dotnet tool install -g upgrade-assistant`
- Output from [skill:dotnet-version-detection] (current TFM, SDK version, preview flags)

---

## References

> **Last verified: 2026-02-12**

- [.NET Support Policy and Lifecycle](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [.NET 9 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [.NET Upgrade Assistant](https://dotnet.microsoft.com/en-us/platform/upgrade-assistant)
- [dotnet-outdated Tool](https://github.com/dotnet-outdated/dotnet-outdated)
- [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- [Target Framework Monikers](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET Analyzers Overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- [Package Validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview)
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
