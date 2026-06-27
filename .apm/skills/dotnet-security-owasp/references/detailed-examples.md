// Configure strong Identity password and lockout policies
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Account lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

```text

```csharp

// Secure cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

```text

**Gotcha:** `CookieSecurePolicy.SameAsRequest` allows cookies over HTTP in development, which is fine. But in production behind a reverse proxy terminating TLS, the app sees HTTP -- so cookies are sent insecurely. Always use `CookieSecurePolicy.Always` in production and configure forwarded headers.

---

## A08: Software and Data Integrity Failures

**Vulnerability:** Code and infrastructure that does not protect against integrity violations -- unsigned packages, insecure CI/CD pipelines, deserialization of untrusted data.

**Risk in .NET:** Using `BinaryFormatter` for deserialization (arbitrary code execution), accepting unsigned NuGet packages from untrusted feeds, missing package source mapping.

### Mitigation

```xml

<!-- NuGet package source mapping in nuget.config -->
<!-- Only allow packages from trusted sources -->
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="internal" value="https://pkgs.example.com/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="internal">
      <package pattern="MyCompany.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>

```text

```csharp

// NEVER use BinaryFormatter -- it is a critical deserialization vulnerability.
// BinaryFormatter is obsolete as error (SYSLIB0011) in .NET 8+ and removed in .NET 9+.
// Use System.Text.Json instead:
var data = JsonSerializer.Deserialize<OrderDto>(jsonString);

// For binary serialization needs, use MessagePack or Protobuf:
// <PackageReference Include="MessagePack" Version="3.*" />
var bytes = MessagePackSerializer.Serialize(order);
var restored = MessagePackSerializer.Deserialize<Order>(bytes);

```text

**Gotcha:** Package source mapping uses most-specific-pattern-wins: `MyCompany.*` beats the `*` wildcard. Always define specific patterns for internal packages to prevent dependency confusion attacks.

---

## A09: Security Logging and Monitoring Failures

**Vulnerability:** Insufficient logging of security-relevant events, lack of monitoring for breaches, inability to detect and respond to active attacks.

**Risk in .NET:** Not logging authentication failures, missing audit trails for sensitive operations, logging sensitive data (passwords, tokens) in plaintext.

### Mitigation

```csharp

// Log security events with structured logging
public sealed class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var path = context.Request.Path.Value;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["UserId"] = userId,
            ["RequestPath"] = path,
            ["RemoteIp"] = context.Connection.RemoteIpAddress?.ToString()
        }))
        {
            await next(context);

            // Log failed authentication attempts
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                logger.LogWarning("Authentication failed for {Path}", path);
            }

            // Log authorization failures
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                logger.LogWarning("Authorization denied for {Path}", path);
            }
        }
    }
}

```text

```csharp

// NEVER log sensitive data -- redact credentials and PII
// Configure log filtering to exclude sensitive paths
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Warning);

// Use IHttpLoggingInterceptor (.NET 8+) to redact request/response headers
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestMethod
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    // Explicitly exclude request/response bodies and auth headers
});

```text

**Gotcha:** Structured logging with `{Placeholder}` syntax is safe, but string interpolation (`$"User {userId}"`) in log calls bypasses structured logging and may leak PII into log sinks that do not support redaction.

---

## A10: Server-Side Request Forgery (SSRF)

**Vulnerability:** Application fetches a remote resource based on user-supplied URL without validation, allowing attackers to reach internal services or metadata endpoints.

**Risk in .NET:** `HttpClient` calls with user-provided URLs, URL redirect following to internal networks, accessing cloud metadata endpoints (169.254.169.254).

### Mitigation

```csharp

// Validate and restrict outbound URLs
public static class UrlValidator
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "api.example.com",
        "cdn.example.com"
    };

    public static bool IsAllowed(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Block non-HTTPS
        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // Block private/internal IPs
        if (IPAddress.TryParse(uri.Host, out var ip))
        {
            if (IsPrivateOrReserved(ip))
                return false;
        }

        // Allowlist hosts
        return AllowedHosts.Contains(uri.Host);
    }

    private static bool IsPrivateOrReserved(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        return bytes[0] switch
        {
            10 => true,                                         // 10.0.0.0/8
            127 => true,                                        // 127.0.0.0/8
            169 when bytes[1] == 254 => true,                   // 169.254.0.0/16 (link-local / cloud metadata)
            172 when bytes[1] >= 16 && bytes[1] <= 31 => true,  // 172.16.0.0/12
            192 when bytes[1] == 168 => true,                   // 192.168.0.0/16
            _ => false
        };
    }
}

// Usage in an endpoint
app.MapPost("/fetch", async (FetchRequest request, IHttpClientFactory factory) =>
{
    if (!UrlValidator.IsAllowed(request.Url))
        return Results.BadRequest("URL not allowed");

    var client = factory.CreateClient();
    var response = await client.GetStringAsync(request.Url);
    return Results.Ok(response);
});

```text

```csharp

// Configure HttpClient to disable automatic redirect following
builder.Services.AddHttpClient("external", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    AllowAutoRedirect = false // Prevent redirect-based SSRF
});

```text

**Gotcha:** DNS rebinding can bypass IP allowlists -- an attacker's domain resolves to a public IP during validation but to an internal IP during the actual request. Pin DNS resolution or re-validate after connection.

---

## Deprecated Security Patterns

This skill is the **canonical owner** of deprecated security pattern warnings. Other skills should cross-reference here rather than duplicating these warnings.

### Code Access Security (CAS)

CAS is **not supported** in .NET Core/.NET 5+. Code that references `System.Security.Permissions`, `SecurityPermission`, or `[SecurityCritical]`/`[SecuritySafeCritical]` attributes for CAS purposes must be removed or replaced with OS-level security boundaries (containers, process isolation).

### AllowPartiallyTrustedCallers (APTCA)

The `[AllowPartiallyTrustedCallers]` attribute has **no effect** in .NET Core/.NET 5+. The partial-trust model is gone. Remove APTCA attributes during migration. Use standard authorization and input validation instead.

### .NET Remoting

.NET Remoting is **not available** in .NET Core/.NET 5+. It was inherently insecure due to unrestricted deserialization of remote objects. Replace with:
- gRPC for cross-process/cross-machine RPC (see [skill:dotnet-cryptography] for transport security)
- Named pipes for same-machine IPC
- HTTP APIs for service-to-service communication

### DCOM

Distributed COM (DCOM) is **Windows-only and not supported** in .NET Core/.NET 5+. Replace with gRPC, REST APIs, or message queues for distributed communication.

### BinaryFormatter

`BinaryFormatter` is **obsolete as error** (SYSLIB0011) in .NET 8 and **removed** in .NET 9+. It enables arbitrary code execution through deserialization attacks. Replace with:
- `System.Text.Json` for JSON serialization
- MessagePack or Protocol Buffers for binary formats
- `XmlSerializer` with strict type allowlists for XML scenarios

Do **not** set `System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization` to `true` as a workaround.

---

## Agent Gotchas

1. **Do not use `[AllowAnonymous]` without explicit justification** -- it overrides the global fallback policy. Mark each anonymous endpoint with a comment explaining why.
2. **Do not disable HTTPS redirection for convenience** -- use `dotnet dev-certs https --trust` for local development instead.
3. **Do not log raw request bodies** -- they may contain credentials, tokens, or PII. Use `HttpLoggingFields` to select safe fields.
4. **Do not rely solely on client-side validation** -- always validate on the server. Razor form validation is for UX, not security.
5. **Do not use `FromSqlRaw` with string interpolation** -- use `FromSqlInterpolated` which auto-parameterizes.
6. **Do not store secrets in `appsettings.json`** -- use user secrets for development and environment variables or managed identity for production. See [skill:dotnet-secrets-management].
7. **Do not generate security-sensitive code using deprecated patterns** -- CAS, APTCA, .NET Remoting, DCOM, and BinaryFormatter are all unsupported in modern .NET. See the Deprecated Security Patterns section above.

---

## Prerequisites

- .NET 8.0+ (LTS baseline)
- ASP.NET Core 8.0+ for security middleware, anti-forgery, and rate limiting
- Microsoft.AspNetCore.Identity for authentication/identity (if using A07 patterns)

---

## References

- [OWASP Top 10 (2021)](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-10.0)
- [Secure Coding Guidelines for .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)
- [Security in .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [ASP.NET Core Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-10.0)
- [Rate Limiting Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)
- [NuGet Package Source Mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [BinaryFormatter Migration Guide](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/)
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
