---

## Alerting Strategies

### PR Comment with Regression Summary

Post benchmark comparison results as a PR comment for reviewer visibility:

```yaml

- name: Comment PR with results
  if: steps.download-baseline.outcome == 'success' && github.event_name == 'pull_request'
  uses: actions/github-script@v7
  with:
    script: |
      const fs = require('fs');
      const body = fs.readFileSync('benchmark-comparison.md', 'utf8');
      await github.rest.issues.createComment({
        owner: context.repo.owner,
        repo: context.repo.repo,
        issue_number: context.issue.number,
        body: body
      });

```text

### Fail the Build on Regression

Exit with non-zero status from the comparison script to fail the GitHub Actions job. This prevents merging PRs that
introduce performance regressions:

```yaml

- name: Check for regressions
  if: steps.download-baseline.outcome == 'success'
  shell: bash
  run: |
    set -euo pipefail
    python3 scripts/compare-benchmarks.py \
      --baseline ./baseline-results \
      --current "${{ env.RESULTS_DIR }}" \
      --threshold 10
    # Script exits non-zero if regressions found -- fails the job

```text

For required status checks and branch protection integration with benchmark gates, see [skill:dotnet-gha-patterns].

### Trend Tracking

For long-term trend analysis beyond single-PR comparison, upload results to a persistent store and track metrics over
time:

| Approach                           | Tool                                          | Complexity                            |
| ---------------------------------- | --------------------------------------------- | ------------------------------------- |
| GitHub Actions artifacts           | Built-in, 90-day retention                    | Low -- artifact download/upload only  |
| GitHub Pages with benchmark-action | `benchmark-action/github-action-benchmark@v1` | Medium -- auto-generates trend charts |
| External time-series DB            | InfluxDB, Prometheus + Grafana                | High -- full observability stack      |

The simplest approach for most projects is the artifact-based baseline comparison shown in this skill. Graduate to trend
tracking when you need historical regression analysis across many releases.

---

## CI-Specific BenchmarkDotNet Configuration

### ShortRun for CI Speed

Full benchmark runs take 10-30+ minutes. Use `Job.ShortRun` in CI to reduce iteration counts while retaining regression
detection capability:

```csharp

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

public class CiConfig : ManualConfig
{
    public CiConfig()
    {
        AddJob(Job.ShortRun
            .WithWarmupCount(3)
            .WithIterationCount(5)
            .WithInvocationCount(1));

        AddExporter(BenchmarkDotNet.Exporters.Json.JsonExporter.Full);
    }
}

```json

Apply conditionally based on environment:

```csharp

var config = Environment.GetEnvironmentVariable("CI") is not null
    ? new CiConfig()
    : DefaultConfig.Instance;

BenchmarkRunner.Run<CriticalPathBenchmarks>(config);

```text

### Filtering Benchmarks for CI

Run only critical-path benchmarks in CI to reduce pipeline duration:

```bash

# Run only benchmarks in the "Critical" category
dotnet run -c Release --project benchmarks/MyBenchmarks.csproj -- \
  --filter *Critical* --exporters json

```bash

```csharp

[BenchmarkCategory("Critical")]
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
public class CriticalPathBenchmarks
{
    [Benchmark]
    public void ProcessOrder() { /* ... */ }
}

[BenchmarkCategory("Extended")]
[MemoryDiagnoser]
public class ExtendedBenchmarks
{
    [Benchmark]
    public void RareCodePath() { /* ... */ }
}

```text

Run `Critical` benchmarks on every PR; run `Extended` benchmarks on a nightly schedule.

### Nightly Benchmark Schedule

```yaml

name: Nightly Benchmarks (Full Suite)

on:
  schedule:
    - cron: '0 3 * * *' # 3 AM UTC daily
  workflow_dispatch:

jobs:
  benchmark-full:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Run full benchmark suite
        run: dotnet run -c Release --project benchmarks/MyBenchmarks.csproj -- --exporters json
        # No --filter: runs all benchmarks including Extended category

      - name: Upload full results
        uses: actions/upload-artifact@v4
        with:
          name: benchmark-full-${{ github.run_number }}
          path: benchmarks/BenchmarkDotNet.Artifacts/results/
          retention-days: 90

```text

For scheduled workflow patterns and matrix builds across TFMs, see [skill:dotnet-gha-patterns].

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
## Agent Gotchas

1. **Use `Job.ShortRun` in CI, not `Job.Default`** -- default benchmark jobs run many iterations for statistical
   precision, taking 10-30+ minutes per benchmark class. CI pipelines need faster feedback with `ShortRun` (3 warmup, 5
   iteration).
2. **Set threshold above measured noise floor** -- shared CI runners introduce 5-10% timing variance from noisy
   neighbors. A 5% threshold on shared runners produces false positives. Calibrate by running the same code multiple
   times and measuring variance.
3. **Use allocation changes as hard gates** -- allocation counts are deterministic and unaffected by runner noise. A
   zero-to-nonzero allocation change is always a real regression, unlike timing variations.
4. **Only update baselines from main branch** -- if PR branches can update the baseline, a regression in one PR becomes
   the new baseline, masking it from subsequent comparisons.
5. **Always set `set -euo pipefail` in bash steps** -- without `pipefail`, a regression detection script that exits
   non-zero in a pipeline (e.g., `script | tee`) does not fail the GitHub Actions step.
6. **Handle missing baselines gracefully** -- the first CI run has no baseline to compare against. Use
   `continue-on-error: true` on the baseline download step and skip comparison when no baseline exists.
7. **Export JSON, not just Markdown** -- Markdown reports are human-readable but not machine-parseable for automated
   regression detection. Always include `[JsonExporterAttribute.Full]` or `JsonExporter.Full` in the config.
````
