---
name: dotnet-github-releases
category: developer-experience
subcategory: cli
description: Creates GitHub Releases for .NET. Release creation, assets, notes, pre-release management.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-github-releases

GitHub Releases for .NET projects: release creation via `gh release create` CLI and GitHub API, asset attachment
patterns (NuGet packages, binaries, SBOMs, checksums), `softprops/action-gh-release` GitHub Actions usage, release notes
generation strategies (GitHub auto-generated, changelog-based, conventional commits), pre-release management (draft
releases, pre-release flag, promoting pre-release to stable), and tag-triggered vs release-triggered workflow concepts.

**Version assumptions:** GitHub CLI (`gh`) 2.x+. `softprops/action-gh-release@v2`. GitHub REST API v3 / GraphQL API v4.
.NET 8.0+ baseline.

## Scope

- Release creation via gh CLI and GitHub API
- Asset attachment (NuGet packages, binaries, SBOMs, checksums)
- softprops/action-gh-release GitHub Actions usage
- Release notes generation strategies
- Pre-release management (draft, pre-release flag, promotion)

## Out of scope

- CLI-specific release automation (build matrix, RID artifacts) -- see [skill:dotnet-cli-release-pipeline]
- CI/CD NuGet push and container publish workflows -- see [skill:dotnet-gha-publish]
- CI pipeline structure and reusable workflows -- see [skill:dotnet-gha-patterns]
- Release lifecycle strategy (NBGV, SemVer, changelogs) -- see [skill:dotnet-release-management]
- NuGet package authoring -- see [skill:dotnet-nuget-authoring]

Cross-references: [skill:dotnet-cli-release-pipeline] for CLI-specific release pipelines with checksums,
[skill:dotnet-gha-publish] for CI publish workflows, [skill:dotnet-gha-patterns] for CI pipeline structure,
[skill:dotnet-nuget-authoring] for NuGet package creation.

---

## Release Creation with GitHub CLI

### Basic Release from Tag

````bash

# Create a release from an existing tag
gh release create v1.2.3 \
  --title "v1.2.3" \
  --notes "Bug fixes and performance improvements."

# Create a release and tag simultaneously
gh release create v1.2.3 \
  --title "v1.2.3" \
  --generate-notes \
  --target main

```text

### Draft Release

Draft releases are invisible to the public until published. Use drafts to stage releases while finalizing assets and
notes.

```bash

# Create a draft release
gh release create v1.2.3 \
  --title "v1.2.3" \
  --draft \
  --generate-notes

# Publish the draft (promote to public)
gh release edit v1.2.3 --draft=false

```text

### Release with Notes from File

```bash

# Write release notes to a file
cat > release-notes.md << 'EOF'
## What's Changed

### New Features
- Added widget caching support
- Improved fluent API ergonomics

### Bug Fixes
- Fixed memory leak in widget pool
- Corrected timezone handling in scheduler

### Breaking Changes
- Removed deprecated `Widget.Create()` overload -- use `WidgetBuilder` instead

**Full Changelog**: https://github.com/mycompany/widgets/compare/v1.1.0...v1.2.0
EOF

gh release create v1.2.0 \
  --title "v1.2.0" \
  --notes-file release-notes.md

```markdown

---

## Asset Attachment

Attach build artifacts to a release for direct download. Common .NET assets include NuGet packages, platform-specific
binaries, SBOMs, and checksum files.

### Attaching Assets at Creation

```bash

# Create release with attached assets
gh release create v1.2.3 \
  --title "v1.2.3" \
  --generate-notes \
  artifacts/MyCompany.Widgets.1.2.3.nupkg \
  artifacts/MyCompany.Widgets.1.2.3.snupkg \
  artifacts/myapp-linux-x64.tar.gz \
  artifacts/myapp-win-x64.zip \
  artifacts/sbom.spdx.json \
  artifacts/SHA256SUMS.txt

```json

### Attaching Assets to Existing Release

```bash

# Upload additional assets after release creation
gh release upload v1.2.3 \
  artifacts/myapp-osx-arm64.tar.gz \
  artifacts/myapp-linux-arm64.tar.gz

# Overwrite an existing asset (same filename)
gh release upload v1.2.3 \
  artifacts/SHA256SUMS.txt --clobber

```text

### Common .NET Asset Types

| Asset Type      | Filename Pattern                   | Purpose                              |
| --------------- | ---------------------------------- | ------------------------------------ |
| NuGet package   | `*.nupkg`                          | Library distribution via NuGet feeds |
| Symbol package  | `*.snupkg`                         | Source-level debugging symbols       |
| Platform binary | `myapp-{rid}.tar.gz` / `.zip`      | Self-contained runtime               |
| SBOM            | `sbom.spdx.json` / `sbom.cdx.json` | Software Bill of Materials           |
| Checksums       | `SHA256SUMS.txt`                   | Integrity verification               |
| Release notes   | `RELEASE-NOTES.md`                 | Detailed change description          |

### Generating Checksums

```bash

# Generate SHA-256 checksums for all release assets
cd artifacts
sha256sum *.nupkg *.tar.gz *.zip > SHA256SUMS.txt

# On macOS
shasum -a 256 *.nupkg *.tar.gz *.zip > SHA256SUMS.txt

```text

For CLI-specific release pipelines with per-RID checksums and automated package manager PRs, see
[skill:dotnet-cli-release-pipeline].

---

## GitHub Actions Release Automation

### softprops/action-gh-release

The `softprops/action-gh-release` action creates GitHub Releases from CI workflows. For full CI pipeline structure
(reusable workflows, matrix strategies), see [skill:dotnet-gha-patterns]. For NuGet push and container publish steps,
see [skill:dotnet-gha-publish].

```yaml

# Release job (add to your CI workflow)
release:
  runs-on: ubuntu-latest
  if: startsWith(github.ref, 'refs/tags/v')
  permissions:
    contents: write
  steps:
    - uses: actions/checkout@v4

    - name: Build and pack
      run: |
        dotnet build --configuration Release
        dotnet pack --configuration Release --output ./artifacts

    - name: Generate checksums
      run: |
        cd artifacts
        sha256sum *.nupkg > SHA256SUMS.txt

    - name: Create GitHub Release
      uses: softprops/action-gh-release@v2
      with:
        generate_release_notes: true
        files: |
          artifacts/*.nupkg
          artifacts/*.snupkg
          artifacts/SHA256SUMS.txt
        draft: false
        prerelease: ${{ contains(github.ref_name, '-') }}

```text

### Tag-Triggered vs Release-Triggered Workflows

Two common patterns for triggering release CI:

**Tag-triggered** -- the workflow runs when a version tag is pushed:

```yaml

on:
  push:
    tags:
      - 'v[0-9]+.[0-9]+.[0-9]+*' # Matches v1.2.3, v1.2.3-beta.1

```text

**Release-triggered** -- the workflow runs when a GitHub Release is published:

```yaml

on:
  release:
    types: [published]

```yaml

| Pattern           | Pros                                       | Cons                                |
| ----------------- | ------------------------------------------ | ----------------------------------- |
| Tag-triggered     | Simple, single event, works with NBGV      | Release must be created in workflow |
| Release-triggered | Draft-then-publish workflow, manual gating | Requires two-step process           |

### Pre-release Detection in CI

Automatically mark releases as pre-release based on SemVer suffix:

```yaml

- name: Determine pre-release status
  id: prerelease
  run: |
    TAG="${GITHUB_REF_NAME}"
    if [[ "$TAG" == *-* ]]; then
      echo "is_prerelease=true" >> "$GITHUB_OUTPUT"
    else
      echo "is_prerelease=false" >> "$GITHUB_OUTPUT"
    fi

- name: Create release
  uses: softprops/action-gh-release@v2
  with:
    prerelease: ${{ steps.prerelease.outputs.is_prerelease }}
    generate_release_notes: true

```text

---

## Release Notes Generation

### GitHub Auto-Generated Notes

GitHub can auto-generate release notes from merged PRs and commits since the last release.

```bash

# Use auto-generated notes
gh release create v1.2.3 --generate-notes

# Auto-generated notes with custom header
gh release create v1.2.3 --generate-notes \
  --notes "## Highlights
- Major performance improvements in widget processing

---
" --notes-start-tag v1.1.0

```text

Configure auto-generated note categories in `.github/release.yml`:

```yaml

# .github/release.yml
changelog:
  exclude:
    labels:
      - ignore-for-release
    authors:
      - dependabot
  categories:
    - title: 'Breaking Changes'
      labels:
        - breaking-change
    - title: 'New Features'
      labels:
        - enhancement
        - feature
    - title: 'Bug Fixes'
      labels:
        - bug
        - fix
    - title: 'Dependencies'
      labels:
        - dependencies
    - title: 'Other Changes'
      labels:
        - '*'

```text

### Changelog-Based Notes

Use a maintained `CHANGELOG.md` as the release notes source. For CHANGELOG format and auto-generation tooling, see
[skill:dotnet-release-management].

```bash

# Extract the section for this version from CHANGELOG.md
# Note: requires a subsequent ## [ section as delimiter. For the last section:
#   sed -n "/^## \[${VERSION}\]/,\$p" CHANGELOG.md | sed '1d'
VERSION="1.2.3"
NOTES=$(sed -n "/^## \[${VERSION}\]/,/^## \[/p" CHANGELOG.md | sed '1d;$d')

gh release create "v${VERSION}" \
  --title "v${VERSION}" \
  --notes "$NOTES"

```text

### Conventional Commit Notes

For projects using conventional commits (`feat:`, `fix:`, `chore:`), tools like `git-cliff` or `conventional-changelog`
can generate structured release notes.

```bash

# Generate release notes from conventional commits using git-cliff
git cliff --tag "v1.2.3" --unreleased --strip header > release-notes.md

gh release create v1.2.3 \
  --title "v1.2.3" \
  --notes-file release-notes.md

```markdown

---

## Pre-Release Management

### Pre-Release Flag

Pre-releases are visible on the releases page but not shown as the "Latest" release. NuGet packages attached to
pre-releases are still stable unless they have SemVer pre-release suffixes.

```bash

# Create a pre-release
gh release create v1.2.3-beta.1 \
  --title "v1.2.3-beta.1" \
  --prerelease \
  --generate-notes

# Create a pre-release from a specific branch
gh release create v2.0.0-alpha.1 \
  --title "v2.0.0-alpha.1" \
  --prerelease \
  --target feature/v2

```text

### Promoting Pre-Release to Stable


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
