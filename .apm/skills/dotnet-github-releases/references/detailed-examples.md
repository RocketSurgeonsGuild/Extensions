
When a pre-release has been validated, promote it to a stable release:

```bash

# Remove pre-release flag
gh release edit v1.2.3-rc.1 --prerelease=false

# Or create a new stable release pointing to the same commit
COMMIT=$(gh release view v1.2.3-rc.1 --json targetCommitish -q .targetCommitish)
gh release create v1.2.3 \
  --title "v1.2.3" \
  --target "$COMMIT" \
  --notes "Stable release based on v1.2.3-rc.1. No changes from RC."

```text

### Draft-Then-Publish Workflow

Use drafts to stage releases with assets before making them public:

```bash

# 1. CI creates a draft release with all assets
gh release create v1.2.3 \
  --draft \
  --title "v1.2.3" \
  --generate-notes \
  artifacts/*.nupkg artifacts/SHA256SUMS.txt

# 2. Team reviews the draft on GitHub

# 3. Publish the draft when ready
gh release edit v1.2.3 --draft=false

# 4. A release-triggered workflow picks up the published event
#    and pushes NuGet packages to nuget.org

```text

### Pre-Release Progression

A typical pre-release progression for a .NET library:

| Stage             | Tag              | GitHub Pre-release | NuGet Version   |
| ----------------- | ---------------- | ------------------ | --------------- |
| Alpha             | `v2.0.0-alpha.1` | Yes                | `2.0.0-alpha.1` |
| Beta              | `v2.0.0-beta.1`  | Yes                | `2.0.0-beta.1`  |
| Release candidate | `v2.0.0-rc.1`    | Yes                | `2.0.0-rc.1`    |
| Stable            | `v2.0.0`         | No (Latest)        | `2.0.0`         |

---

## GitHub API Release Management

### Creating Releases via API

For automation scenarios beyond the `gh` CLI:

```bash

# Create a release via GitHub REST API
curl -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  "https://api.github.com/repos/OWNER/REPO/releases" \
  -d '{
    "tag_name": "v1.2.3",
    "target_commitish": "main",
    "name": "v1.2.3",
    "body": "Release notes here",
    "draft": false,
    "prerelease": false
  }'

```text

### Uploading Assets via API

```bash

# Upload an asset to an existing release (REST API needs numeric release ID)
RELEASE_ID=$(gh api repos/OWNER/REPO/releases/tags/v1.2.3 --jq .id)
curl -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Content-Type: application/octet-stream" \
  "https://uploads.github.com/repos/OWNER/REPO/releases/${RELEASE_ID}/assets?name=MyApp.nupkg" \
  --data-binary @artifacts/MyApp.1.2.3.nupkg

```text

### Listing and Querying Releases

```bash

# List all releases
gh release list

# View a specific release
gh release view v1.2.3

# Get latest release tag
gh release view --json tagName -q .tagName

# List releases as JSON for scripting
gh release list --json tagName,isPrerelease,publishedAt

```json

---

## Agent Gotchas

1. **Never hardcode `GITHUB_TOKEN` values in examples** -- always use `$GITHUB_TOKEN` or `${{ secrets.GITHUB_TOKEN }}`
   environment variable references. The `GITHUB_TOKEN` is automatically available in GitHub Actions.

1. **`softprops/action-gh-release` requires `permissions: contents: write`** -- without this, the action fails with a
   403 error. Always include the permissions block in the workflow job.

1. **Pre-release detection by SemVer suffix requires checking for a hyphen** -- `v1.2.3-beta.1` is pre-release, `v1.2.3`
   is stable. Use `contains(github.ref_name, '-')` or shell pattern matching, not regex on the version number alone.

1. **`--generate-notes` and `--notes` can be combined** -- custom notes appear first, auto-generated notes are appended.
   Use `--notes-start-tag` to control the comparison range.

1. **Draft releases do not trigger `release: published` events** -- only publishing the draft triggers the event. This
   is the intended behavior for draft-then-publish workflows.

1. **Asset filenames must be unique within a release** -- uploading a file with the same name replaces the existing
   asset only with `--clobber`. Without it, the upload fails.

1. **Tag-triggered workflows should validate the tag format** -- use `if: startsWith(github.ref, 'refs/tags/v')` to
   ensure the workflow only runs on version tags, not arbitrary tags.

1. **`gh release create` with `--target` creates the tag if it does not exist** -- this is useful for CI but can cause
   confusion if the tag already exists on a different commit.
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
