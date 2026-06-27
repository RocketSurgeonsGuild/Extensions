```csharp

builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis")!,
        options =>
        {
            options.Configuration.ChannelPrefix =
                RedisChannel.Literal("MyApp:");
        });

```text

**Azure SignalR Service (managed backplane):**

```csharp

builder.Services.AddSignalR()
    .AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]);

```csharp

Azure SignalR Service offloads connection management entirely -- the ASP.NET Core server handles hub logic while Azure manages WebSocket connections, scaling, and message routing.

---

## Server-Sent Events (SSE) -- .NET 10

.NET 10 adds built-in SSE support to ASP.NET Core, making server-to-client streaming straightforward without additional packages.

### Minimal API Endpoint

```csharp

app.MapGet("/events/orders", async (
    OrderEventService eventService,
    CancellationToken cancellationToken) =>
{
    // TypedResults.ServerSentEvents returns an SSE response
    return TypedResults.ServerSentEvents(
        eventService.GetOrderEventsAsync(cancellationToken));
});

```text

### Event Source Implementation

```csharp

public sealed class OrderEventService
{
    public async IAsyncEnumerable<SseItem<OrderEvent>> GetOrderEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var evt = await WaitForNextEvent(cancellationToken);
            yield return new SseItem<OrderEvent>(evt, "order-update");
        }
    }
}

```text

### Browser Client

```javascript

const source = new EventSource('/events/orders');

source.addEventListener('order-update', (event) => {
    const order = JSON.parse(event.data);
    updateDashboard(order);
});

source.onerror = () => {
    // EventSource automatically reconnects
    console.log('SSE connection lost, reconnecting...');
};

```text

### When to Use SSE Over SignalR

- **One-way push only** -- SSE is simpler when you do not need client-to-server messages
- **Browser native** -- no JavaScript library needed (uses `EventSource` API)
- **Automatic reconnection** -- browsers reconnect automatically with `Last-Event-ID`
- **HTTP/1.1 compatible** -- works through proxies that do not support WebSocket upgrade

---

## JSON-RPC 2.0

JSON-RPC 2.0 is a stateless, transport-agnostic remote procedure call protocol encoded in JSON. It is the foundation of the Language Server Protocol (LSP) and is used in some .NET tooling scenarios.

### Protocol Structure

```json

// Request
{"jsonrpc": "2.0", "method": "textDocument/completion", "params": {...}, "id": 1}

// Response
{"jsonrpc": "2.0", "result": {...}, "id": 1}

// Notification (no response expected)
{"jsonrpc": "2.0", "method": "textDocument/didChange", "params": {...}}

// Error
{"jsonrpc": "2.0", "error": {"code": -32600, "message": "Invalid Request"}, "id": 1}

```json

### StreamJsonRpc (.NET Library)

`StreamJsonRpc` is the primary .NET library for JSON-RPC 2.0:

```xml

<PackageReference Include="StreamJsonRpc" Version="2.*" />

```json

```csharp

// Server: expose methods via JSON-RPC over a stream
using StreamJsonRpc;

public sealed class CalculatorService
{
    public int Add(int a, int b) => a + b;
    public Task<double> DivideAsync(double a, double b) =>
        b == 0 ? throw new ArgumentException("Division by zero")
               : Task.FromResult(a / b);
}

// Wire up over a WebSocket -- UseWebSockets() is required for upgrade handling
app.UseWebSockets();
app.Map("/jsonrpc", async (HttpContext context) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var ws = await context.WebSockets.AcceptWebSocketAsync();
    using var rpc = new JsonRpc(new WebSocketMessageHandler(ws));
    rpc.AddLocalRpcTarget(new CalculatorService());
    rpc.StartListening();
    await rpc.Completion;
});

```text

```csharp

// Client
using var ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri("ws://localhost:5000/jsonrpc"),
    CancellationToken.None);

using var rpc = new JsonRpc(new WebSocketMessageHandler(ws));
rpc.StartListening();

var result = await rpc.InvokeAsync<int>("Add", 2, 3);
// result == 5

```text

### When to Use JSON-RPC 2.0

- Building or integrating with Language Server Protocol (LSP) implementations
- Simple RPC over WebSocket or stdio where gRPC is too heavyweight
- Interoperating with non-.NET systems that speak JSON-RPC
- Tooling and editor integrations

---

## gRPC Streaming

See [skill:dotnet-grpc] for complete gRPC implementation details including all four streaming patterns (unary, server streaming, client streaming, bidirectional streaming), authentication, load balancing, and health checks.

### Quick Decision: gRPC Streaming vs SignalR vs SSE

| Requirement | Choose |
|-------------|--------|
| Service-to-service, both .NET | gRPC streaming |
| Browser client needs bidirectional | SignalR |
| Browser client needs server push only | SSE |
| Maximum throughput, binary payloads | gRPC streaming |
| Automatic reconnection with browser clients | SSE (native) or SignalR (built-in) |
| Multiple client platforms (JS, mobile, .NET) | SignalR |

---

## Key Principles

- **Default to SignalR for browser-facing real-time** -- it handles transport negotiation, reconnection, and grouping out of the box
- **Use SSE for simple server push** -- .NET 10 built-in support makes it the lightest option for one-way notifications
- **Use gRPC streaming for service-to-service** -- highest performance, strongly typed contracts, all four streaming patterns
- **Use JSON-RPC 2.0 for tooling protocols** -- when you need structured RPC over simple transports (WebSocket, stdio)
- **Use strongly-typed hubs** -- `Hub<T>` catches method name typos at compile time instead of runtime
- **Scale SignalR with a backplane** -- Redis or Azure SignalR Service for multi-server deployments

See [skill:dotnet-native-aot] for AOT compilation pipeline and [skill:dotnet-aot-architecture] for AOT-compatible real-time communication patterns.

---

## Agent Gotchas

1. **Do not use SignalR when SSE suffices** -- if you only need server-to-client push without bidirectional communication, SSE is simpler and lighter.
2. **Do not forget `AddMessagePackProtocol()` on the server when the client uses MessagePack** -- mismatched protocols cause silent connection failures.
3. **Do not use Long Polling transport with SignalR unless required** -- it has significantly higher latency and server resource usage compared to WebSockets.
4. **Do not store connection IDs long-term** -- SignalR connection IDs change on reconnection. Use user identifiers or groups for addressing.
5. **Do not use gRPC streaming to browsers directly** -- browsers do not support HTTP/2 trailers natively. Use gRPC-Web with a proxy or choose SignalR/SSE instead.
6. **Do not confuse SSE with WebSocket** -- SSE is unidirectional (server-to-client only). If you need client-to-server messages, use SignalR or WebSocket directly.
7. **Do not forget `OnMessageReceived` for JWT with SignalR** -- WebSocket connections cannot send custom HTTP headers after the initial handshake. The access token must be read from the query string in `JwtBearerEvents.OnMessageReceived`.
8. **Do not assume group membership persists across reconnections** -- groups are tied to connection IDs, which change on reconnect. Re-add connections to groups in `OnConnectedAsync`.
9. **Do not deploy multi-server SignalR without a backplane** -- without Redis or Azure SignalR Service, messages sent on one server instance are invisible to connections on other instances.

---

## Attribution

Adapted from [Aaronontheweb/dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills) (MIT license).

---

## References

- [SignalR overview](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0)
- [SignalR hubs](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-10.0)
- [Server-Sent Events in .NET 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/server-sent-events?view=aspnetcore-10.0)
- [StreamJsonRpc](https://github.com/microsoft/vs-streamjsonrpc)
- [gRPC streaming](https://learn.microsoft.com/en-us/aspnet/core/grpc/client?view=aspnetcore-10.0)
- [SignalR scaling with Redis](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane?view=aspnetcore-10.0)
- [SignalR authentication and authorization](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0)
- [Azure SignalR Service](https://learn.microsoft.com/en-us/azure/azure-signalr/signalr-overview)
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
