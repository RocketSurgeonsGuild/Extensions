---
name: dotnet-benchmarkdotnet
category: performance
subcategory: benchmarking
description: Runs BenchmarkDotNet microbenchmarks. Setup, memory diagnosers, baselines, result analysis.
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

# dotnet-benchmarkdotnet

Microbenchmarking guidance for .NET using BenchmarkDotNet v0.14+. Covers benchmark class setup, memory and disassembly
diagnosers, exporters for CI artifact collection, baseline comparisons, and common pitfalls that invalidate
measurements.

**Version assumptions:** BenchmarkDotNet v0.14+ on .NET 8.0+ baseline. Examples use current stable APIs.

## Scope

- Benchmark class setup and configuration
- Memory and disassembly diagnosers
- Exporters for CI artifact collection
- Baseline comparisons and result analysis
- Common pitfalls that invalidate measurements
- Parameterized benchmarks with [Params] and benchmark categories

## Out of scope

- Performance architecture patterns (Span<T>, ArrayPool, sealed) -- see [skill:dotnet-performance-patterns]
- Profiling tools (dotnet-counters, dotnet-trace, dotnet-dump) -- see [skill:dotnet-profiling]
- CI benchmark regression detection -- see [skill:dotnet-ci-benchmarking]
- Native AOT compilation and performance -- see [skill:dotnet-native-aot]
- Serialization format performance -- see [skill:dotnet-serialization]
- Architecture patterns (caching, resilience) -- see [skill:dotnet-architecture-patterns]
- GC tuning and memory management -- see [skill:dotnet-gc-memory]

Cross-references: [skill:dotnet-performance-patterns] for zero-allocation patterns measured by benchmarks,
[skill:dotnet-csharp-modern-patterns] for Span/Memory syntax foundation, [skill:dotnet-csharp-coding-standards] for
sealed class style conventions, [skill:dotnet-native-aot] for AOT performance characteristics and benchmark
considerations, [skill:dotnet-serialization] for serialization format performance tradeoffs.

---

## Package Setup

````xml

<!-- Benchmarks.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.14.*" />
  </ItemGroup>
</Project>

```text

Keep benchmark projects separate from production code. Use a `benchmarks/` directory at the solution root.

---

## Benchmark Class Setup

### Basic Benchmark with [Benchmark] Attribute

```csharp

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class StringConcatBenchmarks
{
    private readonly string[] _items = Enumerable.Range(0, 100)
        .Select(i => i.ToString())
        .ToArray();

    [Benchmark(Baseline = true)]
    public string StringConcat()
    {
        var result = string.Empty;
        foreach (var item in _items)
            result += item;
        return result;
    }

    [Benchmark]
    public string StringBuilder()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var item in _items)
            sb.Append(item);
        return sb.ToString();
    }

    [Benchmark]
    public string StringJoin() => string.Join(string.Empty, _items);
}

```text

### Running Benchmarks

```csharp

// Program.cs
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<StringConcatBenchmarks>();

```csharp

Run in Release mode (mandatory for valid results):

```bash

dotnet run -c Release

```bash

### Parameterized Benchmarks

```csharp

[MemoryDiagnoser]
public class CollectionBenchmarks
{
    [Params(10, 100, 1000)]
    public int Size { get; set; }

    private int[] _data = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, Size).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int ForLoop()
    {
        var sum = 0;
        for (var i = 0; i < _data.Length; i++)
            sum += _data[i];
        return sum;
    }

    [Benchmark]
    public int LinqSum() => _data.Sum();
}

```text

---

## Memory Diagnosers

### MemoryDiagnoser

Tracks GC allocations and collection counts per benchmark invocation. Apply at class level to all benchmarks:

```csharp

[MemoryDiagnoser]
public class AllocationBenchmarks
{
    [Benchmark]
    public byte[] AllocateArray() => new byte[1024];

    [Benchmark]
    public int UseStackalloc()
    {
        Span<byte> buffer = stackalloc byte[1024];
        buffer[0] = 42;
        return buffer[0];
    }
}

```text

Output columns:

| Column      | Meaning                                  |
| ----------- | ---------------------------------------- |
| `Allocated` | Bytes allocated per operation            |
| `Gen0`      | Gen 0 GC collections per 1000 operations |
| `Gen1`      | Gen 1 GC collections per 1000 operations |
| `Gen2`      | Gen 2 GC collections per 1000 operations |

Zero in `Allocated` column confirms zero-allocation code paths.

### DisassemblyDiagnoser

Inspects JIT-compiled assembly to verify optimizations (devirtualization, inlining):

```csharp

[DisassemblyDiagnoser(maxDepth: 2)]
[MemoryDiagnoser]
public class DevirtualizationBenchmarks
{
    // sealed enables JIT devirtualization -- verify in disassembly output
    // See [skill:dotnet-csharp-coding-standards] for sealed class conventions
    [Benchmark]
    public int SealedCall()
    {
        var obj = new SealedService();
        return obj.Calculate(42);
    }

    [Benchmark]
    public int VirtualCall()
    {
        IService obj = new SealedService();
        return obj.Calculate(42);
    }
}

public interface IService { int Calculate(int x); }
public sealed class SealedService : IService
{
    public int Calculate(int x) => x * 2;
}

```text

Use `DisassemblyDiagnoser` to verify that `sealed` classes receive devirtualization from the JIT, confirming the
performance rationale documented in [skill:dotnet-csharp-coding-standards].

---

## Exporters for CI Integration

### Configuring Exporters

```csharp

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;

[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[HtmlExporter]
[MarkdownExporter]
public class CiBenchmarks
{
    [Benchmark]
    public void MyOperation()
    {
        // benchmark code
    }
}

```text

### Exporter Output

| Exporter                     | File                                                   | Use Case                                    |
| ---------------------------- | ------------------------------------------------------ | ------------------------------------------- |
| `JsonExporterAttribute.Full` | `BenchmarkDotNet.Artifacts/results/*-report-full.json` | CI regression comparison (machine-readable) |
| `HtmlExporter`               | `BenchmarkDotNet.Artifacts/results/*-report.html`      | Human-readable PR review artifact           |
| `MarkdownExporter`           | `BenchmarkDotNet.Artifacts/results/*-report-github.md` | Paste into PR comments                      |

### Custom Config for CI

```csharp

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(Job.ShortRun)  // fewer iterations for CI speed
    .AddExporter(JsonExporter.Full)
    .WithArtifactsPath("./benchmark-results");

BenchmarkRunner.Run<CiBenchmarks>(config);

```json

### GitHub Actions Artifact Upload

```yaml

- name: Run benchmarks
  run: dotnet run -c Release --project benchmarks/MyBenchmarks.csproj

- name: Upload benchmark results
  uses: actions/upload-artifact@v4
  with:
    name: benchmark-results
    path: benchmarks/BenchmarkDotNet.Artifacts/results/
    retention-days: 30

```text

---

## Baseline Comparison

### Setting a Baseline

Mark one benchmark as the baseline for ratio comparison:

```csharp

[MemoryDiagnoser]
public class SerializationBenchmarks
{
    // Serialization format choice -- see [skill:dotnet-serialization] for API details
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly WeatherForecast _data = new()
    {
        Date = DateOnly.FromDateTime(DateTime.Now),
        TemperatureC = 25,
        Summary = "Warm"
    };

    [Benchmark(Baseline = true)]
    public string SystemTextJson()
        => System.Text.Json.JsonSerializer.Serialize(_data, _options);

    [Benchmark]
    public byte[] Utf8Serialization()
        => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_data, _options);
}

public record WeatherForecast
{
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
}

```text

The `Ratio` column in output shows performance relative to the baseline (1.00). Values below 1.00 indicate faster than
baseline; above 1.00 indicate slower.

### Benchmark Categories

Group benchmarks with `[BenchmarkCategory]` and filter at runtime:

```csharp

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CategorizedBenchmarks
{
    [Benchmark, BenchmarkCategory("Serialization")]
    public string JsonSerialize() => "...";

    [Benchmark, BenchmarkCategory("Allocation")]
    public byte[] ArrayAlloc() => new byte[1024];
}

```text

Run a specific category:

```bash

dotnet run -c Release -- --filter *Serialization*

```bash

---

## BenchmarkRunner.Run Patterns

### Running Specific Benchmarks

```csharp

// Run a single benchmark class
BenchmarkRunner.Run<StringConcatBenchmarks>();

// Run all benchmarks in assembly
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
