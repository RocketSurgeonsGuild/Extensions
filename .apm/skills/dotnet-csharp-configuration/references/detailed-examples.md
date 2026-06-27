        {
            return Ok(new { version = "v2", dashboard = "new" });
        }

        return Ok(new { version = "v1", dashboard = "legacy" });
    }
}

```text

### Feature Gate Attribute

```csharp

// Entire endpoint gated on feature flag
[FeatureGate("BetaSearch")]
[HttpGet("search")]
public async Task<IActionResult> Search(string query, CancellationToken ct = default)
{
    var results = await _searchService.SearchAsync(query, ct);
    return Ok(results);
}

```text

### Feature Filters

| Filter       | Purpose                                                  |
| ------------ | -------------------------------------------------------- |
| `Percentage` | Enable for N% of requests (random)                       |
| `TimeWindow` | Enable between start/end dates                           |
| `Targeting`  | Enable for specific users, groups, or rollout percentage |
| Custom       | Implement `IFeatureFilter` for domain-specific logic     |

### Custom Feature Filter

```csharp

[FilterAlias("Browser")]
public sealed class BrowserFeatureFilter(IHttpContextAccessor accessor) : IFeatureFilter
{
    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var userAgent = accessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "";
        var settings = context.Parameters.Get<BrowserFilterSettings>();

        return Task.FromResult(
            settings?.AllowedBrowsers?.Any(b =>
                userAgent.Contains(b, StringComparison.OrdinalIgnoreCase)) ?? false);
    }
}

public sealed class BrowserFilterSettings
{
    public string[] AllowedBrowsers { get; init; } = [];
}

// Register
builder.Services.AddFeatureManagement()
    .AddFeatureFilter<BrowserFeatureFilter>();

```text

---

## Named Options

Use named options when you need multiple instances of the same options type (e.g., multiple API clients).

```csharp

// Registration with names
builder.Services
    .AddOptions<ApiClientOptions>("GitHub")
    .BindConfiguration("ApiClients:GitHub");

builder.Services
    .AddOptions<ApiClientOptions>("Jira")
    .BindConfiguration("ApiClients:Jira");

// Resolution via IOptionsSnapshot<T> or IOptionsMonitor<T>
public sealed class ApiClientFactory(IOptionsSnapshot<ApiClientOptions> snapshot)
{
    public HttpClient CreateFor(string name)
    {
        var options = snapshot.Get(name); // "GitHub" or "Jira"
        return new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
    }
}

```text

---

## Post-Configuration

Apply defaults or overrides after all configuration sources have been processed.

```csharp

builder.Services.PostConfigure<SmtpOptions>(options =>
{
    // Ensure a default port if none specified
    if (options.Port == 0)
    {
        options.Port = options.UseSsl ? 465 : 25;
    }
});

```text

---

## Testing Configuration

```csharp

[Fact]
public void SmtpOptions_Validates_InvalidPort()
{
    var options = new SmtpOptions
    {
        Host = "smtp.example.com",
        FromAddress = "test@example.com",
        Port = 25,
        UseSsl = true
    };

    var validator = new SmtpOptionsValidator();
    var result = validator.Validate(null, options);

    Assert.True(result.Failed);
    Assert.Contains("Port 25 does not support SSL", result.FailureMessage);
}

[Fact]
public void Configuration_BindsCorrectly()
{
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "smtp.test.com",
            ["Smtp:Port"] = "465",
            ["Smtp:FromAddress"] = "test@test.com",
        })
        .Build();

    var options = new SmtpOptions();
    config.GetSection("Smtp").Bind(options);

    Assert.Equal("smtp.test.com", options.Host);
    Assert.Equal(465, options.Port);
}

```text

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

- [Options pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [Configuration in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [User secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Feature management in .NET](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core)
- [IValidateOptions](https://learn.microsoft.com/en-us/dotnet/core/extensions/options#options-validation)
- [.NET Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
````
