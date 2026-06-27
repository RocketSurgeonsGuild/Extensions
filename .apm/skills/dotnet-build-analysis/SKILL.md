---
name: dotnet-build-analysis
category: developer-experience
subcategory: cli
description: Interprets MSBuild output, NuGet errors, analyzer warnings. Error codes, CI drift.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
  model: haiku
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-build-analysis

Help agents interpret and act on MSBuild build output. Covers error code prefixes, NuGet restore failures, analyzer
warning interpretation, multi-targeting build differences, and "works locally, fails in CI" diagnosis patterns. Each
subsection includes example output, diagnosis steps, and a fix pattern.

## Scope

- MSBuild error code prefix interpretation (CS, MSB, NU, CA, IDE, NETSDK)
- NuGet restore failure diagnosis and resolution
- Analyzer warning triage and suppression guidance
- CI vs local build drift diagnosis

## Out of scope

- Writing or modifying .csproj files -- see [skill:dotnet-csproj-reading]
- Project structure decisions -- see [skill:dotnet-project-structure]
- Common agent code mistakes -- see [skill:dotnet-agent-gotchas]

## Prerequisites

.NET 8.0+ SDK. MSBuild (included with .NET SDK). Understanding of SDK-style project format.

Cross-references: [skill:dotnet-agent-gotchas] for common code mistakes that cause build errors,
[skill:dotnet-csproj-reading] for project file structure and modification, [skill:dotnet-project-structure] for project
organization and SDK selection.

---

## Error Code Prefixes

MSBuild output uses standardized prefixes to indicate the error source. Understanding the prefix tells you which system
produced the error and where to look for fixes.

### CS -- C# Compiler Errors and Warnings

Produced by the Roslyn C# compiler. These are language-level issues in source code.

**Example output:**

````csharp

src/MyApp.Api/Services/OrderService.cs(42,17): error CS0246: The type or namespace name 'OrderDto' could not be found (are you missing a using directive or an assembly reference?)
src/MyApp.Api/Models/User.cs(15,9): warning CS8618: Non-nullable property 'Name' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the property as nullable.

```csharp

**Diagnosis:**

1. Parse the file path and line number from the error -- `src/MyApp.Api/Services/OrderService.cs` line 42, column 17.
2. CS0246 means a type is missing. Check: is the type defined? Is the namespace imported? Is the project referencing the
   assembly that contains it?
3. CS8618 is a nullable reference type warning. The property needs a `required` modifier, nullable annotation
   (`string?`), or constructor initialization.

**Fix pattern:**

- CS0xxx (syntax/type errors): Fix source code at the indicated location. Add `using` directives, fix type names, add
  missing references.
- CS8xxx (nullable warnings): Add null annotations, null checks, or `required` modifiers. Do NOT suppress with `#pragma`
  or `!` operator.

### MSB -- MSBuild Engine Errors

Produced by the MSBuild build engine itself. These indicate project file problems, target failures, or build system
misconfiguration.

**Example output:**

```text

error MSB4019: The imported project "C:\Program Files\dotnet\sdk\9.0.100\Microsoft\VisualStudio\v17.0\WebApplications\Microsoft.WebApplication.targets" was not found. Confirm that the expression in the Import declaration "..." is correct.
error MSB3644: The reference assemblies for .NETFramework,Version=v4.8 were not found. You might need to install the developer pack for this framework version.

```text

**Diagnosis:**

1. MSB4019: An MSBuild `.targets` file is missing. This usually means wrong SDK type, missing workload, or corrupt SDK
   installation.
2. MSB3644: Targeting a framework version whose targeting pack is not installed. Common when a project targets .NET
   Framework but only .NET (Core) SDK is installed.

**Fix pattern:**

- MSB4019: Verify `<Project Sdk="...">` is correct (e.g., `Microsoft.NET.Sdk.Web` for ASP.NET Core). Run
  `dotnet workload list` and install missing workloads.
- MSB3xxx: Check `<TargetFramework>` value. Ensure the required SDK or targeting pack is installed.

### NU -- NuGet Errors and Warnings

Produced by the NuGet package manager during restore or pack operations.

**Example output:**

```text

error NU1101: Unable to find package Newtonsoft.Json.Extensions. No packages exist with this id in source(s): nuget.org
warning NU1603: Microsoft.EntityFrameworkCore 9.0.0 depends on Microsoft.Extensions.Caching.Memory (>= 9.0.0) but version Microsoft.Extensions.Caching.Memory 8.0.1 was resolved. Approve the package to suppress this warning.
error NU1605: Detected package downgrade: Microsoft.Extensions.Logging from 9.0.0 to 8.0.1. Reference the package directly from the project to select a different version.

```json

**Diagnosis:**

1. NU1101: Package ID does not exist. Check spelling, verify the package source is configured, check if the package was
   renamed or deprecated.
2. NU1603: Transitive dependency version conflict. A package wants a newer version than what is resolved.
3. NU1605: Explicit downgrade detected. Two packages require different versions of the same dependency.

**Fix pattern:**

- NU1101: Fix the package name. Search [nuget.org](https://www.nuget.org/) for the correct ID.
- NU1603/NU1605: Add a direct `<PackageReference>` for the conflicting package at a compatible version, or use central
  package management to pin versions.

### IDE -- IDE/Roslyn Analyzer Code Style Diagnostics

Produced by Roslyn IDE analyzers for code style enforcement. These are usually warnings, not errors (unless
`.editorconfig` escalates them).

**Example output:**

```text

src/MyApp.Api/Program.cs(1,1): warning IDE0005: Using directive is unnecessary.
src/MyApp.Api/Models/Order.cs(8,12): warning IDE0044: Make field readonly
src/MyApp.Api/Services/Report.cs(22,5): warning IDE0058: Expression value is never used

```csharp

**Diagnosis:**

1. IDE0005: Unused `using` directive. Safe to remove.
2. IDE0044: Field can be `readonly` because it is only assigned in the constructor.
3. IDE0058: A method return value is discarded. Either assign it or use `_ = ...` to explicitly discard.

**Fix pattern:**

- IDE analyzers enforce code style. Fix them by applying the suggested change. Configure severity in `.editorconfig` to
  promote warnings to errors for CI enforcement.

### CA -- .NET Code Analysis (FxCop/Microsoft.CodeAnalysis.NetAnalyzers)

Produced by the .NET code analysis SDK analyzers for API design, performance, reliability, and security rules.

**Example output:**

```text

src/MyApp.Api/Services/CacheService.cs(34,9): warning CA1848: Use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogInformation(ILogger, string?, params object?[])'. Using LoggerMessage delegates provides better performance.
src/MyApp.Api/Controllers/UserController.cs(12,5): warning CA2007: Consider calling ConfigureAwait on the awaited task
src/MyApp.Api/Crypto/HashService.cs(8,9): warning CA5351: Do Not Use Broken Cryptographic Algorithms (MD5)

```csharp

**Diagnosis:**

1. CA1848: High-performance logging. Use `[LoggerMessage]` source generator attributes instead of string interpolation
   in log calls.
2. CA2007: `ConfigureAwait(false)` guidance for library code. Not applicable to ASP.NET Core app code (no
   `SynchronizationContext`).
3. CA5351: Security-critical. MD5 is broken for cryptographic purposes. Switch to SHA-256 or SHA-512.

**Fix pattern:**

- CA1xxx (design): Apply suggested API changes. These improve API consistency.
- CA2xxx (reliability/performance): Fix per suggestion. CA2007 can be suppressed in ASP.NET Core apps via
  `.editorconfig`.
- CA5xxx (security): Always fix. These flag real security vulnerabilities.

---

## NuGet Restore Failures

NuGet restore is the first build step. When it fails, no compilation occurs. These are the most common restore failure
patterns.

### Pattern: Package Not Found

**Example output:**

```text

  Determining projects to restore...
  Writing assets file to disk. Path: /src/MyApp.Api/obj/project.assets.json
/src/MyApp.Api/MyApp.Api.csproj : error NU1101: Unable to find package MyCompany.Shared.Models. No packages exist with this id in source(s): nuget.org
  Failed to restore /src/MyApp.Api/MyApp.Api.csproj (in 2.14 sec).

```csharp

**Diagnosis:**

1. Is the package ID spelled correctly? NuGet IDs are case-insensitive but must be exact.
2. Is the package from a private feed? Check `nuget.config` for feed configuration. NuGet searches feeds hierarchically
   upward from the project directory.
3. Is `packageSourceMapping` configured? If so, the package must be mapped to a source that contains it. `MyCompany.*`
   patterns take precedence over `*` wildcard.

**Fix pattern:**

```bash

# Check configured sources
dotnet nuget list source

# Check if nuget.config exists (searches upward from project dir)
ls nuget.config ../nuget.config ../../nuget.config 2>/dev/null

```text

```xml

<!-- Add private feed to nuget.config -->
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="MyCompany" value="https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="MyCompany">
      <package pattern="MyCompany.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>

```text

### Pattern: Version Conflict

**Example output:**

```text

error NU1107: Version conflict detected for Microsoft.Extensions.DependencyInjection.Abstractions.
  MyApp.Api -> Microsoft.EntityFrameworkCore 9.0.0 -> Microsoft.Extensions.DependencyInjection.Abstractions (>= 9.0.0)
  MyApp.Api -> Microsoft.Extensions.Hosting 8.0.1 -> Microsoft.Extensions.DependencyInjection.Abstractions (>= 8.0.1)

```text

**Diagnosis:**

1. Two dependency chains require different major versions of the same package.
2. Trace each chain to find which top-level package is pinned at an older version.
3. The fix is usually upgrading the older top-level package.

**Fix pattern:**

```xml

<!-- Upgrade the older top-level package to match -->
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
<!-- Or add a direct reference to force a specific version -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />

```text

### Pattern: Authentication Failure on Private Feed

**Example output:**

```text

  Retrying 'FindPackagesByIdAsyncCore' for source 'https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json'.
  Response status code does not indicate success: 401 (Unauthorized).
error NU1301: Unable to load the service index for source https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json.

```json

**Diagnosis:**

1. Credentials are missing or expired for the private feed.
2. In CI, check that the credential provider or PAT is configured.
3. Locally, run `dotnet nuget update source` with credentials or use Azure Artifacts Credential Provider.

**Fix pattern:**

```bash

# Install Azure Artifacts Credential Provider (see official docs for platform-specific steps):
# https://github.com/microsoft/artifacts-credprovider#setup
# Windows:  iex "& { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }"
# macOS/Linux: sh -c "$(curl -fsSL https://aka.ms/install-artifacts-credprovider.sh)"

# Or add credentials explicitly to a specific source
dotnet nuget update source MyCompany --username az --password $PAT --store-password-in-clear-text

```text

---

## Analyzer Warning Interpretation

Analyzer warnings are produced by Roslyn analyzers bundled with the SDK or added via NuGet. Understanding when to fix
vs. when to configure severity is critical.

### Example Output

```text

src/MyApp.Api/Controllers/OrdersController.cs(27,5): warning CA2007: Consider calling ConfigureAwait on the awaited task [/src/MyApp.Api/MyApp.Api.csproj]
src/MyApp.Api/Services/OrderService.cs(15,16): warning CA1062: In externally visible method 'OrderService.Process(string)', validate parameter 'input' is non-null before using it [/src/MyApp.Api/MyApp.Api.csproj]
src/MyApp.Api/Models/UserDto.cs(8,12): warning IDE0032: Use auto-implemented property [/src/MyApp.Api/MyApp.Api.csproj]

```csharp

**Diagnosis:**

1. Identify the prefix: `CA` = Code Analysis (.NET analyzers), `IDE` = IDE code style analyzers.
2. Check severity: warnings don't break builds unless `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is set.
3. Determine if the rule applies to your project type (e.g., CA2007 is irrelevant in ASP.NET Core — no
   SynchronizationContext).
4. Decide: fix the code, configure severity in `.editorconfig`, or suppress with documented justification.

**Fix pattern:**

- **Fix the code** when the analyzer identifies a real issue (CA1062 — add null validation or use
  `ArgumentNullException.ThrowIfNull`).
- **Configure severity** in `.editorconfig` when the rule doesn't apply project-wide (see below).
- **Suppress inline** only with documented justification (see "When Suppression Is Acceptable" below).

### Severity Levels

| Severity       | Build Impact                                                                  | Action                              |
| -------------- | ----------------------------------------------------------------------------- | ----------------------------------- |
| **Error**      | Build fails                                                                   | Must fix before build succeeds      |
| **Warning**    | Build succeeds (unless `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) | Fix or configure in `.editorconfig` |
| **Suggestion** | Build succeeds; shown in IDE                                                  | Fix when practical                  |
| **Hidden**     | Not shown; available via code fix                                             | Ignore unless actively refactoring  |

### Configuring Severity

Use `.editorconfig` to control analyzer behavior across the project:

```ini

# .editorconfig (place at solution root)
[*.cs]

# Promote nullable warnings to errors (recommended)
dotnet_diagnostic.CS8600.severity = error
dotnet_diagnostic.CS8602.severity = error
dotnet_diagnostic.CS8603.severity = error

# Suppress ConfigureAwait warning in ASP.NET Core apps (no SynchronizationContext)
dotnet_diagnostic.CA2007.severity = none

# Promote security warnings to errors
dotnet_diagnostic.CA5350.severity = error
dotnet_diagnostic.CA5351.severity = error

```text

### When Suppression Is Acceptable

Suppression is acceptable ONLY when:

1. The analyzer cannot understand the code's safety guarantee (e.g., a custom guard clause that ensures non-null).
2. The rule does not apply to the project type (e.g., CA2007 in ASP.NET Core apps).
3. A documented justification is provided.

```csharp

// ACCEPTABLE: documented justification
[SuppressMessage("Reliability", "CA2007:ConfigureAwait",
    Justification = "ASP.NET Core has no SynchronizationContext")]
// TODO: Audit suppression - add justification or remove if not applicable
public async Task<Order> GetOrderAsync(int id, CancellationToken ct)
{
    return await _repo.GetByIdAsync(id, ct);
}

// NOT ACCEPTABLE: no justification, hides a real issue
#pragma warning disable CA1062

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
