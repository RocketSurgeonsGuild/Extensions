---
name: dotnet-gha-deploy
category: operations
subcategory: ci-cd
description: Deploys .NET from GitHub Actions. Azure Web Apps, GitHub Pages, container registries.
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

# dotnet-gha-deploy

Deployment patterns for .NET applications in GitHub Actions: GitHub Pages deployment for documentation sites
(Starlight/Docusaurus), container registry push patterns for GHCR and ACR, Azure Web Apps deployment via
`azure/webapps-deploy`, GitHub Environments with protection rules for staged rollouts, and rollback strategies for
failed deployments.

**Version assumptions:** GitHub Actions workflow syntax v2. `azure/webapps-deploy@v3` for Azure App Service.
`azure/login@v2` for Azure credential management. GitHub Environments for deployment gates.

## Scope

- Azure Web Apps deployment via azure/webapps-deploy
- GitHub Pages deployment for documentation sites
- Container registry push patterns for GHCR and ACR
- GitHub Environments with protection rules
- Rollback strategies for failed deployments

## Out of scope

- Container orchestration (Kubernetes, Docker Compose) -- see [skill:dotnet-container-deployment]
- Container image authoring -- see [skill:dotnet-containers]
- NuGet publishing and container builds -- see [skill:dotnet-gha-publish]
- Starter CI templates -- see [skill:dotnet-add-ci]
- Azure DevOps deployment -- see [skill:dotnet-ado-unique] and [skill:dotnet-ado-publish]
- CLI release pipelines -- see [skill:dotnet-cli-release-pipeline]

Cross-references: [skill:dotnet-container-deployment] for container orchestration patterns, [skill:dotnet-containers]
for container image authoring, [skill:dotnet-add-ci] for starter CI templates, [skill:dotnet-cli-release-pipeline] for
CLI-specific release automation.

---

## GitHub Pages Deployment for Documentation

### Static Site Deployment (Starlight/Docusaurus)

Deploy a .NET project's documentation site to GitHub Pages:

````yaml

name: Deploy Docs

on:
  push:
    branches: [main]
    paths:
      - 'docs/**'
      - '.github/workflows/deploy-docs.yml'
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: false

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: npm
          cache-dependency-path: docs/package-lock.json

      - name: Install dependencies
        working-directory: docs
        run: npm ci

      - name: Build documentation site
        working-directory: docs
        run: npm run build

      - name: Upload Pages artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: docs/dist

  deploy:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4

```text

**Key decisions:**

- `concurrency.cancel-in-progress: false` prevents cancelling an in-progress Pages deployment
- `id-token: write` permission is required for the Pages deployment token
- Separate `build` and `deploy` jobs allow the deploy job to use the `github-pages` environment with protection rules

### API Documentation from XML Comments

Generate and deploy API reference documentation from .NET XML comments:

```yaml

- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'

- name: Build with XML docs
  run: |
    set -euo pipefail
    dotnet build src/MyLibrary/MyLibrary.csproj \
      -c Release \
      -p:GenerateDocumentationFile=true

- name: Generate API docs with docfx
  run: |
    set -euo pipefail
    dotnet tool install -g docfx
    docfx docs/docfx.json

- name: Upload Pages artifact
  uses: actions/upload-pages-artifact@v3
  with:
    path: docs/_site

```text

---

## Container Registry Push Patterns

### Push to GHCR with Environment Gates

```yaml

jobs:
  build:
    runs-on: ubuntu-latest
    outputs:
      image-digest: ${{ steps.build.outputs.digest }}
    steps:
      - uses: actions/checkout@v4

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push
        id: build
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: ghcr.io/${{ github.repository }}:${{ github.sha }}

  deploy-staging:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: staging
      url: https://staging.example.com
    steps:
      - name: Deploy container to staging
        run: |
          set -euo pipefail
          echo "Deploying ghcr.io/${{ github.repository }}@${{ needs.build.outputs.image-digest }} to staging"
          # Platform-specific deployment command here

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://example.com
    steps:
      - name: Deploy container to production
        run: |
          set -euo pipefail
          echo "Deploying ghcr.io/${{ github.repository }}@${{ needs.build.outputs.image-digest }} to production"

```text

### Promote by Digest (Immutable Deployments)

Use image digest references for immutable deployments across environments:

```yaml

- name: Retag for production
  run: |
    set -euo pipefail
    # Pull by digest (immutable), retag for production
    docker pull ghcr.io/${{ github.repository }}@${{ needs.build.outputs.image-digest }}
    docker tag ghcr.io/${{ github.repository }}@${{ needs.build.outputs.image-digest }} \
      ghcr.io/${{ github.repository }}:production
    docker push ghcr.io/${{ github.repository }}:production

```text

Digest-based promotion ensures the exact same image bytes are deployed to production, regardless of tag mutations.

---

## Azure Web Apps Deployment

### Deploy via `azure/webapps-deploy`

```yaml

name: Deploy to Azure

on:
  push:
    branches: [main]

permissions:
  contents: read
  id-token: write

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Publish
        run: |
          set -euo pipefail
          dotnet publish src/MyApp/MyApp.csproj \
            -c Release \
            -o ./publish

      - name: Upload publish artifact
        uses: actions/upload-artifact@v4
        with:
          name: webapp
          path: ./publish

  deploy-staging:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: staging
      url: https://myapp-staging.azurewebsites.net
    steps:
      - name: Download artifact
        uses: actions/download-artifact@v4
        with:
          name: webapp
          path: ./publish

      - name: Login to Azure
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v3
        with:
          app-name: myapp-staging
          package: ./publish

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://myapp.azurewebsites.net
    steps:
      - name: Download artifact
        uses: actions/download-artifact@v4
        with:
          name: webapp
          path: ./publish

      - name: Login to Azure
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v3
        with:
          app-name: myapp-production
          package: ./publish

```text

### Azure Web App with Deployment Slots

Use deployment slots for zero-downtime deployments with pre-swap validation:

```yaml

- name: Deploy to staging slot
  uses: azure/webapps-deploy@v3
  with:
    app-name: myapp-production
    slot-name: staging
    package: ./publish

- name: Validate staging slot
  shell: bash
  run: |
    set -euo pipefail
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" \
      https://myapp-production-staging.azurewebsites.net/healthz)
    if [ "$HTTP_STATUS" != "200" ]; then
      echo "Health check failed with status $HTTP_STATUS"
      exit 1
    fi

- name: Swap slots
  uses: azure/cli@v2
  with:
    inlineScript: |
      az webapp deployment slot swap \
        --resource-group myapp-rg \
        --name myapp-production \
        --slot staging \
        --target-slot production

```text

### OIDC Authentication (Federated Credentials)

Use OIDC for passwordless Azure authentication instead of service principal secrets:

```yaml

- name: Login to Azure (OIDC)
  uses: azure/login@v2
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

```text

OIDC requires configuring a federated credential in Azure AD that trusts the GitHub Actions OIDC provider. No client
secret is stored in GitHub Secrets.

---

## GitHub Environments with Protection Rules

### Multi-Environment Pipeline

```yaml

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet publish -c Release -o ./publish
      - uses: actions/upload-artifact@v4

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
