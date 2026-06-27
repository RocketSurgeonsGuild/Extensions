#pragma warning disable CA1062
// TODO: Audit suppression - add justification or remove
public void Process(string input) { }  // input could be null
#pragma warning restore CA1062

```text

---

## Multi-Targeting Build Output

When a project targets multiple frameworks, MSBuild builds each TFM separately. Errors may appear for only one target.

### Example Output

```text

  MyApp.Shared -> /src/MyApp.Shared/bin/Debug/net8.0/MyApp.Shared.dll
src/MyApp.Shared/Services/FeatureService.cs(18,30): error CS1061: 'FrozenDictionary<string, int>' does not contain a definition for 'GetAlternateLookup' [/src/MyApp.Shared/MyApp.Shared.csproj -> net8.0]
  MyApp.Shared -> /src/MyApp.Shared/bin/Debug/net9.0/MyApp.Shared.dll
Build succeeded for net9.0.

Build FAILED for net8.0.

```text

**Diagnosis:**

1. The error tag `[...csproj -> net8.0]` shows which TFM failed. `net9.0` succeeded.
2. `GetAlternateLookup` was added in .NET 9. The code uses an API not available in .NET 8.
3. The fix requires conditional compilation or an alternative API for the older TFM.

**Fix pattern:**

```csharp

// Use preprocessor directives for TFM-specific code
#if NET9_0_OR_GREATER
    var lookup = frozenDict.GetAlternateLookup<ReadOnlySpan<char>>();
    return lookup.TryGetValue(key, out var value) ? value : default;
#else
    return frozenDict.TryGetValue(key.ToString(), out var value) ? value : default;
#endif

```text

```xml

<!-- Or constrain the feature to specific TFMs in the project file -->
<PropertyGroup>
  <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
</PropertyGroup>

<!-- TFM-conditional package reference -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="System.Collections.Immutable" Version="8.0.0" />
</ItemGroup>

```text

### Reading Multi-Target Output

Key patterns for identifying TFM-specific issues:

- `[ProjectPath -> TFM]` suffix on every diagnostic line identifies the target.
- `Build succeeded for netX.0` / `Build FAILED for netX.0` summary at the end.
- Restore output shows all TFMs: `Restored ... (net8.0, net9.0)`.
- Build output paths include the TFM: `bin/Debug/net8.0/` vs `bin/Debug/net9.0/`.

---

## CI Drift: Works Locally, Fails in CI

The most frustrating build failures are ones that pass locally but fail in CI. These are almost always caused by
environmental differences.

### Pattern: Different SDK Version

**Example scenario:**

```text

Local:  dotnet --version -> 9.0.200
CI:     dotnet --version -> 9.0.100

Build error in CI:
error CS8652: The feature 'field keyword' is currently in Preview and *unsupported*.

```text

**Diagnosis:**

1. The local SDK (9.0.200) includes a language preview feature that the CI SDK (9.0.100) does not.
2. A `global.json` file is either missing or not pinning the SDK version.

**Fix pattern:**

```json

// global.json -- pin SDK version for consistent builds
{
  "sdk": {
    "version": "9.0.200",
    "rollForward": "latestPatch"
  }
}

```text

### Pattern: Missing Workload in CI

**Example scenario:**

```text

CI error:
error NETSDK1147: To build this project, the following workloads must be installed: maui-android

```text

**Diagnosis:**

1. MAUI/Aspire/WASM workloads installed locally but not in the CI image.
2. CI pipeline needs explicit workload install step.

**Fix pattern:**

```yaml

# GitHub Actions example
- name: Install .NET workloads
  run: dotnet workload install maui-android maui-ios

```yaml

### Pattern: Implicit NuGet Sources

**Example scenario:**

```text

Local restore succeeds (using cached packages).
CI error:
error NU1101: Unable to find package MyCompany.Internal.Lib.

```text

**Diagnosis:**

1. Local machine has the package in the global NuGet cache from a previous restore.
2. CI starts with a clean cache and cannot find the package because the private feed is not configured.
3. A `nuget.config` file is missing from the repository, or CI lacks feed credentials.

**Fix pattern:**

1. Add `nuget.config` to the repository root with all required package sources.
2. Configure CI to authenticate to private feeds (credential provider, PAT, or managed identity).
3. Do NOT rely on global NuGet cache for CI builds.

### Pattern: OS-Specific Path Differences

**Example scenario:**

```text

Local (Windows):  Build succeeds
CI (Linux):       error MSB4018: The "ResolveAssemblyReference" task failed.
                  Could not find file '/src/MyApp/../Shared/MyLib.dll'

```text

**Diagnosis:**

1. Windows file system is case-insensitive; Linux is case-sensitive.
2. A file reference uses different casing than the actual file on disk.
3. Or backslash path separators in MSBuild properties that Linux cannot resolve.

**Fix pattern:**

- Ensure file and directory names match the case used in project references exactly.
- Use forward slashes (`/`) in `.csproj` paths -- MSBuild normalizes them on all platforms.
- Test in a Linux container locally with `docker run` before pushing.

### Pattern: TreatWarningsAsErrors in CI Only

**Example scenario:**

```text

Local: Build succeeds with 3 warnings
CI:    error CS8602: Dereference of a possibly null reference.
       (because CI sets TreatWarningsAsErrors=true)

```text

**Diagnosis:**

1. CI pipeline or `Directory.Build.props` enables `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` via a CI-only
   condition.
2. Developers see warnings locally but never fix them because the build succeeds.

**Fix pattern:**

- Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` unconditionally in `Directory.Build.props` so local and
  CI builds behave identically.
- Fix all warnings. Do NOT add CI-only MSBuild properties that diverge from local behavior.

---

## Slopwatch Anti-Patterns

These patterns indicate an agent is hiding build problems rather than fixing them. Flag these during code review. See
[skill:dotnet-slopwatch] for the automated quality gate that detects these patterns.

### Warning Suppressions

```xml

<!-- RED FLAG: blanket NoWarn in .csproj -->
<PropertyGroup>
  <NoWarn>CS8600;CS8602;CS8603;CS8604;IL2026;IL2046</NoWarn>
</PropertyGroup>

```csharp

```csharp

// RED FLAG: pragma disable without justification
#pragma warning disable CS8618
public class UserModel
{
    public string Name { get; set; } // non-nullable not initialized
    public string Email { get; set; }
}
#pragma warning restore CS8618

```text

**Fix:** Remove `<NoWarn>` entries and fix the underlying issues. If suppression is truly needed, use `.editorconfig`
with per-rule severity and a comment explaining why.

### Silenced Analyzers Without Justification

```csharp

// RED FLAG: suppressing security analyzer with no explanation
[SuppressMessage("Security", "CA5351")]
public byte[] HashData(byte[] input)
{
    using var md5 = MD5.Create(); // insecure algorithm
    return md5.ComputeHash(input);
}

```text

```ini

# RED FLAG: disabling entire analyzer categories in .editorconfig
[*.cs]
dotnet_diagnostic.CA5350.severity = none
dotnet_diagnostic.CA5351.severity = none
dotnet_diagnostic.CA5358.severity = none

```csharp

**Fix:** Replace insecure algorithms (MD5 -> SHA-256). If suppression is unavoidable (e.g., interop with a system
requiring MD5), add a `Justification` string explaining the constraint.

---

## Cross-References

- [skill:dotnet-agent-gotchas] -- common agent coding mistakes that produce build errors
- [skill:dotnet-csproj-reading] -- project file structure, PropertyGroup/ItemGroup conventions
- [skill:dotnet-project-structure] -- SDK selection, project organization, multi-project solutions



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

- [MSBuild Error and Warning Reference](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-errors)
- [C# Compiler Errors](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/)
- [NuGet Error Reference](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings)
- [.NET Code Analysis Rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories)
- [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management)
- [.NET Trimming Warnings](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/fixing-warnings)
````
