---
name: dotnet-test-quality
category: testing
subcategory: fundamentals
description: Measures test effectiveness. Coverlet code coverage, Stryker.NET mutation testing, flaky tests.
license: MIT
targets: ['*']
tags: [testing, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for testing tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-test-quality

Test quality analysis for .NET projects. Covers code coverage collection with coverlet, human-readable coverage reports
with ReportGenerator, CRAP (Change Risk Anti-Patterns) score analysis to identify undertested complex code, mutation
testing with Stryker.NET to evaluate test suite effectiveness, and strategies for detecting and managing flaky tests.

**Version assumptions:** Coverlet 6.x+, ReportGenerator 5.x+, Stryker.NET 4.x+ (.NET 8.0+ baseline). Coverlet supports
both the MSBuild integration (`coverlet.msbuild`) and the `coverlet.collector` data collector; examples use
`coverlet.collector` as the recommended approach.

## Scope

- Coverlet code coverage collection and configuration
- ReportGenerator for human-readable coverage reports
- CRAP score analysis for undertested complex code
- Stryker.NET mutation testing for test suite evaluation
- Flaky test detection and management strategies

## Out of scope

- Test project scaffolding (creating projects, package references, coverlet setup) -- see [skill:dotnet-add-testing]
- Testing strategy and test type decisions -- see [skill:dotnet-testing-strategy]
- CI test reporting and pipeline integration -- see [skill:dotnet-gha-build-test] and [skill:dotnet-ado-build-test]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with coverlet packages referenced.
.NET 8.0+ baseline required.

Cross-references: [skill:dotnet-testing-strategy] for deciding what to test and coverage target guidance,
[skill:dotnet-xunit] for xUnit test framework features and configuration.

---

## Code Coverage with Coverlet

Coverlet is the standard open-source code coverage library for .NET. It instruments assemblies at build time or via a
data collector and produces coverage reports in multiple formats.

### Packages

````xml

<!-- Data collector approach (recommended) -->
<PackageReference Include="coverlet.collector" Version="8.0.0">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>

```text

### Collecting Coverage

```bash

# Collect coverage with Cobertura output (default for ReportGenerator)
dotnet test --collect:"XPlat Code Coverage"

# Specify output format explicitly
dotnet test --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# Multiple formats
dotnet test --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura,opencover

```text

Coverage results are written to `TestResults/<guid>/coverage.cobertura.xml` under each test project's output directory.

### Filtering Coverage

Exclude generated code, test projects, or specific namespaces:

```bash

dotnet test --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*.Tests]*,[*.IntegrationTests]*" \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByAttribute="GeneratedCodeAttribute,ObsoleteAttribute,ExcludeFromCodeCoverageAttribute"

```bash

Or configure via a `runsettings` file for repeatability:

```xml

<!-- coverlet.runsettings -->
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[*.Tests]*,[*.IntegrationTests]*</Exclude>
          <ExcludeByAttribute>
            GeneratedCodeAttribute,ObsoleteAttribute,ExcludeFromCodeCoverageAttribute
          </ExcludeByAttribute>
          <ExcludeByFile>**/Migrations/**</ExcludeByFile>
          <IncludeTestAssembly>false</IncludeTestAssembly>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>

```text

```bash

dotnet test --settings coverlet.runsettings

```bash

### Merge Coverage from Multiple Test Projects

When a solution has multiple test projects, merge their coverage into a single report:

```bash

# Run all tests, collecting coverage per project
dotnet test --collect:"XPlat Code Coverage"

# Find all coverage files and merge via ReportGenerator (see next section)

```text

---

## Coverage Reports with ReportGenerator

ReportGenerator converts raw coverage data (Cobertura, OpenCover) into human-readable HTML reports with line-level highlighting.

### Installation

```bash

# Install as a global tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Or as a local tool
dotnet tool install dotnet-reportgenerator-globaltool

```text

### Generating Reports

```bash

# Single coverage file
reportgenerator \
  -reports:"tests/MyApp.Tests/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;TextSummary"

# Multiple test projects (glob pattern merges automatically)
reportgenerator \
  -reports:"**/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;Cobertura;TextSummary"

```xml

### Report Types

| Type | Description | Use Case |
|------|-------------|----------|
| `Html` | Interactive HTML with line highlighting | Local developer review |
| `HtmlInline_AzurePipelines` | HTML optimized for Azure DevOps | CI artifact |
| `Cobertura` | Merged Cobertura XML | Input for other tools |
| `TextSummary` | Plain text summary | CLI/CI output |
| `Badges` | SVG coverage badges | README badges |
| `MarkdownSummaryGithub` | GitHub-flavored markdown | PR comments |

### Example: Full Coverage Pipeline

```bash

#!/bin/bash
# clean previous results
rm -rf coverage-report TestResults

# run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# generate merged HTML report
reportgenerator \
  -reports:"**/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;TextSummary;Badges"

# display summary
cat coverage-report/Summary.txt

```text

### Setting Coverage Thresholds

Enforce minimum coverage in CI by parsing the text summary or using a threshold parameter:

```bash

# ReportGenerator does not enforce thresholds directly.
# Parse the summary or use dotnet-coverage (Microsoft) for threshold enforcement.

# Alternative: use coverlet's built-in threshold via MSBuild
dotnet test /p:CollectCoverage=true \
  /p:Threshold=80 \
  /p:ThresholdType=line \
  /p:ThresholdStat=total

```text

**Note:** The `/p:Threshold` parameter requires the `coverlet.msbuild` package (not `coverlet.collector`). For `coverlet.collector` workflows, enforce thresholds by parsing the ReportGenerator text summary in your CI script.

---

## CRAP Analysis

CRAP (Change Risk Anti-Patterns) scores identify methods that are both complex and poorly tested. A high CRAP score means the method has high cyclomatic complexity and low code coverage -- a risky combination.

### Formula

```text

CRAP(m) = complexity(m)^2 * (1 - coverage(m)/100)^3 + complexity(m)

```text

Where:
- `complexity(m)` = cyclomatic complexity of method m
- `coverage(m)` = code coverage percentage of method m (0-100)

### Interpreting CRAP Scores

| CRAP Score | Risk Level | Action |
|------------|------------|--------|
| < 5 | Low | Method is simple or well-tested |
| 5-15 | Moderate | Review -- may need additional tests |
| 15-30 | High | Prioritize: add tests or reduce complexity |
| > 30 | Critical | Refactor and add tests immediately |

### Generating CRAP Reports

ReportGenerator includes CRAP analysis when using OpenCover format as input:

```bash

# Step 1: Collect coverage in OpenCover format
dotnet test --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Step 2: Generate report with risk hotspot analysis
reportgenerator \
  -reports:"**/TestResults/*/coverage.opencover.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;RiskHotspots"

```xml

The Risk Hotspots report highlights methods sorted by CRAP score, showing:
- Method name and containing class
- Cyclomatic complexity
- Code coverage percentage
- Computed CRAP score

### Using CRAP Scores Effectively

```csharp

// Example: a method with high complexity and low coverage
// Cyclomatic complexity: 12, Coverage: 20%
// CRAP = 12^2 * (1 - 0.20)^3 + 12 = 144 * 0.512 + 12 = 85.7 (Critical)
public decimal CalculateShipping(Order order)
{
    if (order.Items.Count == 0) return 0;

    decimal baseRate = order.DestinationCountry switch
    {
        "US" => 5.99m,
        "CA" => 9.99m,
        "UK" => 12.99m,
        _ => 19.99m
    };

    if (order.Total > 100) baseRate *= 0.5m;
    if (order.IsPriority) baseRate *= 2.0m;
    if (order.Items.Any(i => i.IsFragile)) baseRate += 4.99m;
    if (order.Items.Any(i => i.IsOversized)) baseRate += 14.99m;
    if (order.HasInsurance) baseRate += order.Total * 0.02m;
    if (order.IsExpedited && order.DestinationCountry != "US") baseRate *= 1.5m;

    return Math.Round(baseRate, 2);
}

```text

Address high CRAP scores by:
1. **Adding targeted tests** for uncovered branches to reduce the score via higher coverage
2. **Reducing complexity** by extracting methods (e.g., separate `CalculateBaseRate` and `ApplySurcharges` methods)
3. **Both** -- the most effective approach combines better coverage with simpler methods

---

## Mutation Testing with Stryker.NET

Mutation testing evaluates test suite quality by introducing small changes (mutations) to production code and checking whether tests detect them. If a mutation survives (tests still pass), the test suite has a gap.

### Installation

```bash

# Install as a global tool
dotnet tool install -g dotnet-stryker

# Or as a local tool (recommended for team consistency)
dotnet tool install dotnet-stryker

```text

### Running Stryker.NET

```bash

# From the test project directory
cd tests/MyApp.Tests
dotnet stryker

# Specify the source project explicitly
dotnet stryker --project MyApp.csproj

# Target specific files
dotnet stryker --mutate "src/Services/**/*.cs"

```csharp

### Configuration File

Create `stryker-config.json` in the test project directory:

```json

{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/stryker-config.schema.json",
  "stryker-config": {
    "project": "MyApp.csproj",
    "reporters": ["html", "progress", "cleartext"],
    "mutation-level": "Standard",
    "thresholds": {
      "high": 80,
      "low": 60,
      "break": 50
    },
    "mutate": [
      "src/Services/**/*.cs",
      "!src/Services/Migrations/**/*.cs"
    ],
    "ignore-mutations": [
      "string",
      "linq"
    ]
  }
}

```text

### Understanding Mutation Results

Stryker reports mutations in four categories:

| Status | Meaning | Action |
|--------|---------|--------|
| **Killed** | A test detected the mutation (failed) | Good -- test suite caught the defect |
| **Survived** | No test detected the mutation (all passed) | Gap -- add or strengthen tests |
| **No Coverage** | No test covers the mutated code | Gap -- add tests for this code |
| **Timeout** | Mutation caused an infinite loop or timeout | Usually killed (counts as detected) |

### Mutation Score

```text

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
