        return base.SendAsync(request, cancellationToken);
    }
}

```text

### Chaining Multiple Handlers

Handlers are added in execution order:

```csharp

builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddTransient<RequestLoggingHandler>();

builder.Services
    .AddHttpClient<CatalogApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://catalog.internal");
    })
    .AddHttpMessageHandler<CorrelationIdHandler>()   // 1st (outermost): add correlation ID
    .AddHttpMessageHandler<BearerTokenHandler>()     // 2nd: add auth token
    .AddHttpMessageHandler<RequestLoggingHandler>()  // 3rd: log request/response
    .AddStandardResilienceHandler();                 // 4th (innermost): resilience pipeline

```text

**Note:** In `IHttpClientFactory`, handlers registered first are outermost. `.AddStandardResilienceHandler()` added last
is innermost -- it wraps the actual HTTP call directly. This means retries happen inside the resilience handler without
re-executing the outer DelegatingHandlers. This is typically correct: correlation IDs and auth tokens are set once by
the outer handlers, and the resilience layer retries the raw HTTP call. If you need per-retry token refresh (e.g.,
expired bearer tokens), move the token handler inside the resilience boundary or use a custom
`ResiliencePipelineBuilder` callback.

---

## Configuration Patterns

### Base Address from Configuration

```csharp

builder.Services.AddHttpClient<CatalogApiClient>(client =>
{
    var baseUrl = builder.Configuration["Services:CatalogApi:BaseUrl"]
        ?? throw new InvalidOperationException(
            "CatalogApi base URL not configured");
    client.BaseAddress = new Uri(baseUrl);
});

```text

```json

{
  "Services": {
    "CatalogApi": {
      "BaseUrl": "https://catalog.internal"
    }
  }
}

```text

### Handler Lifetime

The default handler lifetime is 2 minutes. Adjust for services with different DNS characteristics:

```csharp

builder.Services
    .AddHttpClient<CatalogApiClient>(/* ... */)
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

```csharp

**Shorter lifetime** (1 min): for services behind load balancers with frequent DNS changes. **Longer lifetime** (5-10
min): for stable internal services where connection reuse improves performance.

---

## Testing HTTP Clients

### Unit Testing with MockHttpMessageHandler

Test typed clients by providing a mock handler that returns controlled responses:

```csharp

public sealed class CatalogApiClientTests
{
    [Fact]
    public async Task GetProductAsync_ReturnsProduct_WhenFound()
    {
        // Arrange
        var expectedProduct = new Product { Id = "p1", Name = "Widget" };
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedProduct)
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://test.local")
        };

        var client = new CatalogApiClient(httpClient);

        // Act
        var result = await client.GetProductAsync("p1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Widget", result.Name);
    }

    [Fact]
    public async Task GetProductAsync_ReturnsNull_WhenNotFound()
    {
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://test.local")
        };

        var client = new CatalogApiClient(httpClient);

        var result = await client.GetProductAsync("missing");

        Assert.Null(result);
    }
}

// Reusable mock handler
public sealed class MockHttpMessageHandler(
    HttpResponseMessage response) : HttpMessageHandler
{
    private HttpRequestMessage? _lastRequest;

    public HttpRequestMessage? LastRequest => _lastRequest;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _lastRequest = request;
        return Task.FromResult(response);
    }
}

```text

### Testing DelegatingHandlers

Test handlers in isolation by providing an inner handler:

```csharp

public sealed class ApiKeyHandlerTests
{
    [Fact]
    public async Task AddsApiKeyHeader()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalApi:ApiKey"] = "test-key-123"
            })
            .Build();

        var innerHandler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK));

        var handler = new ApiKeyHandler(config)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://test.local")
        };

        // Act
        await client.GetAsync("/test");

        // Assert
        Assert.NotNull(innerHandler.LastRequest);
        Assert.Equal(
            "test-key-123",
            innerHandler.LastRequest.Headers
                .GetValues("X-Api-Key").Single());
    }
}

```text

### Integration Testing with WebApplicationFactory

Test the full HTTP client pipeline including DI registration:

```csharp

// See [skill:dotnet-integration-testing] for WebApplicationFactory patterns

```csharp

---

## Named vs Typed Clients -- Decision Guide

| Factor         | Named Client                  | Typed Client                        |
| -------------- | ----------------------------- | ----------------------------------- |
| API surface    | Simple (1-2 calls)            | Rich (multiple operations)          |
| Type safety    | Requires string name          | Strongly typed                      |
| Encapsulation  | HTTP logic in consuming class | HTTP logic in client class          |
| Testability    | Mock `IHttpClientFactory`     | Mock the client interface           |
| Multiple APIs  | One name per API              | One class per API                   |
| Recommendation | Ad-hoc or simple calls        | Primary pattern for API consumption |

**Default to typed clients.** Use named clients only for simple, one-off HTTP calls where a full typed client class adds
unnecessary ceremony.

---

## Key Principles

- **Always use IHttpClientFactory** -- never `new HttpClient()` in application code
- **Prefer typed clients** -- encapsulate HTTP logic behind a strongly-typed interface
- **Apply resilience via pipeline** -- use `AddStandardResilienceHandler()` (see [skill:dotnet-resilience]) rather than
  manual retry loops
- **Keep handlers focused** -- each `DelegatingHandler` should do one thing (auth, logging, correlation)
- **Register handlers as Transient** -- DelegatingHandlers are created per-client-instance and should not hold state
  across requests
- **Pass CancellationToken everywhere** -- from endpoint to typed client to HTTP call
- **Use ReadFromJsonAsync / PostAsJsonAsync** -- avoid manual serialization with `StringContent`

---

## Agent Gotchas

1. **Do not create HttpClient with `new`** -- always inject `IHttpClientFactory` or a typed client. Direct instantiation
   causes socket exhaustion.
2. **Do not dispose typed clients** -- the factory manages handler lifetimes. Disposing the `HttpClient` instance is
   harmless (it does not close pooled connections), but wrapping it in `using` is misleading.
3. **Do not set `BaseAddress` with a trailing path** -- `new Uri("https://api.example.com/v2")` will drop `/v2` when
   combining with relative URIs. Use `new Uri("https://api.example.com/v2/")` (trailing slash) or use absolute URIs in
   calls.
4. **Understand that resilience added last is innermost** -- `AddStandardResilienceHandler()` registered after
   `AddHttpMessageHandler` calls wraps the HTTP call directly. Retries do not re-execute outer DelegatingHandlers. This
   is correct for most cases (tokens/correlation IDs set once). If you need per-retry token refresh, place the token
   handler after the resilience handler or use a custom pipeline callback.
5. **Do not register DelegatingHandlers as Singleton** -- they are pooled with the `HttpMessageHandler` pipeline and
   must be Transient.

---

## References

- [IHttpClientFactory with .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Use HttpClientFactory to implement resilient HTTP requests](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- [HttpClient message handlers](https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers)
- [Typed clients](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#typed-clients)
- [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
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
