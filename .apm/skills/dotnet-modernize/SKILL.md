---
name: dotnet-modernize
category: developer-experience
subcategory: analyzers
description: Analyzes .NET code for modernization. Outdated TFMs, deprecated packages, superseded patterns.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-modernize

Analyze existing .NET code for modernization opportunities. Identifies outdated target frameworks, deprecated packages,
superseded API patterns, and missing modern best practices. Provides actionable recommendations for each finding.

## Scope

- Outdated target framework detection
- Deprecated package identification
- Superseded API pattern flagging
- Actionable modernization recommendations

## Out of scope

- Actual migration paths and polyfill strategies -- see [skill:dotnet-version-upgrade]
- Multi-targeting guidance -- see [skill:dotnet-multi-targeting]

**Prerequisites:** Run [skill:dotnet-version-detection] first to determine the current SDK, TFM, and language version.
Run [skill:dotnet-project-analysis] to understand solution structure and dependencies.

Cross-references: [skill:dotnet-project-structure] for modern layout conventions, [skill:dotnet-add-analyzers] for
analyzer-based detection of deprecated patterns, [skill:dotnet-scaffold-project] for the target state of a fully
modernized project.

---

## Modernization Checklist

Run through this checklist against the existing codebase. Each section identifies what to look for and what the modern
replacement is.

### 1. Target Framework

Check `<TargetFramework>` in `.csproj` files (or `Directory.Build.props`):

| Current TFM          | Status                          | Recommendation                          |
| -------------------- | ------------------------------- | --------------------------------------- |
| `net8.0`             | LTS -- supported until Nov 2026 | Plan upgrade to `net10.0` (LTS)         |
| `net9.0`             | STS -- support ends May 2026    | Upgrade to `net10.0` promptly           |
| `net7.0`             | End of life                     | Upgrade immediately                     |
| `net6.0`             | End of life                     | Upgrade immediately                     |
| `net5.0` or lower    | End of life                     | Upgrade immediately                     |
| `netstandard2.0/2.1` | Supported (library compat)      | Keep if multi-targeting for broad reach |
| `netcoreapp3.1`      | End of life                     | Upgrade immediately                     |
| `.NET Framework 4.x` | Legacy                          | Evaluate migration feasibility          |

To scan all projects:

````bash

# Find all TFMs in the solution
find . -name "*.csproj" -exec grep -h "TargetFramework" {} \; | sort -u

# Check Directory.Build.props
grep "TargetFramework" Directory.Build.props 2>/dev/null

```csharp

---

### 2. Deprecated and Superseded Packages

Scan `Directory.Packages.props` (or individual `.csproj` files) for packages that have been superseded:

| Deprecated Package | Replacement | Since |
|-------------------|-------------|-------|
| `Microsoft.Extensions.Http.Polly` | `Microsoft.Extensions.Http.Resilience` | .NET 8 |
| `Newtonsoft.Json` (new projects) | `System.Text.Json` | .NET Core 3.0+ |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | Built-in STJ | .NET Core 3.0+ |
| `Swashbuckle.AspNetCore` | Built-in OpenAPI (`Microsoft.AspNetCore.OpenApi`) for document generation; keep Swashbuckle if using Swagger UI, filters, or codegen | .NET 9 |
| `NSwag.AspNetCore` | Built-in OpenAPI for document generation; keep NSwag if using client generation or Swagger UI features | .NET 9 |
| `Microsoft.Extensions.Logging.Log4Net.AspNetCore` | Built-in logging + `Serilog` or `OpenTelemetry` | .NET Core 2.0+ |
| `Microsoft.AspNetCore.Authentication.JwtBearer` (explicit NuGet package) | Remove explicit PackageReference -- included in `Microsoft.AspNetCore.App` shared framework | .NET Core 3.0+ |
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` | .NET Core 3.0+ |
| `Microsoft.Azure.Storage.*` | `Azure.Storage.*` | 2020+ |
| `WindowsAzure.Storage` | `Azure.Storage.Blobs` / `Azure.Storage.Queues` | 2020+ |
| `Microsoft.Azure.ServiceBus` | `Azure.Messaging.ServiceBus` | 2020+ |
| `Microsoft.Azure.EventHubs` | `Azure.Messaging.EventHubs` | 2020+ |
| `EntityFramework` (EF6) | `Microsoft.EntityFrameworkCore` | .NET Core 1.0+ |
| `RestSharp` (older versions) | `HttpClient` + `System.Text.Json` | .NET Core+ |
| `AutoMapper` | Manual mapping or source-generated mappers | Preference |

To scan for deprecated packages:

```bash

# List all package references
grep -r "PackageReference" *.csproj Directory.Packages.props 2>/dev/null | grep -o 'Include="[^"]*"' | sort -u

```bash

---

### 3. Superseded Patterns

Identify common patterns that have modern replacements:

#### 3.1 Async/Await

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| `async void` (non-event handlers) | `async Task` | Exception handling, composability |
| `.Result` / `.Wait()` | `await` with proper async propagation | No deadlocks |
| `Task.Run` for I/O | Direct async I/O | Fewer thread switches |
| Manual `ConfigureAwait(false)` | Global policy or targeted use | Cleaner code |

#### 3.2 Dependency Injection

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| `new ServiceCollection()` in test | `WebApplicationFactory` | Realistic test environment |
| Manual service location | Constructor injection | Explicit dependencies |
| `IServiceProvider` casting | Generic `GetRequiredService<T>()` | Type safety |

#### 3.3 Configuration

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| `ConfigurationBuilder` manual setup | `HostBuilder`/`WebApplicationBuilder` | Convention over configuration |
| `appSettings.json` only | `IConfiguration` with env vars, Key Vault, AppConfig | 12-factor compliance |
| `ConfigurationManager` | `ConfigurationBuilder` + `AddJsonFile` | Flexibility |

#### 3.4 HTTP Clients

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| `new HttpClient()` per request | `IHttpClientFactory` | Socket exhaustion prevention |
| `HttpClient` singleton | `IHttpClientFactory` + typed clients | DNS rotation |
| Manual retry logic | `Microsoft.Extensions.Http.Resilience` | Polly integration |

#### 3.5 JSON Serialization

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| `Newtonsoft.Json` for new projects | `System.Text.Json` | Native AOT compatible, faster |
| `JsonConvert.SerializeObject` | `JsonSerializer.Serialize` | Consistent with framework |
| Custom converters without source gen | JSON source generators | Better startup performance |

#### 3.6 Logging

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| `Console.WriteLine` | `ILogger<T>` | Structured logging, filtering |
| Static logger access | Injected `ILogger<T>` | Testability, scoping |
| `Log4Net` / `NLog` in new projects | `Microsoft.Extensions.Logging` | Unified ecosystem |

#### 3.7 Data Access

| Old Pattern | Modern Pattern | Benefit |
|-------------|----------------|---------|
| Raw ADO.NET without `await` | `DbContext` + async EF Core | Productivity |
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` | Active development, bug fixes |
| `SqlConnection` per query | Connection pooling via DI | Performance |

---

## Output Format

Provide findings in a structured format:

```markdown

## Modernization Report

### Target Framework
- **Current:** `net6.0` (⚠️ End of life)
- **Recommendation:** Upgrade to `net8.0` or `net10.0`
- **Effort:** Medium -- requires testing

### Deprecated Packages
| Package | Current Version | Status | Replacement |
|---------|----------------|--------|-------------|
| `Newtonsoft.Json` | 13.0.3 | ⚠️ Consider STJ | `System.Text.Json` |
| `Microsoft.Extensions.Http.Polly` | 8.0.0 | ❌ Deprecated | `Microsoft.Extensions.Http.Resilience` |

### Superseded Patterns
| File | Line | Pattern | Modern Alternative |
|------|------|---------|-------------------|
| `Program.cs` | 42 | `async void` | `async Task` |
| `Startup.cs` | 15 | Manual `ConfigurationBuilder` | `WebApplicationBuilder` |

### Recommendations Priority
1. **Critical:** Upgrade EOL TFMs immediately
2. **High:** Replace deprecated packages before next release
3. **Medium:** Refactor superseded patterns incrementally
4. **Low:** Consider optional modernizations for consistency

```text

---

## References

- [.NET Release Lifecycle](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [Obsolete APIs in .NET](https://learn.microsoft.com/en-us/dotnet/core/compatibility/obsolete-apis)
- [PackageDeprecation on NuGet](https://devblogs.microsoft.com/nuget/deprecating-packages/)
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
