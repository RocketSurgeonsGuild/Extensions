# Security in .NET 10

Authentication, authorization, rate limiting, and secure API design.

## JWT Authentication

### Configuration

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.example.com";
        options.Audience = "api://my-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// AFTER UseRouting, BEFORE endpoints
app.UseAuthentication();
app.UseAuthorization();
```

### Minimal API Protection

```csharp
// Require authentication
app.MapGet("/admin-only", [Authorize] () => "Secret");

// Require specific role
app.MapDelete("/users/{id}", [Authorize(Roles = "Admin")] (int id) => ...);

// Require specific policy
app.MapPost("/premium-feature", [Authorize(Policy = "Premium")] () => ...);

// Allow anonymous
app.MapGet("/public", [AllowAnonymous] () => "Public info");
```

### Custom Policy

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Premium", policy =>
        policy.RequireClaim("subscription", "premium", "enterprise"));

    options.AddPolicy("CanDeleteUsers", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("permissions", "user:delete"));
});
```

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type")
              .AllowCredentials();
    });

    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// BEFORE authentication
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");
```

## Rate Limiting

### Basic Setup

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Per-endpoint policy
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Token bucket for burst traffic
    options.AddTokenBucketLimiter("token", opt =>
    {
        opt.TokenLimit = 20;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.TokensPerPeriod = 10;
        opt.AutoReplenishment = true;
    });

    // Sliding window for smooth limiting
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.SegmentsPerWindow = 5;
    });
});

var app = builder.Build();

// AFTER authentication, BEFORE endpoints
app.UseRateLimiter();
```

### Apply to Endpoints

```csharp
// Global rate limit
app.MapControllers().RequireRateLimiting("fixed");

// Per-endpoint rate limit
app.MapGet("/api/search", SearchHandler)
    .RequireRateLimiting("token");

// Anonymous vs authenticated
app.MapGet("/public-api", PublicHandler)
    .RequireRateLimiting("anonymous");

app.MapGet("/authenticated-api", AuthenticatedHandler)
    .RequireRateLimiting("authenticated");
```

### Custom Rate Limit Policy

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("per-user", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return RateLimitPartition.GetTokenBucketLimiter(
            userId ?? "anonymous",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 50
            });
    });
});
```

## Middleware Order (CRITICAL)

```csharp
// CORRECT ORDER - do not change
app.UseExceptionHandler();
app.UseHttpsRedirection();

// CORS before auth
app.UseCors();

// Rate limiting before auth
app.UseRateLimiter();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Other middleware
app.UseOutputCache();
app.UseResponseCaching();

// Endpoints
app.MapControllers();
```

**Why this order matters:**

1. **ExceptionHandler** - Catches all errors
2. **HttpsRedirection** - Enforce HTTPS early
3. **CORS** - Check origin before auth
4. **RateLimiter** - Throttle before expensive auth
5. **Authentication** - Who are you?
6. **Authorization** - What can you do?
7. **OutputCache** - Cache authorized responses
8. **Endpoints** - Actually handle the request

## OpenAPI Security

### Document Security Requirements

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.SecurityRequirements = new[]
        {
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            }
        };

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        };

        return Task.CompletedTask;
    });
});
```

### Endpoint Security Metadata

```csharp
app.MapPost("/users", CreateUser.Handle)
    .WithOpenApi(operation =>
    {
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            }
        };
        return operation;
    });
```

## HTTPS Enforcement

```csharp
// Development - trust dev certificate
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    // Production - strict HTTPS
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}
```

## Secrets Management

```csharp
// User secrets (development)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Key Vault (production)
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// Usage
var connectionString = builder.Configuration.GetConnectionString("Database");
var apiKey = builder.Configuration["ApiKey"];
```

## Secure Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";

    await next();
});
```

## Anti-Forgery

```csharp
// Enable anti-forgery
builder.Services.AddAntiforgery();

// Validate on state-changing endpoints
app.MapPost("/users", async (
    CreateUserRequest dto,
    HttpContext context,
    IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);
    // ... handle request
});
```

## Security Best Practices

### ✅ DO

- Use HTTPS everywhere in production
- Validate JWT tokens properly
- Apply principle of least privilege
- Use rate limiting on all endpoints
- Keep middleware order correct
- Store secrets in Key Vault (not appsettings)
- Use typed authentication schemes
- Enable HSTS in production

### ❌ DON'T

- Trust user input
- Store secrets in code or appsettings
- Skip HTTPS in production
- Use wildcard CORS in production
- Forget to validate tokens
- Use `[Authorize]` without authentication
- Skip rate limiting on public APIs
- Log sensitive data
