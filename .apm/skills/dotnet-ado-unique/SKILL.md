---
name: dotnet-ado-unique
category: operations
subcategory: ci-cd
description: Configures ADO-exclusive features. Environments, approvals, service connections, pipelines.
license: MIT
targets: ['*']
tags: [cicd, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for cicd tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-ado-unique

Azure DevOps-exclusive features not available in GitHub Actions: Environments with approvals and gates (pre-deployment
checks, business hours restrictions), deployment groups vs environments (when to use each), service connections (Azure
Resource Manager, Docker Registry, NuGet), classic release pipelines (legacy migration guidance to YAML), variable
groups and library (linked to Azure Key Vault), pipeline decorators for organization-wide policy, and Azure Artifacts
universal packages.

**Version assumptions:** Azure DevOps Services (cloud). YAML pipelines with multi-stage support. Classic release
pipelines for legacy migration context only.

## Scope

- Environments with approvals and gates (pre-deployment checks)
- Service connections (Azure Resource Manager, Docker Registry, NuGet)
- Classic release pipelines (legacy migration guidance to YAML)
- Variable groups and library linked to Azure Key Vault
- Pipeline decorators for organization-wide policy
- Azure Artifacts universal packages

## Out of scope

- Composable pipeline patterns (templates, triggers) -- see [skill:dotnet-ado-patterns]
- Build/test pipeline configuration -- see [skill:dotnet-ado-build-test]
- Publishing pipelines -- see [skill:dotnet-ado-publish]
- Starter CI templates -- see [skill:dotnet-add-ci]
- GitHub Actions equivalents -- see [skill:dotnet-gha-patterns], [skill:dotnet-gha-build-test],
  [skill:dotnet-gha-publish], [skill:dotnet-gha-deploy]
- CLI release pipelines -- see [skill:dotnet-cli-release-pipeline]

Cross-references: [skill:dotnet-add-ci] for starter CI templates, [skill:dotnet-cli-release-pipeline] for CLI-specific
release automation.

---

## Environments with Approvals and Gates

### Defining Environments in YAML

Environments are first-class Azure DevOps resources that provide deployment targeting, approval gates, and deployment
history:

````yaml

stages:
  - stage: DeployStaging
    jobs:
      - deployment: DeployToStaging
        pool:
          vmImage: 'ubuntu-latest'
        environment: 'staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploying to staging"

  - stage: DeployProduction
    dependsOn: DeployStaging
    jobs:
      - deployment: DeployToProduction
        pool:
          vmImage: 'ubuntu-latest'
        environment: 'production'
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploying to production"

```text

Environments are created automatically on first reference. Configure approvals and gates in Azure DevOps > Pipelines >
Environments > (select environment) > Approvals and checks.

### Approval Checks

| Check Type            | Purpose                                        | Configuration                     |
| --------------------- | ---------------------------------------------- | --------------------------------- |
| Approvals             | Manual sign-off before deployment              | Assign approver users/groups      |
| Branch control        | Restrict deployments to specific branches      | Allow only `main`, `release/*`    |
| Business hours        | Deploy only during allowed time windows        | Define hours and timezone         |
| Template validation   | Require pipeline to extend a specific template | Specify required template path    |
| Invoke Azure Function | Custom validation via Azure Function           | Provide function URL and key      |
| Invoke REST API       | Custom validation via HTTP endpoint            | Provide URL and success criteria  |
| Required template     | Enforce pipeline structure                     | Specify required extends template |

### Configuring Approval Checks

Approval checks are configured in the Azure DevOps UI, not in YAML. The YAML pipeline references the environment, and
the checks are applied:

```yaml

# Pipeline YAML -- environment reference triggers checks
- deployment: DeployToProduction
  environment: 'production' # checks configured in UI
  strategy:
    runOnce:
      deploy:
        steps:
          - script: echo "This runs only after all checks pass"

```text

**Approval configuration (UI):**

- Navigate to Pipelines > Environments > production > Approvals and checks
- Add "Approvals" check: assign individuals or groups
- Set minimum number of approvers (e.g., 2 for production)
- Enable "allow approvers to approve their own runs" only if appropriate

### Business Hours Gate

Restrict deployments to specific time windows to reduce risk:

- Navigate to Pipelines > Environments > production > Approvals and checks
- Add "Business Hours" check
- Configure: Monday-Friday, 09:00-17:00 (team timezone)
- Pipelines will queue and wait until the window opens

### Pre-Deployment Validation with Azure Functions

```yaml

# The environment's "Invoke Azure Function" check calls:
# https://myvalidation.azurewebsites.net/api/pre-deploy
# with the pipeline context as payload.
# Returns 200 to approve, non-200 to reject.

- deployment: DeployToProduction
  environment: 'production' # Azure Function check configured in UI
  strategy:
    runOnce:
      preDeploy:
        steps:
          - script: echo "Pre-deploy hook (in-pipeline)"
      deploy:
        steps:
          - script: echo "Deploying"
      routeTraffic:
        steps:
          - script: echo "Routing traffic"
      postRouteTraffic:
        steps:
          - script: echo "Post-route validation"

```text

The `preDeploy`, `routeTraffic`, and `postRouteTraffic` lifecycle hooks execute within the pipeline. Environment checks
(approvals, Azure Function gates) execute before the deployment job starts.

---

## Deployment Groups vs Environments

### When to Use Each

| Feature            | Deployment Groups                     | Environments                                 |
| ------------------ | ------------------------------------- | -------------------------------------------- |
| Target             | Physical/virtual machines with agents | Any target (VMs, Kubernetes, cloud services) |
| Agent model        | Self-hosted agents on target machines | Pool agents or target-specific resources     |
| Pipeline type      | Classic release pipelines (legacy)    | YAML multi-stage pipelines (modern)          |
| Approvals          | Per-stage in classic UI               | Checks and approvals on environment          |
| Rolling deployment | Built-in rolling strategy             | `strategy: rolling` in YAML                  |
| Recommendation     | Legacy workloads only                 | All new projects                             |

### Deployment Group Example (Legacy)

Deployment groups install an agent on each target machine. Use only for existing on-premises deployments:

```yaml

# Classic release pipeline (not YAML) -- for reference only
# Deployment groups are configured in Project Settings > Deployment Groups
# Each target server runs the ADO agent registered to the group

```yaml

### Environment with Kubernetes Resource

```yaml

- deployment: DeployToK8s
  environment: 'production.my-k8s-namespace'
  strategy:
    runOnce:
      deploy:
        steps:
          - task: KubernetesManifest@1
            inputs:
              action: 'deploy'
              manifests: 'k8s/*.yml'
              containers: '$(ACR_LOGIN_SERVER)/myapp:$(Build.BuildId)'

```yaml

Environments can target Kubernetes clusters and namespaces. Register the cluster as a resource under the environment in
the Azure DevOps UI.

### Migration from Deployment Groups to Environments

1. Create environments matching existing deployment group names
2. Configure the same approval gates in the environment's Approvals and checks
3. Convert classic release pipeline stages to YAML `deployment` jobs targeting the new environments
4. Use `strategy: rolling` for incremental deployments equivalent to deployment group behavior

---

## Service Connections

### Azure Resource Manager (ARM)

Service connections provide authenticated access to external services. ARM connections enable Azure resource
deployments:

```yaml

- task: AzureWebApp@1
  displayName: 'Deploy to Azure App Service'
  inputs:
    azureSubscription: 'MyAzureServiceConnection'
    appType: 'webAppLinux'
    appName: 'myapp-staging'
    package: '$(Pipeline.Workspace)/app'

```text

**Creating an ARM service connection:**

- Navigate to Project Settings > Service Connections > New service connection > Azure Resource Manager
- Choose "Service principal (automatic)" for automatic credential management
- Select the subscription and resource group scope
- ADO creates an app registration and assigns Contributor role

### Workload Identity Federation (Recommended)

Use workload identity federation for passwordless Azure authentication (no client secret):

- Navigate to Project Settings > Service Connections > New service connection > Azure Resource Manager
- Choose "Workload Identity federation (automatic)"
- This creates a federated credential that trusts Azure DevOps pipeline tokens
- No secret rotation required -- the credential uses short-lived pipeline tokens

### Docker Registry Service Connection

```yaml

- task: Docker@2
  displayName: 'Login to ACR'
  inputs:
    command: 'login'
    containerRegistry: 'MyACRServiceConnection'

- task: Docker@2
  displayName: 'Build and push'
  inputs:
    command: 'buildAndPush'
    containerRegistry: 'MyACRServiceConnection'
    repository: 'myapp'
    dockerfile: 'src/MyApp/Dockerfile'

```bash

**Creating a Docker registry connection:**

- Project Settings > Service Connections > New service connection > Docker Registry
- For ACR: select "Azure Container Registry" and choose the registry
- For DockerHub: provide username and access token

### NuGet Service Connection

For pushing to external NuGet feeds (e.g., nuget.org):

```yaml

- task: NuGetCommand@2
  displayName: 'Push to nuget.org'
  inputs:
    command: 'push'
    packagesToPush: '$(Pipeline.Workspace)/nupkgs/*.nupkg'
    nuGetFeedType: 'external'
    publishFeedCredentials: 'NuGetOrgServiceConnection'

```bash

**Creating a NuGet connection:**

- Project Settings > Service Connections > New service connection > NuGet
- Provide the feed URL (`https://api.nuget.org/v3/index.json`) and API key

---

## Classic Release Pipelines (Legacy Migration)

### Why Migrate to YAML

Classic release pipelines use a visual designer and are not stored in source control. Migrate to YAML multi-stage
pipelines for:

- **Source control:** Pipeline definitions live alongside code
- **Code review:** Pipeline changes go through PR review
- **Branch-specific pipelines:** YAML pipelines can vary by branch
- **Reusability:** Templates and extends for composable pipelines
- **Modern features:** Environments, deployment strategies, pipeline decorators

### Migration Pattern

**Classic release structure:**

```text

Build Pipeline -> Release Pipeline
                    Stage 1: Dev (auto-deploy)
                    Stage 2: Staging (manual approval)
                    Stage 3: Production (scheduled + approval)

```text

**Equivalent YAML multi-stage pipeline:**

```yaml

trigger:
  branches:
    include:
      - main

stages:
  - stage: Build
    jobs:
      - job: BuildJob
        pool:
          vmImage: 'ubuntu-latest'
        steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: 'publish'
              projects: 'src/MyApp/MyApp.csproj'
              arguments: '-c Release -o $(Build.ArtifactStagingDirectory)/app'
          - task: PublishPipelineArtifact@1
            inputs:
              targetPath: '$(Build.ArtifactStagingDirectory)/app'
              artifactName: 'app'

  - stage: DeployDev
    dependsOn: Build
    jobs:
      - deployment: DeployDev
        environment: 'development'
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploy to dev"

  - stage: DeployStaging
    dependsOn: DeployDev
    jobs:
      - deployment: DeployStaging
        environment: 'staging' # approvals configured in UI
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploy to staging"

  - stage: DeployProduction
    dependsOn: DeployStaging
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
