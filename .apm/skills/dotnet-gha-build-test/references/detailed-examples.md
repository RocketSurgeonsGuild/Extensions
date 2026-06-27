
For large test suites, split test projects across parallel runners to reduce total CI time:

```yaml

jobs:
  discover:
    runs-on: ubuntu-latest
    outputs:
      projects: ${{ steps.find.outputs.projects }}
    steps:
      - uses: actions/checkout@v4
      - id: find
        shell: bash
        run: |
          set -euo pipefail
          PROJECTS=$(find tests -name '*.csproj' | jq -R . | jq -sc .)
          echo "projects=$PROJECTS" >> "$GITHUB_OUTPUT"

  test:
    needs: discover
    strategy:
      fail-fast: false
      matrix:
        project: ${{ fromJson(needs.discover.outputs.projects) }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Test ${{ matrix.project }}
        run: |
          set -euo pipefail
          dotnet test ${{ matrix.project }} \
            --configuration Release \
            --logger "trx;LogFileName=results.trx" \
            --results-directory ./test-results

      - name: Publish test results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: 'Tests - ${{ matrix.project }}'
          path: 'test-results/**/*.trx'
          reporter: dotnet-trx

```text

### Sharding by Test Class Within a Project

For a single large test project, use `dotnet test --filter` to split by namespace:

```yaml

jobs:
  test:
    strategy:
      fail-fast: false
      matrix:
        shard: ['Unit', 'Integration', 'EndToEnd']
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Test ${{ matrix.shard }}
        run: |
          set -euo pipefail
          dotnet test tests/MyApp.Tests.csproj \
            --configuration Release \
            --filter "FullyQualifiedName~${{ matrix.shard }}" \
            --logger "trx;LogFileName=${{ matrix.shard }}-results.trx" \
            --results-directory ./test-results

```text

---

## Agent Gotchas

1. **Always set `set -euo pipefail` in multi-line bash `run` blocks** -- without `pipefail`, piped commands that fail do
   not propagate the error, producing false-green CI.
2. **Use `continue-on-error: true` on the test step, not on the reporter** -- the test step must not fail the job
   prematurely so the reporter can publish results, but the reporter should fail the check when tests fail.
3. **Include `runner.os` in NuGet cache keys** -- NuGet packages have OS-specific native assets; cross-OS cache hits
   cause restore failures.
4. **Install all required SDK versions for multi-TFM** -- `dotnet test` without the matching SDK produces `NETSDK1045`;
   list every required version in `dotnet-version`.
5. **Do not hardcode TFM strings in workflow files** -- use matrix variables to keep workflow files in sync with project
   configuration; hardcoded `net8.0` in CI breaks when the project moves to `net9.0`.
6. **Coverage collection requires `--collect:"XPlat Code Coverage"`** -- the default `dotnet test` does not produce
   coverage files; the `XPlat Code Coverage` collector is built into the .NET SDK.
7. **TRX logger path must match reporter glob** -- if the logger writes to `test-results/results.trx`, the reporter
   `path` must include that directory in its glob pattern.
8. **Never commit NuGet credentials to workflow files** -- use `${{ secrets.* }}` references for all authentication
   tokens; the `NUGET_AUTH_TOKEN` environment variable is the standard pattern.
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
