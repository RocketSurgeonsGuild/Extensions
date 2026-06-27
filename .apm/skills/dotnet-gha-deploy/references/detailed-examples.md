      - uses: actions/upload-artifact@v4
        with:
          name: app
          path: ./publish

  deploy-dev:
    needs: build
    runs-on: ubuntu-latest
    environment: development
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: app
      - run: echo "Deploy to dev"

  deploy-staging:
    needs: deploy-dev
    runs-on: ubuntu-latest
    environment:
      name: staging
      url: https://staging.example.com
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: app
      - run: echo "Deploy to staging"

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://example.com
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: app
      - run: echo "Deploy to production"

```text

### Protection Rule Configuration

Configure in GitHub Settings > Environments for each environment:

| Environment | Required Reviewers | Wait Timer | Branch Policy       |
| ----------- | ------------------ | ---------- | ------------------- |
| development | None               | None       | Any branch          |
| staging     | 1 reviewer         | None       | `main`, `release/*` |
| production  | 2 reviewers        | 15 minutes | `main` only         |

### Environment-Specific Secrets and Variables

Each environment can override repository-level secrets:

```yaml

jobs:
  deploy:
    environment: production
    runs-on: ubuntu-latest
    steps:
      - name: Deploy with environment-specific config
        env:
          # Resolves to the production environment's secret, not the repo-level one
          DB_CONNECTION: ${{ secrets.DB_CONNECTION_STRING }}
          APP_URL: ${{ vars.APP_URL }}
        run: |
          set -euo pipefail
          echo "Deploying to $APP_URL"

```text

---

## Rollback Patterns

### Revert Deployment

Re-deploy the previous known-good version on failure:

```yaml

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Deploy new version
        id: deploy
        continue-on-error: true
        run: |
          set -euo pipefail
          # Deploy logic here
          ./deploy.sh --version ${{ github.sha }}

      - name: Health check
        id: health
        if: steps.deploy.outcome == 'success'
        continue-on-error: true
        shell: bash
        run: |
          set -euo pipefail
          for i in {1..5}; do
            HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://example.com/healthz)
            if [ "$HTTP_STATUS" = "200" ]; then
              echo "Health check passed"
              exit 0
            fi
            sleep 10
          done
          echo "Health check failed after 5 attempts"
          exit 1

      - name: Rollback on failure
        if: steps.deploy.outcome == 'failure' || steps.health.outcome == 'failure'
        run: |
          set -euo pipefail
          echo "Rolling back to previous version"
          # Re-deploy the last known-good artifact
          ./deploy.sh --version ${{ github.event.before }}

      - name: Fail the job if rollback was needed
        if: steps.deploy.outcome == 'failure' || steps.health.outcome == 'failure'
        run: exit 1

```text

### Azure Deployment Slot Rollback

Swap back to the previous slot on health check failure:

```yaml

- name: Swap to production
  id: swap
  uses: azure/cli@v2
  with:
    inlineScript: |
      az webapp deployment slot swap \
        --resource-group myapp-rg \
        --name myapp-production \
        --slot staging \
        --target-slot production

- name: Post-swap health check
  id: post-health
  continue-on-error: true
  shell: bash
  run: |
    set -euo pipefail
    sleep 30  # allow swap to stabilize
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://myapp.azurewebsites.net/healthz)
    if [ "$HTTP_STATUS" != "200" ]; then
      echo "Post-swap health check failed"
      exit 1
    fi

- name: Rollback swap on failure
  if: steps.post-health.outcome == 'failure'
  uses: azure/cli@v2
  with:
    inlineScript: |
      az webapp deployment slot swap \
        --resource-group myapp-rg \
        --name myapp-production \
        --slot staging \
        --target-slot production
      echo "Rolled back: swapped staging back to production"

```text

### Manual Rollback via workflow_dispatch

Provide a manual trigger for emergency rollbacks:

```yaml

on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Version to roll back to (e.g., v1.2.3)'
        required: true
        type: string
      environment:
        description: 'Target environment'
        required: true
        type: choice
        options:
          - staging
          - production

jobs:
  rollback:
    runs-on: ubuntu-latest
    environment: ${{ inputs.environment }}
    steps:
      - uses: actions/checkout@v4
        with:
          ref: ${{ inputs.version }}

      - name: Publish
        run: |
          set -euo pipefail
          dotnet publish src/MyApp/MyApp.csproj -c Release -o ./publish

      - name: Deploy rollback version
        run: |
          set -euo pipefail
          echo "Rolling back ${{ inputs.environment }} to ${{ inputs.version }}"
          # Platform-specific deployment

```text

---

## Agent Gotchas

1. **Use `set -euo pipefail` in all multi-line bash steps** -- without `pipefail`, failures in piped commands are
   silently swallowed, producing false-green deployments.
2. **Never use `cancel-in-progress: true` for deployment concurrency groups** -- cancelling an in-progress deployment
   can leave infrastructure in a partially deployed state.
3. **Always run health checks after deployment** -- a successful `deploy` step does not guarantee the application is
   running correctly; verify with HTTP health checks.
4. **Use `id-token: write` permission for OIDC Azure login** -- without it, the federated credential exchange fails with
   a cryptic 403 error.
5. **Deployment slot swaps are atomic** -- if the swap fails, both slots retain their original deployments; no partial
   state.
6. **Never hardcode Azure credentials in workflow files** -- use OIDC federated credentials or environment-scoped
   secrets; hardcoded secrets in YAML are visible in repository history.
7. **Use digest-based image references for production deployments** -- tags are mutable and can be overwritten; digests
   are immutable and guarantee the exact image bytes.
8. **Separate build and deploy jobs** -- build artifacts once, deploy to multiple environments from the same artifact to
   ensure consistency.
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
