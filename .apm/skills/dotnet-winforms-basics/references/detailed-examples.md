private async void btnSave_Click(object sender, EventArgs e)
{
    btnSave.Enabled = false;
    try
    {
        var result = await _httpClient.PostAsync(url, content);
        result.EnsureSuccessStatusCode();
        MessageBox.Show("Saved!");
    }
    catch (HttpRequestException ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
    finally
    {
        btnSave.Enabled = true;
    }
}

```text

### Convert to .NET 8+

Use the .NET Upgrade Assistant for automated migration:

```bash

# Install upgrade assistant
dotnet tool install -g upgrade-assistant

# Analyze the project
upgrade-assistant analyze MyWinFormsApp.csproj

# Upgrade the project
upgrade-assistant upgrade MyWinFormsApp.csproj

```csharp

**Common migration issues:**
- `App.config` settings need manual migration to `appsettings.json` or Host builder configuration
- `My.Settings` (VB.NET) and `Settings.settings` need manual migration
- Third-party controls may not have .NET 8 compatible versions
- Designer-generated code in `.Designer.cs` files usually migrates cleanly
- COM interop (`System.Runtime.InteropServices`) syntax may differ

### Adopt Modern C# Features

```csharp

// File-scoped namespaces
namespace MyApp.Forms;

// Null-conditional event invocation
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

// Collection expressions
var columns = new[] { "Name", "Price", "Category" };

// Primary constructors for services (C# 12)
public class ProductService(HttpClient httpClient) : IProductService
{
    public async Task<List<Product>> GetProductsAsync()
        => await httpClient.GetFromJsonAsync<List<Product>>("/products") ?? [];
}

```json

---

## Agent Gotchas

1. **Do not recommend WinForms for new customer-facing applications.** WinForms is appropriate for internal tools, rapid prototyping, and data-centric utilities. For customer-facing apps, recommend WPF, WinUI 3, MAUI, or Blazor depending on requirements.
2. **Do not use deprecated WinForms APIs.** `Menu` (use `MenuStrip`), `MainMenu` (use `MenuStrip`), `ContextMenu` (use `ContextMenuStrip`), `StatusBar` (use `StatusStrip`), `ToolBar` (use `ToolStrip`), `DataGrid` (use `DataGridView`).
3. **Do not assume dark mode is production-ready.** Dark mode via `Application.SetColorMode` is experimental in .NET 9 and targeting finalization in .NET 11. API surface and rendering may change.
4. **Do not use `HighDpiMode.SystemAware` without testing multi-monitor scenarios.** `PerMonitorV2` is recommended for apps used on multi-monitor setups with different DPI settings.
5. **Do not block the UI thread with synchronous calls.** Use `async void` for event handlers and `async Task` for all other async methods. Never use `.Result` or `.Wait()` on the UI thread.
6. **Do not use `Control.Invoke` when `await` suffices.** In .NET 8+ WinForms, `await` automatically marshals back to the UI thread via `SynchronizationContext`. Manual `Invoke`/`BeginInvoke` is only needed when called from non-async code (timers, COM callbacks).
7. **Do not hardcode colors when dark mode is enabled.** Use `SystemColors` properties (e.g., `SystemColors.ControlText`, `SystemColors.Control`) in custom drawing and owner-drawn controls to respond correctly to theme changes.
8. **Do not forget to call `ApplicationConfiguration.Initialize()` before `Application.Run`.** Omitting it disables visual styles and high-DPI configuration.

---

## Prerequisites

- .NET 8.0+ with Windows desktop workload
- TFM: `net8.0-windows` (no Windows SDK version needed for WinForms)
- Visual Studio 2022+ with Windows desktop workload (for designer support)
- For dark mode: .NET 9+ (experimental), Windows 10 version 1809+
- For DPI-unaware designer: .NET 9+

---

## References

- [WinForms on .NET Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
- [High DPI Support in WinForms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms)
- [.NET Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-overview)
- [WinForms Dark Mode (.NET 9)](https://devblogs.microsoft.com/dotnet/winforms-dark-mode/)
- [Microsoft.Extensions.Hosting](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
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
