
Found 1 unique root(s).

```text

**Common root types and their meaning:**

| Root Type | Meaning | Likely Issue |
|-----------|---------|-------------|
| `strong handle` | Static field or GC handle | Static collection growing without eviction |
| `pinned handle` | Pinned for native interop | Buffer pinned longer than needed |
| `async state machine` | Captured in async closure | Long-running async operation holding references |
| `finalizer queue` | Waiting for finalizer thread | Finalizer backlog blocking collection |
| `threadpool` | Referenced from thread-local storage | Thread-static cache without cleanup |

### !finalizequeue -- Finalizer Queue Analysis

Shows objects waiting for finalization, which delays their collection by at least one GC cycle:

```text

> finalizequeue

SyncBlocks to be cleaned up: 0
Free-Threaded Interfaces to be released: 0
MTA Interfaces to be released: 0
STA Interfaces to be released: 0
----------------------------------
generation 0 has 12 finalizable objects
generation 1 has 45 finalizable objects
generation 2 has 230 finalizable objects
Ready for finalization 8 objects

```text

**Key indicators:**

- High count in "Ready for finalization" means the finalizer thread is falling behind
- Objects in Gen 2 finalizable list are expensive -- they survive two GC cycles minimum (one to schedule finalization, one to collect after finalization runs)
- Types implementing `~Destructor()` without `IDisposable.Dispose()` being called are the primary cause

### Additional SOS Commands for Heap Analysis

| Command | Purpose | When to Use |
|---------|---------|-------------|
| `dumpobj <address>` | Display field values of a specific object | Inspect object state after finding it with dumpheap |
| `dumparray <address>` | Display array contents | Investigate large arrays found in heap stats |
| `eeheap -gc` | Show GC heap segment layout | Investigate LOH fragmentation |
| `gcwhere <address>` | Show which GC generation holds an object | Determine if an object is pinned or in LOH |
| `dumpmt <MT>` | Display method table details | Investigate type metadata |
| `threads` | List all managed threads with stack traces | Identify deadlocks or blocking |
| `clrstack` | Display managed call stack for current thread | Correlate thread state with heap data |

### Memory Leak Investigation Workflow

1. **Baseline:** Capture a dump after application startup and initial warm-up
2. **Load:** Run the workload scenario suspected of leaking
3. **Compare:** Capture a second dump after the workload completes
4. **Diff:** Compare `dumpheap -stat` output between the two dumps -- look for types whose count or total size grew significantly
5. **Root:** Use `gcroot` on instances of the growing type to find the retention chain
6. **Fix:** Break the retention chain (remove from static collections, dispose event subscriptions, fix async lifetime issues)

```bash

# Tip: save dumpheap output for comparison
# In dump 1:
> dumpheap -stat > /tmp/heap-before.txt
# In dump 2:
> dumpheap -stat > /tmp/heap-after.txt
# Compare externally:
# diff /tmp/heap-before.txt /tmp/heap-after.txt

```text

---

## Profiling Workflow Summary

Use the diagnostic tools in a structured investigation workflow:

```text

1. dotnet-counters (triage)
   ├── CPU high?         → dotnet-trace --profile cpu-sampling
   │                       → Convert to flame graph (Speedscope)
   │                       → Identify hot methods
   ├── Memory growing?   → dotnet-dump collect
   │                       → dumpheap -stat (find large/numerous types)
   │                       → gcroot (find retention chains)
   │                       → Fix retention + verify with second dump
   ├── GC pressure?      → dotnet-trace --profile gc-collect
   │                       → Identify allocation hot paths
   │                       → Apply zero-alloc patterns [skill:dotnet-performance-patterns]
   └── Thread starvation? → dotnet-dump analyze
                            → threads (list all managed threads)
                            → clrstack (check for blocking calls)

```text

After profiling identifies the bottleneck, use [skill:dotnet-benchmarkdotnet] to create targeted benchmarks that quantify the improvement from fixes.

---

## Agent Gotchas

1. **Start with dotnet-counters, not dotnet-trace** -- counters have near-zero overhead and identify the category of problem (CPU, memory, threads). Only reach for trace or dump after counters narrow the investigation.
2. **Use CPU sampling (not instrumentation) in production** -- sampling overhead is 2-5% and safe for production. Instrumentation adds 10-50%+ overhead and should be limited to development environments.
3. **Always convert traces to flame graphs for analysis** -- reading raw `.nettrace` event logs is impractical. Use `dotnet-trace convert --format Speedscope` and open in https://www.speedscope.app/ for visual analysis.
4. **Capture two dumps for leak investigation** -- a single dump shows current state but cannot distinguish normal resident objects from leaked ones. Compare heap statistics across two dumps taken before and after the suspected leak scenario.
5. **Filter dumpheap by `-min 85000` to find LOH objects** -- objects >= 85,000 bytes go to the Large Object Heap, which is only collected in Gen 2 GC. Large LOH counts indicate potential fragmentation.
6. **Interpret GC counter data with [skill:dotnet-observability]** -- runtime GC/threadpool counters overlap with OpenTelemetry metrics. Use the observability skill for correlating profiling findings with distributed trace context.
7. **Do not confuse dotnet-trace gc-collect with dotnet-dump** -- gc-collect traces allocation events over time (which methods allocate); dotnet-dump captures a point-in-time heap snapshot (what objects exist). Use gc-collect for allocation rate analysis; use dotnet-dump for retention/leak analysis.
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
