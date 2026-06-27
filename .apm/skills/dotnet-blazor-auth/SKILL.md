---
name: dotnet-blazor-auth
category: web
subcategory: blazor
description: Implements Blazor auth flows -- login/logout, AuthorizeView, Identity UI, OIDC.
license: MIT
targets: ['*']
tags: [ui, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for ui tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-blazor-auth

Authentication and authorization across all Blazor hosting models. Covers AuthorizeView, CascadingAuthenticationState,
Identity UI scaffolding, role/policy-based authorization, per-hosting-model auth flow differences (cookie vs token), and
external identity providers.

## Scope

- Auth flow per Blazor hosting model (Server, WASM, Auto, SSR, Hybrid)
- AuthorizeView and CascadingAuthenticationState patterns
- Identity UI scaffolding and customization
- Role/policy-based authorization in Blazor
- Client-side token handling and external identity providers
- Explicit login/logout/auth UI implementation tasks for Blazor apps

## Out of scope

- JWT token generation and validation -- see [skill:dotnet-api-security]
- OWASP security principles -- see [skill:dotnet-security-owasp]
- CSRF/XSS/CSP/rate-limiting hardening without auth-flow work -- see [skill:dotnet-security-owasp]
- Hardening-only reviews of existing login pages without auth-flow implementation changes -- see
  [skill:dotnet-security-owasp]
- bUnit testing of auth components -- see [skill:dotnet-blazor-testing]
- E2E auth testing -- see [skill:dotnet-playwright]
- UI framework selection -- see [skill:dotnet-ui-chooser]

Cross-references: [skill:dotnet-api-security] for API-level auth, [skill:dotnet-security-owasp] for OWASP principles,
[skill:dotnet-blazor-patterns] for hosting models, [skill:dotnet-blazor-components] for component architecture,
[skill:dotnet-blazor-testing] for bUnit testing, [skill:dotnet-playwright] for E2E testing, [skill:dotnet-ui-chooser]
for framework selection.

Routing note: do not load this skill for OWASP hardening reviews unless the task explicitly includes Blazor auth flow/UI
implementation.

---

## Auth Flow per Hosting Model

Authentication patterns differ significantly across Blazor hosting models:

| Concern           | InteractiveServer           | InteractiveWebAssembly             | InteractiveAuto                           | Static SSR                           | Hybrid                          |
| ----------------- | --------------------------- | ---------------------------------- | ----------------------------------------- | ------------------------------------ | ------------------------------- |
| Auth mechanism    | Cookie-based (server-side)  | Token-based (JWT/OIDC)             | Cookie (Server phase), Token (WASM phase) | Cookie-based (standard ASP.NET Core) | Platform-native or cookie       |
| User state access | Direct `HttpContext` access | `AuthenticationStateProvider`      | Varies by phase                           | `HttpContext`                        | Platform auth APIs              |
| Token storage     | Not needed (cookie)         | `localStorage` or `sessionStorage` | Transition from cookie to token           | Not needed (cookie)                  | Secure storage (Keychain, etc.) |
| Refresh handling  | Circuit reconnection        | Token refresh via interceptor      | Automatic                                 | Standard cookie renewal              | Platform-specific               |

### InteractiveServer Auth

Server-side Blazor uses cookie authentication. The user authenticates via a standard ASP.NET Core login flow, and the
cookie is sent with the initial HTTP request that establishes the SignalR circuit.

````csharp

// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

```text

**Gotcha:** `HttpContext` is available during the initial HTTP request but is `null` inside interactive components after
the SignalR circuit is established. Do not access `HttpContext` in interactive component lifecycle methods. Use
`AuthenticationStateProvider` instead.

### InteractiveWebAssembly Auth

WASM runs in the browser. Cookie auth works for same-origin APIs (and Backend-for-Frontend / BFF patterns), but
token-based auth (OIDC/JWT) is the standard approach for cross-origin APIs and delegated access scenarios:

```csharp

// Client Program.cs (WASM)
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = "https://login.example.com";
    options.ProviderOptions.ClientId = "blazor-wasm-client";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("api");
});

```text

```csharp

// Attach tokens to API calls using BaseAddressAuthorizationMessageHandler
// (auto-attaches tokens for requests to the app's base address)
builder.Services.AddHttpClient("API", client =>
    client.BaseAddress = new Uri("https://api.example.com"))
    .AddHttpMessageHandler(sp =>
        sp.GetRequiredService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: ["https://api.example.com"],
                scopes: ["api"]));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

```text

### InteractiveAuto Auth

Auto mode starts as InteractiveServer (cookie auth), then transitions to WASM (token auth). Handle both:

```csharp

// Server Program.cs
builder.Services.AddAuthentication()
    .AddCookie()
    .AddJwtBearer(); // For WASM API calls after transition

builder.Services.AddCascadingAuthenticationState();

```text

### Hybrid (MAUI) Auth

```csharp

// Register platform-specific auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MauiAuthStateProvider>();

// Custom provider using secure storage
public class MauiAuthStateProvider : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}

```text

---

## AuthorizeView

`AuthorizeView` conditionally renders content based on the user's authentication and authorization state.

### Basic Usage

```razor

<AuthorizeView>
    <Authorized>
        <p>Welcome, @context.User.Identity?.Name!</p>
        <a href="/Account/Logout">Log out</a>
    </Authorized>
    <NotAuthorized>
        <a href="/Account/Login">Log in</a>
    </NotAuthorized>
    <Authorizing>
        <p>Checking authentication...</p>
    </Authorizing>
</AuthorizeView>

```text

### Role-Based

```razor

<AuthorizeView Roles="Admin,Manager">
    <Authorized>
        <AdminDashboard />
    </Authorized>
    <NotAuthorized>
        <p>You do not have access to the admin dashboard.</p>
    </NotAuthorized>
</AuthorizeView>

```text

### Policy-Based

```razor

<AuthorizeView Policy="CanEditProducts">
    <Authorized>
        <button @onclick="EditProduct">Edit</button>
    </Authorized>
</AuthorizeView>

```text

```csharp

// Register policy in Program.cs
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditProducts", policy =>
        policy.RequireClaim("permission", "products.edit"));

```csharp

---

## CascadingAuthenticationState

`CascadingAuthenticationState` provides the current `AuthenticationState` as a cascading parameter to all descendant
components.

### Setup

```csharp

// Program.cs -- register cascading auth state
builder.Services.AddCascadingAuthenticationState();

```csharp

This replaces wrapping the entire app in `<CascadingAuthenticationState>` (the older pattern). The service-based
registration (.NET 8+) is preferred.

### Consuming Auth State in Components

```razor

@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    private string? userName;

    protected override async Task OnInitializedAsync()
    {
        if (AuthState is not null)
        {
            var state = await AuthState;
            userName = state.User.Identity?.Name;
        }
    }
}

```text

### Accessing Claims

```csharp

var state = await AuthState;
var user = state.User;

// Check authentication
if (user.Identity?.IsAuthenticated == true)
{
    var email = user.FindFirst(ClaimTypes.Email)?.Value;
    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    var isAdmin = user.IsInRole("Admin");
}

```text

---

## Identity UI Scaffolding

ASP.NET Core Identity provides a complete authentication system with registration, login, email confirmation, password
reset, and two-factor authentication.

### Adding Identity to a Blazor Web App

```bash

# Add Identity scaffolding
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI

```bash

```csharp

// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

```text

### Scaffolding Identity Pages

```bash

# Scaffold individual Identity pages for customization
dotnet aspnet-codegenerator identity -dc ApplicationDbContext --files "Account.Login;Account.Register;Account.Logout"

```bash

### Custom Identity UI with Blazor Components

For a fully Blazor-native auth experience, create Blazor components that call Identity APIs:

```razor

@page "/Account/Login"
@inject SignInManager<ApplicationUser> SignInManager
@inject NavigationManager Navigation

<EditForm Model="loginModel" OnValidSubmit="HandleLogin" FormName="login" Enhance>
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div>
        <InputText @bind-Value="loginModel.Email" placeholder="Email" />
    </div>
    <div>
        <InputText @bind-Value="loginModel.Password" type="password" placeholder="Password" />
    </div>
    <div>
        <InputCheckbox @bind-Value="loginModel.RememberMe" /> Remember me
    </div>

    <button type="submit">Log in</button>
</EditForm>

@if (!string.IsNullOrEmpty(errorMessage))
{
    <p class="text-danger">@errorMessage</p>
}

@code {
    [SupplyParameterFromForm]
    private LoginModel loginModel { get; set; } = new();

    private string? errorMessage;

    private async Task HandleLogin()
    {
        var result = await SignInManager.PasswordSignInAsync(
            loginModel.Email, loginModel.Password,
            loginModel.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            Navigation.NavigateTo("/", forceLoad: true);
        }
        else if (result.RequiresTwoFactor)
        {
            Navigation.NavigateTo("/Account/LoginWith2fa");
        }
        else if (result.IsLockedOut)
        {
            errorMessage = "Account is locked. Try again later.";
        }
        else
        {
            errorMessage = "Invalid login attempt.";
        }
    }
}

```text

**Gotcha:** `SignInManager` uses `HttpContext` to set cookies. In Interactive render modes, `HttpContext` is not
available after the circuit is established. Login/logout pages must use Static SSR (no `@rendermode`) so they have

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
