        }
        finally
        {
            await reader.CompleteAsync();
            await writer.CompleteAsync();
        }
    }

    private static async Task WriteResponseAsync(
        PipeWriter writer, ReadOnlyMemory<byte> response)
    {
        // Write length prefix + payload
        var memory = writer.GetMemory(4 + response.Length);
        BinaryPrimitives.WriteInt32BigEndian(
            memory.Span, response.Length);
        response.CopyTo(memory[4..]);
        writer.Advance(4 + response.Length);
        await writer.FlushAsync();
    }
}

```text

### IDuplexPipe

Kestrel exposes connections as `IDuplexPipe`, combining `PipeReader` and `PipeWriter` into a single transport
abstraction. This pattern also works for custom TCP servers, WebSocket handlers, and named-pipe protocols.

```csharp

public interface IDuplexPipe
{
    PipeReader Input { get; }
    PipeWriter Output { get; }
}

```text

---

## Performance Tips

1. **Minimize copies** -- use `ReadOnlySequence<byte>` slicing instead of copying to `byte[]`. Parse directly from the
   sequence when possible.
2. **Use `GetSpan`/`GetMemory` correctly** -- request the minimum size you need. The pipe may return a larger buffer,
   which is fine. Do not cache the returned `Span`/`Memory` across `Advance`/`FlushAsync` calls.
3. **Set `useSynchronizationContext: false`** -- server code should never capture the synchronization context. This is
   the default for `PipeOptions` but explicit is clearer.
4. **Tune pause/resume thresholds** -- the defaults (64 KB / 32 KB) work for most scenarios. Increase for
   high-throughput bulk transfer; decrease for low-latency interactive protocols.
5. **Prefer `SequenceReader<byte>`** -- for complex parsing, `SequenceReader<byte>` provides `TryRead`,
   `TryReadBigEndian`, `AdvancePast`, and `IsNext` methods that handle multi-segment sequences transparently.

```csharp

static bool TryParseHeader(
    ref ReadOnlySequence<byte> buffer,
    out int messageType,
    out int length)
{
    var reader = new SequenceReader<byte>(buffer);

    if (!reader.TryRead(out byte typeByte) ||
        !reader.TryReadBigEndian(out int len))
    {
        messageType = 0;
        length = 0;
        return false;
    }

    messageType = typeByte;
    length = len;
    buffer = buffer.Slice(reader.Position);
    return true;
}

```text

---

## Agent Gotchas

1. **Do not forget to call `AdvanceTo` after `ReadAsync`** -- skipping `AdvanceTo` leaks pooled memory and eventually
   causes `OutOfMemoryException`. Every `ReadAsync` must be paired with an `AdvanceTo`.
2. **Do not access `ReadResult.Buffer` after calling `AdvanceTo`** -- the underlying memory segments may be returned to
   the pool. Copy or parse all needed data before advancing.
3. **Do not set `consumed` equal to `examined` when no complete message was found** -- this creates a busy-wait loop.
   Set `consumed` to `buffer.Start` (nothing consumed) and `examined` to `buffer.End` (everything examined) so the pipe
   waits for new data.
4. **Do not ignore `FlushResult.IsCompleted`** -- it means the reader has stopped consuming. Continue writing after this
   and data will be silently discarded.
5. **Do not use `Pipe` for simple stream-to-stream copying** -- `Stream.CopyToAsync` is simpler and equally efficient.
   Use pipelines when you need parsing, backpressure, or zero-copy slicing.
6. **Do not use `BinaryPrimitives` methods on spans shorter than required** -- always check `buffer.Length` before
   reading fixed-width values to avoid `ArgumentOutOfRangeException`.

---

## Knowledge Sources

- Stephen Toub,
  [System.IO.Pipelines: High performance IO in .NET](https://devblogs.microsoft.com/dotnet/system-io-pipelines-high-performance-io-in-net/)
  -- canonical deep dive on pipeline design, motivation, and usage patterns

## References

- [System.IO.Pipelines overview](https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines)
- [Pipe class API reference](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipelines.pipe)
- [PipeReader API reference](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipelines.pipereader)
- [PipeWriter API reference](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipelines.pipewriter)
- [SequenceReader<T>](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.sequencereader-1)
- [Kestrel connection middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints)
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
