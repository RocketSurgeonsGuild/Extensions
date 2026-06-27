    context.Response.Body = responseBody;

    await _next(context);

    // Read the response written by downstream middleware
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseText = await new StreamReader(
        context.Response.Body).ReadToEndAsync();
    context.Response.Body.Seek(0, SeekOrigin.Begin);

    // Copy back to original stream
    await responseBody.CopyToAsync(originalBodyStream);
}

```text

**Caution:** Response body replacement adds memory overhead and should only be used for diagnostics or specific
transformation requirements, not in high-throughput paths.

---

## Exception Handling Middleware

### Built-in Exception Handler

ASP.NET Core provides `UseExceptionHandler` for production-grade exception handling. This should always be the outermost
middleware:

```csharp

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features
            .Get<IExceptionHandlerFeature>();

        var logger = context.RequestServices
            .GetRequiredService<ILogger<Program>>();
        logger.LogError(
            exceptionFeature?.Error,
            "Unhandled exception for {Path}",
            context.Request.Path);

        await context.Response.WriteAsJsonAsync(new
        {
            Error = "An internal error occurred",
            TraceId = context.TraceIdentifier
        });
    });
});

```text

### IExceptionHandler (.NET 8+)

.NET 8 introduced `IExceptionHandler` for DI-friendly, composable exception handling. Multiple handlers can be
registered and are invoked in order until one handles the exception:

```csharp

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not ValidationException validationException)
            return false; // Not handled -- pass to next handler

        context.Response.StatusCode =
            StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            Error = "Validation failed",
            Details = validationException.Errors
        }, ct);

        return true; // Handled -- stop the chain
    }
}

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            Error = "An internal error occurred",
            TraceId = context.TraceIdentifier
        }, ct);

        return true;
    }
}

// Register handlers in order (first match wins)
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

app.UseExceptionHandler();

```text

### StatusCodePages for Non-Exception Errors

For HTTP error status codes that are not caused by exceptions (404, 403), use `UseStatusCodePages`:

```csharp

app.UseStatusCodePagesWithReExecute("/error/{0}");

// Or inline
app.UseStatusCodePages(async context =>
{
    context.HttpContext.Response.ContentType = "application/json";
    await context.HttpContext.Response.WriteAsJsonAsync(new
    {
        Error = $"HTTP {context.HttpContext.Response.StatusCode}",
        TraceId = context.HttpContext.TraceIdentifier
    });
});

```text

---

## Conditional Middleware

### UseWhen -- Conditional Branch (Rejoins Pipeline)

`UseWhen` branches the pipeline based on a predicate. The branch rejoins the main pipeline after execution:

```csharp

// Only apply rate limiting headers for API routes
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    apiApp =>
    {
        // Requires builder.Services.AddRateLimiter() in service registration
        apiApp.UseRateLimiter();
    });

```text

### MapWhen -- Conditional Branch (Does Not Rejoin)

`MapWhen` creates a terminal branch that does not rejoin the main pipeline:

```csharp

// Serve a special handler for WebSocket upgrade requests
app.MapWhen(
    context => context.WebSockets.IsWebSocketRequest,
    wsApp =>
    {
        wsApp.Run(async context =>
        {
            using var ws = await context.WebSockets
                .AcceptWebSocketAsync();
            // Handle WebSocket connection
        });
    });

```text

### Environment-Specific Middleware

```csharp

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

```text

---

## Key Principles

- **Order is everything** -- middleware executes top-to-bottom for requests and bottom-to-top for responses; incorrect
  order causes auth bypasses, missing headers, and unhandled exceptions
- **Exception handler goes first** -- `UseExceptionHandler` must be the outermost middleware to catch exceptions from
  all downstream components
- **Prefer classes over inline for reusable middleware** -- convention-based middleware classes are testable,
  composable, and follow the single-responsibility principle
- **Use `IMiddleware` for scoped dependencies** -- convention-based middleware is singleton; if you need scoped services
  (DbContext, user-scoped caches), use `IMiddleware`
- **Short-circuit intentionally** -- always document why a middleware does not call `next()` and ensure it writes a
  complete response
- **Avoid response body manipulation in hot paths** -- replacing `Response.Body` with `MemoryStream` doubles memory
  usage per request

---

## Agent Gotchas

1. **Do not place `UseAuthorization()` before `UseRouting()`** -- authorization requires endpoint metadata from routing
   to evaluate policies. Without routing, all authorization checks are skipped.
2. **Do not place `UseCors()` after `UseAuthorization()`** -- CORS preflight (OPTIONS) requests do not carry auth
   tokens. If auth runs first, preflights are rejected with 401.
3. **Do not forget to call `next()` in pass-through middleware** -- forgetting `await _next(context)` silently
   short-circuits the pipeline, causing downstream middleware and endpoints to never execute.
4. **Do not read `Request.Body` without `EnableBuffering()`** -- the request body stream is forward-only by default.
   Reading it without buffering consumes it, causing model binding and subsequent reads to fail with empty data.
5. **Do not register `IMiddleware` implementations without DI registration** -- unlike convention-based middleware,
   `IMiddleware` requires explicit `services.AddScoped<T>()` or `services.AddTransient<T>()`. Without it,
   `UseMiddleware<T>()` throws at startup.
6. **Do not write to `Response.Body` after calling `next()` if downstream middleware has already started the response**
   -- once headers are sent (response has started), modifications throw `InvalidOperationException`. Check
   `context.Response.HasStarted` before writing.

---

## Knowledge Sources

Middleware patterns in this skill are grounded in publicly available content from:

- **Andrew Lock's "Exploring ASP.NET Core" Blog Series** -- Deep coverage of middleware authoring patterns, including
  IMiddleware vs convention-based trade-offs, pipeline ordering pitfalls, endpoint routing internals, and
  IExceptionHandler composition. Source: https://andrewlock.net/
- **Official ASP.NET Core Middleware Documentation** -- Middleware fundamentals, factory-based activation, and error
  handling patterns. Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/

> **Note:** This skill applies publicly documented guidance. It does not represent or speak for the named sources.

## References

- [ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Write custom ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [Factory-based middleware activation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/extensibility)
- [Handle errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [IExceptionHandler in .NET 8](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling#iexceptionhandler)
- [Exploring ASP.NET Core (Andrew Lock)](https://andrewlock.net/)

---

## Attribution

Adapted from [Aaronontheweb/dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills) (MIT license).
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
