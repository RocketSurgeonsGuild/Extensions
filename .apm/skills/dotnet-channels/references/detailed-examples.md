var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(1000)
{
    SingleReader = true,   // One consumer task
    SingleWriter = true,   // One producer task
    FullMode = BoundedChannelFullMode.Wait
});

```text

### WaitToReadAsync + TryRead Pattern

The most efficient consumer pattern. `WaitToReadAsync` suspends until data is available, then `TryRead` drains all
buffered items synchronously -- avoiding per-item async state machine overhead.

```csharp

while (await reader.WaitToReadAsync(ct))
{
    // Drain all currently buffered items synchronously
    while (reader.TryRead(out var item))
    {
        Process(item);
    }
}

```text

### TryWrite Fast Path

`TryWrite` is synchronous and allocation-free when the channel has space. Prefer it over `WriteAsync` in hot paths where
you can handle the `false` return.

```csharp

// Hot path -- avoid async overhead when channel has space
if (!writer.TryWrite(item))
{
    // Slow path -- wait for space (or handle overflow)
    await writer.WriteAsync(item, ct);
}

```text

### Bounded Channel Memory Behavior

Bounded channels pre-allocate an internal array of `capacity` slots. Items are stored by reference (for reference
types), so the channel holds references until consumed. For memory-sensitive workloads:

- Choose capacity based on expected item size multiplied by count
- Items are eligible for GC as soon as `TryRead`/`ReadAsync` returns them
- Drop modes (`DropOldest`, `DropNewest`) keep memory stable but lose data

---

## Cancellation and Graceful Shutdown

### Basic Cancellation

Pass a `CancellationToken` to all async channel operations. When cancelled, operations throw
`OperationCanceledException`.

```csharp

try
{
    await foreach (var item in reader.ReadAllAsync(stoppingToken))
    {
        await ProcessAsync(item, stoppingToken);
    }
}
catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
{
    // Expected during shutdown
}

```text

### Drain Pattern

Complete the writer to signal no more items will arrive, then drain remaining items before stopping. This prevents data
loss during shutdown.

```csharp

public sealed class DrainableProcessor(
    Channel<WorkItem> channel,
    IServiceScopeFactory scopeFactory,
    ILogger<DrainableProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = channel.Reader;

        try
        {
            while (await reader.WaitToReadAsync(stoppingToken))
            {
                while (reader.TryRead(out var item))
                {
                    using var scope = scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetRequiredService<IWorkItemHandler>();
                    await handler.HandleAsync(item, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown requested -- fall through to drain
        }

        // Signal producers to stop -- any concurrent WriteAsync will throw ChannelClosedException
        channel.Writer.TryComplete();

        // Drain remaining items with a deadline
        logger.LogInformation("Draining remaining work items");
        using var drainCts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

        while (reader.TryRead(out var remaining))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<IWorkItemHandler>();
                await handler.HandleAsync(remaining, drainCts.Token);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error during drain");
            }
        }

        logger.LogInformation("Drain complete");
    }
}

```text

### Host Shutdown Timeout

The default host shutdown timeout is 30 seconds. If your drain needs more time, configure it:

```csharp

builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(60);
});

```text

---

## Agent Gotchas

1. **Do not use unbounded channels in production without rate control** -- they can exhaust memory under sustained
   producer pressure. Always prefer bounded channels with explicit capacity.
2. **Do not violate SingleReader/SingleWriter promises** -- these flags enable lock-free optimizations. Multiple
   concurrent readers with `SingleReader = true` causes data corruption, not exceptions.
3. **Do not forget to call `Complete()` on the writer** -- without completion, consumers using `ReadAllAsync()` or
   `WaitToReadAsync` will wait indefinitely after the last item.
4. **Do not catch `ChannelClosedException` globally** -- it signals that the writer called `Complete()`, possibly with
   an error. Catch it only around `ReadAsync` calls; `WaitToReadAsync`/`TryRead` loops handle completion via `false`
   return.
5. **Do not use `ReadAsync` in hot paths** -- prefer the `WaitToReadAsync` + `TryRead` pattern to drain buffered items
   synchronously and reduce async state machine allocations.
6. **Do not block in the `itemDropped` callback** -- it runs synchronously on the writer's thread. Keep it fast
   (increment counter, log) or offload heavy work.

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

- [System.Threading.Channels overview](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
- [Channel<T> API reference](https://learn.microsoft.com/en-us/dotnet/api/system.threading.channels.channel-1)
- [BoundedChannelOptions](https://learn.microsoft.com/en-us/dotnet/api/system.threading.channels.boundedchanneloptions)
- [ChannelReader.ReadAllAsync](https://learn.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1.readallasync)
````
