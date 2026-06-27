---
name: dotnet-realtime-communication
category: architecture
subcategory: messaging
description: Builds real-time features. SignalR hubs, SSE (.NET 10), JSON-RPC 2.0, gRPC streaming, scaling.
license: MIT
targets: ['*']
tags: [api, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for api tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-realtime-communication

Real-time communication patterns for .NET applications. Compares SignalR (full-duplex over WebSockets with automatic
fallback), Server-Sent Events (SSE, built-in to ASP.NET Core in .NET 10), JSON-RPC 2.0 (structured request-response over
any transport), and gRPC streaming (high-performance binary streaming). Provides decision guidance for choosing the
right protocol based on requirements.

## Scope

- SignalR hubs (WebSocket, auto-fallback, scaling)
- Server-Sent Events (SSE, built-in .NET 10)
- JSON-RPC 2.0 over any transport
- gRPC streaming for high-performance binary
- Protocol comparison and decision guidance

## Out of scope

- HTTP client factory and resilience pipelines -- see [skill:dotnet-http-client] and [skill:dotnet-resilience]
- Native AOT architecture and trimming -- see [skill:dotnet-native-aot] and [skill:dotnet-trimming]
- Blazor-specific SignalR usage -- see [skill:dotnet-blazor-patterns]

Cross-references: [skill:dotnet-grpc] for gRPC streaming implementation details and all four streaming patterns. See
[skill:dotnet-integration-testing] for testing real-time communication endpoints. See [skill:dotnet-blazor-patterns] for
Blazor-specific SignalR circuit management and render mode interaction.

---

## Protocol Comparison

| Protocol           | Direction             | Transport                                       | Format                      | Browser Support              | Best For                                                       |
| ------------------ | --------------------- | ----------------------------------------------- | --------------------------- | ---------------------------- | -------------------------------------------------------------- |
| **SignalR**        | Full-duplex           | WebSocket, SSE, Long Polling (auto-negotiation) | JSON or MessagePack         | Yes (JS/TS client)           | Interactive apps, chat, dashboards, collaborative editing      |
| **SSE (.NET 10)**  | Server-to-client only | HTTP/1.1+                                       | Text (typically JSON lines) | Yes (native EventSource API) | Notifications, live feeds, status updates                      |
| **JSON-RPC 2.0**   | Request-response      | Any (HTTP, WebSocket, stdio)                    | JSON                        | Depends on transport         | Tooling protocols (LSP), structured RPC over simple transports |
| **gRPC streaming** | All four patterns     | HTTP/2                                          | Protobuf (binary)           | Limited (gRPC-Web)           | Service-to-service, high-throughput, low-latency streaming     |

### When to Choose What

- **SignalR**: You need bidirectional real-time communication with browser clients. SignalR handles transport
  negotiation automatically (WebSocket preferred, falls back to SSE, then Long Polling). Use when clients need to both
  send and receive in real time.
- **SSE (.NET 10 built-in)**: You only need server-to-client push. Simpler than SignalR when bidirectional communication
  is not required. Built into ASP.NET Core in .NET 10 -- no additional packages needed. Works with the browser's native
  `EventSource` API.
- **JSON-RPC 2.0**: You need structured request-response semantics over a simple transport. Used by Language Server
  Protocol (LSP) and some .NET tooling. Not a streaming protocol -- use when you need named methods with typed
  parameters over WebSocket or stdio.
- **gRPC streaming**: Service-to-service streaming with maximum performance. Supports all four streaming patterns
  (unary, server streaming, client streaming, bidirectional). Best when both endpoints are .NET services or
  gRPC-compatible. See [skill:dotnet-grpc] for implementation details.

---

## SignalR

SignalR provides real-time web functionality with automatic connection management and transport negotiation.

### Server Setup

````csharp

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 64 * 1024; // 64 KB
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

app.MapHub<NotificationHub>("/hubs/notifications");

```text

### Hub Implementation

```csharp

public sealed class NotificationHub(
    ILogger<NotificationHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        await base.OnConnectedAsync();
    }

    // Client-to-server method
    public async Task SendMessage(string channel, string message)
    {
        // Broadcast to all clients in the channel group
        await Clients.Group(channel).SendAsync("ReceiveMessage",
            Context.UserIdentifier, message);
    }

    // Server-to-client streaming
    public async IAsyncEnumerable<StockPrice> StreamPrices(
        string symbol,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await GetLatestPrice(symbol, cancellationToken);
            await Task.Delay(1000, cancellationToken);
        }
    }
}

```text

### Strongly-Typed Hubs

Use interfaces to get compile-time safety for client method calls:

```csharp

public interface INotificationClient
{
    Task ReceiveMessage(string user, string message);
    Task OrderStatusChanged(int orderId, string status);
}

public sealed class NotificationHub(
    ILogger<NotificationHub> logger) : Hub<INotificationClient>
{
    public async Task SendMessage(string channel, string message)
    {
        // Compile-time checked -- no magic strings
        await Clients.Group(channel).ReceiveMessage(
            Context.UserIdentifier!, message);
    }
}

```text

### Sending from Outside Hubs

Inject `IHubContext` to send messages from background services or controllers:

```csharp

public sealed class OrderService(
    IHubContext<NotificationHub, INotificationClient> hubContext)
{
    public async Task UpdateOrderStatus(int orderId, string userId, string status)
    {
        // Send to specific user group
        await hubContext.Clients.Group($"user:{userId}")
            .OrderStatusChanged(orderId, status);
    }
}

```text

### Transport Negotiation

SignalR automatically negotiates the best transport:

1. **WebSocket** (preferred) -- full-duplex, lowest latency
2. **Server-Sent Events** -- server-to-client only, falls back when WebSockets unavailable
3. **Long Polling** -- universal fallback, highest latency

Force a specific transport when needed:

```csharp

// Server: disable specific transports
app.MapHub<NotificationHub>("/hubs/notifications", options =>
{
    options.Transports = HttpTransportType.WebSockets |
                         HttpTransportType.ServerSentEvents;
    // Disables Long Polling
});

```text

### MessagePack Protocol

Use MessagePack for smaller payloads and faster serialization:

```csharp

// Server
builder.Services.AddSignalR()
    .AddMessagePackProtocol();

// Client (JavaScript)
// new signalR.HubConnectionBuilder()
//     .withUrl("/hubs/notifications")
//     .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
//     .build();

```text

### Connection Lifecycle

Override `OnConnectedAsync` and `OnDisconnectedAsync` to manage connection state:

```csharp

public sealed class NotificationHub(
    ILogger<NotificationHub> logger,
    IConnectionTracker tracker) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        logger.LogInformation("Client {ConnectionId} connected (user: {UserId})",
            connectionId, userId);

        // Track connection for presence features
        if (userId is not null)
        {
            await tracker.AddConnectionAsync(userId, connectionId);
            await Groups.AddToGroupAsync(connectionId, $"user:{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (exception is not null)
        {
            logger.LogWarning(exception,
                "Client {ConnectionId} disconnected with error", connectionId);
        }

        if (userId is not null)
        {
            await tracker.RemoveConnectionAsync(userId, connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

```text

### Groups Management

Groups provide a lightweight pub/sub mechanism. Connections can belong to multiple groups and group membership is managed per-connection:

```csharp

public sealed class ChatHub : Hub<IChatClient>
{
    // Join a room (called by clients)
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).UserJoined(Context.UserIdentifier!, roomName);
    }

    // Leave a room
    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).UserLeft(Context.UserIdentifier!, roomName);
    }

    // Send to specific group
    public async Task SendToRoom(string roomName, string message)
    {
        await Clients.Group(roomName).ReceiveMessage(
            Context.UserIdentifier!, message);
    }

    // Send to all except caller
    public async Task BroadcastExceptSelf(string message)
    {
        await Clients.Others.ReceiveMessage(
            Context.UserIdentifier!, message);
    }
}

```text

Groups are not persisted -- they are cleared when a connection disconnects. Re-add connections to groups in `OnConnectedAsync` if needed (e.g., from a database or cache).

### Client-to-Server Streaming

Clients can stream data to the hub using `IAsyncEnumerable<T>` or `ChannelReader<T>`:

```csharp

public sealed class UploadHub : Hub
{
    // Accept a stream of items from the client
    public async Task UploadData(
        IAsyncEnumerable<SensorReading> stream,
        CancellationToken cancellationToken)
    {
        await foreach (var reading in stream.WithCancellation(cancellationToken))
        {
            await ProcessReading(reading);
        }
    }
}

```text

### Authentication

SignalR uses the same authentication as the ASP.NET Core host. For WebSocket connections, the access token is sent via query string because WebSocket does not support custom headers:

```csharp

// Server: configure JWT for SignalR
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://identity.example.com";
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Read token from query string for WebSocket requests
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications")
    .RequireAuthorization();

```text

Access `Context.UserIdentifier` in the hub to identify the authenticated user. By default this maps to the `ClaimTypes.NameIdentifier` claim. Customize with `IUserIdProvider`:

```csharp

public sealed class EmailUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}

// Register
builder.Services.AddSingleton<IUserIdProvider, EmailUserIdProvider>();

```text

### Scaling with Backplane

For multi-server deployments, use a backplane to synchronize messages across instances. Without a backplane, messages sent on one server are not visible to connections on other servers.

**Redis backplane:**

```csharp

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
