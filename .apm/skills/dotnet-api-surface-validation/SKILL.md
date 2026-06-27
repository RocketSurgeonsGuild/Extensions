---
name: dotnet-api-surface-validation
category: web
subcategory: validation
description: Detects API surface changes in CI. PublicApiAnalyzers, Verify snapshots, ApiCompat gating.
license: MIT
targets: ['*']
tags: [api, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for api tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-api-surface-validation

Tools and workflows for validating and tracking the public API surface of .NET libraries. Covers three complementary
approaches: **PublicApiAnalyzers** for text-file tracking of shipped/unshipped APIs with Roslyn diagnostics, the
**Verify snapshot pattern** for reflection-based API surface snapshot testing, and **ApiCompat CI enforcement** for
gating pull requests on API surface changes.

**Version assumptions:** .NET 8.0+ baseline. PublicApiAnalyzers 3.3+ (ships with `Microsoft.CodeAnalysis.Analyzers` or
standalone `Microsoft.CodeAnalysis.PublicApiAnalyzers`). ApiCompat tooling included in .NET 8+ SDK.

## Scope

- PublicApiAnalyzers text-file tracking of shipped/unshipped APIs
- Verify snapshot pattern for reflection-based API surface testing
- ApiCompat CI enforcement for gating PRs on breaking changes
- Multi-TFM and monorepo API tracking strategies
- PR labeling and suppression file workflows

## Out of scope

- Binary vs source compatibility rules, type forwarders, SemVer impact -- see [skill:dotnet-library-api-compat]
- NuGet packaging, `EnablePackageValidation` basics, and suppression file mechanics -- see
  [skill:dotnet-nuget-authoring] and [skill:dotnet-multi-targeting]
- Verify library fundamentals (setup, scrubbing, converters) -- see [skill:dotnet-snapshot-testing]
- General Roslyn analyzer configuration (EditorConfig, severity levels) -- see [skill:dotnet-roslyn-analyzers]
- HTTP API versioning -- see [skill:dotnet-api-versioning]

Cross-references: [skill:dotnet-library-api-compat] for binary/source compatibility rules,
[skill:dotnet-nuget-authoring] for `EnablePackageValidation` and NuGet SemVer, [skill:dotnet-multi-targeting] for
multi-TFM ApiCompat tool mechanics, [skill:dotnet-snapshot-testing] for Verify fundamentals,
[skill:dotnet-roslyn-analyzers] for general analyzer configuration, [skill:dotnet-api-versioning] for HTTP API
versioning.

---

## PublicApiAnalyzers

PublicApiAnalyzers tracks every public API member in text files committed to source control. The analyzer enforces that
new APIs go through an explicit "unshipped" phase before being marked "shipped," preventing accidental public API
exposure and undocumented surface area changes.

### Setup

Install the analyzer package:

````xml

<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" Version="3.3.*" PrivateAssets="all" />
</ItemGroup>

```xml

Create the two tracking files at the project root (adjacent to the `.csproj`):

```csharp

MyLib/
  MyLib.csproj
  PublicAPI.Shipped.txt    # APIs shipped in released versions
  PublicAPI.Unshipped.txt  # APIs added since last release

```csharp

Both files must exist, even if empty. Each must contain a header comment:

```text

#nullable enable

```text

The `#nullable enable` header tells the analyzer to track nullable annotations in API signatures. Without it, nullable
context differences are ignored.

### Diagnostic Rules

| Rule   | Severity | Meaning                                                      |
| ------ | -------- | ------------------------------------------------------------ |
| RS0016 | Warning  | Public API member not declared in API tracking files         |
| RS0017 | Warning  | Public API member removed but still in tracking files        |
| RS0024 | Warning  | Public API member has wrong nullable annotation              |
| RS0025 | Warning  | Public API symbol marked shipped but has changed signature   |
| RS0026 | Warning  | New public API added without `PublicAPI.Unshipped.txt` entry |
| RS0036 | Warning  | API file missing `#nullable enable` header                   |
| RS0037 | Warning  | Public API declared but does not exist in source             |

**RS0016** is the most common diagnostic. When you add a new `public` or `protected` member, RS0016 fires until you add
the member's signature to `PublicAPI.Unshipped.txt`. Use the code fix (lightbulb) in the IDE to automatically add the
entry.

**RS0017** fires when you remove or rename a `public` member but the old signature still exists in the tracking files.
Remove the stale line from the appropriate file.

### File Format

Each line in the tracking files represents one public API symbol using its documentation comment ID format:

```text

#nullable enable
MyLib.Widget
MyLib.Widget.Widget() -> void
MyLib.Widget.Name.get -> string!
MyLib.Widget.Name.set -> void
MyLib.Widget.Calculate(int count) -> decimal
MyLib.Widget.CalculateAsync(int count, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) -> System.Threading.Tasks.Task<decimal>!
MyLib.IWidgetFactory
MyLib.IWidgetFactory.Create(string! name) -> MyLib.Widget!
MyLib.WidgetOptions
MyLib.WidgetOptions.WidgetOptions() -> void
MyLib.WidgetOptions.MaxRetries.get -> int
MyLib.WidgetOptions.MaxRetries.set -> void

```text

Key formatting rules:

- The `!` suffix denotes a non-nullable reference type in nullable-enabled context
- The `?` suffix denotes a nullable reference type or nullable value type
- Constructors use the type name (e.g., `Widget.Widget() -> void`)
- Properties expand to `.get` and `.set` entries
- Default parameter values are included in the signature

### Shipped/Unshipped Lifecycle

The workflow across release cycles:

**During development (between releases):**

1. Add new public API member to source code
2. RS0016 fires -- member not tracked
3. Use code fix or manually add to `PublicAPI.Unshipped.txt`
4. RS0016 clears

**At release time:**

1. Move all entries from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`
2. Clear `PublicAPI.Unshipped.txt` back to just the `#nullable enable` header
3. Commit both files as part of the release PR
4. Tag the release

**When removing a previously shipped API (major version):**

1. Remove the member from source code
2. Remove the entry from `PublicAPI.Shipped.txt`
3. RS0017 clears (if it fired)
4. Document the removal in release notes

**When removing an unshipped API (before release):**

1. Remove the member from source code
2. Remove the entry from `PublicAPI.Unshipped.txt`
3. No SemVer impact -- the API was never released

### Multi-TFM Projects

For multi-targeted projects, PublicApiAnalyzers supports per-TFM tracking files when the API surface differs across
targets:

```text

MyLib/
  MyLib.csproj
  PublicAPI.Shipped.txt           # Shared across all TFMs
  PublicAPI.Unshipped.txt         # Shared across all TFMs
  PublicAPI.Shipped.net8.0.txt    # net8.0-specific APIs
  PublicAPI.Unshipped.net8.0.txt  # net8.0-specific APIs
  PublicAPI.Shipped.net10.0.txt   # net10.0-specific APIs
  PublicAPI.Unshipped.net10.0.txt # net10.0-specific APIs

```text

The shared files contain APIs common to all TFMs. The TFM-specific files contain APIs that only exist on that target.
The analyzer merges them at build time.

To enable per-TFM files, add to the `.csproj`:

```xml

<PropertyGroup>
  <RoslynPublicApiPerTfm>true</RoslynPublicApiPerTfm>
</PropertyGroup>

```xml

See [skill:dotnet-multi-targeting] for multi-TFM packaging mechanics.

### Integrating with CI

PublicApiAnalyzers runs as part of the standard build. To enforce it in CI, ensure warnings are treated as errors for
the RS-series rules:

```xml

<!-- In Directory.Build.props or the library .csproj -->
<PropertyGroup>
  <WarningsAsErrors>$(WarningsAsErrors);RS0016;RS0017;RS0036;RS0037</WarningsAsErrors>
</PropertyGroup>

```csharp

This gates CI builds on any undeclared public API changes. Developers must explicitly update the tracking files before
the build passes.

---

## Verify API Surface Snapshot Pattern

Use the Verify library to snapshot-test the entire public API surface of an assembly. This approach uses reflection to
enumerate all public types and members, producing a human-readable snapshot that is committed to source control and
compared on every test run. Any change to the public API surface causes a test failure until the snapshot is explicitly
approved.

This pattern complements PublicApiAnalyzers -- the analyzer catches changes at build time within the project, while the
Verify snapshot catches changes from the perspective of a compiled assembly consumer.

For Verify fundamentals (setup, scrubbing, converters, diff tool integration, CI configuration), see
[skill:dotnet-snapshot-testing].

### Extracting the Public API Surface

Create a helper method that reflects over an assembly to produce a stable, sorted representation of all public types and
their members:

```csharp

using System.Reflection;
using System.Text;

public static class PublicApiExtractor
{
    public static string GetPublicApi(Assembly assembly)
    {
        var sb = new StringBuilder();

        var publicTypes = assembly
            .GetTypes()
            .Where(t => t.IsPublic || t.IsNestedPublic)
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

        foreach (var type in publicTypes)
        {
            AppendType(sb, type);
        }

        return sb.ToString();
    }

    private static void AppendType(StringBuilder sb, Type type)
    {
        var kind = type switch
        {
            { IsEnum: true } => "enum",
            { IsValueType: true } => "struct",
            { IsInterface: true } => "interface",
            { IsAbstract: true, IsSealed: true } => "static class",
            { IsAbstract: true } => "abstract class",
            { IsSealed: true } => "sealed class",
            _ => "class"
        };

        sb.AppendLine($"{kind} {type.FullName}");

        var members = type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .OrderBy(m => m.MemberType)
            .ThenBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.ToString(), StringComparer.Ordinal);

        foreach (var member in members)
        {
            sb.AppendLine($"  {FormatMember(member)}");
        }

        sb.AppendLine();
    }

    private static string FormatMember(MemberInfo member) =>
        member switch
        {
            ConstructorInfo c => $".ctor({FormatParameters(c.GetParameters())})",
            MethodInfo m when !m.IsSpecialName =>
                $"{m.ReturnType.Name} {m.Name}({FormatParameters(m.GetParameters())})",
            PropertyInfo p => $"{p.PropertyType.Name} {p.Name} {{ {GetAccessors(p)} }}",
            FieldInfo f => $"{f.FieldType.Name} {f.Name}",
            EventInfo e => $"event {e.EventHandlerType?.Name} {e.Name}",
            _ => member.ToString() ?? string.Empty
        };

    private static string FormatParameters(ParameterInfo[] parameters) =>
        string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));

    private static string GetAccessors(PropertyInfo prop)
    {
        var parts = new List<string>();
        if (prop.GetMethod?.IsPublic == true) parts.Add("get;");
        if (prop.SetMethod?.IsPublic == true) parts.Add("set;");
        return string.Join(" ", parts);
    }
}

```text

### Writing the Snapshot Test

```csharp

[UsesVerify]
public class PublicApiSurfaceTests
{
    [Fact]
    public Task PublicApi_ShouldMatchApprovedSurface()
    {
        var assembly = typeof(Widget).Assembly;
        var publicApi = PublicApiExtractor.GetPublicApi(assembly);

        return Verify(publicApi);
    }
}

```text

On first run, this creates a `.verified.txt` file containing the full public API listing. Subsequent runs compare the
current API surface against the approved snapshot. Any addition, removal, or modification of public members causes a
test failure with a clear diff.

### Reviewing API Surface Changes

When the snapshot test fails:

1. Verify generates a `.received.txt` file showing the new API surface
2. Diff the `.received.txt` against `.verified.txt` to review changes
3. If the changes are intentional, accept the new snapshot with `verify accept`
4. If the changes are accidental, revert the code changes

This creates a code-review checkpoint where every public API change must be explicitly approved by someone reviewing the
snapshot diff in the pull request.

### Combining with PublicApiAnalyzers

The two approaches serve different purposes:

| Concern              | PublicApiAnalyzers                  | Verify Snapshot              |
| -------------------- | ----------------------------------- | ---------------------------- |
| Detection timing     | Build time (in-IDE)                 | Test time (post-compile)     |
| Granularity          | Per-member signatures               | Assembly-wide surface        |
| Nullable annotations | Tracked via `#nullable enable`      | Requires explicit reflection |
| Approval workflow    | Edit text files (shipped/unshipped) | Accept snapshot diffs        |
| Multi-TFM            | Per-TFM files                       | Per-TFM test targets         |
| CI gating            | Warnings-as-errors                  | Test failures                |

Use both for maximum coverage: PublicApiAnalyzers catches changes during development, while Verify snapshots provide an
end-to-end assembly-level validation in the test suite.

---

## ApiCompat CI Enforcement

ApiCompat compares two assemblies (or a baseline NuGet package against the current build) and reports API differences.
When integrated into CI, it gates pull requests on API surface changes -- any breaking change produces a build error
that the author must explicitly acknowledge.

For `EnablePackageValidation` basics and suppression file mechanics, see [skill:dotnet-nuget-authoring] and
[skill:dotnet-multi-targeting].

### Package Validation in CI

The simplest enforcement uses `EnablePackageValidation` during `dotnet pack`:

```xml

<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>1.2.0</PackageValidationBaselineVersion>
</PropertyGroup>


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
