---
name: dotnet-release-management
category: operations
subcategory: release
description: Manages .NET release lifecycle. NBGV versioning, SemVer, changelogs, pre-release, branching.
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

# dotnet-release-management

Release lifecycle management for .NET projects: Nerdbank.GitVersioning (NBGV) setup with `version.json` configuration,
version height calculation, and public release vs pre-release modes; SemVer 2.0 strategy for .NET libraries (when to
bump major/minor/patch, API compatibility considerations) and applications (build metadata, deployment versioning);
changelog generation (Keep a Changelog format, auto-generation with git-cliff and conventional commits); pre-release
version workflows (alpha, beta, rc, stable progression); and release branching patterns (release branches, hotfix
branches, trunk-based releases with tags).

**Version assumptions:** .NET 8.0+ baseline. `Nerdbank.GitVersioning` 3.6+ (current stable). SemVer 2.0 specification.

## Scope

- Nerdbank.GitVersioning (NBGV) setup and version height
- SemVer 2.0 strategy for libraries and applications
- Changelog generation (Keep a Changelog, git-cliff)
- Pre-release version workflows (alpha, beta, rc, stable)
- Release branching patterns (release branches, trunk-based)

## Out of scope

- CI/CD NuGet push and deployment workflows -- see [skill:dotnet-gha-publish] and [skill:dotnet-ado-publish]
- GitHub Release creation and asset attachment -- see [skill:dotnet-github-releases]
- NuGet package metadata and signing -- see [skill:dotnet-nuget-authoring]
- Project-level configuration (SourceLink, CPM) -- see [skill:dotnet-project-structure]

Cross-references: [skill:dotnet-gha-publish] for CI publish workflows, [skill:dotnet-ado-publish] for ADO publish
workflows, [skill:dotnet-nuget-authoring] for NuGet package versioning properties.

---

## NBGV (Nerdbank.GitVersioning)

NBGV calculates deterministic version numbers from git history. The version is derived from a `version.json` file and
the git commit height (number of commits since the version was set), producing unique versions for every commit without
manual version bumps.

### Installation

````bash

# Install NBGV CLI tool
dotnet tool install --global nbgv

# Initialize NBGV in a repository
nbgv install

# This creates version.json at the repo root

```json

### version.json Configuration

```json

{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/main/src/NerdBank.GitVersioning/version.schema.json",
  "version": "1.0",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+(\\.\\d+)?(-.*)?$"
  ],
  "cloudBuild": {
    "buildNumber": {
      "enabled": true
    },
    "setVersionVariables": true
  }
}

```text

### version.json Field Reference

| Field | Purpose | Example |
|-------|---------|---------|
| `version` | Base version (major.minor, optional patch) | `"1.0"`, `"2.3.0"` |
| `publicReleaseRefSpec` | Regex patterns for branches/tags that produce public versions | `["^refs/heads/main$"]` |
| `cloudBuild.buildNumber.enabled` | Set CI build number to calculated version | `true` |
| `cloudBuild.setVersionVariables` | Export version as CI environment variables | `true` |
| `nugetPackageVersion` | Override NuGet package version format | `{"semVer": 2}` |
| `assemblyVersion.precision` | Assembly version component count | `"major"`, `"minor"`, `"build"`, `"revision"` |
| `inherit` | Inherit from parent directory version.json | `true` |

### How Version Height Works

NBGV counts the number of commits since the `version` field was last changed in `version.json`. This count becomes the patch version:

```json

version.json: "version": "1.2"

Commit history:
  abc1234  feat: add caching          -> 1.2.3
  def5678  fix: null check            -> 1.2.2
  ghi9012  chore: update deps         -> 1.2.1
  jkl3456  Bump version to 1.2        -> 1.2.0  (version.json changed here)

```json

The version height ensures every commit has a unique version without manual intervention.

### Pre-Release vs Public Release

```json

{
  "version": "1.2-beta",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+(\\.\\d+)?(-.*)?$"
  ]
}

```text

| Branch/Ref | Computed Version | Notes |
|-----------|-----------------|-------|
| `main` (public) | `1.2.5-beta` | Public pre-release, height=5 |
| `feature/foo` (non-public) | `1.2.5-beta.gcommithash` | Includes git hash suffix |
| Tag `v1.2.5` (public) | `1.2.5` | Remove `-beta` before tagging |

To release a stable version, remove the pre-release suffix from `version.json` before the release commit:

```json

{
  "version": "1.2"
}

```json

### NBGV CLI Commands

```bash

# Show the current calculated version
nbgv get-version

# Show specific version properties
nbgv get-version -v NuGetPackageVersion
nbgv get-version -v SemVer2

# Prepare for a release (creates release branch, bumps version)
nbgv prepare-release

# Set version variables for CI
nbgv cloud

```text

### Monorepo NBGV Configuration

For monorepos with independently versioned projects, place `version.json` in each project directory and use `inherit`:

```json

repo-root/
  version.json              <- { "version": "1.0" }
  src/
    LibraryA/
      version.json          <- { "version": "2.3", "inherit": true }
    LibraryB/
      version.json          <- { "version": "1.1-beta", "inherit": true }

```json

The `inherit` field pulls settings (like `publicReleaseRefSpec` and `cloudBuild`) from the parent `version.json` while overriding the version number.

---

## SemVer Strategy for .NET Libraries

### When to Bump Versions

SemVer 2.0 specifies version format `MAJOR.MINOR.PATCH`:

| Change Type | Version Bump | Examples |
|-------------|-------------|----------|
| Breaking API changes | **Major** | Removing public types/members, changing method signatures, renaming namespaces |
| New features (backward compatible) | **Minor** | Adding public types/members, new extension methods, new overloads |
| Bug fixes (backward compatible) | **Patch** | Fixing incorrect behavior, performance improvements, internal refactors |

### .NET-Specific Breaking Change Considerations

| Change | Breaking? | Notes |
|--------|-----------|-------|
| Remove public type | Yes (Major) | Consumers referencing it will fail to compile |
| Remove public method | Yes (Major) | Direct callers will fail |
| Add required parameter to public method | Yes (Major) | Existing callers do not supply it |
| Add optional parameter to public method | No (Minor) | Binary compatible but source-breaking for callers using named arguments |
| Change return type | Yes (Major) | Binary and source breaking |
| Add new public type | No (Minor) | No existing code affected |
| Add new overload | No (Minor) | Existing calls still resolve |
| Change internal implementation | No (Patch) | No public API change |
| Change default value of optional parameter | No (Patch) | Binary compatible (value embedded at call site on recompile) |
| Seal a previously unsealed class | Yes (Major) | Consumers inheriting from it will fail |
| Make a virtual method non-virtual | Yes (Major) | Consumers overriding it will fail |

### API Compatibility Validation

Use `EnablePackageValidation` to catch accidental breaking changes. For full package validation setup, see [skill:dotnet-nuget-authoring].

```xml

<PropertyGroup>
  <EnablePackageValidation>true</EnablePackageValidation>
  <PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>
</PropertyGroup>

```text

---

## SemVer Strategy for Applications

Applications (web apps, desktop apps, services) have different versioning considerations than libraries because they do not have public API consumers.

### Application Versioning Approaches

| Approach | Format | Best For |
|----------|--------|----------|
| SemVer (feature-driven) | `1.2.3` | Installed desktop/mobile apps with user-visible versioning |
| CalVer (calendar-based) | `2024.1.15` | SaaS apps with continuous deployment |
| Build number | `1.2.3+42` | CI-driven versioning with build metadata |
| NBGV height | `1.2.42` | Automated versioning from git commits |

### Build Metadata

SemVer 2.0 allows `+` suffixed build metadata that does not affect version precedence:

```text

1.2.3+build.42        Build number
1.2.3+abcdef          Git commit hash
1.2.3+2024.01.15      Build date
1.2.3-beta.1+42       Pre-release with build metadata

```text

Build metadata is useful for tracing a deployed binary back to its source commit. NBGV appends git metadata automatically.

### Deployment Versioning

For continuously deployed services, version stamping aids troubleshooting:

```xml

<PropertyGroup>
  <!-- Embed full version in assembly for runtime introspection -->
  <InformationalVersion>1.2.3+abcdef.2024-01-15</InformationalVersion>
</PropertyGroup>

```text

Read at runtime:

```csharp

var version = typeof(Program).Assembly
    .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion;
// Returns "1.2.3+abcdef.2024-01-15"

```text

---

## Changelog Generation

### Keep a Changelog Format

The [Keep a Changelog](https://keepachangelog.com/) format is a widely adopted standard:

```markdown

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Widget caching support for improved throughput

## [1.2.0] - 2024-03-15

### Added
- Fluent API for widget configuration
- Batch processing support

### Changed
- Improved error messages for invalid widget states

### Fixed
- Memory leak in widget pool under high concurrency
- Timezone handling in scheduled widget operations

### Deprecated
- `Widget.Create()` static method -- use `WidgetBuilder` instead

## [1.1.0] - 2024-01-10

### Added
- Widget serialization support

[Unreleased]: https://github.com/mycompany/widgets/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/mycompany/widgets/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/mycompany/widgets/releases/tag/v1.1.0

```text

### Section Types

| Section | Purpose |
|---------|---------|
| `Added` | New features |
| `Changed` | Changes to existing functionality |
| `Deprecated` | Features that will be removed in future versions |
| `Removed` | Features removed in this release |
| `Fixed` | Bug fixes |
| `Security` | Vulnerability fixes |

### Auto-Generation with git-cliff

[git-cliff](https://git-cliff.org/) generates changelogs from conventional commits:

```bash

# Install git-cliff
cargo install git-cliff

# Generate changelog for all versions
git cliff --output CHANGELOG.md

# Generate changelog for unreleased changes only
git cliff --unreleased --output CHANGELOG.md

# Generate notes for a specific tag range
git cliff --tag v1.2.0 --unreleased

```markdown

Configure `cliff.toml` for .NET conventional commit patterns:

```toml

# cliff.toml
[changelog]
header = """
# Changelog\n
All notable changes to this project will be documented in this file.\n
"""
body = """
{% if version %}\
    ## [{{ version | trim_start_matches(pat="v") }}] - {{ timestamp | date(format="%Y-%m-%d") }}
{% else %}\
    ## [Unreleased]
{% endif %}\
{% for group, commits in commits | group_by(attribute="group") %}
    ### {{ group | upper_first }}
    {% for commit in commits %}
        - {{ commit.message | upper_first }}\
    {% endfor %}
{% endfor %}\n
"""
trim = true

[git]
conventional_commits = true
filter_unconventional = true
commit_parsers = [
    { message = "^feat", group = "Added" },
    { message = "^fix", group = "Fixed" },
    { message = "^perf", group = "Changed" },

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
