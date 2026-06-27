    { message = "^perf", group = "Changed" },
    { message = "^refactor", group = "Changed" },
    { message = "^docs", group = "Documentation" },
    { message = "^chore\\(deps\\)", group = "Dependencies" },
    { message = "^chore", skip = true },
    { message = "^ci", skip = true },
    { message = "^test", skip = true },
]

```text

### Conventional Commit Format

```text

feat: add widget caching support
fix: correct timezone handling in scheduler
feat!: rename Widget.Create() to WidgetBuilder.Build()
chore(deps): update System.Text.Json to 8.0.5
docs: update API reference for caching

Breaking change in body:
feat: redesign widget API

BREAKING CHANGE: Widget.Create() has been removed. Use WidgetBuilder instead.

```text

| Prefix | SemVer Impact | Changelog Section |
|--------|--------------|-------------------|
| `feat:` | Minor | Added |
| `fix:` | Patch | Fixed |
| `feat!:` or `BREAKING CHANGE:` | Major | Breaking Changes |
| `perf:` | Patch | Changed |
| `refactor:` | Patch | Changed |
| `docs:` | None | Documentation |
| `chore:` | None | (skipped) |

---

## Pre-Release Version Workflows

### Standard Pre-Release Progression

```text

alpha -> beta -> rc -> stable

1.0.0-alpha.1  Early development, API unstable
1.0.0-alpha.2  Continued alpha iteration
1.0.0-beta.1   Feature-complete, API stabilizing
1.0.0-beta.2   Beta bug fixes
1.0.0-rc.1     Release candidate, final validation
1.0.0-rc.2     RC bug fix (if needed)
1.0.0          Stable release

```text

### NBGV Pre-Release Workflow

```bash

# Start with pre-release suffix in version.json
# version.json: { "version": "1.0-alpha" }
# Produces: 1.0.1-alpha, 1.0.2-alpha, ...

# Promote to beta
# Edit version.json: { "version": "1.0-beta" }
# Produces: 1.0.1-beta, 1.0.2-beta, ...

# Promote to rc
# Edit version.json: { "version": "1.0-rc" }
# Produces: 1.0.1-rc, 1.0.2-rc, ...

# Promote to stable
# Edit version.json: { "version": "1.0" }
# Produces: 1.0.1, 1.0.2, ...

```json

### Manual Pre-Release Workflow

For projects not using NBGV:

```xml

<!-- In .csproj or Directory.Build.props -->
<PropertyGroup>
  <VersionPrefix>1.0.0</VersionPrefix>
  <VersionSuffix>beta.1</VersionSuffix>
  <!-- Produces: 1.0.0-beta.1 -->
</PropertyGroup>

```text

Override from CI:

```bash

# CI sets the pre-release suffix
dotnet pack /p:VersionSuffix="beta.$(BUILD_NUMBER)"

# Stable release: omit VersionSuffix
dotnet pack

```text

### NuGet Pre-Release Ordering

NuGet follows SemVer 2.0 pre-release precedence:

```text

1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.2
1.0.0-alpha.2 < 1.0.0-beta
1.0.0-beta < 1.0.0-beta.1
1.0.0-rc.1 < 1.0.0

```text

Numeric identifiers are compared as integers; alphabetic identifiers are compared lexically.

---

## Release Branching Patterns

### Trunk-Based with Tags

The simplest release model. All development happens on `main`, releases are marked with tags.

```text

main:  A -- B -- C -- D -- E -- F -- G
                 |              |
              v1.0.0         v1.1.0

```text

```bash

# Tag and push for release
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0

```bash

**Best for:** Libraries, small teams, continuous delivery.

### Release Branches

Create a release branch for stabilization while `main` continues development.

```text

main:      A -- B -- C -- D -- E -- F -- G
                      \
release/1.0:           C' -- D' -- E'
                              |
                           v1.0.0

```text

```bash

# Create release branch
git checkout -b release/1.0 main

# Stabilize on release branch (bug fixes only)
git commit -m "fix: correct null check in widget pool"

# Tag and release
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin release/1.0 v1.0.0

# Merge fixes back to main
git checkout main
git merge release/1.0

```text

**Best for:** Products with support contracts, LTS versions, teams needing parallel development and stabilization.

### NBGV prepare-release

NBGV automates release branch creation and version bumping:

```bash

# Creates release/v1.0 branch, bumps main to 1.1-alpha
nbgv prepare-release

# What this does:
# 1. Creates branch "release/v1.0" from current commit
# 2. On release branch: removes pre-release suffix (version: "1.0")
# 3. On main: bumps to "1.1-alpha" (next development version)

```text

### Hotfix Branches

Emergency fixes for released versions:

```text

main:         A -- B -- C -- D -- E
                         \
release/1.0:              C' -- v1.0.0
                                  \
hotfix/1.0.1:                      F' -- v1.0.1

```text

```bash

# Branch from the release tag
git checkout -b hotfix/1.0.1 v1.0.0

# Fix the critical issue
git commit -m "fix: critical security vulnerability in auth handler"

# Tag and release the hotfix
git tag -a v1.0.1 -m "Hotfix v1.0.1"
git push origin hotfix/1.0.1 v1.0.1

# Merge hotfix back to main
git checkout main
git merge hotfix/1.0.1

```text

### Branching Pattern Comparison

| Pattern | Release Cadence | Parallel Versions | Complexity |
|---------|----------------|-------------------|------------|
| Trunk + tags | Continuous | No | Low |
| Release branches | Scheduled | Yes | Medium |
| GitFlow (full) | Scheduled | Yes | High |

For most .NET open-source libraries, trunk-based with tags and NBGV is sufficient. Reserve release branches for products that maintain multiple supported versions simultaneously.

---

## Agent Gotchas

1. **NBGV `version.json` uses major.minor only (not major.minor.patch)** -- the patch version is calculated from commit height. Setting `"version": "1.2.3"` fixes the patch to 3, defeating the purpose of automatic versioning.

1. **NBGV requires git history to calculate version height** -- shallow clones (`git clone --depth 1`) produce incorrect versions. In CI, use `fetch-depth: 0` with `actions/checkout` to get full history.

1. **`publicReleaseRefSpec` patterns are regex, not globs** -- use `^refs/heads/main$` not `main`. Missing anchors will match unintended refs.

1. **SemVer pre-release ordering is lexical for non-numeric segments** -- `alpha` < `beta` < `rc` because of alphabetical comparison. Numeric segments are compared as integers, so `beta.2` < `beta.10` (because 2 < 10). Do not assume lexical ordering for numeric identifiers.

1. **Do not use CalVer for NuGet libraries** -- NuGet resolution depends on SemVer ordering. CalVer versions like `2024.1.0` work mechanically but violate consumer expectations for API stability signals.

1. **`VersionPrefix` + `VersionSuffix` combine to form `Version`** -- setting all three causes conflicts. Use either `Version` alone or `VersionPrefix`/`VersionSuffix` together, not both.

1. **Keep a Changelog `[Unreleased]` section must be updated before release** -- move entries from `[Unreleased]` to the new version section, update comparison links, and add a new empty `[Unreleased]` section.

1. **`nbgv prepare-release` modifies both the new branch and the current branch** -- it bumps the version on the current branch to the next minor. Run it from the branch you want to continue development on (usually `main`).
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
