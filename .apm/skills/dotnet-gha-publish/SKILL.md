---
name: dotnet-gha-publish
category: operations
subcategory: ci-cd
description: Publishes .NET artifacts from GitHub Actions. NuGet push, container images, signing, SBOM.
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

# dotnet-gha-publish

Publishing workflows for .NET projects in GitHub Actions: NuGet package push to nuget.org and GitHub Packages, container
image build and push to GHCR/DockerHub/ACR, artifact signing with NuGet signing and sigstore, SBOM generation with
Microsoft SBOM tool, and conditional publishing triggered by tags and releases.

**Version assumptions:** `actions/setup-dotnet@v4` for .NET 8/9/10. `docker/build-push-action@v6` for container image
builds. `docker/login-action@v3` for registry authentication. .NET SDK container publish (`dotnet publish` with
`PublishContainer`) for Dockerfile-free container builds.

## Scope

- NuGet package push to nuget.org and GitHub Packages
- Container image build and push to GHCR/DockerHub/ACR
- Artifact signing with NuGet signing and sigstore
- SBOM generation with Microsoft SBOM tool
- Conditional publishing triggered by tags and releases

## Out of scope

- Container image authoring (Dockerfile, base image selection) -- see [skill:dotnet-containers]
- Native AOT MSBuild configuration -- see [skill:dotnet-native-aot]
- CLI release pipelines -- see [skill:dotnet-cli-release-pipeline]
- Starter CI templates -- see [skill:dotnet-add-ci]
- Azure DevOps publishing -- see [skill:dotnet-ado-publish]
- Deployment to target environments -- see [skill:dotnet-gha-deploy]

Cross-references: [skill:dotnet-containers] for container image authoring and SDK container properties,
[skill:dotnet-native-aot] for AOT publish configuration in CI, [skill:dotnet-cli-release-pipeline] for CLI-specific
release automation, [skill:dotnet-add-ci] for starter publish templates.

---

## NuGet Push to nuget.org

### Tag-Triggered Package Publishing

````yaml

name: Publish NuGet Package

on:
  push:
    tags:
      - 'v*'

permissions:
  contents: read

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Extract version from tag
        id: version
        shell: bash
        run: |
          set -euo pipefail
          VERSION="${GITHUB_REF_NAME#v}"
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"

      - name: Pack
        run: |
          set -euo pipefail
          dotnet pack src/MyLibrary/MyLibrary.csproj \
            -c Release \
            -p:Version=${{ steps.version.outputs.version }} \
            -o ./nupkgs

      - name: Push to nuget.org
        run: |
          set -euo pipefail
          dotnet nuget push ./nupkgs/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate

```json

The `--skip-duplicate` flag prevents failures when a package version is already published (idempotent retries).

### Publishing to GitHub Packages

```yaml

- name: Push to GitHub Packages
  run: |
    set -euo pipefail
    dotnet nuget push ./nupkgs/*.nupkg \
      --api-key ${{ secrets.GITHUB_TOKEN }} \
      --source https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json \
      --skip-duplicate

```json

### Publishing to Both Feeds

Publish to nuget.org for public consumption and GitHub Packages for organization-internal pre-release:

```yaml

- name: Push to nuget.org (stable releases)
  if: "!contains(steps.version.outputs.version, '-')"
  run: |
    set -euo pipefail
    dotnet nuget push ./nupkgs/*.nupkg \
      --api-key ${{ secrets.NUGET_API_KEY }} \
      --source https://api.nuget.org/v3/index.json \
      --skip-duplicate

- name: Push to GitHub Packages (all versions)
  run: |
    set -euo pipefail
    dotnet nuget push ./nupkgs/*.nupkg \
      --api-key ${{ secrets.GITHUB_TOKEN }} \
      --source https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json \
      --skip-duplicate

```json

Pre-release versions (containing `-` like `1.2.3-preview.1`) go only to GitHub Packages; stable versions go to both.

---

## Container Image Build and Push

### Dockerfile-Based Build with docker/build-push-action

For projects with a custom Dockerfile -- see [skill:dotnet-containers] for Dockerfile authoring guidance:

```yaml

name: Publish Container Image

on:
  push:
    tags:
      - 'v*'

permissions:
  contents: read
  packages: write

jobs:
  container:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ghcr.io/${{ github.repository }}
          tags: |
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=sha

      - name: Build and push
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}

```text

### SDK Container Publish (Dockerfile-Free)

Use .NET SDK container publish for projects without a Dockerfile -- see [skill:dotnet-containers] for `PublishContainer`
MSBuild configuration:

```yaml

- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'

- name: Log in to GHCR
  run: |
    set -euo pipefail
    echo "${{ secrets.GITHUB_TOKEN }}" | \
      docker login ghcr.io -u ${{ github.actor }} --password-stdin

- name: Publish container image
  run: |
    set -euo pipefail
    VERSION="${GITHUB_REF_NAME#v}"
    dotnet publish src/MyApp/MyApp.csproj \
      -c Release \
      -p:PublishProfile=DefaultContainer \
      -p:ContainerRegistry=ghcr.io \
      -p:ContainerRepository=${{ github.repository }} \
      -p:ContainerImageTags="\"${VERSION};latest\""

```text

### Push to Multiple Registries

Push to GHCR and DockerHub from the same workflow:

```yaml

- name: Log in to GHCR
  uses: docker/login-action@v3
  with:
    registry: ghcr.io
    username: ${{ github.actor }}
    password: ${{ secrets.GITHUB_TOKEN }}

- name: Log in to DockerHub
  uses: docker/login-action@v3
  with:
    username: ${{ secrets.DOCKERHUB_USERNAME }}
    password: ${{ secrets.DOCKERHUB_TOKEN }}

- name: Extract metadata
  id: meta
  uses: docker/metadata-action@v5
  with:
    images: |
      ghcr.io/${{ github.repository }}
      ${{ secrets.DOCKERHUB_USERNAME }}/${{ github.event.repository.name }}
    tags: |
      type=semver,pattern={{version}}

- name: Build and push to both registries
  uses: docker/build-push-action@v6
  with:
    context: .
    push: true
    tags: ${{ steps.meta.outputs.tags }}
    labels: ${{ steps.meta.outputs.labels }}

```text

### Push to Azure Container Registry (ACR)

```yaml

- name: Log in to ACR
  uses: docker/login-action@v3
  with:
    registry: ${{ secrets.ACR_LOGIN_SERVER }}
    username: ${{ secrets.ACR_USERNAME }}
    password: ${{ secrets.ACR_PASSWORD }}

- name: Build and push to ACR
  uses: docker/build-push-action@v6
  with:
    context: .
    push: true
    tags: ${{ secrets.ACR_LOGIN_SERVER }}/myapp:${{ github.ref_name }}

```text

### Native AOT Container Publish

Publish a Native AOT binary as a container image. AOT configuration is owned by [skill:dotnet-native-aot]; this shows
the CI pipeline step only:

```yaml

- name: Publish AOT container
  run: |
    set -euo pipefail
    dotnet publish src/MyApp/MyApp.csproj \
      -c Release \
      -r linux-x64 \
      -p:PublishAot=true \
      -p:PublishProfile=DefaultContainer \
      -p:ContainerRegistry=ghcr.io \
      -p:ContainerRepository=${{ github.repository }} \
      -p:ContainerBaseImage=mcr.microsoft.com/dotnet/runtime-deps:8.0-noble-chiseled

```text

The `runtime-deps` base image is sufficient for AOT binaries since they include the runtime. See
[skill:dotnet-native-aot] for AOT MSBuild properties and [skill:dotnet-containers] for base image selection.

---

## Artifact Signing

### NuGet Package Signing

Sign NuGet packages with a certificate for tamper detection:

```yaml

- name: Sign NuGet packages
  run: |
    set -euo pipefail
    dotnet nuget sign ./nupkgs/*.nupkg \
      --certificate-path ${{ runner.temp }}/signing-cert.pfx \
      --certificate-password ${{ secrets.CERT_PASSWORD }} \
      --timestamper http://timestamp.digicert.com

```text

For CI, extract the certificate from a base64-encoded secret:

```yaml

- name: Decode signing certificate
  shell: bash
  run: |
    set -euo pipefail
    echo "${{ secrets.SIGNING_CERT_BASE64 }}" | base64 -d > "${{ runner.temp }}/signing-cert.pfx"

- name: Sign NuGet packages
  run: |
    set -euo pipefail
    dotnet nuget sign ./nupkgs/*.nupkg \
      --certificate-path ${{ runner.temp }}/signing-cert.pfx \
      --certificate-password ${{ secrets.CERT_PASSWORD }} \
      --timestamper http://timestamp.digicert.com

- name: Clean up certificate
  if: always()
  run: rm -f "${{ runner.temp }}/signing-cert.pfx"

```text

### Container Image Signing with Sigstore

Sign container images with keyless signing via sigstore/cosign:

```yaml

- name: Install cosign
  uses: sigstore/cosign-installer@v3

- name: Sign container image
  env:
    COSIGN_EXPERIMENTAL: '1'
  run: |
    set -euo pipefail
    cosign sign --yes ghcr.io/${{ github.repository }}@${{ steps.build.outputs.digest }}

```text

Keyless signing uses GitHub's OIDC token -- no private key management required.

---

## SBOM Generation

### Microsoft SBOM Tool

Generate a Software Bill of Materials for supply chain transparency:

```yaml

- name: Generate SBOM
  uses: microsoft/sbom-action@v0
  with:
    BuildDropPath: ./nupkgs
    PackageName: MyLibrary
    PackageVersion: ${{ steps.version.outputs.version }}
    NamespaceUriBase: https://github.com/${{ github.repository }}

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
