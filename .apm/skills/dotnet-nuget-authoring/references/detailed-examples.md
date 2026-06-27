# Sign with a certificate from the certificate store (Windows)
dotnet nuget sign "MyCompany.Widgets.1.0.0.nupkg" \
  --certificate-fingerprint "ABC123..." \
  --timestamper http://timestamp.digicert.com

```text

### Certificate Requirements

| Requirement | Detail |
|-------------|--------|
| Key usage | Code signing (1.3.6.1.5.5.7.3.3) |
| Algorithm | RSA 2048-bit minimum |
| Timestamping | Required for long-term validity |
| Trusted CA | DigiCert, Sectigo, or other trusted CA for nuget.org |
| Self-signed | Accepted for private feeds; rejected by nuget.org |

### Repository Signing

Repository signing is applied by feed operators (e.g., nuget.org signs all packages). Package authors do not need to configure repository signing -- it is applied automatically by the feed infrastructure.

### Verifying Package Signatures

```bash

# Verify a signed package
dotnet nuget verify "MyCompany.Widgets.1.0.0.nupkg"

# Verify with verbose output
dotnet nuget verify "MyCompany.Widgets.1.0.0.nupkg" --verbosity detailed

```text

---

## Package Validation

Package validation catches API breaks, invalid package layouts, and compatibility issues before publishing.

### Built-in Pack Validation

```xml

<PropertyGroup>
  <!-- Enable package validation on dotnet pack -->
  <EnablePackageValidation>true</EnablePackageValidation>
</PropertyGroup>

```text

This validates:
- All TFMs have compatible API surface
- No accidental API removals between package versions
- Package layout follows NuGet conventions

### API Compatibility with Baseline Version

Compare the current package against a previously published baseline version to detect breaking changes:

```xml

<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <!-- Compare against last released version -->
  <PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>
</PropertyGroup>

```text

### Microsoft.DotNet.ApiCompat.Task

For advanced API compatibility checking across assemblies:

```xml

<ItemGroup>
  <PackageReference Include="Microsoft.DotNet.ApiCompat.Task" Version="8.0.0" PrivateAssets="all" />
</ItemGroup>

<PropertyGroup>
  <!-- Enable API compat analysis -->
  <ApiCompatEnableRuleAttributesMustMatch>true</ApiCompatEnableRuleAttributesMustMatch>
  <ApiCompatEnableRuleCannotChangeParameterName>true</ApiCompatEnableRuleCannotChangeParameterName>
</PropertyGroup>

```text

### Suppressing Known Breaks

When intentional API changes are made, generate and commit a suppression file:

```bash

# Generate suppression file for known breaks
dotnet pack /p:GenerateCompatibilitySuppressionFile=true

```bash

This creates `CompatibilitySuppressions.xml`:

```xml

<!-- CompatibilitySuppressions.xml (committed to source control) -->
<?xml version="1.0" encoding="utf-8"?>
<Suppressions xmlns:ns="https://learn.microsoft.com/dotnet/fundamentals/package-validation/diagnostic-ids">
  <Suppression>
    <DiagnosticId>CP0002</DiagnosticId>
    <Target>M:MyCompany.Widgets.Widget.OldMethod</Target>
    <Left>lib/net8.0/MyCompany.Widgets.dll</Left>
    <Right>lib/net8.0/MyCompany.Widgets.dll</Right>
  </Suppression>
</Suppressions>

```text

Reference the suppression file:

```xml

<ItemGroup>
  <ApiCompatSuppressionFile Include="CompatibilitySuppressions.xml" />
</ItemGroup>

```xml

---

## NuGet Versioning Strategies

### SemVer 2.0 for NuGet

NuGet follows Semantic Versioning 2.0:

| Version | Meaning |
|---------|---------|
| `1.0.0` | Stable release |
| `1.0.1` | Patch (bug fixes, no API changes) |
| `1.1.0` | Minor (new features, backward compatible) |
| `2.0.0` | Major (breaking changes) |
| `1.0.0-alpha.1` | Pre-release alpha |
| `1.0.0-beta.1` | Pre-release beta |
| `1.0.0-rc.1` | Release candidate |

### Pre-release Suffixes

```xml

<!-- Stable release -->
<Version>1.2.3</Version>

<!-- Pre-release with SemVer 2.0 dot-separated suffix -->
<Version>1.2.3-beta.1</Version>

<!-- CI build with commit height (NBGV pattern) -->
<!-- Produces: 1.2.3-beta.42+abcdef -->

```text

### NBGV Integration

Nerdbank.GitVersioning (NBGV) calculates versions from git history. For NBGV setup and `version.json` configuration, see [skill:dotnet-release-management]. This skill covers how NBGV-generated versions interact with NuGet packaging:

```xml

<PropertyGroup>
  <!-- NBGV sets Version, PackageVersion, AssemblyVersion automatically -->
  <!-- Do NOT set Version explicitly when using NBGV -->
</PropertyGroup>

```text

NBGV produces versions like `1.2.42-beta+abcdef` where:
- `1.2` comes from `version.json`
- `42` is git commit height
- `-beta` is the pre-release suffix from `version.json`
- `+abcdef` is the git commit hash (build metadata, ignored by NuGet resolution)

### Version Properties Reference

| Property | Purpose | Set By |
|----------|---------|--------|
| `Version` | Full SemVer version (drives PackageVersion) | Manual or NBGV |
| `PackageVersion` | NuGet package version (defaults to Version) | Manual or NBGV |
| `AssemblyVersion` | CLR assembly version | Manual or NBGV |
| `FileVersion` | Windows file version | Manual or NBGV |
| `InformationalVersion` | Full version string with metadata | Manual or NBGV |

---

## Packing and Local Testing

### Building the Package

```bash

# Pack in Release configuration
dotnet pack --configuration Release

# Pack with specific version override
dotnet pack --configuration Release /p:Version=1.2.3-beta.1

# Output to specific directory
dotnet pack --configuration Release --output ./artifacts

```text

### Local Feed Testing

Test a package locally before publishing:

```bash

# Create a local feed directory
mkdir -p ~/local-nuget-feed

# Add the package to the local feed
dotnet nuget push "bin/Release/MyCompany.Widgets.1.0.0.nupkg" \
  --source ~/local-nuget-feed

# In the consuming project, add the local feed
dotnet nuget add source ~/local-nuget-feed --name LocalFeed

```text

### Package Content Inspection

```bash

# List package contents (nupkg is a zip file)
unzip -l MyCompany.Widgets.1.0.0.nupkg

# Verify analyzer placement
unzip -l MyCompany.Generators.1.0.0.nupkg | grep analyzers/

```text

---

## Agent Gotchas

1. **Do not set both `PackageLicenseExpression` and `PackageLicenseFile`** -- they are mutually exclusive. Use `PackageLicenseExpression` for standard SPDX identifiers, `PackageLicenseFile` for custom licenses only.

1. **Source generators MUST target `netstandard2.0`** -- the Roslyn host requires this. Do not multi-target generators themselves; multi-target the runtime library that references the generator project.

1. **Do not set `IncludeBuildOutput` to `false` on library projects** -- only on pure analyzer/generator projects that should not contribute runtime assemblies.

1. **`buildTransitive` vs `build` folder** -- use `buildTransitive` for props/targets that should flow through transitive `PackageReference` dependencies. The `build` folder only affects direct consumers.

1. **Package validation suppression uses `ApiCompatSuppressionFile` with `CompatibilitySuppressions.xml`** -- not a `PackageValidationSuppression` MSBuild item. Generate the file with `/p:GenerateCompatibilitySuppressionFile=true`.

1. **SDK-style projects auto-include all `*.cs` files** -- adding TFM-conditional `Compile Include` without a preceding `Compile Remove` causes NETSDK1022 duplicate items.

1. **Never hardcode API keys in CLI examples** -- always use environment variable placeholders (`$NUGET_API_KEY`) with a note about CI secret storage.

1. **`ContinuousIntegrationBuild` must be conditional on CI** -- setting it unconditionally breaks local debugging by making PDBs non-reproducible with local file paths.
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
