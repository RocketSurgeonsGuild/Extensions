available after the circuit is established. Login/logout pages must use Static SSR (no `@rendermode`) so they have
access to `HttpContext` for cookie operations.

---

## Role and Policy-Based Authorization

### Page-Level Authorization

```razor

@page "/admin"
@attribute [Authorize(Roles = "Admin")]

<h1>Admin Panel</h1>

```text

```razor

@page "/products/manage"
@attribute [Authorize(Policy = "ProductManager")]

<h1>Manage Products</h1>

```text

### Defining Policies

```csharp

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ProductManager", policy =>
        policy.RequireRole("Admin", "ProductManager"))
    .AddPolicy("CanDeleteOrders", policy =>
        policy.RequireClaim("permission", "orders.delete")
              .RequireAuthenticatedUser())
    .AddPolicy("MinimumAge", policy =>
        policy.AddRequirements(new MinimumAgeRequirement(18)));

```text

### Custom Authorization Handler

```csharp

public sealed class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}

public sealed class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var dateOfBirthClaim = context.User.FindFirst("date_of_birth");
        if (dateOfBirthClaim is not null
            && DateOnly.TryParse(dateOfBirthClaim.Value, out var dob))
        {
            var age = DateOnly.FromDateTime(DateTime.UtcNow).Year - dob.Year;
            if (age >= requirement.MinimumAge)
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}

// Register
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

```text

### Procedural Authorization in Components

```razor

@inject IAuthorizationService AuthorizationService

@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    private bool canEdit;

    protected override async Task OnInitializedAsync()
    {
        if (AuthState is not null)
        {
            var state = await AuthState;
            var result = await AuthorizationService.AuthorizeAsync(
                state.User, "CanEditProducts");
            canEdit = result.Succeeded;
        }
    }
}

```text

---

## External Identity Providers

### Adding External Providers

```csharp

builder.Services.AddAuthentication()
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Auth:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Microsoft:ClientSecret"]!;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Auth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
    });

```text

### External Login Flow per Hosting Model

| Hosting Model                  | Flow                                  | Notes                            |
| ------------------------------ | ------------------------------------- | -------------------------------- |
| InteractiveServer / Static SSR | Standard OAuth redirect (server-side) | Cookie stored after callback     |
| InteractiveWebAssembly         | OIDC with PKCE (client-side)          | Token stored in browser          |
| Hybrid (MAUI)                  | `WebAuthenticator` or MSAL            | Platform-specific secure storage |

For WASM, configure the OIDC provider in the client project:

```csharp

// Client Program.cs
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = "https://login.microsoftonline.com/{tenant}";
    options.ProviderOptions.ClientId = "{client-id}";
    options.ProviderOptions.ResponseType = "code";
});

```text

For MAUI Hybrid:

```csharp

var result = await WebAuthenticator.Default.AuthenticateAsync(
    new Uri("https://login.example.com/authorize"),
    new Uri("myapp://callback"));
var token = result.AccessToken;

```text

---

## Agent Gotchas

1. **Do not access `HttpContext` in interactive components.** `HttpContext` is only available during the initial HTTP
   request. After the SignalR circuit is established (InteractiveServer) or the WASM runtime loads, it is `null`. Use
   `AuthenticationStateProvider` or `CascadingAuthenticationState` instead.
2. **Do not rely on cookies for cross-origin or delegated API access in WASM.** Use OIDC/JWT with
   `AuthorizationMessageHandler` for cross-origin APIs. Same-origin and Backend-for-Frontend (BFF) cookie auth remains
   valid for WASM apps.
3. **Do not render login/logout pages in Interactive mode.** `SignInManager` requires `HttpContext` to set/clear
   cookies. Login and logout pages must use Static SSR render mode.
4. **Do not store tokens in `localStorage` without considering XSS.** If the app is vulnerable to XSS, tokens in
   `localStorage` can be stolen. Use `sessionStorage` (cleared on tab close) or the OIDC library's built-in storage
   mechanisms with PKCE.
5. **Do not forget `AddCascadingAuthenticationState()`.** Without it, `[CascadingParameter] Task<AuthenticationState>`
   is always `null` in components, silently breaking auth checks.
6. **Do not use `AddIdentity` and `AddDefaultIdentity` together.** `AddDefaultIdentity` includes UI scaffolding;
   `AddIdentity` does not. Choose one based on whether you want the default Identity UI pages.

---

## Prerequisites

- .NET 8.0+ (Blazor Web App with render modes, `AddCascadingAuthenticationState` service registration)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` for Identity with EF Core
- `Microsoft.AspNetCore.Identity.UI` for default Identity UI scaffolding
- `Microsoft.AspNetCore.Authentication.MicrosoftAccount` / `.Google` for external providers
- `Microsoft.Authentication.WebAssembly.Msal` for WASM with Microsoft Identity (Azure AD/Entra)

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

- [Blazor Authentication and Authorization](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0)
- [Blazor Server Auth](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/?view=aspnetcore-10.0)
- [Blazor WebAssembly Auth](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/?view=aspnetcore-10.0)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0)
- [External Login Providers](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-10.0)
- [Role/Policy-Based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0)
````
