  with:
    path: ~/.nuget/packages
    key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.lock.json') }}
    restore-keys: |
      nuget-${{ runner.os }}-

# Use locked restore for speed and determinism
- name: Restore
  run: dotnet restore --locked-mode

```text

### NoWarn and TreatWarningsAsErrors Strategy

Build-level warning configuration affects build time when analyzers are involved:

```xml

<!-- Directory.Build.props: set warning policy for all projects -->
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

  <!-- Suppress specific warnings globally (with justification) -->
  <NoWarn>$(NoWarn);CA2007</NoWarn>  <!-- ConfigureAwait: not needed in ASP.NET Core apps -->
</PropertyGroup>

```text

**Rules for warning configuration:**

- Enable `TreatWarningsAsErrors` in `Directory.Build.props` so local and CI builds behave identically
- Use `NoWarn` sparingly and always with inline justification comments
- Prefer `.editorconfig` severity rules over `NoWarn` for per-rule control
- For detecting misuse of warning suppression, see [skill:dotnet-build-analysis]

---

## Agent Gotchas

1. **Running `dotnet build` without `/bl` when diagnosing build issues.** Console output at default verbosity omits
   critical information about why targets ran. Always capture a binary log (`/bl`) for diagnosis -- it records
   everything regardless of console verbosity level.

1. **Assuming incremental build works without Inputs/Outputs.** A target without `Inputs`/`Outputs` runs on every build
   unconditionally. There is no implicit incrementality in MSBuild -- you must declare what files the target reads and
   writes. See [skill:dotnet-msbuild-authoring] for the full pattern.

1. **Forgetting `SkipUnchangedFiles="true"` on Copy tasks.** Without this flag, `Copy` always updates the destination
   timestamp, which triggers downstream targets to re-run even when file content is identical.

1. **Using `/v:diagnostic` instead of `/bl` for build investigation.** Diagnostic verbosity floods the console with
   thousands of lines and is hard to search. Binary logs contain the same information in a structured, searchable
   format. Use `/bl` and the Structured Log Viewer instead.

1. **Sharing the `.binlog` file without reviewing it first.** Binary logs contain full file paths, environment variable
   values, and potentially secrets passed via MSBuild properties. Review or sanitize before sharing externally.

1. **Assuming `/m` (parallel build) is always faster.** For small solutions (fewer than 5 projects), the overhead of
   spawning worker nodes can exceed the parallelism benefit. Profile with and without `/m` to confirm. For large
   solutions, `/graph` mode provides better scheduling than default `/m`.

1. **Committing `packages.lock.json` without using `--locked-mode` in CI.** The lock file is only useful if CI restores
   in locked mode. Without `--locked-mode`, NuGet ignores the lock file and resolves normally, defeating the purpose of
   deterministic restores.

1. **Modifying `.csproj` properties to fix build performance without checking the binary log first.** Many "slow build"
   issues are caused by a single non-incremental target, not by global build configuration. Diagnose with `/bl` before
   making broad configuration changes.

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

- [MSBuild Binary Log](https://learn.microsoft.com/en-us/visualstudio/msbuild/obtaining-build-logs#save-a-binary-log)
- [MSBuild Structured Log Viewer](https://msbuildlog.com/)
- [Incremental Builds](https://learn.microsoft.com/en-us/visualstudio/msbuild/incremental-builds)
- [MSBuild Parallel Builds](https://learn.microsoft.com/en-us/visualstudio/msbuild/building-multiple-projects-in-parallel-with-msbuild)
- [Graph Build](https://github.com/dotnet/msbuild/blob/main/documentation/specs/static-graph.md)
- [NuGet Lock Files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [Customize Your Build](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-your-build)
````
