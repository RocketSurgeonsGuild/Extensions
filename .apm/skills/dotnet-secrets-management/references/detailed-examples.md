builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// No password in the connection string -- identity is resolved from the environment

```

**When to use managed identity:**

- Production and staging environments hosted on cloud platforms
- Any service-to-service communication where the platform supports identity federation
- Database connections, message queue access, storage access

**When you still need secrets:**

- Third-party APIs that only support API keys
- Legacy systems without identity federation
- Local development (use user secrets as a fallback)

---

## Anti-Patterns

### Secrets in Source Control

```csharp

// NEVER: hardcoded secrets in source code
// Replaced real keys with placeholders. Do NOT store real keys in source.
private const string ApiKey = "<API_KEY_PLACEHOLDER>"; // WRONG: hardcoded secret
private const string ConnectionString = "Server=prod-db;Database=myapp;User=sa;Password=<DB_PASSWORD_PLACEHOLDER>"; // WRONG: hardcoded secret

```

**Fix:** Use user secrets (dev) or environment variables (production). See sections above.

### Secrets in appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultDb": "Server=prod-db;Password=<DB_PASSWORD_PLACEHOLDER>"
  }
}
```

**Fix:** `appsettings.json` should contain only non-sensitive defaults. Use placeholder values that fail visibly:

```json
{
  "ConnectionStrings": {
    "DefaultDb": "Server=localhost;Database=myapp;Integrated Security=true"
  },
  "Smtp": {
    "ApiKey": "REPLACE_VIA_ENV_OR_USER_SECRETS"
  }
}
```

### Hardcoded Connection Strings

```csharp

// NEVER: connection strings directly in code
// Use IConfiguration or environment variables instead of hardcoded credentials.
var connection = new SqlConnection("Server=prod-db;Database=myapp;User=sa;Password=<DB_PASSWORD_PLACEHOLDER>"); // WRONG

```

**Fix:** Always resolve connection strings from `IConfiguration`:

```csharp

// Correct: resolve from configuration
public sealed class OrderRepository(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultDb")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultDb is not configured");
}

// Better: use Options pattern with validation
public sealed class OrderRepository(IOptions<DatabaseOptions> options)
{
    private readonly string _connectionString = options.Value.ConnectionString;
}

```

### Logging Secrets

```csharp

// NEVER: log secret values
logger.LogInformation("Using API key: {ApiKey}", apiKey);       // WRONG
logger.LogDebug("Connection string: {Conn}", connectionString); // WRONG

```

**Fix:** Log that a secret was loaded, not its value:

```csharp

logger.LogInformation("API key configured: {IsConfigured}", !string.IsNullOrEmpty(apiKey));
logger.LogInformation("Database connection configured for {Server}", new SqlConnectionStringBuilder(connectionString).DataSource);

```

---

## Agent Gotchas

1. **Do not generate code with hardcoded secrets** -- always use `IConfiguration` or `IOptions<T>` to resolve secrets.
   Even in examples, use placeholder values.
2. **Do not put real secrets in `appsettings.json`** -- it is committed to source control. Use user secrets for
   development, environment variables for production.
3. **Do not use `{ get; init; }` on Options classes** -- the configuration binder requires mutable setters. Use
   `{ get; set; }` with data annotation validation instead.
4. **Do not skip `ValidateOnStart()`** -- without it, missing secrets cause runtime failures at first use rather than a
   clear startup error.
5. **Do not log secret values** -- log whether a secret is configured (`IsConfigured: true/false`) or metadata (server
   name from connection string), never the value.
6. **Do not use `IOptions<T>` for secrets that rotate** -- use `IOptionsMonitor<T>` for runtime-reloadable secrets so
   rotation does not require a restart.
7. **Do not bake secrets into Docker images** -- use environment variables or mounted secrets at container runtime.

---

## Prerequisites

- .NET 8.0+ (LTS baseline)
- `Microsoft.Extensions.Configuration.UserSecrets` (included in ASP.NET Core SDK; add manually for console apps)
- `Microsoft.Extensions.Options.DataAnnotations` for `ValidateDataAnnotations()`

---

## References

- [Safe Storage of App Secrets in Development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0)
- [Options Pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-10.0)
- [Secure Coding Guidelines for .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)
- [Use Managed Identities](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)

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
