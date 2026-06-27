
For components with many JS calls, use loose mode to avoid setting up every call:

```csharp

[Fact]
public void RichEditor_Render_DoesNotThrowJSErrors()
{
    // Loose mode: unmatched JS calls return default values instead of throwing
    JSInterop.Mode = JSRuntimeMode.Loose;

    var cut = RenderComponent<RichTextEditor>(parameters => parameters
        .Add(p => p.Content, "Initial content"));

    // Component renders without JS exceptions
    Assert.NotEmpty(cut.Markup);
}

```text

---

## Async Component Lifecycle

### Testing OnInitializedAsync

```csharp

[Fact]
public void ProductList_WhileLoading_ShowsSpinner()
{
    var tcs = new TaskCompletionSource<List<ProductDto>>();
    var productService = Substitute.For<IProductService>();
    productService.GetProductsAsync().Returns(tcs.Task);
    Services.AddSingleton(productService);

    var cut = RenderComponent<ProductList>();

    // Component is still loading -- spinner should be visible
    Assert.NotNull(cut.Find("[data-testid='loading-spinner']"));

    // Complete the async operation
    tcs.SetResult([new ProductDto { Name = "Widget", Price = 9.99m }]);
    cut.WaitForState(() => cut.FindAll("[data-testid='product-item']").Count > 0);

    // Spinner gone, products visible
    Assert.Throws<ElementNotFoundException>(
        () => cut.Find("[data-testid='loading-spinner']"));
    Assert.Single(cut.FindAll("[data-testid='product-item']"));
}

```text

### Testing Error States

```csharp

[Fact]
public void ProductList_ServiceError_ShowsErrorMessage()
{
    var productService = Substitute.For<IProductService>();
    productService.GetProductsAsync()
        .ThrowsAsync(new HttpRequestException("Service unavailable"));
    Services.AddSingleton(productService);

    var cut = RenderComponent<ProductList>();

    cut.WaitForState(() =>
        cut.Find("[data-testid='error-message']").TextContent.Length > 0);

    Assert.Contains("Service unavailable",
        cut.Find("[data-testid='error-message']").TextContent);
}

```text

---

## Key Principles

- **Render components in isolation.** bUnit tests individual components without a browser, making them fast and
  deterministic. Use this for component logic; use [skill:dotnet-playwright] for full-app E2E flows.
- **Register all dependencies before rendering.** Any service the component injects via `[Inject]` must be registered in
  `Services` before `RenderComponent` is called.
- **Use `WaitForState` and `WaitForAssertion` for async components.** Do not use `Task.Delay` -- bUnit provides
  purpose-built waiting mechanisms.
- **Mock JS interop explicitly.** Unhandled JS interop calls throw by default in bUnit strict mode. Set up expected
  calls or switch to loose mode for JS-heavy components.
- **Test the rendered output, not component internals.** Assert on markup, text content, and element attributes -- not
  on private fields or internal state.

---

## Agent Gotchas

1. **Do not forget to register services before `RenderComponent`.** bUnit throws at render time if an `[Inject]`-ed
   service is missing. Register mocks or fakes for every injected dependency.
2. **Do not use `cut.Instance` to access private members.** `Instance` exposes the component's public API only. If you
   need to test internal state, expose it through public properties or test through rendered output.
3. **Do not forget to call `cut.WaitForState()` after triggering async operations.** Without it, assertions run before
   the component re-renders, causing false failures.
4. **Do not mix bUnit and Playwright in the same test class.** bUnit runs components in-memory (no browser); Playwright
   runs in a real browser. They serve different purposes and have incompatible lifecycles.
5. **Do not forget cascading values for components that expect them.** A component with `[CascadingParameter]` will
   receive `null` if no `CascadingValue` is provided, which may cause `NullReferenceException` during rendering.

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

- [bUnit Documentation](https://bunit.dev/)
- [bUnit Getting Started](https://bunit.dev/docs/getting-started/)
- [bUnit JS Interop](https://bunit.dev/docs/test-doubles/emulating-ijsruntime)
- [Blazor Component Testing](https://learn.microsoft.com/en-us/aspnet/core/blazor/test)
- [Testing Blazor Components with bUnit (tutorial)](https://bunit.dev/docs/providing-input/)
````
