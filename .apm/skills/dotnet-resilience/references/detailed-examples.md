---

## Telemetry and Observability

Polly v8 emits metrics and traces via `System.Diagnostics` out of the box when using the DI integration.

### Built-in Metrics

The `Microsoft.Extensions.Resilience` package automatically reports:

| Metric | Description |
|--------|-------------|
| `polly.strategy.attempt.duration` | Duration of each attempt |
| `polly.strategy.pipeline.duration` | Duration of the entire pipeline execution |
| `polly.strategy.attempt.count` | Count of attempts (including retries) |

These integrate with OpenTelemetry automatically when the OpenTelemetry SDK is configured in your application -- see [skill:dotnet-observability] for collector setup.

### Enabling Telemetry

Resilience telemetry is enabled automatically when using the DI-based registration (`AddResiliencePipeline`, `AddStandardResilienceHandler`). The `Microsoft.Extensions.Resilience` package registers a `MeteringEnricher` and `LoggingEnricher` that emit structured logs and metrics through the standard `ILoggerFactory` and `IMeterFactory` from DI:

```csharp

// Telemetry is automatic -- no extra configuration needed.
// Structured logs appear via ILogger; metrics via IMeter.
builder.Services
    .AddHttpClient("catalog-api")
    .AddStandardResilienceHandler();

// To see resilience logs, set the Polly category to Information:
// appsettings.json
// {
//   "Logging": {
//     "LogLevel": {
//       "Polly": "Information"
//     }
//   }
// }

```text

---

## Migrating from Microsoft.Extensions.Http.Polly

If upgrading from the superseded `Microsoft.Extensions.Http.Polly` package:

### Before (Legacy)

```csharp

// Using Microsoft.Extensions.Http.Polly (superseded)
builder.Services
    .AddHttpClient("catalog-api")
    .AddTransientHttpErrorPolicy(p =>
        p.WaitAndRetryAsync(3, attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt))))
    .AddTransientHttpErrorPolicy(p =>
        p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

```text

### After (Modern)

```csharp

// Using Microsoft.Extensions.Http.Resilience (current)
builder.Services
    .AddHttpClient("catalog-api")
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.Retry.BackoffType = DelayBackoffType.Exponential;

        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    });

```text

### Migration Steps

1. Replace `Microsoft.Extensions.Http.Polly` NuGet reference with `Microsoft.Extensions.Http.Resilience`
2. Replace `AddTransientHttpErrorPolicy` calls with `AddStandardResilienceHandler` or custom pipeline
3. Translate Polly v7 policy configuration to v8 strategy options
4. Remove explicit `IAsyncPolicy<HttpResponseMessage>` type references
5. Verify retry/circuit breaker behavior matches previous configuration

---

## Key Principles

- **Use the standard pipeline for HTTP** -- `AddStandardResilienceHandler()` covers the most common case with battle-tested defaults
- **Customize via options, not custom code** -- the standard handler options cover retry count, backoff, circuit breaker thresholds, and timeouts
- **Use `AddResiliencePipeline` for non-HTTP** -- database calls, message queues, file I/O
- **Always add jitter to retries** -- prevents thundering herd when multiple clients retry simultaneously
- **Configure via appsettings.json** -- different environments need different thresholds; avoid hardcoded values in production
- **Do not suppress `TimeoutRejectedException`** -- let it propagate through the pipeline so outer strategies (retry, circuit breaker) can react
- **Do not use `Microsoft.Extensions.Http.Polly`** for new projects -- it wraps Polly v7 and is superseded

---

## Agent Gotchas

1. **Do not mix Polly v7 and v8 APIs** -- v8 uses `ResiliencePipeline` and strategy options; v7 uses `IAsyncPolicy`. They are not interchangeable.
2. **Do not add `AddStandardResilienceHandler` twice** -- it composes a full pipeline; adding it twice doubles every strategy layer.
3. **Do not set attempt timeout higher than total timeout** -- the attempt timeout must be shorter than the total timeout or it has no effect.
4. **Do not retry non-idempotent operations** -- only retry operations that are safe to repeat (GETs, idempotent writes with idempotency keys).
5. **Do not use hardcoded retry delays in production** -- always use `UseJitter = true` and configure delays via appsettings.

---

## References

- [Polly v8 documentation](https://www.pollydocs.org/)
- [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Microsoft.Extensions.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/)
- [Migration from Http.Polly to Http.Resilience](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/resilience/migration-guide)
- [Standard resilience pipeline](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience#standard-resilience-handler)
- [Polly v8 strategy options](https://www.pollydocs.org/strategies/)
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