@code {
    private ProductModel product = new();

    private async Task HandleSubmit()
    {
        await ProductService.CreateAsync(product);
        Navigation.NavigateTo("/products");
    }
}

```text

### Model with Validation Attributes

```csharp

public sealed class ProductModel
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [Range(0.01, 1_000_000, ErrorMessage = "Price must be between {1} and {2}")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = "";
}

```text

### EditForm with Enhanced Form Handling (.NET 8+)

Static SSR forms require `FormName` and use `[SupplyParameterFromForm]`:

```razor

@page "/products/create"

<EditForm Model="product" OnValidSubmit="HandleSubmit" FormName="create-product" Enhance>
    <DataAnnotationsValidator />
    <!-- form fields -->
    <button type="submit">Create</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    private ProductModel product { get; set; } = new();

    private async Task HandleSubmit()
    {
        await ProductService.CreateAsync(product);
        Navigation.NavigateTo("/products");
    }
}

```text

The `Enhance` attribute enables enhanced form handling -- the form submits via fetch and patches the DOM without a full
page reload.

**Gotcha:** `FormName` must be unique across all forms on the page. Duplicate `FormName` values cause ambiguous form
submission errors.

---

## QuickGrid

QuickGrid is a high-performance grid component built into Blazor (.NET 8+). It supports sorting, filtering, pagination,
and virtualization.

### Basic QuickGrid

```razor

@using Microsoft.AspNetCore.Components.QuickGrid

<QuickGrid Items="products">
    <PropertyColumn Property="p => p.Name" Sortable="true" />
    <PropertyColumn Property="p => p.Price" Format="C2" Sortable="true" />
    <PropertyColumn Property="p => p.Category" Sortable="true" />
    <TemplateColumn Title="Actions">
        <button @onclick="() => Edit(context)">Edit</button>
    </TemplateColumn>
</QuickGrid>

@code {
    private IQueryable<Product> products = Enumerable.Empty<Product>().AsQueryable();

    protected override async Task OnInitializedAsync()
    {
        var list = await ProductService.GetAllAsync();
        products = list.AsQueryable();
    }

    private void Edit(Product product) => Navigation.NavigateTo($"/products/{product.Id}/edit");
}

```text

### QuickGrid with Pagination

```razor

<QuickGrid Items="products" Pagination="pagination">
    <PropertyColumn Property="p => p.Name" Sortable="true" />
    <PropertyColumn Property="p => p.Price" Format="C2" />
</QuickGrid>

<Paginator State="pagination" />

@code {
    private PaginationState pagination = new() { ItemsPerPage = 20 };
    private IQueryable<Product> products = default!;
}

```text

### QuickGrid with Virtualization

For large datasets, virtualization renders only visible rows:

```razor

<QuickGrid Items="products" Virtualize="true" ItemSize="50">
    <PropertyColumn Property="p => p.Name" />
    <PropertyColumn Property="p => p.Price" Format="C2" />
</QuickGrid>

```text

<!-- net11-preview -->

### QuickGrid OnRowClick (.NET 11 Preview)

.NET 11 adds `OnRowClick` to QuickGrid for row-level click handling without template columns:

```razor

<QuickGrid Items="products" OnRowClick="HandleRowClick">
    <PropertyColumn Property="p => p.Name" />
    <PropertyColumn Property="p => p.Price" Format="C2" />
</QuickGrid>

@code {
    private void HandleRowClick(GridRowClickEventArgs<Product> args)
    {
        Navigation.NavigateTo($"/products/{args.Item.Id}");
    }
}

```text

**Fallback (net10.0):** Use a `TemplateColumn` with a click handler or wrap each row in a clickable element.

Source:
[ASP.NET Core .NET 11 Preview - QuickGrid enhancements](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11.0)

---

<!-- net11-preview -->

## .NET 11 Preview Features

### EnvironmentBoundary Component

`EnvironmentBoundary` conditionally renders content based on the hosting environment (Development, Staging, Production):

```razor

<EnvironmentBoundary Include="Development">
    <p>Debug panel -- only visible in Development</p>
    <DebugToolbar />
</EnvironmentBoundary>

<EnvironmentBoundary Exclude="Production">
    <p>Testing controls -- hidden in Production</p>
</EnvironmentBoundary>

```text

**Fallback (net10.0):** Inject `IWebHostEnvironment` and use conditional rendering in `@code`.

Source:
[ASP.NET Core .NET 11 Preview - EnvironmentBoundary](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11.0)

### Label and DisplayName Support

.NET 11 adds `[DisplayName]` support for input components, automatically generating `<label>` elements:

```razor

<EditForm Model="model" FormName="contact">
    <!-- Automatically renders <label> from [DisplayName] -->
    <InputText @bind-Value="model.FullName" />
    <InputText @bind-Value="model.EmailAddress" />
</EditForm>

@code {
    private ContactModel model = new();
}

// Model
public sealed class ContactModel
{
    [DisplayName("Full Name")]
    [Required]
    public string FullName { get; set; } = "";

    [DisplayName("Email Address")]
    [EmailAddress]
    public string EmailAddress { get; set; } = "";
}

```text

**Fallback (net10.0):** Add explicit `<label for="...">` elements manually.

Source:
[ASP.NET Core .NET 11 Preview - Label/DisplayName](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11.0)

### IHostedService in WebAssembly

.NET 11 allows `IHostedService` implementations to run in Blazor WebAssembly, enabling background tasks in the browser:

```csharp

// Register in WASM Program.cs
builder.Services.AddHostedService<DataSyncService>();

public sealed class DataSyncService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncDataFromServer();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

```text

**Fallback (net10.0):** Use a `Timer` in a component or inject a singleton service that starts background work on first
use.

Source:
[ASP.NET Core .NET 11 Preview - IHostedService in WASM](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11.0)

<!-- net11-preview -->

### SignalR ConfigureConnection

.NET 11 adds `ConfigureConnection` to the Blazor Server circuit hub, allowing customization of the SignalR connection
(e.g., adding custom headers, configuring reconnection):

```csharp

// Program.cs
app.MapBlazorHub(options =>
{
    options.ConfigureConnection = connection =>
    {
        connection.Metadata["tenant"] = "default";
    };
});

```text

**Fallback (net10.0):** Use `IHubFilter` or middleware to inspect/modify connections at the hub level.

Source:
[ASP.NET Core .NET 11 Preview - SignalR ConfigureConnection](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11.0)

---

## Agent Gotchas

1. **Do not call JS interop in `OnInitializedAsync`.** The DOM is not available yet. Use
   `OnAfterRenderAsync(firstRender: true)` for JS calls that need DOM elements.
2. **Do not forget `StateHasChanged()` after external state changes.** When state changes from a non-Blazor context
   (timer, event handler, JS callback), call `StateHasChanged()` or `InvokeAsync(StateHasChanged)` to trigger re-render.
3. **Do not use `ProtectedBrowserStorage` during prerendering.** It throws because no interactive circuit exists yet.
   Access it only in `OnAfterRenderAsync`.
4. **Do not forget `FormName` on Static SSR forms.** Without it, form submissions in Static SSR mode are not routed to
   the correct handler.
5. **Do not dispose `DotNetObjectReference` before JS is done with it.** Premature disposal causes `JSException` when
   JavaScript tries to invoke the callback. Dispose in `Dispose()` or `DisposeAsync()`.
6. **Do not assume Scoped services are per-request in Blazor Server.** Scoped services live for the entire circuit. Use
   `OwningComponentBase<T>` when you need component-scoped service lifetimes.

---

## Prerequisites

- .NET 8.0+ (QuickGrid, enhanced form handling, cascading values with `IsFixed`)
- `Microsoft.AspNetCore.Components.QuickGrid` package for QuickGrid
- .NET 11 preview for EnvironmentBoundary, Label/DisplayName, QuickGrid OnRowClick, IHostedService in WASM

---

## Knowledge Sources

Blazor component patterns in this skill are grounded in guidance from:

- **Damian Edwards** -- Razor and Blazor component design patterns, render mode architecture, and performance best
  practices. Principal architect on the ASP.NET team.

> These sources inform the patterns and rationale presented above. This skill does not claim to represent or speak for
> any individual.

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

- [Blazor Component Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-10.0)
- [Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-10.0)
- [Blazor JS Interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-10.0)
- [Blazor Forms and Validation](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/?view=aspnetcore-10.0)
- [QuickGrid Component](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/quickgrid?view=aspnetcore-10.0)
- [Cascading Values and Parameters](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/cascading-values-and-parameters?view=aspnetcore-10.0)
````
