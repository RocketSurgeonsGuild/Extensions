
### Combining with Route Groups

Apply validation filters at the route group level for consistent validation across all endpoints in a group (see
[skill:dotnet-minimal-apis] for route group patterns):

```csharp

var orders = app.MapGroup("/api/orders")
    .AddEndpointFilter<FluentValidationFilter<CreateOrderRequest>>();

```csharp

**Gotcha:** Filter execution order matters -- first-registered filter is outermost. Register validation filters after
logging but before authorization enrichment so that invalid requests are rejected early without unnecessary processing.

---

## Error Responses

Use the ProblemDetails standard (RFC 9457) for consistent API error responses. ASP.NET Core has built-in support via
`TypedResults.ValidationProblem()` and `IProblemDetailsService`.

### ValidationProblem Response

```csharp

// Returns HTTP 400 with RFC 9457-compliant body
app.MapPost("/api/products", async (CreateProductDto dto, IValidator<CreateProductDto> validator) =>
{
    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid)
    {
        // Produces: { "type": "...", "title": "...", "status": 400, "errors": { ... } }
        return TypedResults.ValidationProblem(result.ToDictionary());
    }

    // ... create product
    return TypedResults.Created($"/api/products/{product.Id}", product);
});

```text

### Customizing ProblemDetails

```csharp

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["instance"] =
            context.HttpContext.Request.Path.Value;
    };
});

var app = builder.Build();
app.UseStatusCodePages();
app.UseExceptionHandler();

```text

### IProblemDetailsService for Global Error Handling

```csharp

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var problemDetailsService = context.RequestServices
            .GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails =
            {
                Title = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            }
        });
    });
});

```text

**Gotcha:** `ConfigureHttpJsonOptions` applies to Minimal APIs only, not MVC controllers. Validation error formatting
(e.g., camelCase property names in the `errors` dictionary) may differ between Minimal APIs and MVC if JSON options are
not configured consistently. For MVC controllers, configure via `builder.Services.AddControllers().AddJsonOptions(...)`.

---

## Security-Focused Validation

Input validation is a first line of defense against injection and abuse. These patterns complement the OWASP security
principles in [skill:dotnet-security-owasp] with practical validation techniques.

### ReDoS Prevention

Regular expressions with backtracking can be exploited to cause catastrophic performance degradation (Regular Expression
Denial of Service). Always apply timeouts or use source-generated regex.

```csharp

// PREFERRED: [GeneratedRegex] -- compiled at build time, AOT-compatible (.NET 7+).
// Combine with RegexOptions.NonBacktracking or a timeout for ReDoS safety.
public static partial class InputPatterns
{
    [GeneratedRegex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    public static partial Regex EmailPattern();

    [GeneratedRegex(@"^[A-Z]{2,4}-\d{4,8}$")]
    public static partial Regex SkuPattern();
}

// Usage in validation
if (!InputPatterns.EmailPattern().IsMatch(input))
{
    return TypedResults.ValidationProblem(
        new Dictionary<string, string[]>
        {
            ["email"] = ["Invalid email format"]
        });
}

```text

```csharp

// FALLBACK: Regex with explicit timeout (when source generation is not available)
var pattern = new Regex(
    @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
    RegexOptions.Compiled,
    matchTimeout: TimeSpan.FromSeconds(1));

try
{
    if (!pattern.IsMatch(input))
        return TypedResults.BadRequest("Invalid format");
}
catch (RegexMatchTimeoutException)
{
    return TypedResults.BadRequest("Input validation timed out");
}

```text

### Allowlist vs Denylist

Always prefer allowlist validation over denylist. Denylists are inherently incomplete because new attack vectors bypass
them.

```csharp

// CORRECT: Allowlist -- only permit known-good values
private static readonly FrozenSet<string> AllowedFileExtensions =
    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
    .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

public static bool IsAllowedExtension(string filename) =>
    AllowedFileExtensions.Contains(Path.GetExtension(filename));

// WRONG: Denylist -- attackers find extensions not in the list
// private static readonly string[] BlockedExtensions = [".exe", ".bat", ".cmd", ".ps1"];

```powershell

```csharp

// Allowlist for input characters
public static bool IsValidUsername(string username) =>
    username.Length is >= 3 and <= 50
    && username.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

```text

### Max Length Enforcement

Enforce maximum length on all user-controlled string inputs to prevent memory exhaustion and buffer-based attacks:

```csharp

[ValidatableType]
public partial class SearchRequest
{
    [Required]
    [StringLength(200)]  // Always set max length on search inputs
    public required string Query { get; set; }

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

```text

Also enforce at the Kestrel level for defense in depth:

```csharp

// Configure BEFORE builder.Build()
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024; // 32 KB
});

```text

### File Upload Validation

Validate file uploads by content type, size, and extension -- never trust the client-provided `Content-Type` header
alone:

```csharp

app.MapPost("/api/uploads", async (IFormFile file) =>
{
    // 1. Validate extension (allowlist)
    var extension = Path.GetExtension(file.FileName);
    if (!AllowedFileExtensions.Contains(extension))
        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["file"] = [$"File type '{extension}' is not allowed"]
            });

    // 2. Validate file size
    const long maxSize = 5 * 1024 * 1024; // 5 MB
    if (file.Length > maxSize)
        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["file"] = [$"File size exceeds {maxSize / (1024 * 1024)} MB limit"]
            });

    // 3. Validate content by reading magic bytes (not Content-Type header)
    using var stream = file.OpenReadStream();
    const int headerSize = 12; // Need 12 bytes for WebP (RIFF + WEBP)
    var header = new byte[headerSize];

    // Guard against files shorter than header size
    int bytesRead = await stream.ReadAtLeastAsync(header, headerSize, throwOnEndOfStream: false);
    if (bytesRead < headerSize)
        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["file"] = ["File is too small to be a valid image"]
            });

    stream.Position = 0;

    if (!IsValidImageHeader(header))
        return TypedResults.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["file"] = ["File content does not match an allowed image format"]
            });

    // 4. Save with a generated filename (never use the original)
    var safeName = $"{Guid.NewGuid()}{extension}";
    var path = Path.Combine("uploads", safeName);
    using var output = File.Create(path);
    await stream.CopyToAsync(output);

    return TypedResults.Ok(new { FileName = safeName });
})
.DisableAntiforgery(); // Only if using JWT/bearer auth, not cookie auth

static bool IsValidImageHeader(ReadOnlySpan<byte> header) =>
    header[..2].SequenceEqual(new byte[] { 0xFF, 0xD8 })                                   // JPEG
    || header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }) // PNG
    || header[..4].SequenceEqual("GIF8"u8)                                                  // GIF
    || (header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8));      // WebP

```text

For OWASP injection prevention beyond input validation (SQL injection, XSS, command injection), see
[skill:dotnet-security-owasp].

---

## Agent Gotchas

1. **Do not use FluentValidation auto-validation pipeline** -- it was deprecated and removed in FluentValidation 11. Use
   manual validation or endpoint filters with `IValidator<T>` instead.
2. **Do not mix validation frameworks on the same DTO** -- pick one (Data Annotations OR FluentValidation OR .NET 10
   built-in) per model type. Mixing causes confusing partial validation.
3. **Do not use `Regex` without a timeout or `[GeneratedRegex]`** -- unbounded regex matching on user input enables
   ReDoS attacks. Always set `matchTimeout` or use source-generated regex.
4. **Do not trust client-provided `Content-Type` headers** -- validate file content by reading magic bytes. Attackers
   rename executables with image extensions.
5. **Do not forget `validateAllProperties: true`** -- `Validator.TryValidateObject` without this flag only validates
   `[Required]` attributes, silently skipping `[Range]`, `[StringLength]`, and others.
6. **Do not use denylist validation for security** -- denylists are inherently incomplete. Always validate against an
   allowlist of known-good values.
7. **Do not omit max length on string inputs** -- unbounded strings enable memory exhaustion. Apply `[StringLength]` or
   `[MaxLength]` to every user-controlled string property.

---

## Prerequisites

- .NET 8.0+ (LTS baseline for endpoint filters, ProblemDetails, Data Annotations)
- .NET 10.0 for built-in validation (`AddValidation`, `[ValidatableType]`, `Microsoft.Extensions.Validation`)
- `Microsoft.Extensions.Validation` package for .NET 10 built-in validation
- `FluentValidation` and `FluentValidation.DependencyInjectionExtensions` for FluentValidation patterns
- .NET 7+ for `[GeneratedRegex]` source-generated regular expressions

---

## References

- [Model Validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
- [Minimal API Filters](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters?view=aspnetcore-10.0)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/en/latest/aspnet.html)
- [ProblemDetails (RFC 9457)](https://www.rfc-editor.org/rfc/rfc9457)
- [Handle Errors in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-10.0)
- [OWASP Input Validation Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html)
- [.NET Regular Expression Source Generators](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators)
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
