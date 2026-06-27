### Creating the Release

```yaml

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

```text

`generate_release_notes: true` auto-generates release notes from merged PRs and commit messages since the last tag.

---

## Automated Formula/Manifest PR Creation

### Homebrew Formula Update

After the GitHub Release is published, update the Homebrew tap automatically:

```yaml

update-homebrew:
  needs: release
  if: ${{ !contains(github.ref_name, '-') }}
  runs-on: ubuntu-latest
  steps:
    - name: Extract version
      id: version
      run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"

    - uses: actions/checkout@v4
      with:
        repository: myorg/homebrew-tap
        token: ${{ secrets.TAP_GITHUB_TOKEN }}

    - name: Download checksums
      run: |
        set -euo pipefail
        curl -sL "https://github.com/myorg/mytool/releases/download/v${{ steps.version.outputs.version }}/checksums-sha256.txt" \
          -o checksums.txt

    - name: Update formula
      run: |
        set -euo pipefail
        VERSION="${{ steps.version.outputs.version }}"
        LINUX_X64_SHA=$(grep "linux-x64" checksums.txt | awk '{print $1}')
        LINUX_ARM64_SHA=$(grep "linux-arm64" checksums.txt | awk '{print $1}')
        OSX_ARM64_SHA=$(grep "osx-arm64" checksums.txt | awk '{print $1}')

        # Use sed or a templating script to update Formula/mytool.rb
        # with new version and SHA-256 values
        python3 scripts/update-formula.py \
          --version "$VERSION" \
          --linux-x64-sha "$LINUX_X64_SHA" \
          --linux-arm64-sha "$LINUX_ARM64_SHA" \
          --osx-arm64-sha "$OSX_ARM64_SHA"

    - name: Create PR
      uses: peter-evans/create-pull-request@v6
      with:
        title: 'mytool ${{ steps.version.outputs.version }}'
        commit-message: 'Update mytool to ${{ steps.version.outputs.version }}'
        branch: 'update-mytool-${{ steps.version.outputs.version }}'
        body: |
          Automated update for mytool v${{ steps.version.outputs.version }}
          Release: https://github.com/myorg/mytool/releases/tag/v${{ steps.version.outputs.version }}

```text

### winget Manifest Update

```yaml

update-winget:
  needs: release
  if: ${{ !contains(github.ref_name, '-') }}
  runs-on: windows-latest
  steps:
    - name: Extract version
      id: version
      shell: bash
      run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"

    - name: Submit to winget-pkgs
      uses: vedantmgoyal9/winget-releaser@main
      with:
        identifier: MyOrg.MyTool
        version: ${{ steps.version.outputs.version }}
        installers-regex: '\.zip$'
        token: ${{ secrets.WINGET_GITHUB_TOKEN }}

```text

### Scoop Manifest Update

```yaml

update-scoop:
  needs: release
  if: ${{ !contains(github.ref_name, '-') }}
  runs-on: ubuntu-latest
  steps:
    - name: Extract version
      id: version
      run: echo "version=${GITHUB_REF_NAME#v}" >> "$GITHUB_OUTPUT"

    - uses: actions/checkout@v4
      with:
        repository: myorg/scoop-mytool
        token: ${{ secrets.SCOOP_GITHUB_TOKEN }}

    - name: Download checksums
      run: |
        set -euo pipefail
        curl -sL "https://github.com/myorg/mytool/releases/download/v${{ steps.version.outputs.version }}/checksums-sha256.txt" \
          -o checksums.txt

    - name: Update manifest
      run: |
        set -euo pipefail
        VERSION="${{ steps.version.outputs.version }}"
        WIN_X64_SHA=$(grep "win-x64" checksums.txt | awk '{print $1}')

        # Update bucket/mytool.json with new version and hash
        jq --arg v "$VERSION" --arg h "$WIN_X64_SHA" \
          '.version = $v | .architecture."64bit".hash = $h |
           .architecture."64bit".url = "https://github.com/myorg/mytool/releases/download/v\($v)/mytool-\($v)-win-x64.zip"' \
          bucket/mytool.json > tmp.json && mv tmp.json bucket/mytool.json

    - name: Create PR
      uses: peter-evans/create-pull-request@v6
      with:
        title: 'mytool ${{ steps.version.outputs.version }}'
        commit-message: 'Update mytool to ${{ steps.version.outputs.version }}'
        branch: 'update-mytool-${{ steps.version.outputs.version }}'

```text

---

## Versioning Strategy Details

### SemVer for CLI Tools

| Change Type                      | Version Bump       | Example        |
| -------------------------------- | ------------------ | -------------- |
| Breaking CLI flag rename/removal | Major              | 1.x.x -> 2.0.0 |
| New command or option            | Minor              | x.1.x -> x.2.0 |
| Bug fix, performance improvement | Patch              | x.x.1 -> x.x.2 |
| Release candidate                | Pre-release suffix | x.x.x-rc.1     |

### Version Embedding

The version flows from the git tag through `dotnet publish` into the binary:

```xml

<!-- .csproj -- Version is set at publish time via /p:Version -->
<PropertyGroup>
  <!-- Fallback version for local development -->
  <Version>0.0.0-dev</Version>
</PropertyGroup>

```text

```bash

# --version output matches the git tag
$ mytool --version
1.2.3

```bash

### Tagging Workflow

```bash

# 1. Update CHANGELOG.md (if applicable)
# 2. Commit the changelog
git commit -am "docs: update changelog for v1.2.3"

# 3. Tag the release
git tag -a v1.2.3 -m "Release v1.2.3"

# 4. Push tag -- triggers the release workflow
git push origin v1.2.3

```text

---

## Workflow Security

### Secret Management

```yaml

# Required repository secrets:
# NUGET_API_KEY         - NuGet.org API key for package publishing
# TAP_GITHUB_TOKEN      - PAT with repo scope for homebrew-tap
# WINGET_GITHUB_TOKEN   - PAT with public_repo scope for winget-pkgs PRs
# SCOOP_GITHUB_TOKEN    - PAT with repo scope for scoop bucket
# CHOCO_API_KEY         - Chocolatey API key for package push

```text

### Permissions

```yaml

permissions:
  contents: write # Minimum: create GitHub Releases and upload assets

```yaml

Use job-level permissions when different jobs need different scopes. Never grant `write-all`.

---

## Agent Gotchas

1. **Do not use `set -e` without `set -o pipefail` in GitHub Actions bash steps.** Without `pipefail`, a failing command
   piped to `tee` or another utility exits 0, masking the failure. Always use `set -euo pipefail`.
2. **Do not hardcode the .NET version in the publish path.** Use `dotnet publish -o ./publish` to control the output
   directory explicitly. Hardcoding `net8.0` in artifact paths breaks when upgrading to .NET 9+.
3. **Do not skip the pre-release detection step.** Package manager submissions (Homebrew, winget, Scoop, Chocolatey,
   NuGet) must be gated on stable versions. Publishing a `-rc.1` to winget-pkgs or NuGet as stable causes user
   confusion.
4. **Do not use `actions/upload-artifact` v3 with `merge-multiple`.** The `merge-multiple` parameter requires
   `actions/download-artifact@v4`. Using v3 silently ignores the flag and creates nested directories.
5. **Do not forget `retention-days: 1` on intermediate build artifacts.** Release artifacts are published to GitHub
   Releases (permanent). Workflow artifacts are temporary and should expire quickly to save storage.
6. **Do not create GitHub Releases with `gh release create` in a matrix job.** Only the release job (after all builds
   complete) should create the release. Matrix jobs upload artifacts; the release job assembles them.

---



## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**
- ✅ **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- ✅ **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- ✅ **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**
```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
## References

- [GitHub Actions workflow syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [softprops/action-gh-release](https://github.com/softprops/action-gh-release)
- [peter-evans/create-pull-request](https://github.com/peter-evans/create-pull-request)
- [vedantmgoyal9/winget-releaser](https://github.com/vedantmgoyal9/winget-releaser)
- [Semantic Versioning](https://semver.org/)
- [.NET versioning](https://learn.microsoft.com/en-us/dotnet/core/versions/)
- [GitHub Actions artifacts](https://docs.github.com/en/actions/using-workflows/storing-workflow-data-as-artifacts)
````
