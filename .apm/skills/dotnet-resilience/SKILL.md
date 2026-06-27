---
name: dotnet-resilience
description: Adds fault tolerance. Polly v8 + MS.Extensions.Http.Resilience, retry/circuit breaker/timeout.
license: MIT
targets: ['*']
tags: [architecture, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for architecture tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
category: fundamentals
subcategory: coding-standards
---

# dotnet-resilience

Modern resilience patterns for .NET applications using Polly v8 and `Microsoft.Extensions.Http.Resilience`. Covers the
standard resilience pipeline (rate limiter, total timeout, retry, circuit breaker, attempt timeout), custom pipeline
configuration, and integration with the .NET dependency injection system.

**Superseded package:** `Microsoft.Extensions.Http.Polly` is superseded by `Microsoft.Extensions.Http.Resilience`. Do
not use `Microsoft.Extensions.Http.Polly` for new projects. See the
[migration guide](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/resilience/migration-guide) for
upgrading existing code.

## Scope

- Standard resilience pipeline (rate limiter, timeout, retry, circuit breaker)
- Custom resilience pipeline configuration with Polly v8
- DI integration via MS.Extensions.Http.Resilience
- Resilience telemetry and Polly metering

## Out of scope

- DI container mechanics and service lifetimes -- see [skill:dotnet-csharp-dependency-injection]
- Async/await patterns and cancellation token propagation -- see [skill:dotnet-csharp-async-patterns]
- HTTP client factory patterns (typed clients, DelegatingHandlers) -- see [skill:dotnet-http-client]
- Testing resilience policies -- see [skill:dotnet-integration-testing] and [skill:dotnet-xunit]

Cross-references: [skill:dotnet-csharp-dependency-injection] for service registration,
[skill:dotnet-csharp-async-patterns] for cancellation token propagation, [skill:dotnet-http-client] for applying
resilience to HTTP clients.

---

## Package Landscape

| Package                                | Status         | Purpose                                                                     |
| -------------------------------------- | -------------- | --------------------------------------------------------------------------- |
| `Polly` (v8+)                          | **Current**    | Core resilience library -- strategies, pipelines, telemetry                 |
| `Microsoft.Extensions.Resilience`      | **Current**    | DI integration for non-HTTP resilience pipelines                            |
| `Microsoft.Extensions.Http.Resilience` | **Current**    | DI integration for `IHttpClientFactory` resilience pipelines                |
| `Microsoft.Extensions.Http.Polly`      | **Superseded** | Legacy HTTP resilience -- migrate to `Microsoft.Extensions.Http.Resilience` |
| `Polly` (v7 and earlier)               | **Legacy**     | Older API -- migrate to v8                                                  |

Install the modern stack:

````xml

<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.*" />
<!-- Transitively brings in Polly v8 and Microsoft.Extensions.Resilience -->

```xml

For non-HTTP scenarios only:

```xml

<PackageReference Include="Microsoft.Extensions.Resilience" Version="9.*" />

```xml

---

## Standard Resilience Pipeline

`Microsoft.Extensions.Http.Resilience` provides a standard resilience pipeline that follows the recommended order. The pipeline layers execute from outermost to innermost:

```text

Request
  --> Rate Limiter        (1. shed excess load)
    --> Total Timeout      (2. cap total wall-clock time)
      --> Retry             (3. retry transient failures)
        --> Circuit Breaker  (4. stop calling failing services)
          --> Attempt Timeout (5. cap individual attempt time)
            --> HTTP call

```text

### Why This Order Matters

- **Rate limiter first**: prevents retry storms from overwhelming downstream services
- **Total timeout wraps retry**: ensures the entire operation (including all retries) has a deadline
- **Retry wraps circuit breaker**: retries can try again after the breaker resets; a broken circuit counts as a retriable failure
- **Circuit breaker wraps attempt timeout**: timed-out attempts count toward the breaker's failure threshold
- **Attempt timeout innermost**: each individual HTTP call has its own deadline

### Standard Pipeline with Defaults

```csharp

builder.Services
    .AddHttpClient("catalog-api", client =>
    {
        client.BaseAddress = new Uri("https://catalog.internal");
    })
    .AddStandardResilienceHandler();

```text

This applies the standard pipeline with sensible defaults:
- **Rate limiter**: 1000 concurrent requests
- **Total timeout**: 30 seconds
- **Retry**: 3 attempts, exponential backoff (2s base), jitter
- **Circuit breaker**: 10% failure ratio, 100 sample size, 5s break duration
- **Attempt timeout**: 10 seconds

### Standard Pipeline with Custom Options

```csharp

builder.Services
    .AddHttpClient("catalog-api", client =>
    {
        client.BaseAddress = new Uri("https://catalog.internal");
    })
    .AddStandardResilienceHandler(options =>
    {
        // Total timeout for the entire operation including retries
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);

        // Retry strategy
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.StatusCode is
                HttpStatusCode.RequestTimeout or
                HttpStatusCode.TooManyRequests or
                >= HttpStatusCode.InternalServerError);

        // Circuit breaker
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.1;
        options.CircuitBreaker.MinimumThroughput = 20;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);

        // Per-attempt timeout
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    });

```text

### Configuration via appsettings.json

Bind resilience options from configuration for environment-specific tuning:

```csharp

builder.Services
    .AddHttpClient("catalog-api", client =>
    {
        client.BaseAddress = new Uri("https://catalog.internal");
    })
    .AddStandardResilienceHandler(options =>
    {
        builder.Configuration
            .GetSection("Resilience:CatalogApi")
            .Bind(options);
    });

```text

```json

{
  "Resilience": {
    "CatalogApi": {
      "Retry": {
        "MaxRetryAttempts": 5,
        "Delay": "00:00:02",
        "BackoffType": "Exponential"
      },
      "CircuitBreaker": {
        "BreakDuration": "00:00:15"
      },
      "TotalRequestTimeout": {
        "Timeout": "00:01:00"
      }
    }
  }
}

```text

---

## Custom Resilience Pipelines

When the standard pipeline does not fit, build custom pipelines with Polly v8 directly.

### Retry Strategy

```csharp

builder.Services.AddResiliencePipeline("db-retry", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(500),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = new PredicateBuilder()
            .Handle<DbUpdateConcurrencyException>()
            .Handle<TimeoutException>(),
        OnRetry = args =>
        {
            // Structured logging of retry attempts
            var logger = args.Context.Properties
                .GetValue(new ResiliencePropertyKey<ILogger>("logger"), null!);
            logger?.LogWarning(
                args.Outcome.Exception,
                "Retry attempt {AttemptNumber} after {Delay}ms",
                args.AttemptNumber,
                args.RetryDelay.TotalMilliseconds);
            return ValueTask.CompletedTask;
        }
    });
});

// Inject and use
public sealed class OrderRepository(
    [FromKeyedServices("db-retry")] ResiliencePipeline pipeline,
    AppDbContext db)
{
    public async Task<Order> UpdateAsync(Order order, CancellationToken ct)
    {
        return await pipeline.ExecuteAsync(async token =>
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync(token);
            return order;
        }, ct);
    }
}

```text

### Circuit Breaker Strategy

```csharp

builder.Services.AddResiliencePipeline("payment-gateway", pipelineBuilder =>
{
    pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(30),
        FailureRatio = 0.25,           // Open after 25% failure rate
        MinimumThroughput = 10,        // Need at least 10 calls to evaluate
        BreakDuration = TimeSpan.FromSeconds(15),
        ShouldHandle = new PredicateBuilder()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
    });
});

```text

### Timeout Strategy

```csharp

builder.Services.AddResiliencePipeline("external-api", pipelineBuilder =>
{
    // Total timeout for the entire pipeline execution
    pipelineBuilder.AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(30),
        OnTimeout = args =>
        {
            // Log timeout details for diagnostics
            return ValueTask.CompletedTask;
        }
    });
});

```text

### Composing Multiple Strategies

Build a composite pipeline by chaining strategies. Order matters -- outermost strategy is added first:

```csharp

builder.Services.AddResiliencePipeline("composed", pipelineBuilder =>
{
    // 1. Total timeout (outermost -- caps entire operation)
    pipelineBuilder.AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(45)
    });

    // 2. Retry (retries on transient failures)
    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = new PredicateBuilder()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
    });

    // 3. Circuit breaker (stops calling failing services)
    pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.1,
        MinimumThroughput = 20,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(10),
        ShouldHandle = new PredicateBuilder()
            .Handle<HttpRequestException>()
            .Handle<TimeoutException>()
    });

    // 4. Attempt timeout (innermost -- caps single attempt)
    pipelineBuilder.AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10)
    });
});

```text

---

## Typed Resilience Pipelines

For result-bearing operations, use `ResiliencePipeline<T>`:

```csharp

builder.Services.AddResiliencePipeline<string, HttpResponseMessage>(
    "typed-http",
    pipelineBuilder =>
    {
        pipelineBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => r.StatusCode >= HttpStatusCode.InternalServerError)
        });
    });

```text

---

## Hedging Strategy

Send parallel requests to reduce tail latency. The hedging strategy dispatches additional attempts if the initial request is slow:

```csharp

builder.Services
    .AddHttpClient("search-api")
    .AddStandardHedgingHandler(options =>
    {
        options.Hedging.MaxHedgedAttempts = 2;
        options.Hedging.Delay = TimeSpan.FromMilliseconds(500);
        // Hedging sends a parallel request if the first hasn't
        // responded within 500ms
    });

```text

**Use hedging when:**
- Operations are idempotent (GET requests, read-only queries)
- Tail latency reduction matters more than extra load

**Do not use hedging when:**
- Operations have side effects (POST, PUT, DELETE)
- Downstream services cannot handle increased load

---

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
