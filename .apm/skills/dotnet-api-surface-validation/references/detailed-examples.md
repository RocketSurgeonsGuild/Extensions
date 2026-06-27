
```text

In a CI pipeline, `dotnet pack` runs package validation automatically:

```yaml

# GitHub Actions -- gate PRs on API compatibility
name: API Compatibility Check
on:
  pull_request:
    paths:
      - 'src/**'
      - '*.props'
      - '*.targets'

jobs:
  api-compat:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Pack with API validation
        run: dotnet pack --configuration Release --no-build
        # EnablePackageValidation runs during pack and fails
        # the build if breaking changes are detected

```text

### Standalone ApiCompat Tool for Assembly Comparison

When you need to compare assemblies without packing (e.g., comparing a feature branch build against the main branch
build), use the standalone ApiCompat tool:

```yaml

# GitHub Actions -- compare assemblies directly
name: API Diff Check
on:
  pull_request:

jobs:
  api-diff:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Install ApiCompat tool
        run: dotnet tool install --global Microsoft.DotNet.ApiCompat.Tool

      - name: Build current branch
        run: dotnet build src/MyLib/MyLib.csproj -c Release -o artifacts/current

      - name: Build baseline (main branch)
        run: |
          git stash
          git checkout origin/main -- src/MyLib/
          dotnet build src/MyLib/MyLib.csproj -c Release -o artifacts/baseline
          git checkout - -- src/MyLib/
          git stash pop || true

      - name: Compare APIs
        run: |
          apicompat --left-assembly artifacts/baseline/MyLib.dll \
                    --right-assembly artifacts/current/MyLib.dll

```text

### PR Labeling for API Changes

Combine ApiCompat with PR labeling to surface API changes to reviewers:

```yaml

- name: Check for API changes
  id: api-check
  continue-on-error: true
  run: |
    apicompat --left-assembly artifacts/baseline/MyLib.dll \
              --right-assembly artifacts/current/MyLib.dll 2>&1 | tee api-diff.txt
    echo "has_changes=$([[ -s api-diff.txt ]] && echo true || echo false)" >> "$GITHUB_OUTPUT"

- name: Label PR with API changes
  if: steps.api-check.outputs.has_changes == 'true'
  run: gh pr edit "${{ github.event.pull_request.number }}" --add-label "api-change"
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}

```text

### Handling Intentional Breaking Changes

When a breaking change is intentional (new major version), generate a suppression file:

```bash

dotnet pack /p:GenerateCompatibilitySuppressionFile=true

```bash

This creates `CompatibilitySuppressions.xml` in the project directory. Reference it explicitly if stored elsewhere:

```xml

<ItemGroup>
  <ApiCompatSuppressionFile Include="CompatibilitySuppressions.xml" />
</ItemGroup>

```xml

Note: `ApiCompatSuppressionFile` is an **ItemGroup item**, not a PropertyGroup property. Using PropertyGroup syntax
silently does nothing.

The suppression file documents the specific breaking changes that are accepted:

```xml

<?xml version="1.0" encoding="utf-8"?>
<Suppressions xmlns:xsd="http://www.w3.org/2001/XMLSchema"
              xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Suppression>
    <DiagnosticId>CP0002</DiagnosticId>
    <Target>M:MyLib.Widget.Calculate</Target>
    <Left>lib/net8.0/MyLib.dll</Left>
    <Right>lib/net8.0/MyLib.dll</Right>
  </Suppression>
</Suppressions>

```text

Commit suppression files to source control. Reviewers can inspect the file to verify that breaking changes are
documented and intentional.

### Enforcing PublicApiAnalyzers Files in CI

Combine PublicApiAnalyzers warnings-as-errors with a CI step that verifies tracking files are not stale:

```yaml

- name: Build with API tracking enforcement
  run: dotnet build -c Release /p:TreatWarningsAsErrors=true /warnaserror:RS0016,RS0017,RS0036,RS0037

- name: Verify PublicAPI files are committed
  run: |
    if git diff --name-only | grep -q 'PublicAPI'; then
      echo "::error::PublicAPI tracking files have uncommitted changes"
      git diff -- '**/PublicAPI.*.txt'
      exit 1
    fi

```text

### Multi-Library Monorepo Enforcement

For repositories with multiple libraries, apply API validation at the solution level:

```xml

<!-- Directory.Build.props -- applied to all library projects -->
<Project>
  <PropertyGroup Condition="'$(IsPackable)' == 'true'">
    <EnablePackageValidation>true</EnablePackageValidation>
    <WarningsAsErrors>$(WarningsAsErrors);RS0016;RS0017;RS0036;RS0037</WarningsAsErrors>
  </PropertyGroup>

  <ItemGroup Condition="'$(IsPackable)' == 'true'">
    <PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers"
                      Version="3.3.*" PrivateAssets="all" />
  </ItemGroup>
</Project>

```text

This ensures every packable project in the repository has both PublicApiAnalyzers and package validation enabled without
duplicating configuration.

---

## Agent Gotchas

1. **Do not forget to create both `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`** -- PublicApiAnalyzers requires
   both files to exist, even if empty. Missing files cause RS0037 warnings on every public member.
2. **Do not omit the `#nullable enable` header from PublicAPI tracking files** -- without it (RS0036), the analyzer
   ignores nullable annotation differences, missing real API surface changes in nullable-enabled libraries.
3. **Do not put `ApiCompatSuppressionFile` in a PropertyGroup** -- it is an ItemGroup item
   (`<ApiCompatSuppressionFile Include="..." />`). PropertyGroup syntax is silently ignored, and suppression will not
   work.
4. **Do not move entries from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt` mid-development** -- move entries
   only at release time. Premature shipping makes it impossible to cleanly revert unreleased API additions.
5. **Do not use the Verify API surface snapshot as the sole validation mechanism** -- it runs at test time, after
   compilation. Use PublicApiAnalyzers for immediate build-time feedback and ApiCompat for baseline comparison; add
   Verify snapshots as an additional safety net.
6. **Do not hardcode TFM-specific paths in CI ApiCompat workflows** -- use MSBuild output path variables or parameterize
   the TFM to avoid breakage when TFMs are added or changed.
7. **Do not suppress RS0016 globally with `<NoWarn>`** -- this silently disables all public API tracking. Instead, add
   the missing API entries to the tracking files. If an API is intentionally internal but must be `public` (e.g., for
   `InternalsVisibleTo` alternatives), use `[EditorBrowsable(EditorBrowsableState.Never)]` and add it to the tracking
   files.
8. **Do not generate the suppression file with `GenerateCompatibilitySuppressionFile=true` and forget to review it** --
   the file may suppress more changes than intended. Always review the generated XML before committing.

---

## Prerequisites

- .NET 8.0+ SDK
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` NuGet package (for RS0016/RS0017 diagnostics)
- `EnablePackageValidation` MSBuild property (for baseline API comparison during `dotnet pack`)
- `Microsoft.DotNet.ApiCompat.Tool` (optional, for standalone assembly comparison outside of `dotnet pack`)
- Verify test library and test framework integration package (for API surface snapshot testing) -- see
  [skill:dotnet-snapshot-testing] for setup
- Understanding of binary vs source compatibility rules -- see [skill:dotnet-library-api-compat]

---



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

- [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md)
- [Microsoft Learn: Package validation](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview)
- [Microsoft Learn: API compatibility](https://learn.microsoft.com/dotnet/fundamentals/apicompat/overview)
- [Microsoft.DotNet.ApiCompat.Tool](https://www.nuget.org/packages/Microsoft.DotNet.ApiCompat.Tool)
- [Verify library](https://github.com/VerifyTests/Verify) -- snapshot testing framework
- [PublicApiAnalyzers diagnostics reference](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/api-design-rules)
````
