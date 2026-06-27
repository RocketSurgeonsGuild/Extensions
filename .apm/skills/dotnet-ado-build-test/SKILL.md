---
name: dotnet-ado-build-test
category: operations
subcategory: ci-cd
description: Configures .NET build/test in Azure DevOps. DotNetCoreCLI task, Artifacts, test results.
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

# dotnet-ado-build-test

.NET build and test pipeline patterns for Azure DevOps: `DotNetCoreCLI@2` task for build, test, and pack operations,
NuGet restore with Azure Artifacts feeds using `NuGetAuthenticate@1`, test result publishing with `PublishTestResults@2`
for TRX and JUnit formats, code coverage with `PublishCodeCoverageResults@2` for Cobertura and JaCoCo formats, and
multi-TFM matrix strategy across net8.0 and net9.0.

**Version assumptions:** `DotNetCoreCLI@2` task (current). `UseDotNet@2` for SDK installation. `NuGetAuthenticate@1` for
Azure Artifacts. `PublishTestResults@2` and `PublishCodeCoverageResults@2` for reporting.

## Scope

- DotNetCoreCLI@2 task for build, test, pack, and custom commands
- NuGet restore with Azure Artifacts feeds (NuGetAuthenticate@1)
- Test result publishing with PublishTestResults@2 (TRX, JUnit)
- Code coverage with PublishCodeCoverageResults@2 (Cobertura)
- Multi-TFM matrix strategy across TFMs and operating systems

## Out of scope

- Starter CI templates -- see [skill:dotnet-add-ci]
- Test architecture and strategy -- see [skill:dotnet-testing-strategy]
- Benchmark regression detection in CI -- see [skill:dotnet-ci-benchmarking]
- Publishing and deployment -- see [skill:dotnet-ado-publish] and [skill:dotnet-ado-unique]
- GitHub Actions build/test workflows -- see [skill:dotnet-gha-build-test]

Cross-references: [skill:dotnet-add-ci] for starter build/test templates, [skill:dotnet-testing-strategy] for test
architecture guidance, [skill:dotnet-ci-benchmarking] for benchmark CI integration.

---

## `DotNetCoreCLI@2` Task

### Build

````yaml

steps:
  - task: UseDotNet@2
    displayName: 'Install .NET SDK'
    inputs:
      packageType: 'sdk'
      version: '8.0.x'

  - task: DotNetCoreCLI@2
    displayName: 'Restore'
    inputs:
      command: 'restore'
      projects: 'MyApp.sln'

  - task: DotNetCoreCLI@2
    displayName: 'Build'
    inputs:
      command: 'build'
      projects: 'MyApp.sln'
      arguments: '-c Release --no-restore'

```bash

### Test

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Run tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: >-
      -c Release --logger "trx;LogFileName=test-results.trx" --results-directory
      $(Build.ArtifactStagingDirectory)/test-results

```csharp

### Pack

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Pack NuGet packages'
  inputs:
    command: 'pack'
    packagesToPack: 'src/**/*.csproj'
    configuration: 'Release'
    outputDir: '$(Build.ArtifactStagingDirectory)/nupkgs'
    nobuild: true

```csharp

### Custom Command

For commands not directly supported by the task (e.g., `dotnet tool install`):

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Install dotnet tools'
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'restore'

```bash

### Multi-Version SDK Install

Install multiple SDK versions for multi-TFM builds:

```yaml

- task: UseDotNet@2
  displayName: 'Install .NET 8'
  inputs:
    packageType: 'sdk'
    version: '8.0.x'

- task: UseDotNet@2
  displayName: 'Install .NET 9'
  inputs:
    packageType: 'sdk'
    version: '9.0.x'

```text

Each `UseDotNet@2` invocation adds the SDK version to PATH. The last installed version becomes the default, but all
versions are available via `--framework` targeting.

---

## NuGet Restore with Azure Artifacts Feeds

### `NuGetAuthenticate@1` for Feed Authentication

```yaml

steps:
  - task: NuGetAuthenticate@1
    displayName: 'Authenticate NuGet feeds'

  - task: DotNetCoreCLI@2
    displayName: 'Restore'
    inputs:
      command: 'restore'
      projects: 'MyApp.sln'
      feedsToUse: 'config'
      nugetConfigPath: 'nuget.config'

```bash

The `NuGetAuthenticate@1` task configures credentials for all Azure Artifacts feeds referenced in `nuget.config`. No
explicit PAT or API key is needed -- the task uses the pipeline's identity.

### Selecting Feeds Directly

For simple setups without a `nuget.config`, select feeds directly in the restore task:

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Restore with Azure Artifacts'
  inputs:
    command: 'restore'
    projects: 'MyApp.sln'
    feedsToUse: 'select'
    vstsFeed: 'MyProject/MyFeed'
    includeNuGetOrg: true

```text

### Upstream Sources

Azure Artifacts feeds can proxy nuget.org as an upstream source. When configured, a single feed reference provides
access to both private packages and public NuGet packages:

```xml

<!-- nuget.config with Azure Artifacts upstream -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="MyFeed" value="https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json" />
  </packageSources>
</configuration>

```json

With upstream sources enabled on the feed, nuget.org packages are cached in the Azure Artifacts feed, providing a single
authenticated source for all packages.

### Cross-Organization Feed Access

For feeds in different Azure DevOps organizations, use a service connection:

```yaml

- task: NuGetAuthenticate@1
  displayName: 'Authenticate external feed'
  inputs:
    nuGetServiceConnections: 'ExternalOrgFeedConnection'

- task: DotNetCoreCLI@2
  displayName: 'Restore'
  inputs:
    command: 'restore'
    projects: 'MyApp.sln'
    feedsToUse: 'config'
    nugetConfigPath: 'nuget.config'

```bash

---

## Test Result Publishing

### `PublishTestResults@2` with TRX Format

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Run tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: >-
      -c Release --logger "trx;LogFileName=results.trx" --results-directory $(Common.TestResultsDirectory)
  continueOnError: true

- task: PublishTestResults@2
  displayName: 'Publish test results'
  condition: always()
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '$(Common.TestResultsDirectory)/**/*.trx'
    mergeTestResults: true
    testRunTitle: '.NET Unit Tests'

```text

**Key decisions:**

- `continueOnError: true` on the test task ensures the publish step always runs, even on test failures
- `condition: always()` on the publish task runs regardless of previous step outcome
- `mergeTestResults: true` combines results from multiple test projects into a single test run
- `testRunTitle` provides a descriptive name in the Azure DevOps Test tab

### JUnit Format

Some third-party test frameworks output JUnit XML. Use the `JUnit` format:

```yaml

- task: PublishTestResults@2
  displayName: 'Publish JUnit results'
  condition: always()
  inputs:
    testResultsFormat: 'JUnit'
    testResultsFiles: '**/junit-results.xml'
    mergeTestResults: true

```xml

### Test Results with Attachments

Attach screenshots or logs to test results for debugging failed tests:

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Run tests with attachments'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: >-
      -c Release --logger "trx;LogFileName=results.trx" --results-directory $(Common.TestResultsDirectory)
      --collect:"XPlat Code Coverage"
  continueOnError: true

- task: PublishTestResults@2
  displayName: 'Publish test results'
  condition: always()
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '$(Common.TestResultsDirectory)/**/*.trx'
    mergeTestResults: true
    testRunTitle: '.NET Tests'
    publishRunAttachments: true

```text

---

## Code Coverage

### `PublishCodeCoverageResults@2` with Cobertura

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Test with coverage'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: >-
      -c Release --collect:"XPlat Code Coverage" --results-directory $(Agent.TempDirectory)/coverage

- task: PublishCodeCoverageResults@2
  displayName: 'Publish code coverage'
  inputs:
    summaryFileLocation: '$(Agent.TempDirectory)/coverage/**/coverage.cobertura.xml'

```xml

The `PublishCodeCoverageResults@2` task (v2) auto-generates HTML coverage reports in the Azure DevOps Build Summary tab
without requiring `reportgenerator`.

### Coverage with ReportGenerator for Detailed Reports

For custom coverage reports beyond the built-in rendering:

```yaml

- task: DotNetCoreCLI@2
  displayName: 'Test with coverage'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: >-
      -c Release --collect:"XPlat Code Coverage" --results-directory $(Agent.TempDirectory)/coverage

- script: |
    set -euo pipefail
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator \
      -reports:$(Agent.TempDirectory)/coverage/**/coverage.cobertura.xml \
      -targetdir:$(Build.ArtifactStagingDirectory)/coverage-report \
      -reporttypes:HtmlInline_AzurePipelines\;Cobertura
  displayName: 'Generate coverage report'

- task: PublishCodeCoverageResults@2
  displayName: 'Publish coverage'
  inputs:
    summaryFileLocation: '$(Build.ArtifactStagingDirectory)/coverage-report/Cobertura.xml'

- task: PublishPipelineArtifact@1
  displayName: 'Upload coverage report'
  inputs:
    targetPath: '$(Build.ArtifactStagingDirectory)/coverage-report'
    artifactName: 'coverage-report'

```text

### Coverage Thresholds

Enforce minimum coverage by parsing the Cobertura XML in a script step:

```yaml

- script: |
    set -euo pipefail
    COVERAGE_FILE=$(find $(Agent.TempDirectory)/coverage -name 'coverage.cobertura.xml' | head -1)
    COVERAGE=$(python3 -c "
    import xml.etree.ElementTree as ET
    tree = ET.parse('$COVERAGE_FILE')
    print(float(tree.getroot().attrib['line-rate']) * 100)
    ")
    echo "Line coverage: ${COVERAGE}%"
    if (( $(echo "$COVERAGE < 80" | bc -l) )); then
      echo "##vso[task.logissue type=error]Coverage ${COVERAGE}% is below 80% threshold"
      exit 1
    fi
  displayName: 'Enforce coverage threshold'

```text

---

## Multi-TFM Matrix Strategy

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
