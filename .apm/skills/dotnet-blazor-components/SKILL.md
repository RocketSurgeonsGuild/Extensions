---
name: dotnet-blazor-components
category: web
subcategory: blazor
description: Implements Blazor components. Lifecycle, state management, JS interop, EditForm, QuickGrid.
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

# dotnet-blazor-components

Blazor component architecture: lifecycle methods, state management (cascading values, DI, browser storage), JavaScript
interop (AOT-safe), EditForm validation, and QuickGrid. Covers per-render-mode behavior differences where relevant.

## Scope

- Component lifecycle methods (SetParametersAsync, OnInitialized, OnAfterRender)
- State management (cascading values, DI, browser storage)
- JavaScript interop (AOT-safe module imports)
- EditForm validation and input components
- QuickGrid data binding and virtualization
- Per-render-mode behavior differences (Static SSR, InteractiveServer, WASM)

## Out of scope

- Hosting model selection and render modes -- see [skill:dotnet-blazor-patterns]
- Auth components (AuthorizeView, CascadingAuthenticationState) -- see [skill:dotnet-blazor-auth]
- bUnit testing -- see [skill:dotnet-blazor-testing]
- Standalone SignalR hub patterns -- see [skill:dotnet-realtime-communication]
- E2E testing -- see [skill:dotnet-playwright]
- UI framework selection -- see [skill:dotnet-ui-chooser]
- Accessibility patterns (ARIA, keyboard navigation) -- see [skill:dotnet-accessibility]

Cross-references: [skill:dotnet-blazor-patterns] for hosting models and render modes, [skill:dotnet-blazor-auth] for
authentication, [skill:dotnet-blazor-testing] for bUnit testing, [skill:dotnet-realtime-communication] for standalone
SignalR, [skill:dotnet-playwright] for E2E testing, [skill:dotnet-ui-chooser] for framework selection,
[skill:dotnet-accessibility] for accessibility patterns (ARIA, keyboard nav, screen readers).

---

## Component Lifecycle

### Lifecycle Methods

````csharp

@code {
    // 1. Called when parameters are set/updated
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Access raw parameters before they are applied
        await base.SetParametersAsync(parameters);
    }

    // 2. Called after parameters are assigned (sync)
    protected override void OnInitialized()
    {
        // One-time initialization (runs once per component instance)
    }

    // 3. Called after parameters are assigned (async)
    protected override async Task OnInitializedAsync()
    {
        // Async initialization (data fetching, service calls)
        products = await ProductService.GetProductsAsync();
    }

    // 4. Called every time parameters change
    protected override void OnParametersSet()
    {
        // React to parameter changes
    }

    // 5. Called after each render
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            // JS interop safe here -- DOM is available
        }
    }

    // 6. Async version of OnAfterRender
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initializeChart", chartElement);
        }
    }

    // 7. Cleanup
    public void Dispose()
    {
        // Unsubscribe from events, dispose resources
    }

    // 8. Async cleanup
    public async ValueTask DisposeAsync()
    {
        // Async cleanup (dispose JS object references)
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }
}

```text

### Lifecycle Behavior per Render Mode

| Lifecycle Event        | Static SSR            | InteractiveServer                            | InteractiveWebAssembly           | InteractiveAuto                                                    | Hybrid                      |
| ---------------------- | --------------------- | -------------------------------------------- | -------------------------------- | ------------------------------------------------------------------ | --------------------------- |
| `OnInitialized(Async)` | Runs on server        | Runs on server                               | Runs in browser                  | Server on first load, browser after WASM cached                    | Runs in-process             |
| `OnAfterRender(Async)` | Never called          | Runs on server after SignalR confirms render | Runs in browser after DOM update | Server-side then browser-side (matches active runtime)             | Runs after WebView render   |
| `Dispose(Async)`       | Called after response | Called when circuit ends                     | Called on component removal      | Called when circuit ends (Server phase) or on removal (WASM phase) | Called on component removal |

**Gotcha:** In Static SSR, `OnAfterRender` never executes because there is no persistent connection. Do not place
critical logic in `OnAfterRender` for Static SSR pages.

---

## State Management

### Cascading Values

Cascading values flow data down the component tree without explicit parameter passing.

```razor

<!-- Parent: provide a cascading value -->
<CascadingValue Value="@theme" Name="AppTheme">
    <Router AppAssembly="typeof(App).Assembly">
        <!-- All descendants can receive AppTheme -->
    </Router>
</CascadingValue>

@code {
    private ThemeSettings theme = new() { IsDarkMode = false, AccentColor = "#0078d4" };
}

```text

```razor

<!-- Child: consume the cascading value -->
@code {
    [CascadingParameter(Name = "AppTheme")]
    public ThemeSettings? Theme { get; set; }
}

```text

**Fixed cascading values (.NET 8+):** For values that never change after initial render, use `IsFixed="true"` to avoid
re-render overhead:

```razor

<CascadingValue Value="@config" IsFixed="true">
    <ChildComponent />
</CascadingValue>

```text

### Dependency Injection

```csharp

// Register services in Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<AppState>();

// Inject in components
@inject IProductService ProductService
@inject AppState State

```text

**DI lifetime behavior per render mode:**

| Lifetime  | InteractiveServer                        | InteractiveWebAssembly                  | InteractiveAuto                                                                            | Hybrid                                   |
| --------- | ---------------------------------------- | --------------------------------------- | ------------------------------------------------------------------------------------------ | ---------------------------------------- |
| Singleton | Shared across all circuits on the server | One per browser tab                     | Server-shared during Server phase; per-tab after WASM switch                               | One per app instance                     |
| Scoped    | One per circuit (acts like per-user)     | One per browser tab (same as Singleton) | Per-circuit (Server phase), per-tab (WASM phase) -- state does not transfer between phases | One per app instance (same as Singleton) |
| Transient | New instance each injection              | New instance each injection             | New instance each injection                                                                | New instance each injection              |

**Gotcha:** In Blazor Server, `Scoped` services live for the entire circuit duration (not per-request like in MVC). A
circuit persists until the user navigates away or the connection drops. Long-lived scoped services may accumulate state
-- use `OwningComponentBase<T>` for component-scoped DI.

### Browser Storage

```csharp

// ProtectedBrowserStorage -- encrypted, per-user storage
// Available in InteractiveServer only (not WASM -- server encrypts/decrypts)
@inject ProtectedSessionStorage SessionStorage
@inject ProtectedLocalStorage LocalStorage

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Session storage (cleared when tab closes)
        await SessionStorage.SetAsync("cart", cartItems);
        var result = await SessionStorage.GetAsync<List<CartItem>>("cart");
        if (result.Success) { cartItems = result.Value!; }

        // Local storage (persists across sessions)
        await LocalStorage.SetAsync("preferences", userPrefs);
    }
}

```text

For InteractiveWebAssembly, use JS interop to access browser storage directly:

```csharp

// WASM: Direct browser storage via JS interop
await JSRuntime.InvokeVoidAsync("localStorage.setItem", "key",
    JsonSerializer.Serialize(value, AppJsonContext.Default.UserPrefs));

var json = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "key");
if (json is not null)
{
    value = JsonSerializer.Deserialize(json, AppJsonContext.Default.UserPrefs);
}

```json

**Gotcha:** `ProtectedBrowserStorage` is not available during prerendering. Always access it in
`OnAfterRenderAsync(firstRender: true)`, never in `OnInitializedAsync`.

---

## JavaScript Interop

### Calling JavaScript from .NET

```csharp

@inject IJSRuntime JSRuntime

// Invoke a global JS function
await JSRuntime.InvokeVoidAsync("console.log", "Hello from Blazor");

// Invoke and get a return value
var width = await JSRuntime.InvokeAsync<int>("getWindowWidth");

// With timeout (important for Server to avoid hanging circuits)
var result = await JSRuntime.InvokeAsync<string>(
    "expensiveOperation",
    TimeSpan.FromSeconds(10),
    inputData);

```text

### JavaScript Module Imports (AOT-Safe)

```csharp

// Import a JS module -- trim-safe, no reflection
private IJSObjectReference? module;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        module = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/interop.js");
        await module.InvokeVoidAsync("initialize", elementRef);
    }
}

// Always dispose module references
public async ValueTask DisposeAsync()
{
    if (module is not null)
    {
        await module.DisposeAsync();
    }
}

```text

```javascript

// wwwroot/js/interop.js
export function initialize(element) {
  // Set up the element
}

export function getValue(element) {
  return element.value;
}

```text

### Calling .NET from JavaScript

```csharp

// Instance method callback
private DotNetObjectReference<MyComponent>? dotNetRef;

protected override void OnInitialized()
{
    dotNetRef = DotNetObjectReference.Create(this);
}

[JSInvokable]
public void OnJsEvent(string data)
{
    message = data;
    StateHasChanged();
}

public void Dispose()
{
    dotNetRef?.Dispose();
}

```text

```javascript

// Call .NET from JS
export function registerCallback(dotNetRef) {
  document.addEventListener('custom-event', e => {
    dotNetRef.invokeMethodAsync('OnJsEvent', e.detail);
  });
}

```text

### JS Interop per Render Mode

| Concern                   | InteractiveServer             | InteractiveWebAssembly          | InteractiveAuto                                                         | Hybrid                          |
| ------------------------- | ----------------------------- | ------------------------------- | ----------------------------------------------------------------------- | ------------------------------- |
| JS call timing            | After SignalR confirms render | After WASM runtime loads        | SignalR initially, then direct after WASM switch                        | After WebView loads             |
| `OnAfterRender` available | Yes                           | Yes                             | Yes                                                                     | Yes                             |
| IJSRuntime sync calls     | Not supported (async only)    | `IJSInProcessRuntime` available | Async-only during Server phase; `IJSInProcessRuntime` after WASM switch | `IJSInProcessRuntime` available |
| Module imports            | Via SignalR (latency)         | Direct (fast)                   | SignalR (Server phase), direct (WASM phase)                             | Direct (fast)                   |

**Gotcha:** In InteractiveServer, all JS interop calls travel over SignalR, adding network latency. Minimize round trips
by batching operations into a single JS function call.

---

## EditForm Validation

### Basic EditForm with Data Annotations

```razor

<EditForm Model="product" OnValidSubmit="HandleSubmit" FormName="product-form">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div>
        <label for="name">Name:</label>
        <InputText id="name" @bind-Value="product.Name" />
        <ValidationMessage For="() => product.Name" />
    </div>

    <div>
        <label for="price">Price:</label>
        <InputNumber id="price" @bind-Value="product.Price" />
        <ValidationMessage For="() => product.Price" />
    </div>

    <div>
        <label for="category">Category:</label>
        <InputSelect id="category" @bind-Value="product.Category">
            <option value="">Select...</option>
            <option value="Electronics">Electronics</option>
            <option value="Clothing">Clothing</option>
        </InputSelect>
        <ValidationMessage For="() => product.Category" />
    </div>

    <button type="submit">Save</button>
</EditForm>

@code {

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
