              steps:
                - download: current
                  artifact: app
                - script: echo "Deploying from $(Pipeline.Workspace)/app"

```text

The `download: current` keyword downloads artifacts from the current pipeline run. Use `download: pipelineName` for
artifacts from a different pipeline.

---

## Pipeline Artifacts for Release Pipelines

### Multi-Stage Release with Artifact Promotion

```yaml

trigger:
  tags:
    include:
      - 'v*'

stages:
  - stage: Build
    jobs:
      - job: BuildAndPack
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: '8.0.x'

          - task: DotNetCoreCLI@2
            displayName: 'Build'
            inputs:
              command: 'build'
              projects: 'MyApp.sln'
              arguments: '-c Release'

          - task: DotNetCoreCLI@2
            displayName: 'Publish'
            inputs:
              command: 'publish'
              projects: 'src/MyApp/MyApp.csproj'
              arguments: '-c Release -o $(Build.ArtifactStagingDirectory)/app'

          - task: DotNetCoreCLI@2
            displayName: 'Pack'
            inputs:
              command: 'pack'
              packagesToPack: 'src/MyLibrary/MyLibrary.csproj'
              configuration: 'Release'
              outputDir: '$(Build.ArtifactStagingDirectory)/nupkgs'

          - task: PublishPipelineArtifact@1
            displayName: 'Upload app'
            inputs:
              targetPath: '$(Build.ArtifactStagingDirectory)/app'
              artifactName: 'app'

          - task: PublishPipelineArtifact@1
            displayName: 'Upload packages'
            inputs:
              targetPath: '$(Build.ArtifactStagingDirectory)/nupkgs'
              artifactName: 'nupkgs'

  - stage: DeployStaging
    dependsOn: Build
    jobs:
      - deployment: DeployStaging
        environment: 'staging'
        pool:
          vmImage: 'ubuntu-latest'
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploying to staging from $(Pipeline.Workspace)/app"

  - stage: PublishPackages
    dependsOn: DeployStaging
    jobs:
      - job: PushPackages
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - download: current
            artifact: nupkgs

          - task: NuGetAuthenticate@1

          - task: NuGetCommand@2
            displayName: 'Push to nuget.org'
            inputs:
              command: 'push'
              packagesToPush: '$(Pipeline.Workspace)/nupkgs/*.nupkg'
              nuGetFeedType: 'external'
              publishFeedCredentials: 'NuGetOrgServiceConnection'

  - stage: DeployProduction
    dependsOn: DeployStaging
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployProduction
        environment: 'production'
        pool:
          vmImage: 'ubuntu-latest'
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploying to production from $(Pipeline.Workspace)/app"

```text

### Cross-Pipeline Artifact Consumption

Consume artifacts from a different pipeline (e.g., a shared build pipeline):

```yaml

resources:
  pipelines:
    - pipeline: buildPipeline
      source: 'MyApp-Build'
      trigger:
        branches:
          include:
            - main

stages:
  - stage: Deploy
    jobs:
      - deployment: DeployFromBuild
        environment: 'staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - download: buildPipeline
                  artifact: app
                - script: echo "Deploying from $(Pipeline.Workspace)/buildPipeline/app"

```text

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

1. **Use `PublishPipelineArtifact@1` over `PublishBuildArtifacts@1`** -- pipeline artifacts are faster, support
   deduplication, and work with multi-stage YAML pipelines; build artifacts are legacy and required only for classic
   release pipelines.
2. **Azure Artifacts returns 409 for duplicate package versions** -- use `continueOnError: true` for idempotent reruns,
   or handle duplicates in feed settings by allowing pre-release version overwrites.
3. **`NuGetCommand@2` with `external` feed type requires a service connection** -- do not hardcode API keys in pipeline
   YAML; create a NuGet service connection in Project Settings that stores the key securely.
4. **SDK container publish requires Docker on the agent** -- `dotnet publish` with `PublishProfile=DefaultContainer`
   needs Docker; hosted `ubuntu-latest` agents include Docker, but self-hosted agents may not.
5. **AOT publish requires matching RID** -- `dotnet publish -r linux-x64` must match the agent OS; do not use
   `-r win-x64` on a Linux agent.
6. **`download: current` uses `$(Pipeline.Workspace)` not `$(Build.ArtifactStagingDirectory)`** -- artifacts downloaded
   in deployment jobs are at `$(Pipeline.Workspace)/artifactName`, not the staging directory.
7. **Never hardcode registry credentials in pipeline YAML** -- use Docker service connections for ACR/DockerHub
   authentication; service connections store credentials securely and rotate independently.
8. **Tag triggers require explicit `tags.include` in the trigger section** -- tags are not included by default CI
   triggers; add `tags: include: ['v*']` to trigger on version tags.
````
