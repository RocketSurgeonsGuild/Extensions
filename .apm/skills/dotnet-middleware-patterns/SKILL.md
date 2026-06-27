---
name: dotnet-middleware-patterns
category: web
subcategory: middleware
description: Builds ASP.NET Core middleware. Pipeline ordering, short-circuit, exception handling.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-middleware-patterns

ASP.NET Core middleware patterns for the HTTP request pipeline. Covers correct ordering, writing custom middleware as
classes or inline delegates, short-circuit logic, request/response manipulation, exception handling middleware, and
conditional middleware registration.

## Scope

- Correct middleware pipeline ordering and common ordering mistakes
- Custom middleware classes (convention-based and IMiddleware)
- Inline middleware (Use, Run, Map)
- Short-circuit logic for early validation and feature flags
- Request/response body manipulation
- Exception handling middleware (IExceptionHandler, StatusCodePages)
- Conditional middleware (UseWhen, MapWhen)

## Out of scope

- Authentication/authorization middleware configuration -- see [skill:dotnet-api-security]
- Observability middleware (OpenTelemetry, health checks) -- see [skill:dotnet-observability]
- Minimal API endpoint filters -- see [skill:dotnet-minimal-apis]

Cross-references: [skill:dotnet-observability] for logging and telemetry middleware, [skill:dotnet-api-security] for
auth middleware, [skill:dotnet-minimal-apis] for endpoint filters (the Minimal API equivalent of middleware).

---

## Pipeline Ordering

Middleware executes in the order it is registered. The order is critical -- placing middleware in the wrong position
causes subtle bugs (missing CORS headers, unhandled exceptions, auth bypasses).

### Recommended Order

````csharp

var app = builder.Build();

// 1. Exception handling (outermost -- catches everything below)
app.UseExceptionHandler("/error");

// 2. HSTS (before any response is sent)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// 3. HTTPS redirection
app.UseHttpsRedirection();

// 4. Static files (short-circuits for static content before routing)
app.UseStaticFiles();

// 5. Routing (matches endpoints but does not execute them yet)
// .NET 6+ calls UseRouting() implicitly if omitted; shown here for clarity
app.UseRouting();

// 6. CORS (must be after routing, before auth)
app.UseCors();

// 7. Authentication (identifies the user)
app.UseAuthentication();

// 8. Authorization (checks permissions against the matched endpoint)
app.UseAuthorization();

// 9. Custom middleware (runs after auth, before endpoint execution)
app.UseRequestLogging();

// 10. Endpoint execution (terminal -- executes the matched endpoint)
app.MapControllers();
app.MapRazorPages();

```text

### Why Order Matters

| Mistake                                         | Consequence                                                 |
| ----------------------------------------------- | ----------------------------------------------------------- |
| `UseAuthorization()` before `UseRouting()`      | Authorization has no endpoint metadata -- all requests pass |
| `UseCors()` after `UseAuthorization()`          | Preflight requests fail because they lack auth tokens       |
| `UseExceptionHandler()` after custom middleware | Exceptions in custom middleware are unhandled               |
| `UseStaticFiles()` after `UseAuthorization()`   | Static files require authentication unnecessarily           |

---

## Custom Middleware Classes

Convention-based middleware uses a constructor with `RequestDelegate` and an `InvokeAsync` method. This is the standard
pattern for reusable middleware.

### Basic Pattern

```csharp

public sealed class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
    }
}

// Registration via extension method (conventional pattern)
public static class RequestTimingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(
        this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimingMiddleware>();
}

// Usage in Program.cs
app.UseRequestTiming();

```csharp

### Factory-Based (IMiddleware)

For middleware that requires scoped services, implement `IMiddleware`. This uses DI to create middleware instances
per-request instead of once at startup:

```csharp

public sealed class TenantMiddleware : IMiddleware
{
    private readonly TenantDbContext _db;

    // Scoped services can be injected directly
    public TenantMiddleware(TenantDbContext db)
    {
        _db = db;
    }

    public async Task InvokeAsync(
        HttpContext context, RequestDelegate next)
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"]
            .FirstOrDefault();

        if (tenantId is not null)
        {
            var tenant = await _db.Tenants.FindAsync(tenantId);
            context.Items["Tenant"] = tenant;
        }

        await next(context);
    }
}

// IMiddleware requires explicit DI registration
builder.Services.AddScoped<TenantMiddleware>();

// Then register in pipeline
app.UseMiddleware<TenantMiddleware>();

```text

**Convention-based vs IMiddleware:**

| Aspect          | Convention-based                            | `IMiddleware`                                                    |
| --------------- | ------------------------------------------- | ---------------------------------------------------------------- |
| Lifetime        | Singleton (created once)                    | Per-request (from DI)                                            |
| Scoped services | Via `InvokeAsync` parameters only           | Via constructor injection                                        |
| Registration    | `UseMiddleware<T>()` only                   | Requires `services.Add*<T>()` + `UseMiddleware<T>()`             |
| Performance     | Slightly faster (no per-request allocation) | Resolved from DI each request (lifetime depends on registration) |

---

## Inline Middleware

For simple, one-off middleware logic, use `app.Use()`, `app.Map()`, or `app.Run()`:

### app.Use -- Pass-Through

```csharp

// Adds a header to every response, then passes to next middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Request-Id"] =
        context.TraceIdentifier;

    await next(context);
});

```text

### app.Run -- Terminal

```csharp

// Terminal middleware -- does NOT call next
app.Run(async context =>
{
    await context.Response.WriteAsync("Fallback response");
});

```text

### app.Map -- Branch by Path

```csharp

// Branch the pipeline for requests matching /api/diagnostics
app.Map("/api/diagnostics", diagnosticApp =>
{
    diagnosticApp.Run(async context =>
    {
        var data = new
        {
            MachineName = Environment.MachineName,
            Timestamp = DateTimeOffset.UtcNow
        };
        await context.Response.WriteAsJsonAsync(data);
    });
});

```json

---

## Short-Circuit Logic

Middleware can short-circuit the pipeline by not calling `next()`. Use this for early validation, rate limiting, or
feature flags.

### Request Validation

```csharp

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _expectedKey;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration config)
    {
        _next = next;
        _expectedKey = config["ApiKey"]
            ?? throw new InvalidOperationException(
                "ApiKey configuration is required");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(
                "X-Api-Key", out var providedKey)
            || !string.Equals(
                providedKey, _expectedKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Invalid or missing API key"
            });
            return; // Short-circuit -- do NOT call _next
        }

        await _next(context);
    }
}

```text

### Feature Flag Gate

```csharp

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/beta"),
    betaApp =>
    {
        betaApp.Use(async (context, next) =>
        {
            var featureManager = context.RequestServices
                .GetRequiredService<IFeatureManager>();

            if (!await featureManager.IsEnabledAsync("BetaFeatures"))
            {
                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;
                return; // Short-circuit
            }

            await next(context);
        });
    });

```text

---

## Request and Response Manipulation

### Reading the Request Body

The request body is a forward-only stream by default. Enable buffering to read it multiple times:

```csharp

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Enable buffering so the body can be read multiple times
        context.Request.EnableBuffering();

        if (context.Request.ContentLength > 0
            && context.Request.ContentLength < 64_000)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(
                context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            _logger.LogDebug(
                "Request body for {Path}: {Body}",
                context.Request.Path, body);
            context.Request.Body.Position = 0; // Reset for next reader
        }

        await _next(context);
    }
}

```text

### Modifying the Response

To capture or modify the response body, replace `context.Response.Body` with a `MemoryStream`:

```csharp

public async Task InvokeAsync(HttpContext context)
{
    var originalBodyStream = context.Response.Body;

    using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
