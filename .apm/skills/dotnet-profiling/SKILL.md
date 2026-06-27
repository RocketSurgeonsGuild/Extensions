---
name: dotnet-profiling
category: performance
subcategory: profiling
description: Diagnoses .NET performance issues. dotnet-counters, dotnet-trace, dotnet-dump, flame graphs.
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

# dotnet-profiling

Diagnostic tool guidance for investigating .NET performance problems. Covers real-time metric monitoring with
dotnet-counters, event tracing and flame graph generation with dotnet-trace, and memory dump capture and analysis with
dotnet-dump. Focuses on interpreting profiling data (reading flame graphs, analyzing heap dumps, correlating GC metrics)
rather than just invoking tools.

**Version assumptions:** .NET SDK 8.0+ baseline. All three diagnostic tools (dotnet-counters, dotnet-trace, dotnet-dump)
ship with the .NET SDK -- no separate installation required.

## Scope

- Real-time metric monitoring with dotnet-counters
- Event tracing and flame graph generation with dotnet-trace
- Memory dump capture and analysis with dotnet-dump
- Interpreting profiling data (flame graphs, heap dumps, GC metrics)

## Out of scope

- OpenTelemetry metrics and distributed tracing -- see [skill:dotnet-observability]
- Microbenchmarking setup (BenchmarkDotNet) -- see [skill:dotnet-benchmarkdotnet]
- Performance architecture patterns (Span<T>, ArrayPool, sealed) -- see [skill:dotnet-performance-patterns]
- Continuous benchmark regression detection in CI -- see [skill:dotnet-ci-benchmarking]
- Architecture patterns (caching, resilience) -- see [skill:dotnet-architecture-patterns]

Cross-references: [skill:dotnet-observability] for GC/threadpool metrics interpretation and OpenTelemetry correlation,
[skill:dotnet-benchmarkdotnet] for structured benchmarking after profiling identifies hot paths,
[skill:dotnet-performance-patterns] for optimization patterns to apply based on profiling results.

---

## dotnet-counters -- Real-Time Metric Monitoring

### Overview

`dotnet-counters` provides real-time monitoring of .NET runtime metrics without modifying application code. Use it as a
first-pass triage tool to identify whether a performance problem is CPU-bound, memory-bound, or I/O-bound before
reaching for heavier instrumentation.

### Monitoring Running Processes

````bash

# List running .NET processes
dotnet-counters ps

# Monitor default runtime counters for a process
dotnet-counters monitor --process-id <PID>

# Monitor with a specific refresh interval (seconds)
dotnet-counters monitor --process-id <PID> --refresh-interval 2

```text

### Key Built-In Counter Providers

| Provider | Counters | What It Tells You |
|----------|----------|-------------------|
| `System.Runtime` | CPU usage, GC heap size, Gen 0/1/2 collections, threadpool queue length, exception count | Overall runtime health |
| `Microsoft.AspNetCore.Hosting` | Request rate, request duration, active requests | HTTP request throughput and latency |
| `Microsoft.AspNetCore.Http.Connections` | Connection duration, current connections | WebSocket/SignalR connection load |
| `System.Net.Http` | Requests started/failed, active requests, connection pool size | Outbound HTTP client behavior |
| `System.Net.Sockets` | Bytes sent/received, datagrams, connections | Network I/O volume |

### Monitoring Specific Providers

```bash

# Monitor runtime and ASP.NET counters together
dotnet-counters monitor --process-id <PID> \
  --counters System.Runtime,Microsoft.AspNetCore.Hosting

# Monitor only GC-related counters
dotnet-counters monitor --process-id <PID> \
  --counters System.Runtime[gc-heap-size,gen-0-gc-count,gen-1-gc-count,gen-2-gc-count]

```text

### Custom EventCounters

Applications can publish custom counters for domain-specific metrics:

```csharp

using System.Diagnostics.Tracing;

[EventSource(Name = "MyApp.Orders")]
public sealed class OrderMetrics : EventSource
{
    public static readonly OrderMetrics Instance = new();

    private EventCounter? _orderProcessingTime;
    private IncrementingEventCounter? _ordersProcessed;

    private OrderMetrics()
    {
        _orderProcessingTime = new EventCounter("order-processing-time", this)
        {
            DisplayName = "Order Processing Time (ms)",
            DisplayUnits = "ms"
        };
        _ordersProcessed = new IncrementingEventCounter("orders-processed", this)
        {
            DisplayName = "Orders Processed",
            DisplayRateTimeScale = TimeSpan.FromSeconds(1)
        };
    }

    public void RecordProcessingTime(double milliseconds)
        => _orderProcessingTime?.WriteMetric(milliseconds);

    public void RecordOrderProcessed()
        => _ordersProcessed?.Increment();

    protected override void Dispose(bool disposing)
    {
        _orderProcessingTime?.Dispose();
        _ordersProcessed?.Dispose();
        base.Dispose(disposing);
    }
}

```text

Monitor custom counters:

```bash

dotnet-counters monitor --process-id <PID> --counters MyApp.Orders

```bash

### Interpreting Counter Data

Use counter values to direct further investigation. See [skill:dotnet-observability] for correlating these runtime metrics with OpenTelemetry traces:

| Symptom | Counter Evidence | Next Step |
|---------|------------------|-----------|
| High CPU usage | `cpu-usage` > 80%, `threadpool-queue-length` low | CPU profiling with dotnet-trace |
| Memory growth | `gc-heap-size` increasing, frequent Gen 2 GC | Memory dump with dotnet-dump |
| Thread starvation | `threadpool-queue-length` growing, `threadpool-thread-count` at max | Check for sync-over-async or blocking calls |
| Request latency | `request-duration` high, `active-requests` normal | Trace individual requests with dotnet-trace |
| GC pauses | High `gen-2-gc-count`, `time-in-gc` > 10% | Allocation profiling with dotnet-trace gc-collect |

### Exporting Counter Data

```bash

# Export to CSV for analysis
dotnet-counters collect --process-id <PID> \
  --format csv \
  --output counters.csv \
  --counters System.Runtime

# Export to JSON for programmatic consumption
dotnet-counters collect --process-id <PID> \
  --format json \
  --output counters.json

```json

---

## dotnet-trace -- Event Tracing and Flame Graphs

### Overview

`dotnet-trace` captures detailed event traces from a running .NET process. Traces can be analyzed as flame graphs to identify CPU hot paths, or configured for allocation tracking to find GC pressure sources.

### CPU Sampling

CPU sampling records stack frames at a fixed interval to build a statistical profile of where the application spends time:

```bash

# Collect a CPU sampling trace (default profile)
dotnet-trace collect --process-id <PID> --duration 00:00:30

# Collect with the cpu-sampling profile (explicit)
dotnet-trace collect --process-id <PID> \
  --profile cpu-sampling \
  --output cpu-trace.nettrace

```text

### CPU Sampling vs Instrumentation

| Approach | Overhead | Best For | Tool |
|----------|----------|----------|------|
| CPU sampling | Low (~2-5%) | Finding CPU hot paths in production | dotnet-trace `--profile cpu-sampling` |
| Instrumentation | High (10-50%+) | Exact call counts, method entry/exit timing | Rider/VS profiler, PerfView |

CPU sampling is safe for production use due to low overhead. Use it as the default approach. Reserve instrumentation for development environments where exact call counts matter.

### Flame Graph Generation

Trace files (`.nettrace`) must be converted to a flame graph format for visual analysis:

**Using Speedscope (browser-based, recommended):**

```bash

# Convert to Speedscope format
dotnet-trace convert cpu-trace.nettrace --format Speedscope

# Opens cpu-trace.speedscope.json -- load at https://www.speedscope.app/

```json

**Using PerfView (Windows, deep .NET integration):**

```bash

# Convert to Chromium trace format (also viewable in chrome://tracing)
dotnet-trace convert cpu-trace.nettrace --format Chromium

```bash

### Reading Flame Graphs

Flame graphs display call stacks where:

- **Width** of a frame represents the proportion of total sample time spent in that function (wider = more time)
- **Height** represents call stack depth (taller stacks = deeper call chains)
- **Color** is typically arbitrary (not meaningful) unless the tool uses a specific color scheme

**Analysis workflow:**

1. Look for **wide plateaus** -- functions that consume a large proportion of samples
2. Follow the widest frames **upward** to find which callers contribute the most time
3. Identify **unexpected width** -- framework methods that should be fast appearing wide indicate misuse
4. Compare **before/after** traces to validate optimizations reduced the width of target functions

**Common patterns in .NET flame graphs:**

| Pattern | Likely Cause | Investigation |
|---------|-------------|---------------|
| Wide `System.Linq` frames | LINQ-heavy hot path with delegate overhead | Replace with foreach loops or Span-based processing |
| Wide `JIT_New` / `gc_heap::allocate` | Excessive allocations triggering GC | Allocation profiling with `--profile gc-collect` |
| Wide `Monitor.Enter` / `SpinLock` | Lock contention | Review synchronization strategy |
| Wide `System.Text.RegularExpressions` | Regex backtracking | Use `RegexOptions.NonBacktracking` or compile regex |
| Deep async state machine frames | Async overhead in tight loops | Consider sync path for CPU-bound work |

### Allocation Tracking with gc-collect Profile

The `gc-collect` profile captures allocation events to identify what code paths allocate the most memory:

```bash

# Collect allocation data
dotnet-trace collect --process-id <PID> \
  --profile gc-collect \
  --duration 00:00:30 \
  --output alloc-trace.nettrace

```text

This produces a trace that shows:

- Which methods allocate the most bytes
- Which types are allocated most frequently
- Allocation sizes and the call stacks that trigger them

Correlate allocation data with GC counter evidence from dotnet-counters. If `gen-2-gc-count` is high, the allocation trace shows which code paths produce long-lived objects that survive to Gen 2. See [skill:dotnet-performance-patterns] for zero-allocation patterns to apply once hot allocation sites are identified.

### Custom Trace Providers

Target specific event providers for focused tracing:

```bash

# Trace specific providers with keywords and verbosity
dotnet-trace collect --process-id <PID> \
  --providers "Microsoft-Diagnostics-DiagnosticSource:::FilterAndPayloadSpecs=[AS]System.Net.Http"

# Trace EF Core queries (useful with [skill:dotnet-efcore-patterns])
dotnet-trace collect --process-id <PID> \
  --providers Microsoft.EntityFrameworkCore

# Trace ASP.NET Core request processing
dotnet-trace collect --process-id <PID> \
  --providers Microsoft.AspNetCore

```text

### Trace File Management

| Format | Extension | Viewer | Cross-Platform |
|--------|-----------|--------|----------------|
| NetTrace | `.nettrace` | PerfView, VS, dotnet-trace convert | Yes (capture); Windows (PerfView) |
| Speedscope | `.speedscope.json` | https://www.speedscope.app/ | Yes |
| Chromium | `.chromium.json` | Chrome DevTools (chrome://tracing) | Yes |

---

## dotnet-dump -- Memory Dump Analysis

### Overview

`dotnet-dump` captures and analyzes process memory dumps. Use it to investigate memory leaks, large object heap fragmentation, and object reference chains. Unlike dotnet-trace, dumps capture a point-in-time snapshot of the entire managed heap.

### Capturing Dumps

```bash

# Capture a full heap dump
dotnet-dump collect --process-id <PID> --output app-dump.dmp

# Capture a minimal dump (faster, smaller, but less detail)
dotnet-dump collect --process-id <PID> --type Mini --output app-mini.dmp

```text

**When to capture:**

- Memory usage has grown beyond expected baseline (compare against dotnet-counters `gc-heap-size`)
- Application is approaching OOM conditions
- Suspected memory leak after load testing
- Investigating finalizer queue backlog

### Analyzing Dumps with SOS Commands

Open the dump in the interactive analyzer:

```bash

dotnet-dump analyze app-dump.dmp

```bash

### !dumpheap -- Heap Object Summary

Lists objects on the managed heap grouped by type, sorted by total size:

```text

> dumpheap -stat

Statistics:
              MT    Count    TotalSize Class Name
00007fff2c6a4320      125        4,000 System.String[]
00007fff2c6a1230    8,432      269,824 System.String
00007fff2c7b5640    2,100      504,000 MyApp.Models.OrderEntity
00007fff2c6a0988   15,230    1,218,400 System.Byte[]

```text

**Analysis approach:**

1. Look for unexpectedly high counts or sizes for application types
2. Compare counts against expected cardinality (e.g., 2,100 OrderEntity objects -- is that expected for current load?)
3. Large `System.Byte[]` counts often indicate unbounded buffering or stream handling issues

Filter by type:

```text

> dumpheap -type MyApp.Models.OrderEntity
> dumpheap -type System.Byte[] -min 85000

```text

The `-min 85000` filter shows Large Object Heap entries (objects >= 85,000 bytes that cause Gen 2 GC pressure).

### !gcroot -- Finding Object Retention

Traces the reference chain from a GC root to a specific object, explaining why it is not collected:

```text

> gcroot 00007fff3c4a2100

HandleTable:
    00007fff3c010010 (strong handle)
        -> 00007fff3c3a1000 MyApp.Services.CacheService
            -> 00007fff3c3a1020 System.Collections.Generic.Dictionary`2
                -> 00007fff3c4a2100 MyApp.Models.OrderEntity


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
