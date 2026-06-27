---
name: dotnet-http-client
category: web
subcategory: minimal-apis
description: Consumes HTTP APIs. IHttpClientFactory, typed/named clients, resilience, DelegatingHandlers.
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
---

# dotnet-http-client

Best practices for consuming HTTP APIs in .NET applications using `IHttpClientFactory`. Covers named and typed clients,
resilience pipeline integration, `DelegatingHandler` chains for cross-cutting concerns, and testing strategies.

## Scope

- IHttpClientFactory patterns (named and typed clients)
- DelegatingHandler chains for cross-cutting concerns
- Resilience pipeline integration with HTTP clients
- Testing strategies for HTTP client code

## Out of scope

- DI container mechanics and service lifetimes -- see [skill:dotnet-csharp-dependency-injection]
- Async/await patterns and cancellation token propagation -- see [skill:dotnet-csharp-async-patterns]
- Resilience pipeline configuration (Polly v8, retry, circuit breaker) -- see [skill:dotnet-resilience]
- Integration testing frameworks -- see [skill:dotnet-integration-testing]

Cross-references: [skill:dotnet-resilience] for resilience pipeline configuration,
[skill:dotnet-csharp-dependency-injection] for service registration, [skill:dotnet-csharp-async-patterns] for async HTTP
patterns.

---

## Why IHttpClientFactory

Creating `HttpClient` instances directly causes two problems:

1. **Socket exhaustion** -- each `HttpClient` instance holds its own connection pool. Creating and disposing many
   instances exhausts available sockets (`SocketException: Address already in use`).
2. **DNS staleness** -- a long-lived singleton `HttpClient` caches DNS lookups indefinitely, missing DNS changes during
   blue-green deployments or failovers.

`IHttpClientFactory` solves both by managing `HttpMessageHandler` lifetimes with automatic pooling and rotation
(default: 2-minute handler lifetime).

````csharp

// Do not do this
var client = new HttpClient(); // Socket exhaustion risk

// Do not do this either
static readonly HttpClient _client = new(); // DNS staleness risk

// Do this -- use IHttpClientFactory
builder.Services.AddHttpClient();

```text

---

## Named Clients

Register clients by name for scenarios where you consume multiple APIs with different configurations:

```csharp

// Registration
builder.Services.AddHttpClient("catalog-api", client =>
{
    client.BaseAddress = new Uri("https://catalog.internal");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("payment-api", client =>
{
    client.BaseAddress = new Uri("https://payments.internal");
    client.DefaultRequestHeaders.Add("X-Api-Version", "2");
});

// Usage
public sealed class OrderService(IHttpClientFactory clientFactory)
{
    public async Task<Product?> GetProductAsync(
        string productId, CancellationToken ct)
    {
        var client = clientFactory.CreateClient("catalog-api");
        var response = await client.GetAsync($"/products/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content
            .ReadFromJsonAsync<Product>(ct);
    }
}

```json

---

## Typed Clients

Typed clients encapsulate HTTP logic behind a strongly-typed interface. Prefer typed clients when a service consumes a
single API with multiple operations:

```csharp

// Typed client class
public sealed class CatalogApiClient(HttpClient httpClient)
{
    public async Task<Product?> GetProductAsync(
        string productId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"/products/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content
            .ReadFromJsonAsync<Product>(ct);
    }

    public async Task<PagedResult<Product>> ListProductsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"/products?page={page}&pageSize={pageSize}", ct);

        response.EnsureSuccessStatusCode();
        return (await response.Content
            .ReadFromJsonAsync<PagedResult<Product>>(ct))!;
    }

    public async Task<Product> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/products", request, ct);

        response.EnsureSuccessStatusCode();
        return (await response.Content
            .ReadFromJsonAsync<Product>(ct))!;
    }
}

// Registration
builder.Services.AddHttpClient<CatalogApiClient>(client =>
{
    client.BaseAddress = new Uri("https://catalog.internal");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

```json

### Typed Client with Interface

For testability, define an interface:

```csharp

public interface ICatalogApiClient
{
    Task<Product?> GetProductAsync(string productId, CancellationToken ct = default);
    Task<PagedResult<Product>> ListProductsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}

public sealed class CatalogApiClient(HttpClient httpClient) : ICatalogApiClient
{
    // Implementation as above
}

// Registration with interface
builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
{
    client.BaseAddress = new Uri("https://catalog.internal");
});

```text

---

## Resilience Pipelines

Apply resilience to HTTP clients using `Microsoft.Extensions.Http.Resilience`. See [skill:dotnet-resilience] for
detailed pipeline configuration, strategy options, and migration guidance.

### Standard Resilience Handler (Recommended)

The standard handler applies the full pipeline (rate limiter, total timeout, retry, circuit breaker, attempt timeout)
with sensible defaults:

```csharp

builder.Services
    .AddHttpClient<CatalogApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://catalog.internal");
    })
    .AddStandardResilienceHandler();

```text

### Standard Handler with Custom Options

```csharp

builder.Services
    .AddHttpClient<CatalogApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://catalog.internal");
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
    });

```text

### Hedging Handler (for Read-Only APIs)

For idempotent read operations where tail latency matters:

```csharp

builder.Services
    .AddHttpClient("search-api")
    .AddStandardHedgingHandler(options =>
    {
        options.Hedging.MaxHedgedAttempts = 2;
        options.Hedging.Delay = TimeSpan.FromMilliseconds(500);
    });

```text

See [skill:dotnet-resilience] for when to use hedging vs standard retry.

---

## DelegatingHandlers

`DelegatingHandler` provides a pipeline of message handlers that process outgoing requests and incoming responses. Use
them for cross-cutting concerns that apply to HTTP traffic.

### Handler Pipeline Order

Handlers execute in registration order for requests (outermost to innermost) and reverse order for responses:

```text

Request  --> Handler A --> Handler B --> Handler C --> HttpClientHandler --> Server
Response <-- Handler A <-- Handler B <-- Handler C <-- HttpClientHandler <-- Server

```text

### Common Handlers

#### Request Logging

```csharp

public sealed class RequestLoggingHandler(
    ILogger<RequestLoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "HTTP {Method} {Uri}",
            request.Method,
            request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken);

        stopwatch.Stop();
        logger.LogInformation(
            "HTTP {Method} {Uri} responded {StatusCode} in {ElapsedMs}ms",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}

// Registration
builder.Services.AddTransient<RequestLoggingHandler>();
builder.Services
    .AddHttpClient<CatalogApiClient>(/* ... */)
    .AddHttpMessageHandler<RequestLoggingHandler>();

```text

#### API Key Authentication

```csharp

public sealed class ApiKeyHandler(IConfiguration config) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var apiKey = config["ExternalApi:ApiKey"]
            ?? throw new InvalidOperationException("API key not configured");

        request.Headers.Add("X-Api-Key", apiKey);

        return base.SendAsync(request, cancellationToken);
    }
}

// Registration
builder.Services.AddTransient<ApiKeyHandler>();
builder.Services
    .AddHttpClient<CatalogApiClient>(/* ... */)
    .AddHttpMessageHandler<ApiKeyHandler>();

```text

#### Bearer Token (from Downstream Auth)

```csharp

public sealed class BearerTokenHandler(
    IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?
            .Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

```text

#### Correlation ID Propagation

```csharp

public sealed class CorrelationIdHandler : DelegatingHandler
{
    private const string HeaderName = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(HeaderName))
        {
            var correlationId = Activity.Current?.Id
                ?? Guid.NewGuid().ToString();
            request.Headers.Add(HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
