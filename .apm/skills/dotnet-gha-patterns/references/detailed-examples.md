```yaml

concurrency:
  group: deploy-production
  cancel-in-progress: false # queue, do not cancel deployments

```yaml

Use `cancel-in-progress: true` for build/test (newer commit supersedes older), but `cancel-in-progress: false` for
deployments (do not cancel an in-progress deploy).

---

## Environment Protection Rules

### Configuring Environments

```yaml

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    environment:
      name: staging
      url: https://staging.example.com
    steps:
      - name: Deploy to staging
        run: echo "Deploying..."

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://example.com
    steps:
      - name: Deploy to production
        run: echo "Deploying..."

```text

Configure protection rules in GitHub Settings > Environments:

| Rule                               | Purpose                                        |
| ---------------------------------- | ---------------------------------------------- |
| Required reviewers                 | Manual approval before deployment              |
| Wait timer                         | Cooldown period (e.g., 15 minutes)             |
| Branch restrictions                | Only `main` or `release/*` branches can deploy |
| Custom deployment protection rules | Third-party integrations (monitoring checks)   |

### Environment Secrets

Environments can have their own secrets that override repository-level secrets. Use environment-scoped secrets for
deployment credentials:

```yaml

jobs:
  deploy:
    environment: production
    runs-on: ubuntu-latest
    steps:
      - name: Deploy
        env:
          # These resolve to environment-specific values
          CONNECTION_STRING: ${{ secrets.CONNECTION_STRING }}
          API_KEY: ${{ secrets.API_KEY }}
        run: ./deploy.sh

```text

---

## Caching Strategies

### NuGet Package Cache

```yaml

- name: Cache NuGet packages
  uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props') }}
    restore-keys: |
      nuget-${{ runner.os }}-

```csharp

The `restore-keys` prefix match ensures a partial cache hit when csproj files change (most packages remain cached).

### .NET SDK Cache

For self-hosted runners or scenarios where SDK installation is slow:

```yaml

- name: Setup .NET with cache
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'
    cache: true
    cache-dependency-path: '**/packages.lock.json'

```json

The `cache: true` option in `actions/setup-dotnet@v4` enables built-in NuGet caching using `packages.lock.json` as the
cache key.

### Build Output Cache (.NET 9+)

.NET 9 introduced MSBuild build-check caching. For incremental CI builds:

```yaml

- name: Cache build output
  uses: actions/cache@v4
  with:
    path: |
      **/bin/
      **/obj/
    key: build-${{ runner.os }}-${{ hashFiles('**/*.csproj', '**/*.cs') }}
    restore-keys: |
      build-${{ runner.os }}-

```csharp

Use build output caching cautiously -- stale caches can mask build errors. Prefer NuGet caching as the primary CI speed
optimization.

---

## `workflow_dispatch` Inputs

### Manual Trigger with Parameters

```yaml

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target deployment environment'
        required: true
        type: choice
        options:
          - staging
          - production
        default: staging
      version:
        description: 'Version to deploy (e.g., 1.2.3)'
        required: true
        type: string
      dry-run:
        description: 'Simulate deployment without applying changes'
        required: false
        type: boolean
        default: false

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: ${{ inputs.environment }}
    steps:
      - uses: actions/checkout@v4
        with:
          ref: v${{ inputs.version }}

      - name: Deploy
        env:
          DRY_RUN: ${{ inputs.dry-run }}
        run: |
          set -euo pipefail
          if [ "$DRY_RUN" = "true" ]; then
            echo "DRY RUN: would deploy v${{ inputs.version }} to ${{ inputs.environment }}"
          else
            ./deploy.sh --version ${{ inputs.version }}
          fi

```text

Input types: `string`, `boolean`, `choice`, `environment` (selects from configured environments).

---

## Agent Gotchas

1. **Do not mix `paths` and `paths-ignore` on the same event** -- when both are specified, `paths-ignore` is silently
   ignored. Use one or the other.
2. **Set `fail-fast: false` on matrix builds** -- default `fail-fast: true` cancels sibling jobs when one fails, hiding
   which other combinations also break.
3. **Use `set -euo pipefail` in all bash steps** -- without `pipefail`, a non-zero exit from a piped command (e.g.,
   `script | tee`) does not fail the step.
4. **Reusable workflow inputs are strings by default** -- boolean and number types must be explicitly declared with
   `type:` in the workflow_call inputs.
5. **Cache keys must include `runner.os`** -- NuGet packages are OS-dependent; a Linux-built cache restoring on Windows
   causes restore failures.
6. **Do not hardcode TFMs in workflow files** -- use matrix variables or extract from csproj to keep workflows in sync
   with project configuration.
7. **`secrets: inherit` passes all caller secrets** -- use explicit secret declarations for security-sensitive reusable
   workflows to limit exposure.
8. **Concurrency groups for deploys must use `cancel-in-progress: false`** -- cancelling an in-progress deployment can
   leave infrastructure in an inconsistent state.
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
