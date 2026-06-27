BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

```text

### Command-Line Filtering

```bash

# Run benchmarks matching a pattern
dotnet run -c Release -- --filter *StringBuilder*

# List all available benchmarks without running
dotnet run -c Release -- --list flat

# Dry run (validates setup without full benchmark)
dotnet run -c Release -- --filter *StringBuilder* --job Dry

```text

### AOT Benchmark Considerations

When benchmarking Native AOT scenarios, the JIT diagnosers are not available (there is no JIT). Use wall-clock time and
memory comparisons instead. See [skill:dotnet-native-aot] for AOT compilation setup:

```csharp

[MemoryDiagnoser]
// Do NOT use DisassemblyDiagnoser with AOT -- no JIT to disassemble
public class AotBenchmarks
{
    [Benchmark]
    public string SourceGenSerialize()
        => System.Text.Json.JsonSerializer.Serialize(
            new { Value = 42 },
            AppJsonContext.Default.Options);
}

```json

---

## Common Pitfalls

### Dead Code Elimination

The JIT may eliminate benchmark code whose result is unused. Always **return** or **consume** the result:

```csharp

// BAD: JIT may eliminate the entire loop
[Benchmark]
public void DeadCode()
{
    var sum = 0;
    for (var i = 0; i < 1000; i++)
        sum += i;
    // sum is never used -- JIT removes the loop
}

// GOOD: return the value to prevent elimination
[Benchmark]
public int LiveCode()
{
    var sum = 0;
    for (var i = 0; i < 1000; i++)
        sum += i;
    return sum;
}

```text

### Measurement Bias

| Pitfall                | Cause                                        | Fix                                                                                                    |
| ---------------------- | -------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| Running in Debug mode  | No JIT optimizations applied                 | Always use `-c Release`                                                                                |
| Shared mutable state   | Benchmarks interfere with each other         | Use `[IterationSetup]` or immutable data                                                               |
| Cold-start measurement | First run includes JIT compilation           | BenchmarkDotNet handles warmup automatically -- do not add manual warmup                               |
| Allocations in setup   | Setup allocations inflate `Allocated` column | Use `[GlobalSetup]` (runs once) vs `[IterationSetup]` (runs per iteration)                             |
| Environment noise      | Background processes skew results            | BenchmarkDotNet detects and warns about environment issues; use `Job.MediumRun` for noisy environments |

### Setup vs Iteration Lifecycle

```csharp

[MemoryDiagnoser]
public class LifecycleBenchmarks
{
    private byte[] _data = null!;

    [GlobalSetup]    // Runs once before all benchmark iterations
    public void GlobalSetup() => _data = new byte[1024];

    [IterationSetup] // Runs before each benchmark iteration
    public void IterationSetup() => Array.Fill(_data, (byte)0);

    [Benchmark]
    public int Process()
    {
        // uses _data
        return _data.Length;
    }

    [GlobalCleanup]    // Runs once after all iterations
    public void GlobalCleanup() { /* dispose resources */ }
}

```text

Prefer `[GlobalSetup]` over `[IterationSetup]` unless the benchmark mutates shared state. `[IterationSetup]` adds
overhead that BenchmarkDotNet excludes from timing, but it still affects GC pressure measurement.

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

1. **Always run benchmarks in Release mode** -- `dotnet run -c Release`. Debug mode disables JIT optimizations and
   produces meaningless results.
2. **Never benchmark in a test project** -- xUnit/NUnit test runners interfere with BenchmarkDotNet's measurement
   harness. Use a standalone console project.
3. **Return values from benchmark methods** to prevent dead code elimination. The JIT will remove computation whose
   result is discarded.
4. **Do not add manual Thread.Sleep or Task.Delay in benchmarks** -- BenchmarkDotNet manages warmup and iteration timing
   automatically.
5. **Use `[GlobalSetup]` not constructor** for initialization -- BenchmarkDotNet creates benchmark instances multiple
   times during a run; constructor code runs repeatedly.
6. **Prefer `[Params]` over manual loops** for parameterized benchmarks. BenchmarkDotNet runs each parameter combination
   independently with proper statistical analysis.
7. **Export JSON for CI** -- use `[JsonExporterAttribute.Full]` to produce machine-readable artifacts for regression
   detection, not just Markdown.
````
