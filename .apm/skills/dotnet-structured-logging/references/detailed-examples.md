}

// Registration
app.UseMiddleware<CorrelationIdMiddleware>();

```text

### Message Queue Correlation

For asynchronous messaging (Azure Service Bus, RabbitMQ), propagate correlation through message properties:

```csharp

// Producer -- attach correlation to message
var message = new ServiceBusMessage(payload)
{
    CorrelationId = Activity.Current?.TraceId.ToString()
        ?? Guid.NewGuid().ToString("N"),
    ApplicationProperties =
    {
        ["BusinessCorrelationId"] = orderId.ToString()
    }
};

// Consumer -- restore correlation in log scope
processor.ProcessMessageAsync += async args =>
{
    using var scope = logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = args.Message.CorrelationId,
        ["BusinessCorrelationId"] =
            args.Message.ApplicationProperties["BusinessCorrelationId"]
    });

    logger.LogInformation("Processing message {MessageId}", args.Message.MessageId);
    await ProcessAsync(args.Message, args.CancellationToken);
};

```text

### Correlation Best Practices

| Practice | Rationale |
|----------|-----------|
| Always include `TraceId` in log output | Enables log-to-trace joins in observability platforms |
| Use `CorrelationId` for business flows | Survives async gaps where trace context resets |
| Store correlation IDs in message headers | Enables end-to-end tracing through queues |
| Include correlation in error responses | Enables support teams to look up the full trace |
| Use Serilog `LogContext.PushProperty` or MEL `BeginScope` | Automatically attaches to all log events in scope |

---

## Agent Gotchas

1. **Do not conflate log emission with log pipeline** -- this skill covers pipeline, query, and operations. For Serilog/MEL configuration, enrichers, sink registration, and source-generated LoggerMessage, see [skill:dotnet-observability].
2. **Do not store PII in production logs** -- apply masking enrichers or OTel processor rules at the pipeline level. Redacting after storage is insufficient for compliance.
3. **Do not skip log sampling for high-throughput services** -- unsampled Debug/Info logs in a service handling thousands of requests per second will overwhelm storage and degrade query performance. Use tail-based sampling to keep all errors and slow requests.
4. **Do not hardcode aggregation platform endpoints in application code** -- use environment variables (`OTEL_EXPORTER_OTLP_ENDPOINT`) or configuration so the same image works across environments.
5. **Do not rely solely on TraceId for business correlation** -- trace context resets at async boundaries (message queues, scheduled jobs). Add explicit business correlation IDs for workflows that span these boundaries.
6. **Do not forget retention policies** -- logs without retention policies accumulate indefinitely, increasing costs and slowing queries. Set per-severity retention (e.g., 30 days for Info, 90 days for Error).

---

## References

- [OpenTelemetry Collector configuration](https://opentelemetry.io/docs/collector/configuration/)
- [Seq documentation](https://docs.datalust.co/docs)
- [Seq signal expressions](https://docs.datalust.co/docs/the-seq-query-language)
- [Grafana Loki LogQL](https://grafana.com/docs/loki/latest/query/)
- [Elasticsearch KQL syntax](https://www.elastic.co/guide/en/kibana/current/kuery-query.html)
- [Serilog.Expressions](https://github.com/serilog/serilog-expressions)
- [Serilog PII masking](https://github.com/serilog/serilog/wiki/Structured-Data#masking-sensitive-data)
- [Azure Monitor KQL reference](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)
- [W3C Trace Context specification](https://www.w3.org/TR/trace-context/)
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
