    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployProduction
        environment: 'production' # approvals + business hours in UI
        strategy:
          runOnce:
            deploy:
              steps:
                - download: current
                  artifact: app
                - script: echo "Deploy to production"

```text

### Migration Checklist

1. **Identify all classic release stages** and map to YAML stages
2. **Convert environment variables** to YAML variable groups or templates
3. **Replace classic approval gates** with environment checks
4. **Convert artifact sources** to `download: current` or pipeline resources
5. **Replace task groups** with YAML step or job templates
6. **Test the YAML pipeline** on a non-production branch before decommissioning the classic release

---

## Variable Groups and Library

### Variable Groups Linked to Azure Key Vault

Variable groups can pull secrets directly from Azure Key Vault at pipeline runtime:

```yaml

variables:
  - group: 'kv-production-secrets'
  - group: 'build-settings'
  - name: buildConfiguration
    value: 'Release'

steps:
  - script: |
      echo "Building with configuration $(buildConfiguration)"
    displayName: 'Build'
    env:
      SQL_CONNECTION: $(sql-connection-string) # from Key Vault
      API_KEY: $(api-key) # from Key Vault

```text

**Setting up Key Vault-linked variable groups:**

1. Navigate to Pipelines > Library > Variable Groups > New variable group
2. Enable "Link secrets from an Azure key vault as variables"
3. Select the Azure subscription (service connection) and Key Vault
4. Choose which secrets to include
5. Secrets are fetched at pipeline runtime and available as `$(secret-name)`

### Scoping Variable Groups to Environments

Use conditional variable group references based on pipeline stage:

```yaml

stages:
  - stage: DeployStaging
    variables:
      - group: 'staging-config'
      - group: 'kv-staging-secrets'
    jobs:
      - deployment: Deploy
        environment: 'staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: echo "Deploying with staging config"
                  env:
                    CONNECTION_STRING: $(sql-connection-string)

  - stage: DeployProduction
    variables:
      - group: 'production-config'
      - group: 'kv-production-secrets'
    jobs:
      - deployment: Deploy
        environment: 'production'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: echo "Deploying with production config"
                  env:
                    CONNECTION_STRING: $(sql-connection-string)

```text

### Secure Files in Library

Store certificates, SSH keys, and other binary secrets in the Pipelines Library:

```yaml

- task: DownloadSecureFile@1
  displayName: 'Download signing certificate'
  name: signingCert
  inputs:
    secureFile: 'code-signing.pfx'

- script: |
    dotnet nuget sign ./nupkgs/*.nupkg \
      --certificate-path $(signingCert.secureFilePath) \
      --certificate-password $(CERT_PASSWORD) \
      --timestamper http://timestamp.digicert.com
  displayName: 'Sign NuGet packages'

```text

---

## Pipeline Decorators

Pipeline decorators inject steps into every pipeline in an organization or project without modifying individual pipeline
files. They enforce organizational policies:

### Decorator Use Cases

| Use Case                    | Implementation                                  |
| --------------------------- | ----------------------------------------------- |
| Mandatory security scanning | Inject credential scanner before every job      |
| Compliance audit logging    | Inject telemetry step after every job           |
| Required code analysis      | Inject SonarQube analysis on main branch builds |
| License compliance          | Inject dependency license scanner               |

### Decorator Definition

Decorators are packaged as Azure DevOps extensions:

```yaml

# vss-extension.json (extension manifest)
{
  'contributions':
    [
      {
        'id': 'required-security-scan',
        'type': 'ms.azure-pipelines.pipeline-decorator',
        'targets': ['ms.azure-pipelines-agent-job'],
        'properties': { 'template': 'decorator.yml', 'targetsExecutionOrder': 'PreJob' },
      },
    ],
}

```yaml

```yaml

# decorator.yml
steps:
  - task: CredentialScanner@1
    displayName: '[Policy] Credential scan'
    condition: always()

```text

### Deployment Limitations

- Decorators require Azure DevOps organization admin permissions to install
- They apply to all pipelines in the organization (or selected projects)
- Pipeline authors cannot override or skip decorator steps
- Decorator steps run under the pipeline's agent pool and service connection context

---

## Azure Artifacts Universal Packages

Universal packages store arbitrary files (binaries, tools, datasets) in Azure Artifacts feeds, not limited to
NuGet/npm/Maven formats:

### Publish a Universal Package

```yaml

- task: UniversalPackages@0
  displayName: 'Publish universal package'
  inputs:
    command: 'publish'
    publishDirectory: '$(Build.ArtifactStagingDirectory)/tools'
    feedsToUsePublish: 'internal'
    vstsFeedPublish: 'MyProject/MyFeed'
    vstsFeedPackagePublish: 'my-dotnet-tool'
    versionOption: 'custom'
    versionPublish: '$(Build.BuildNumber)'
    packagePublishDescription: '.NET CLI tool binaries'

```text

### Download a Universal Package

```yaml

- task: UniversalPackages@0
  displayName: 'Download universal package'
  inputs:
    command: 'download'
    feedsToUse: 'internal'
    vstsFeed: 'MyProject/MyFeed'
    vstsFeedPackage: 'my-dotnet-tool'
    vstsPackageVersion: '*'
    downloadDirectory: '$(Pipeline.Workspace)/tools'

```text

### Use Cases for .NET Projects

- **CLI tool distribution:** Publish self-contained .NET CLI tool binaries for cross-team consumption
- **Build tool caching:** Store custom MSBuild tasks or analyzers used across repositories
- **Test fixture data:** Publish large test datasets that should not be stored in Git
- **AOT binaries:** Distribute pre-built Native AOT binaries for platforms where on-demand compilation is impractical

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

1. **Environment checks (approvals, gates) are configured in the UI, not YAML** -- the YAML pipeline references the
   environment name; all checks are managed through the Azure DevOps web UI.
2. **Deployment groups are legacy** -- use environments for all new projects; deployment groups exist only for backward
   compatibility with classic release pipelines.
3. **Service connection scope matters** -- ARM connections scoped to a resource group cannot deploy to resources outside
   that group; use subscription-level scope for cross-resource-group deployments.
4. **Workload identity federation is preferred over service principal secrets** -- federated credentials eliminate
   secret rotation; use automatic federation for new connections.
5. **Key Vault-linked variable groups fetch secrets at runtime** -- template expressions (`${{ }}`) cannot access Key
   Vault secrets because they resolve at compile time; use runtime expressions (`$()`) instead.
6. **Classic release pipelines are not stored in source control** -- this is a primary motivation for migration; YAML
   pipelines enable PR review and branch-specific definitions.
7. **Pipeline decorators cannot be bypassed by pipeline authors** -- this is intentional for policy enforcement; test
   decorator changes in a separate organization or project to avoid breaking all pipelines.
8. **Universal packages have a 4 GiB size limit per file** -- for larger artifacts, split files or use Azure Blob
   Storage with a SAS token instead.
````
