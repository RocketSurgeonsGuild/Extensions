.NET projects use several configuration files scattered across the solution. Knowing where to find them is essential for understanding application behavior.

### appsettings*.json

**Discovery command:**

```bash

# Find all appsettings files
find . -name "appsettings*.json" -not -path "*/obj/*" -not -path "*/bin/*" | sort

```bash

**Example output:**

```text

./src/MyApp.Api/appsettings.json
./src/MyApp.Api/appsettings.Development.json
./src/MyApp.Api/appsettings.Production.json
./src/MyApp.Worker/appsettings.json

```json

**Key behavior:** Environment-specific files (`appsettings.{ENVIRONMENT}.json`) override values from the base `appsettings.json`. The environment is set via `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT`.

### launchSettings.json

**Discovery command:**

```bash

# Find launch settings (inside Properties/ folder of each project)
find . -name "launchSettings.json" -not -path "*/obj/*" -not -path "*/bin/*"

```bash

**Example output:**

```text

./src/MyApp.Api/Properties/launchSettings.json
./src/MyApp.Web/Properties/launchSettings.json

```json

**Key behavior:** Used by `dotnet run` and Visual Studio to configure launch profiles (ports, environment variables, launch URLs). Not deployed to production.

### Directory.Build.props and Directory.Build.targets

**Discovery command:**

```bash

# Find all Directory.Build.props/targets files (may exist at multiple levels)
find . -name "Directory.Build.props" -o -name "Directory.Build.targets" | sort

```bash

**Example output:**

```text

./Directory.Build.props
./Directory.Build.targets
./src/Directory.Build.props
./tests/Directory.Build.props

```xml

**Key behavior:** MSBuild imports the nearest file found walking upward from the project directory. Nested files shadow parent files unless they explicitly import the parent (see [skill:dotnet-csproj-reading] for chaining).

### Other Configuration Files

```bash

# Find all .NET configuration files in one sweep
find . \( -name "nuget.config" -o -name "global.json" -o -name ".editorconfig" \
  -o -name "Directory.Packages.props" \) -not -path "*/obj/*" | sort

```bash

**Example output:**

```text

./.editorconfig
./Directory.Packages.props
./global.json
./nuget.config
./src/.editorconfig

```json

| File | Purpose | Resolution |
|------|---------|-----------|
| `nuget.config` | NuGet package sources and mappings | Hierarchical upward from project dir |
| `global.json` | SDK version pinning | Nearest file walking upward |
| `.editorconfig` | Code style and analyzer severity | Hierarchical (sections merge upward) |
| `Directory.Packages.props` | Central package version management | Hierarchical upward from project dir |

---

## Subsection 5: Common Solution Layouts

Recognizing the layout pattern helps agents navigate unfamiliar codebases faster.

### Pattern 1: src/tests Layout

The most common layout. Source projects in `src/`, test projects in `tests/`, mirroring names.

```text

MyApp/
  MyApp.sln
  Directory.Build.props
  Directory.Packages.props
  global.json
  nuget.config
  .editorconfig
  src/
    MyApp.Api/
      MyApp.Api.csproj
      Program.cs
      Controllers/
      Services/
    MyApp.Core/
      MyApp.Core.csproj
      Models/
      Interfaces/
    MyApp.Infrastructure/
      MyApp.Infrastructure.csproj
      Data/
      Repositories/
  tests/
    MyApp.Api.Tests/
      MyApp.Api.Tests.csproj
    MyApp.Core.Tests/
      MyApp.Core.Tests.csproj
  docs/
    architecture.md

```csharp

**Heuristics:**
- `src/` and `tests/` directories at root level.
- Test project names mirror source project names with `.Tests` suffix.
- Shared build config (`Directory.Build.props`, `global.json`) at the root.

**Discovery:**

```bash

# Detect src/tests layout
ls -d src/ tests/ 2>/dev/null && echo "src/tests layout detected"

```bash

### Pattern 2: Vertical Slice Layout

Organizes code by feature rather than by technical layer. Each slice contains its own models, handlers, and endpoints.

```text

MyApp/
  MyApp.sln
  src/
    MyApp.Api/
      MyApp.Api.csproj
      Program.cs
      Features/
        Orders/
          CreateOrder.cs          # Handler + request + response
          GetOrder.cs
          OrderValidator.cs
          OrderEndpoints.cs       # Minimal API endpoint mapping
        Products/
          CreateProduct.cs
          ListProducts.cs
          ProductEndpoints.cs
      Common/
        Behaviors/
          ValidationBehavior.cs
        Middleware/
          ExceptionMiddleware.cs
  tests/
    MyApp.Api.Tests/
      Features/
        Orders/
          CreateOrderTests.cs
          GetOrderTests.cs

```csharp

**Heuristics:**
- `Features/` directory within a project.
- Each feature folder contains multiple related files (handler, validator, endpoint).
- Tests mirror the feature folder structure.

**Discovery:**

```bash

# Detect vertical slice layout
find . -type d -name "Features" -not -path "*/obj/*" -not -path "*/bin/*"

```bash

### Pattern 3: Modular Monolith

Multiple bounded contexts as separate projects within a single solution, communicating through explicit interfaces or a shared message bus.

```text

MyApp/
  MyApp.sln
  src/
    MyApp.Host/
      MyApp.Host.csproj          # Composition root -- references all modules
      Program.cs
    Modules/
      Ordering/
        MyApp.Ordering/
          MyApp.Ordering.csproj
          OrderingModule.cs       # Module registration (DI, endpoints)
          Domain/
          Application/
          Infrastructure/
        MyApp.Ordering.Tests/
      Catalog/
        MyApp.Catalog/
          MyApp.Catalog.csproj
          CatalogModule.cs
          Domain/
          Application/
          Infrastructure/
        MyApp.Catalog.Tests/
    MyApp.Shared/
      MyApp.Shared.csproj        # Cross-cutting contracts (events, interfaces)

```csharp

**Heuristics:**
- `Modules/` directory with self-contained bounded contexts.
- A `Host` or `Startup` project that references all modules.
- A `Shared` project for cross-module contracts.

**Discovery:**

```bash

# Detect modular monolith layout
find . -type d -name "Modules" -not -path "*/obj/*" -not -path "*/bin/*"
# Or look for module registration patterns
grep -rn "Module\|AddModule\|RegisterModule" --include="*.cs" . | grep -v "obj/" | head -10

```csharp

---

## Slopwatch Anti-Patterns

These patterns in test project discovery indicate an agent is hiding testing gaps rather than addressing them. See [skill:dotnet-slopwatch] for the automated quality gate that detects these patterns.

### Disabled or Skipped Tests in Test Project Discovery

When navigating a solution and identifying test projects, watch for tests that exist but are silently disabled:

```csharp

// RED FLAG: skipped tests that will not run during dotnet test
[Fact(Skip = "Flaky -- revisit later")]
public async Task ProcessOrder_ConcurrentRequests_HandledCorrectly() { }

// RED FLAG: entire test class disabled via conditional compilation
#if false
public class OrderIntegrationTests
{
    [Fact]
    public async Task CreateOrder_PersistsToDatabase() { }
}
#endif

// RED FLAG: commented-out test methods
// [Fact]
// public void CalculateDiscount_NegativeAmount_ThrowsException() { }

```text

**Discovery commands to check for disabled tests:**

```bash

# Find skipped tests
grep -rEn 'Skip[[:space:]]*=' --include="*.cs" . | grep -v "obj/" | grep -v "bin/"

# Find tests hidden behind #if false
grep -rn "#if false" --include="*.cs" . | grep -v "obj/" | grep -v "bin/"

# Find commented-out test attributes
grep -rEn '//[[:space:]]*\[(Fact|Theory|Test)\]' --include="*.cs" . | grep -v "obj/" | grep -v "bin/"

```csharp

**Fix:** Investigate why tests are disabled. If they are flaky due to timing, fix the non-determinism or use `[Retry]` (xUnit v3). If they test removed functionality, delete them. Never leave disabled tests as invisible technical debt.

---

## Cross-References

- [skill:dotnet-project-structure] -- project organization, SDK selection, solution layout decisions
- [skill:dotnet-csproj-reading] -- reading and modifying .csproj files found during navigation
- [skill:dotnet-testing-strategy] -- test project identification, test types, test organization

## References

- [.NET Project SDK Overview](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- [dotnet sln Command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln)
- [.slnx Solution Format](https://learn.microsoft.com/en-us/visualstudio/ide/reference/solution-file-slnx)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Directory.Build.props/targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)
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
