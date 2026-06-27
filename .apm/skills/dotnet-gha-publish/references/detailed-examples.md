    NamespaceUriBase: https://github.com/${{ github.repository }}

- name: Upload SBOM
  uses: actions/upload-artifact@v4
  with:
    name: sbom-${{ steps.version.outputs.version }}
    path: ./nupkgs/_manifest/
    retention-days: 365

```text

### SBOM for Container Images

```yaml

- name: Generate container SBOM
  uses: anchore/sbom-action@v0
  with:
    image: ghcr.io/${{ github.repository }}:${{ steps.version.outputs.version }}
    artifact-name: container-sbom
    output-file: container-sbom.spdx.json

```json

### Attach SBOM to GitHub Release

```yaml

- name: Create GitHub Release with SBOM
  uses: softprops/action-gh-release@v2
  with:
    files: |
      ./nupkgs/*.nupkg
      ./nupkgs/_manifest/spdx_2.2/manifest.spdx.json
    generate_release_notes: true

```json

---

## Conditional Publishing on Tags and Releases

### Tag Pattern Matching

```yaml

on:
  push:
    tags:
      - 'v[0-9]+.[0-9]+.[0-9]+' # stable: v1.2.3
      - 'v[0-9]+.[0-9]+.[0-9]+-*' # pre-release: v1.2.3-preview.1

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - name: Determine release type
        id: release-type
        shell: bash
        run: |
          set -euo pipefail
          VERSION="${GITHUB_REF_NAME#v}"
          if [[ "$VERSION" == *-* ]]; then
            echo "prerelease=true" >> "$GITHUB_OUTPUT"
          else
            echo "prerelease=false" >> "$GITHUB_OUTPUT"
          fi
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"

```text

### Release-Triggered Publishing

Publish only when a GitHub Release is created (provides manual approval gate):

```yaml

on:
  release:
    types: [published]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          ref: ${{ github.event.release.tag_name }}

      - name: Extract version
        id: version
        shell: bash
        run: |
          set -euo pipefail
          VERSION="${{ github.event.release.tag_name }}"
          VERSION="${VERSION#v}"
          echo "version=$VERSION" >> "$GITHUB_OUTPUT"

      - name: Pack and publish
        run: |
          set -euo pipefail
          dotnet pack -c Release -p:Version=${{ steps.version.outputs.version }} -o ./nupkgs
          dotnet nuget push ./nupkgs/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate

```json

---

## Agent Gotchas

1. **Always use `--skip-duplicate` with `dotnet nuget push`** -- without it, re-running a publish workflow for an
   already-published version fails the job instead of being idempotent.
2. **Never hardcode API keys in workflow files** -- use `${{ secrets.NUGET_API_KEY }}` or environment-scoped secrets for
   all credentials.
3. **Use `set -euo pipefail` in all multi-line bash steps** -- without `pipefail`, a failure in a piped command does not
   propagate, producing false-green CI.
4. **Clean up signing certificates in an `if: always()` step** -- temporary files with private key material must be
   removed even when the job fails.
5. **SDK container publish requires Docker daemon** -- `dotnet publish` with `PublishProfile=DefaultContainer` needs
   Docker installed on the runner; use `ubuntu-latest` which includes Docker.
6. **AOT publish requires matching RID** -- `dotnet publish -r linux-x64` must match the runner OS; do not use
   `-r win-x64` on `ubuntu-latest`.
7. **Tag-triggered workflows do not run on pull requests** -- tags pushed from PRs still trigger the workflow; use
   `if: github.ref_type == 'tag'` as an extra guard if needed.
8. **GHCR authentication uses `GITHUB_TOKEN`, not a PAT** -- for public repositories, `packages: write` permission is
   sufficient; PATs are only needed for cross-repository access.
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
