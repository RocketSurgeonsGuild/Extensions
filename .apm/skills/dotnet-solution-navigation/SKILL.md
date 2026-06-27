---
name: dotnet-solution-navigation
category: developer-experience
subcategory: project
description: Orients in .NET solutions -- entry points, .sln/.slnx, dependency graphs, config.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
  model: haiku
copilot: {}
codexcli:
  short-description: '.NET skill guidance for csharp tasks'
geminicli: {}
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
antigravity: {}
---

````! find . -maxdepth 2 ( -name "*.sln" -o -name "*.slnx" ) 2>/dev/null | head -5


```bash

# dotnet-solution-navigation

Teaches agents to orient in .NET solutions: finding entry points, parsing solution files, traversing project dependencies, locating configuration files, and recognizing common solution layouts. Each subsection includes discovery commands/heuristics and example output.

## Scope

- Entry point discovery (Program.cs, top-level statements, worker services)
- Solution file parsing (.sln, .slnx)
- Project dependency graph traversal
- Configuration file location (appsettings.json, launchSettings.json)

## Out of scope

- Project file structure and modification -- see [skill:dotnet-csproj-reading]
- Project organization decisions and SDK selection -- see [skill:dotnet-project-structure]
- Test framework configuration and test type decisions -- see [skill:dotnet-testing-strategy]

## Prerequisites

.NET 8.0+ SDK. `dotnet` CLI available on PATH. Familiarity with SDK-style projects.

Cross-references: [skill:dotnet-project-structure] for project organization guidance, [skill:dotnet-csproj-reading] for reading and modifying .csproj files found during navigation, [skill:dotnet-testing-strategy] for test project identification and test type decisions.

---

## Subsection 1: Entry Point Discovery

.NET applications can start from several patterns. Do not assume every app has a traditional `Program.cs` with a `Main` method.

### Pattern 1: Traditional Program.cs with Main Method

Used in older projects, worker services, and when explicit control over hosting is needed.

**Discovery command:**

```bash

# Find Program.cs files containing a Main method
grep -rn "static.*void Main\|static.*Task Main\|static.*async.*Main" --include="*.cs" .

```bash

**Example output:**

```text

src/MyApp.Worker/Program.cs:5:    public static async Task Main(string[] args)
src/MyApp.Console/Program.cs:3:    static void Main(string[] args)

```csharp

### Pattern 2: Top-Level Statements (C# 9+)

Modern .NET projects (templates since .NET 6) use top-level statements -- the file contains no class or Main method, just executable code.

**Discovery command:**

```bash

# Find Program.cs files that do NOT contain class/namespace declarations
# (top-level statements have no enclosing class)
for f in $(find . -name "Program.cs" -not -path "*/obj/*" -not -path "*/bin/*"); do
  if ! grep -Eq '^[[:space:]]*(class|namespace)[[:space:]]' "$f" 2>/dev/null; then
    echo "Top-level: $f"
  fi
done

```text

**Example output:**

```text

Top-level: ./src/MyApp.Api/Program.cs
Top-level: ./src/MyApp.Web/Program.cs

```csharp

**Typical content of a top-level Program.cs:**

```csharp

// No namespace, no class, no Main -- this IS the entry point
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();

```text

### Pattern 3: Worker Services and Background Hosts

Worker services use `Host.CreateDefaultBuilder` or `Host.CreateApplicationBuilder` without a web server. They appear as `Exe` output type with `Microsoft.NET.Sdk.Worker` SDK.

**Discovery command:**

```bash

# Find worker service projects by SDK type
grep -rn 'Sdk="Microsoft.NET.Sdk.Worker"' --include="*.csproj" .

# Or find IHostedService/BackgroundService implementations
grep -rn "BackgroundService\|IHostedService" --include="*.cs" . | grep -v "obj/" | grep -v "bin/"

```csharp

**Example output:**

```text

src/MyApp.Worker/MyApp.Worker.csproj:1:<Project Sdk="Microsoft.NET.Sdk.Worker">
src/MyApp.Worker/Services/OrderProcessor.cs:8:public class OrderProcessor : BackgroundService
src/MyApp.Worker/Services/EmailSender.cs:5:public class EmailSender : IHostedService

```csharp

### Pattern 4: Test Projects

Test projects are entry points for `dotnet test`. They may not have a `Program.cs` at all -- the test runner provides the entry point.

**Discovery command:**

```bash

# Find test projects by IsTestProject property or test SDK references
grep -rn "<IsTestProject>true</IsTestProject>" --include="*.csproj" .
grep -rn "Microsoft.NET.Test.Sdk\|xunit\|NUnit\|MSTest" --include="*.csproj" . | grep -v "obj/"  # Matches both xunit.v3 and legacy xunit

```bash

**Example output:**

```text

tests/MyApp.Api.Tests/MyApp.Api.Tests.csproj:5:    <IsTestProject>true</IsTestProject>
tests/MyApp.Core.Tests/MyApp.Core.Tests.csproj:8:    <PackageReference Include="xunit.v3" />

```csharp

### Summary Heuristic

When orienting in a new .NET solution, run these commands in sequence:

```bash

# 1. Find all .csproj files
find . -name "*.csproj" -not -path "*/obj/*" | sort

# 2. Identify output types (Exe = app entry point, Library = dependency)
grep -rn "<OutputType>" --include="*.csproj" .

# 3. Find all Program.cs files
find . -name "Program.cs" -not -path "*/obj/*" -not -path "*/bin/*"

# 4. Identify test projects
grep -rn "<IsTestProject>true" --include="*.csproj" .

```csharp

---

## Subsection 2: Solution File Formats

.NET solutions use `.sln` (text-based, legacy format) or `.slnx` (XML-based, .NET 9+ preview). Both files list projects and their relationships.

### .sln Format

The traditional solution format is a text file with a custom syntax (not XML).

**Discovery and parsing commands:**

```bash

# Find solution files
find . -name "*.sln" -maxdepth 2

# List all projects in a solution using dotnet CLI
dotnet sln list
# Or specify the solution file explicitly:
dotnet sln MyApp.sln list

```text

**Example output of `dotnet sln list`:**

```text

Project(s)
----------
src/MyApp.Api/MyApp.Api.csproj
src/MyApp.Core/MyApp.Core.csproj
src/MyApp.Infrastructure/MyApp.Infrastructure.csproj
tests/MyApp.Api.Tests/MyApp.Api.Tests.csproj
tests/MyApp.Core.Tests/MyApp.Core.Tests.csproj

```csharp

**Reading the .sln file directly** (useful when `dotnet sln list` is not available):

```bash

# Extract project entries from .sln file
grep "^Project(" MyApp.sln

```bash

**Example output:**

```text

Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MyApp.Api", "src\MyApp.Api\MyApp.Api.csproj", "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MyApp.Core", "src\MyApp.Core\MyApp.Core.csproj", "{B2C3D4E5-F6A7-8901-BCDE-F12345678901}"

```csharp

The GUID `{FAE04EC0-...}` identifies C# projects. The second value is the relative path to the `.csproj` file.

### .slnx Format (.NET 9+)

The `.slnx` format is an XML-based solution file introduced as a preview feature in .NET 9.

**Discovery and parsing commands:**

```bash

# Find .slnx files
find . -name "*.slnx" -maxdepth 2

# dotnet sln commands work with .slnx files too
dotnet sln MyApp.slnx list

```bash

**Example .slnx content:**

```xml

<Solution>
  <Folder Name="/src/">
    <Project Path="src/MyApp.Api/MyApp.Api.csproj" />
    <Project Path="src/MyApp.Core/MyApp.Core.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/MyApp.Api.Tests/MyApp.Api.Tests.csproj" />
  </Folder>
</Solution>

```csharp

**Key differences from .sln:**

| Feature | .sln | .slnx |
|---------|------|-------|
| Format | Custom text | XML |
| Readability | Low (GUIDs, custom syntax) | High (clean XML) |
| Availability | All .NET versions | .NET 9+ preview |
| Tooling | Full support | Partial (growing) |
| Solution folders | Nested GUID references | `<Folder>` elements |

### When No Solution File Exists

Some repositories use individual `.csproj` files without a `.sln`. Build and run from project directories:

```bash

# If no .sln exists, find all .csproj files and build individually
find . -name "*.csproj" -not -path "*/obj/*" | sort
dotnet build src/MyApp.Api/MyApp.Api.csproj

```bash

---

## Subsection 3: Project Dependency Traversal

Understanding `ProjectReference` chains is critical for determining build order, finding shared code, and identifying the impact of changes.

### Discovery Commands

```bash

# Find all ProjectReference entries across the solution
grep -rn "<ProjectReference" --include="*.csproj" . | grep -v "obj/"

```bash

**Example output:**

```text

src/MyApp.Api/MyApp.Api.csproj:12:    <ProjectReference Include="../MyApp.Core/MyApp.Core.csproj" />
src/MyApp.Api/MyApp.Api.csproj:13:    <ProjectReference Include="../MyApp.Infrastructure/MyApp.Infrastructure.csproj" />
src/MyApp.Infrastructure/MyApp.Infrastructure.csproj:10:    <ProjectReference Include="../MyApp.Core/MyApp.Core.csproj" />
tests/MyApp.Api.Tests/MyApp.Api.Tests.csproj:14:    <ProjectReference Include="../../src/MyApp.Api/MyApp.Api.csproj" />

```csharp

### Building a Dependency Graph

From the above output, the dependency graph is:

```text

MyApp.Api.Tests
  -> MyApp.Api
       -> MyApp.Core
       -> MyApp.Infrastructure
            -> MyApp.Core

```text

**Automated traversal using `dotnet list reference`:**

```bash

# List direct references for a specific project
dotnet list src/MyApp.Api/MyApp.Api.csproj reference

```bash

**Example output:**

```text

Project reference(s)
--------------------
../MyApp.Core/MyApp.Core.csproj
../MyApp.Infrastructure/MyApp.Infrastructure.csproj

```csharp

**Full transitive dependency analysis:**

```bash

# Build the full dependency tree by traversing transitively
# Start from the top-level project and follow each reference
dotnet list src/MyApp.Api/MyApp.Api.csproj reference
dotnet list src/MyApp.Infrastructure/MyApp.Infrastructure.csproj reference
# Continue until you reach projects with no ProjectReference entries

```csharp

### Impact Analysis

When modifying a shared project like `MyApp.Core`, all projects that reference it (directly or transitively) are affected:

```bash

# Find all projects that reference a specific project
grep -rn "MyApp.Core.csproj" --include="*.csproj" . | grep -v "obj/"

```bash

**Example output:**

```text

src/MyApp.Api/MyApp.Api.csproj:12:    <ProjectReference Include="../MyApp.Core/MyApp.Core.csproj" />
src/MyApp.Infrastructure/MyApp.Infrastructure.csproj:10:    <ProjectReference Include="../MyApp.Core/MyApp.Core.csproj" />
tests/MyApp.Core.Tests/MyApp.Core.Tests.csproj:14:    <ProjectReference Include="../../src/MyApp.Core/MyApp.Core.csproj" />

```csharp

This means changes to `MyApp.Core` require testing `MyApp.Api`, `MyApp.Infrastructure`, and `MyApp.Core.Tests`.

---

## Subsection 4: Configuration File Locations

.NET projects use several configuration files scattered across the solution. Knowing where to find them is essential for understanding application behavior.

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
