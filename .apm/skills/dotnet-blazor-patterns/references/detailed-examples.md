### Trim-Safe JS Interop

```csharp

// CORRECT: Use IJSRuntime with explicit method names (no dynamic dispatch)
await JSRuntime.InvokeVoidAsync("localStorage.setItem", "key", "value");
var value = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "key");

// CORRECT: Use IJSObjectReference for module imports (.NET 8+)
var module = await JSRuntime.InvokeAsync<IJSObjectReference>(
    "import", "./js/chart.js");
await module.InvokeVoidAsync("initChart", elementRef, data);
await module.DisposeAsync();

```javascript

```csharp

// WRONG: Dynamic dispatch via reflection (trimmed away)
// var method = typeof(JSRuntime).GetMethod("InvokeAsync");
// method.MakeGenericMethod(returnType).Invoke(...)

```csharp

### Linker Configuration

```xml

<!-- Preserve types used dynamically in components -->
<ItemGroup>
  <TrimmerRootAssembly Include="MyApp.Client" />
</ItemGroup>

```text

For types that must be preserved from trimming:

```csharp

// Mark types that are accessed via reflection
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class DynamicFormModel
{
    // Properties discovered at runtime for form generation
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

```text

### Anti-Patterns to Avoid

1. **Reflection-based DI** -- Do not use `Activator.CreateInstance` or `Type.GetType` to resolve services. Use the
   built-in DI container with explicit registrations.
2. **Dynamic type loading** -- Do not use `Assembly.Load` or `Assembly.GetTypes()` at runtime. Register all types at
   startup.
3. **Runtime code generation** -- Do not use `System.Reflection.Emit` or `System.Linq.Expressions.Expression.Compile()`.
   Use source generators instead.
4. **Untyped JSON deserialization** -- Do not use `JsonSerializer.Deserialize<T>(json)` without a
   `JsonSerializerContext`. Always provide a source-generated context.

---

## Prerendering

Prerendering generates HTML on the server before the interactive runtime loads. This improves perceived performance and
SEO.

### Prerender with Interactive Modes

```razor

<!-- Component prerenders on server, then becomes interactive -->
<Counter @rendermode="InteractiveServer" />

```text

By default, interactive components prerender. To disable:

```razor

@rendermode @(new InteractiveServerRenderMode(prerender: false))

```text

### Persisting State Across Prerender

State computed during prerendering is lost when the component reinitializes interactively. Use
`PersistentComponentState` to preserve it:

```razor

@inject PersistentComponentState ApplicationState
@implements IDisposable

@code {
    private List<ProductDto>? products;
    private PersistingComponentStateSubscription _subscription;

    protected override async Task OnInitializedAsync()
    {
        _subscription = ApplicationState.RegisterOnPersisting(PersistState);

        if (!ApplicationState.TryTakeFromJson<List<ProductDto>>(
            "products", out var restored))
        {
            products = await ProductService.GetProductsAsync();
        }
        else
        {
            products = restored;
        }
    }

    private Task PersistState()
    {
        ApplicationState.PersistAsJson("products", products);
        return Task.CompletedTask;
    }

    public void Dispose() => _subscription.Dispose();
}

```text

---

## .NET 10 Stable Features

These features are available when `net10.0` TFM is detected. They are stable and require no preview opt-in.

### WebAssembly Preloading

.NET 10 adds `blazor.web.js` preloading of WebAssembly assemblies during the Server phase of InteractiveAuto. When the
user first loads a page, the WASM runtime and app assemblies download in the background while the Server circuit handles
interactivity. Subsequent navigations switch to WASM faster because assemblies are already cached.

```razor

<!-- No code changes needed -- preloading is automatic in .NET 10 -->
<!-- Verify in browser DevTools Network tab: assemblies download during Server phase -->

```text

### Enhanced Form Validation

.NET 10 extends `EditForm` validation with improved error message formatting and support for `IValidatableObject` in
Static SSR forms. Validation messages render correctly with enhanced form handling (`Enhance` attribute) without
requiring a full page reload.

```csharp

// IValidatableObject works in Static SSR enhanced forms in .NET 10
public sealed class OrderModel : IValidatableObject
{
    [Required]
    public string ProductId { get; set; } = "";

    [Range(1, 100)]
    public int Quantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ProductId == "DISCONTINUED" && Quantity > 0)
        {
            yield return new ValidationResult(
                "Cannot order discontinued products",
                [nameof(ProductId), nameof(Quantity)]);
        }
    }
}

```text

### Blazor Diagnostics Middleware

.NET 10 adds `MapBlazorDiagnostics` middleware for inspecting Blazor circuit and component state in development:

```csharp

// Program.cs -- available in .NET 10
if (app.Environment.IsDevelopment())
{
    app.MapBlazorDiagnostics(); // Exposes /_blazor/diagnostics endpoint
}

```text

The diagnostics endpoint shows active circuits, component tree, render mode assignments, and timing data. Use it to
debug render mode boundaries and component lifecycle issues during development.

---

## Agent Gotchas

1. **Do not default to InteractiveServer for every page.** Static SSR is the default and most efficient render mode.
   Only add interactivity where user interaction requires it.
2. **Do not put WASM-targeted components in the server project.** Components that must run in the browser
   (InteractiveWebAssembly or InteractiveAuto) belong in the `.Client` project.
3. **Do not forget `PersistentComponentState` when prerendering.** Without it, data fetched during prerender is
   discarded and re-fetched when the component becomes interactive, causing a visible flicker.
4. **Do not use reflection-based serialization in WASM.** Always use `JsonSerializerContext` with source-generated
   serializers for AOT compatibility and trimming safety.
5. **Do not force-load navigation unless leaving the Blazor app.** `NavigateTo("/page", forceLoad: true)` bypasses
   enhanced navigation and causes a full page reload.
6. **Do not nest interactive render modes incorrectly.** A child component cannot request a more interactive mode than
   its parent. Plan render mode boundaries from the layout down.

---

## Prerequisites

- .NET 8.0+ (Blazor Web App template, render modes, enhanced navigation, streaming rendering)
- .NET 10.0 for stable enhanced features (WebAssembly preloading, enhanced form validation, diagnostics middleware)
- MAUI workload for Blazor Hybrid (`dotnet workload install maui`)

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

- [Blazor Overview](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-10.0)
- [Blazor Render Modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0)
- [Blazor Routing](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-10.0)
- [Enhanced Navigation](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-10.0#enhanced-navigation-and-form-handling)
- [Streaming Rendering](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-10.0#streaming-rendering)
- [Blazor Hybrid](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/?view=aspnetcore-10.0)
- [AOT Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
````
