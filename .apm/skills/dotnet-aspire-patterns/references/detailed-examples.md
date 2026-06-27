  mcr.microsoft.com/dotnet/aspire-dashboard:9.1

```bash

Configure your app to export OTLP telemetry to `http://localhost:4317` and view it at `http://localhost:18888`.

---

## Health Checks and Distributed Tracing

### Component Health Checks

Each Aspire component automatically registers health checks. The AppHost uses these to determine resource readiness:

```csharp

// In AppHost -- WaitFor uses health checks to gate startup
builder.AddProject<Projects.MyApi>("api")
    .WithReference(postgres)
    .WaitFor(postgres);     // Waits for Npgsql health check to pass

```text

### Custom Health Checks

Add application-specific health checks alongside Aspire defaults:

```csharp

builder.Services.AddHealthChecks()
    .AddCheck<OrderProcessingHealthCheck>(
        "order-processing",
        tags: ["ready"]);

```text

See [skill:dotnet-observability] for detailed health check patterns (liveness vs readiness, custom checks, health check
publishing).

### Distributed Tracing Integration

Aspire configures OpenTelemetry tracing through ServiceDefaults. Traces propagate automatically across HTTP boundaries.
For custom spans:

```csharp

private static readonly ActivitySource s_activitySource = new("MyApp.Orders");

public async Task<Order> ProcessOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    using var activity = s_activitySource.StartActivity("ProcessOrder");
    activity?.SetTag("order.customer_id", request.CustomerId);

    // Calls to other Aspire services carry trace context automatically
    var inventory = await _httpClient.GetFromJsonAsync<InventoryResponse>(
        $"https+http://inventory-api/api/stock/{request.ProductId}", ct);

    // ... process order
    return order;
}

```text

See [skill:dotnet-observability] for comprehensive distributed tracing guidance (custom ActivitySource, trace context
propagation, span events).

---

## Container Resources

### Adding Container Images

For services not available as Aspire components, add arbitrary container images:

```csharp

var seq = builder.AddContainer("seq", "datalust/seq")
    .WithHttpEndpoint(port: 5341, targetPort: 80)
    .WithEnvironment("ACCEPT_EULA", "Y");

// Reference the container from a project
builder.AddProject<Projects.MyApi>("api")
    .WithReference(seq);

```text

### Persistent Volumes

By default, Aspire containers use ephemeral storage. Add volumes for data persistence across restarts:

```csharp

var postgres = builder.AddPostgres("pg")
    .WithDataVolume("pg-data")     // Named volume for data persistence
    .AddDatabase("ordersdb");

```csharp

### External Resources

Reference existing infrastructure not managed by Aspire:

```csharp

// Connection string from configuration (not an Aspire-managed container)
var existingDb = builder.AddConnectionString("legacydb");

builder.AddProject<Projects.MyApi>("api")
    .WithReference(existingDb);

```text

---

## Aspire vs Manual Container Orchestration

| Concern                | Aspire                                 | Docker Compose / Manual         |
| ---------------------- | -------------------------------------- | ------------------------------- |
| Configuration language | C# (strongly typed)                    | YAML                            |
| Service discovery      | Automatic (env var injection)          | Manual DNS/env config           |
| Health checks          | Automatic per component                | Manual HEALTHCHECK per service  |
| Observability          | Pre-configured OTel + dashboard        | Manual OTel collector setup     |
| IDE integration        | Hot reload, F5 debugging               | Attach debugger manually        |
| Production deployment  | Generates manifests (AZD, K8s)         | Write manifests directly        |
| Non-.NET services      | Container references (less integrated) | Equal support for all languages |
| Learning curve         | .NET-specific abstractions             | Industry-standard tooling       |

Choose Aspire when your stack is primarily .NET and you want standardized observability, service discovery, and a
simplified local dev experience. Choose manual orchestration when you need fine-grained control, polyglot services, or
your team is already proficient with Compose/Kubernetes.

---

## Key Principles

- **AppHost is dev-time only** -- it orchestrates local development, not production deployment
- **Use components over raw connection strings** -- components add health checks, telemetry, and resilience
  automatically
- **ServiceDefaults is non-negotiable** -- every Aspire service project must reference it for consistent observability
- **WaitFor for ordered startup** -- use it for real dependencies (schema migrations, seed data), not for every resource
- **Do not duplicate OTel config** -- Aspire ServiceDefaults configure OpenTelemetry; manual configuration causes
  double-collection

---

## Agent Gotchas

1. **Do not manually configure OpenTelemetry in Aspire service projects** -- ServiceDefaults already registers OTel
   providers. Adding manual `.AddOpenTelemetry()` calls causes duplicate trace/metric collection and inflated telemetry
   costs.
2. **Do not hardcode connection strings in Aspire service projects** -- use `builder.AddNpgsqlDbContext<T>("name")` or
   `builder.Configuration.GetConnectionString("name")`. Aspire injects connection strings via environment variables;
   hardcoded values bypass service discovery.
3. **Do not use `WaitFor` on every resource** -- it serializes startup and increases launch time. Use it only when a
   service genuinely cannot start without the dependency (e.g., database migration on startup).
4. **Do not reference `Aspire.Hosting.*` packages from service projects** -- hosting packages belong in the AppHost
   only. Service projects use client packages (`Aspire.Npgsql`, `Aspire.StackExchange.Redis`, etc.).
5. **Do not confuse the AppHost with a production host** -- the AppHost runs locally (or in CI) to orchestrate
   resources. Production deployment uses generated manifests or infrastructure-as-code.
6. **Do not omit `AddServiceDefaults()` in new service projects** -- without it, the project lacks service discovery,
   health checks, and telemetry, breaking Aspire integration silently.

---

## Prerequisites

- .NET 10 SDK (or .NET 8/9 with Aspire workload)
- Docker Desktop or Podman (for container resources)
- Aspire workload: `dotnet workload install aspire`

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

- [.NET Aspire overview](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- [.NET Aspire components](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/components-overview)
- [.NET Aspire service discovery](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-discovery)
- [.NET Aspire dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview)
- [.NET Aspire orchestration](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview)
- [Aspire samples repository](https://github.com/dotnet/aspire-samples)
````
