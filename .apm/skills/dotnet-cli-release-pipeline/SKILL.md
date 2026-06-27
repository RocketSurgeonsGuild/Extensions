---
name: dotnet-cli-release-pipeline
category: operations
subcategory: release
description: Releases CLI tools. GitHub Actions build matrix, artifact staging, Releases, checksums.
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

# dotnet-cli-release-pipeline

Unified release CI/CD pipeline for .NET CLI tools: GitHub Actions workflow producing all distribution formats from a
single version tag trigger, build matrix per Runtime Identifier (RID), artifact staging between jobs, GitHub Releases
with SHA-256 checksums, automated Homebrew formula and winget manifest PR creation, and SemVer versioning strategy with
git tags.

**Version assumptions:** .NET 8.0+ baseline. GitHub Actions workflow syntax v2. Patterns apply to any CI system but
examples use GitHub Actions.

## Scope

- Tag-triggered GitHub Actions release workflow
- Build matrix per Runtime Identifier (RID)
- Artifact staging between CI jobs
- GitHub Releases with SHA-256 checksums
- Automated Homebrew formula and winget manifest PR creation
- SemVer versioning with git tags

## Out of scope

- General CI/CD patterns (branch strategies, matrix testing) -- see [skill:dotnet-gha-patterns] and
  [skill:dotnet-ado-patterns]
- Native AOT compilation configuration -- see [skill:dotnet-native-aot]
- Distribution strategy decisions -- see [skill:dotnet-cli-distribution]
- Package format details -- see [skill:dotnet-cli-packaging]
- Container image publishing -- see [skill:dotnet-containers]

Cross-references: [skill:dotnet-cli-distribution] for RID matrix and publish strategy, [skill:dotnet-cli-packaging] for
package format authoring, [skill:dotnet-native-aot] for AOT publish configuration, [skill:dotnet-containers] for
container-based distribution.

---

## Versioning Strategy

### SemVer + Git Tags

Use Semantic Versioning (SemVer) with git tags as the single source of truth for release versions.

**Tag format:** `v{major}.{minor}.{patch}` (e.g., `v1.2.3`)

````bash

# Tag a release
git tag -a v1.2.3 -m "Release v1.2.3"
git push origin v1.2.3

```bash

### Version Flow

```text

git tag v1.2.3
    │
    ▼
GitHub Actions trigger (on push tags: v*)
    │
    ▼
Extract version from tag: GITHUB_REF_NAME → v1.2.3 → 1.2.3
    │
    ▼
Pass to dotnet publish /p:Version=1.2.3
    │
    ▼
Embed in binary (--version output)
    │
    ▼
Stamp in package manifests (Homebrew, winget, Scoop, NuGet)

```text

### Extracting Version from Tag

```yaml

- name: Extract version from tag
  id: version
  run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"
  # v1.2.3 → 1.2.3

```text

### Pre-release Versions

```bash

# Pre-release tag
git tag -a v1.3.0-rc.1 -m "Release candidate 1"

# CI detects pre-release and skips package manager submissions
# but still creates GitHub Release as pre-release

```text

---

## Unified GitHub Actions Workflow

### Complete Workflow

```yaml

name: Release

on:
  push:
    tags:
      - 'v[0-9]+.[0-9]+.[0-9]+*' # v1.2.3, v1.2.3-rc.1

permissions:
  contents: write # Create GitHub Releases

defaults:
  run:
    shell: bash

env:
  PROJECT: src/MyCli/MyCli.csproj
  DOTNET_VERSION: '8.0.x'

jobs:
  build:
    strategy:
      matrix:
        include:
          - rid: linux-x64
            os: ubuntu-latest
          - rid: linux-arm64
            os: ubuntu-latest
          - rid: osx-arm64
            os: macos-latest
          - rid: win-x64
            os: windows-latest
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Extract version
        id: version
        shell: bash
        run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"

      - name: Publish
        run: >-
          dotnet publish ${{ env.PROJECT }} -c Release -r ${{ matrix.rid }} -o ./publish /p:Version=${{
          steps.version.outputs.version }}

      - name: Package (Unix)
        if: runner.os != 'Windows'
        run: |
          set -euo pipefail
          cd publish
          tar -czf "$GITHUB_WORKSPACE/mytool-${{ steps.version.outputs.version }}-${{ matrix.rid }}.tar.gz" .

      - name: Package (Windows)
        if: runner.os == 'Windows'
        shell: pwsh
        run: |
          Compress-Archive -Path "publish/*" `
            -DestinationPath "mytool-${{ steps.version.outputs.version }}-${{ matrix.rid }}.zip"

      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: release-${{ matrix.rid }}
          path: |
            *.tar.gz
            *.zip

  release:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Extract version
        id: version
        run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"

      - name: Download all artifacts
        uses: actions/download-artifact@v4
        with:
          path: artifacts
          merge-multiple: true

      - name: Generate checksums
        working-directory: artifacts
        run: |
          set -euo pipefail
          shasum -a 256 *.tar.gz *.zip > checksums-sha256.txt
          cat checksums-sha256.txt

      - name: Detect pre-release
        id: prerelease
        run: |
          set -euo pipefail
          if [[ "${{ steps.version.outputs.version }}" == *-* ]]; then
            echo "is_prerelease=true" >> "$GITHUB_OUTPUT"
          else
            echo "is_prerelease=false" >> "$GITHUB_OUTPUT"
          fi

      # Pin third-party actions to a commit SHA in production for supply-chain security
      - name: Create GitHub Release
        uses: softprops/action-gh-release@v2
        with:
          name: v${{ steps.version.outputs.version }}
          prerelease: ${{ steps.prerelease.outputs.is_prerelease }}
          generate_release_notes: true
          files: |
            artifacts/*.tar.gz
            artifacts/*.zip
            artifacts/checksums-sha256.txt

  publish-nuget:
    needs: release
    if: ${{ !contains(github.ref_name, '-') }} # Skip pre-releases
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Extract version
        id: version
        run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"

      - name: Pack
        run: >-
          dotnet pack ${{ env.PROJECT }} -c Release /p:Version=${{ steps.version.outputs.version }} -o ./nupkgs

      - name: Push to NuGet
        run: >-
          dotnet nuget push ./nupkgs/*.nupkg --source https://api.nuget.org/v3/index.json --api-key ${{
          secrets.NUGET_API_KEY }}

```json

---

## Build Matrix per RID

### Matrix Strategy

The build matrix produces one artifact per RID. Each RID runs on the appropriate runner OS.

```yaml

strategy:
  matrix:
    include:
      - rid: linux-x64
        os: ubuntu-latest
      - rid: linux-arm64
        os: ubuntu-latest # Cross-compile ARM64 on x64 runner
      - rid: osx-arm64
        os: macos-latest # Native ARM64 runner
      - rid: win-x64
        os: windows-latest

```text

### Cross-Compilation Notes

- **linux-arm64 on ubuntu-latest:** .NET supports cross-compilation for managed (non-AOT) builds.
  `dotnet publish -r linux-arm64` on an x64 runner produces a valid ARM64 binary without QEMU. For Native AOT,
  cross-compiling ARM64 on an x64 runner requires the ARM64 cross-compilation toolchain (`gcc-aarch64-linux-gnu` or
  equivalent). See [skill:dotnet-native-aot] for cross-compile prerequisites.
- **osx-arm64:** Use `macos-latest` (which provides ARM64 runners) for native compilation. Cross-compiling macOS ARM64
  from Linux is not supported.
- **win-x64 on windows-latest:** Native compilation on Windows runner.

### Extended Matrix (Optional)

```yaml

strategy:
  matrix:
    include:
      # Primary targets
      - rid: linux-x64
        os: ubuntu-latest
      - rid: linux-arm64
        os: ubuntu-latest
      - rid: osx-arm64
        os: macos-latest
      - rid: win-x64
        os: windows-latest
      # Extended targets
      - rid: osx-x64
        os: macos-13 # Intel macOS runner
      - rid: linux-musl-x64
        os: ubuntu-latest # Alpine musl cross-compile

```text

---

## Artifact Staging

### Upload Per-RID Artifacts

Each matrix job uploads its artifact with a RID-specific name:

```yaml

- name: Upload artifact
  uses: actions/upload-artifact@v4
  with:
    name: release-${{ matrix.rid }}
    path: |
      *.tar.gz
      *.zip
    retention-days: 1 # Short retention -- artifacts are published to GitHub Releases

```text

### Download in Release Job

The release job downloads all artifacts from the build matrix:

```yaml

- name: Download all artifacts
  uses: actions/download-artifact@v4
  with:
    path: artifacts
    merge-multiple: true # Merge all release-* artifacts into one directory

```text

After download, `artifacts/` contains:

```text

artifacts/
  mytool-1.2.3-linux-x64.tar.gz
  mytool-1.2.3-linux-arm64.tar.gz
  mytool-1.2.3-osx-arm64.tar.gz
  mytool-1.2.3-win-x64.zip

```text

---

## GitHub Releases with Checksums

### Checksum Generation

```yaml

- name: Generate checksums
  working-directory: artifacts
  run: |
    set -euo pipefail
    shasum -a 256 *.tar.gz *.zip > checksums-sha256.txt
    cat checksums-sha256.txt

```text

**Output format (checksums-sha256.txt):**

```text

abc123...  mytool-1.2.3-linux-x64.tar.gz
def456...  mytool-1.2.3-linux-arm64.tar.gz
ghi789...  mytool-1.2.3-osx-arm64.tar.gz
jkl012...  mytool-1.2.3-win-x64.zip

```text

### Creating the Release

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
