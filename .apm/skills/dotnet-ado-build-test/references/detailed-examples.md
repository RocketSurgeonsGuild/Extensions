## Multi-TFM Matrix Strategy

### Matrix Build Across TFMs and Operating Systems

```yaml

jobs:
  - job: Test
    strategy:
      matrix:
        Linux_net80:
          vmImage: 'ubuntu-latest'
          tfm: 'net8.0'
          dotnetVersion: '8.0.x'
        Linux_net90:
          vmImage: 'ubuntu-latest'
          tfm: 'net9.0'
          dotnetVersion: '9.0.x'
        Windows_net80:
          vmImage: 'windows-latest'
          tfm: 'net8.0'
          dotnetVersion: '8.0.x'
        Windows_net90:
          vmImage: 'windows-latest'
          tfm: 'net9.0'
          dotnetVersion: '9.0.x'
    pool:
      vmImage: $(vmImage)
    steps:
      - task: UseDotNet@2
        displayName: 'Install .NET $(dotnetVersion)'
        inputs:
          packageType: 'sdk'
          version: $(dotnetVersion)

      - task: DotNetCoreCLI@2
        displayName: 'Test $(tfm) on $(vmImage)'
        inputs:
          command: 'test'
          projects: '**/*Tests.csproj'
          arguments: >-
            -c Release --framework $(tfm) --logger "trx;LogFileName=$(tfm)-results.trx" --results-directory
            $(Common.TestResultsDirectory)
        continueOnError: true

      - task: PublishTestResults@2
        displayName: 'Publish $(tfm) results'
        condition: always()
        inputs:
          testResultsFormat: 'VSTest'
          testResultsFiles: '$(Common.TestResultsDirectory)/**/*.trx'
          testRunTitle: '$(tfm) on $(vmImage)'

```text

### Installing Multiple SDKs for Multi-TFM in a Single Job

When running all TFMs in one job (instead of matrix), install all required SDKs:

```yaml

steps:
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

  - task: DotNetCoreCLI@2
    displayName: 'Test all TFMs'
    inputs:
      command: 'test'
      projects: '**/*Tests.csproj'
      arguments: '-c Release'

```bash

Without the matching SDK installed, `dotnet test` cannot build for that TFM and fails with `NETSDK1045`.

### Template-Based Matrix for Reusability

```yaml

# templates/jobs/matrix-test.yml
parameters:
  - name: configurations
    type: object
    default:
      - tfm: 'net8.0'
        dotnetVersion: '8.0.x'
      - tfm: 'net9.0'
        dotnetVersion: '9.0.x'

jobs:
  - ${{ each config in parameters.configurations }}:
      - job: Test_${{ replace(config.tfm, '.', '_') }}
        displayName: 'Test ${{ config.tfm }}'
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: ${{ config.dotnetVersion }}

          - task: DotNetCoreCLI@2
            displayName: 'Test ${{ config.tfm }}'
            inputs:
              command: 'test'
              projects: '**/*Tests.csproj'
              arguments: '-c Release --framework ${{ config.tfm }}'

```bash

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
## Agent Gotchas

1. **Use `set -euo pipefail` in multi-line `script:` steps** -- ADO `script:` tasks on Linux default to `set -e` but do
   not set `pipefail` or `nounset`; without `pipefail`, a failure in a piped command is silently swallowed.
2. **Use `continueOnError: true` on the test task, not on the result publisher** -- the test task must not fail the
   pipeline before results are published, but the publisher should reflect the actual test outcome.
3. **Install all required SDK versions for multi-TFM builds** -- `dotnet test` without the matching SDK produces
   `NETSDK1045`; add a `UseDotNet@2` step for each required version.
4. **`NuGetAuthenticate@1` must precede the restore step** -- authentication tokens are injected into the agent's NuGet
   config at task execution time; restoring before authentication fails with 401.
5. **Use `feedsToUse: 'config'` with `nuget.config` for complex feed setups** -- `feedsToUse: 'select'` supports only
   one Azure Artifacts feed; multi-feed scenarios require a `nuget.config` file.
6. **Coverage collection requires `--collect:"XPlat Code Coverage"`** -- the default `dotnet test` does not produce
   coverage files; the `XPlat Code Coverage` collector is built into the .NET SDK.
7. **`PublishCodeCoverageResults@2` expects Cobertura XML** -- passing TRX or other formats to the coverage publisher
   produces no output; ensure the collector outputs Cobertura format.
8. **ADO matrix syntax differs from GHA** -- ADO uses named matrix entries with key-value pairs, not arrays; each entry
   must define all variable names used in the job.
9. **Never hardcode credentials in pipeline YAML** -- use variable groups linked to Azure Key Vault or pipeline-level
   secret variables; hardcoded secrets are visible in repository history.
````
