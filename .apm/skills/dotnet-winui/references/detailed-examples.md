Unpackaged mode removes MSIX requirements. The app runs as a standard Win32 executable without app identity.

```xml

<!-- .csproj: enable unpackaged mode -->
<PropertyGroup>
  <WindowsPackageType>None</WindowsPackageType>
</PropertyGroup>

```csharp

**Trade-offs:**

| Feature | MSIX Packaged | Unpackaged |
|---------|--------------|------------|
| App identity | Yes | No |
| Clean install/uninstall | Yes (Add/Remove Programs) | Manual |
| Auto-update | Yes (Store, App Installer) | Manual |
| Background tasks | Full support | Limited |
| Toast notifications | Full support | Requires COM registration |
| Widgets (Windows 11) | Yes | No |
| File type associations | Via manifest | Via registry |
| Distribution | Store, sideload, App Installer | xcopy, installer (MSI/EXE) |
| Startup time | Slightly slower (package verification) | Faster |

**When to choose unpackaged:**
- Internal enterprise tools with existing deployment infrastructure
- Apps that need xcopy deployment or integration with existing MSI/EXE installers
- Quick prototypes where packaging overhead is unnecessary
- Apps that do not need Windows identity features

---

## Windows Integration

### App Lifecycle

WinUI 3 apps use the Windows App SDK activation and lifecycle model, distinct from UWP's `CoreApplication`.

```csharp

// Handle activation kinds (protocol, file, toast, etc.)
public partial class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Check for specific activation
        var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

        switch (activationArgs.Kind)
        {
            case ExtendedActivationKind.Protocol:
                var protocolArgs = (ProtocolActivatedEventArgs)activationArgs.Data;
                HandleProtocolActivation(protocolArgs.Uri);
                break;

            case ExtendedActivationKind.File:
                var fileArgs = (FileActivatedEventArgs)activationArgs.Data;
                HandleFileActivation(fileArgs.Files);
                break;

            default:
                // Normal launch
                break;
        }
    }
}

```text

### Notifications

Toast notifications require the Windows App SDK notification APIs:

```csharp

using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

// Register for notification activation
var notificationManager = AppNotificationManager.Default;
notificationManager.NotificationInvoked += OnNotificationInvoked;
notificationManager.Register();

// Send a toast notification
var builder = new AppNotificationBuilder()
    .AddText("Order Shipped")
    .AddText("Your order #12345 has shipped.")
    .AddButton(new AppNotificationButton("Track")
        .AddArgument("action", "track")
        .AddArgument("orderId", "12345"));

AppNotificationManager.Default.Show(builder.BuildNotification());

```text

### Widgets (Windows 11)

Widgets require Windows 11 (build 22000+) and MSIX packaged deployment. The implementation involves creating a widget provider that implements `IWidgetProvider` and registering it in the MSIX manifest.

**Key steps:**
1. Implement `IWidgetProvider` interface (methods: `CreateWidget`, `DeleteWidget`, `OnActionInvoked`, `OnWidgetContextChanged`, `OnCustomizationRequested`, `Activate`, `Deactivate`)
2. Register the provider as a COM class in the MSIX manifest
3. Define widget templates using Adaptive Cards JSON format
4. Return updated widget content from provider methods

See the [Windows App SDK Widget documentation](https://learn.microsoft.com/en-us/windows/apps/develop/widgets/widget-providers) for the complete interface contract and manifest registration.

### Taskbar Integration

Taskbar progress in WinUI 3 requires Win32 COM interop via the `ITaskbarList3` interface. Unlike UWP which had a managed `TaskbarManager`, WinUI 3 does not expose a managed wrapper.

```csharp

// Taskbar progress requires COM interop in WinUI 3
// Use CsWin32 source generator or manual P/Invoke for ITaskbarList3
// 1. Add CsWin32: <PackageReference Include="Microsoft.Windows.CsWin32" Version="0.3.*" />
// 2. Add to NativeMethods.txt: ITaskbarList3
// See: https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-itaskbarlist3

```csharp

---

## UWP Migration

Migrating from UWP to WinUI 3 involves namespace changes, API replacements, and project restructuring.

### Namespace Changes

| UWP Namespace | WinUI 3 Namespace |
|---------------|-------------------|
| `Windows.UI.Xaml` | `Microsoft.UI.Xaml` |
| `Windows.UI.Xaml.Controls` | `Microsoft.UI.Xaml.Controls` |
| `Windows.UI.Xaml.Media` | `Microsoft.UI.Xaml.Media` |
| `Windows.UI.Xaml.Input` | `Microsoft.UI.Xaml.Input` |
| `Windows.UI.Composition` | `Microsoft.UI.Composition` |
| `Windows.UI.Text` | `Microsoft.UI.Text` |
| `Windows.UI.Colors` | `Microsoft.UI.Colors` |

**Keep as-is:** `Windows.Storage`, `Windows.Networking`, `Windows.Security`, `Windows.ApplicationModel`, `Windows.Devices` -- these WinRT APIs remain in the `Windows.*` namespace.

### API Replacements

| UWP API | WinUI 3 Replacement |
|---------|---------------------|
| `CoreApplication.MainView` | `App.MainWindow` (track your own window reference) |
| `CoreDispatcher.RunAsync` | `DispatcherQueue.TryEnqueue` |
| `Window.Current` | Track window reference manually in App class |
| `ApplicationView.Title` | `window.Title = "..."` |
| `CoreWindow.GetForCurrentThread` | Not available; use `InputKeyboardSource` for keyboard APIs |
| `SystemNavigationManager.BackRequested` | `NavigationView.BackRequested` |

### Migration Steps

1. **Create a new WinUI 3 project** using the Windows App SDK template
2. **Copy source files** and update namespaces (`Windows.UI.Xaml` to `Microsoft.UI.Xaml`)
3. **Update XAML namespaces** in all `.xaml` files
4. **Replace deprecated APIs** (see table above)
5. **Migrate packaging** from `.appxmanifest` UWP format to Windows App SDK format
6. **Update NuGet packages** to Windows App SDK-compatible versions
7. **Test Windows integration** features (notifications, background tasks, file associations)

For comprehensive migration path guidance across frameworks, see [skill:dotnet-wpf-migration].

**UWP .NET 9 preview path:** Microsoft announced UWP support on .NET 9 as a preview. This allows UWP apps to use modern .NET without migrating to WinUI 3. Evaluate this path if full WinUI migration is too costly but you need modern .NET runtime features.

---

## Agent Gotchas

1. **Do not confuse UWP XAML with WinUI 3 XAML.** The root namespace changed from `Windows.UI.Xaml` to `Microsoft.UI.Xaml`. Code using `Windows.UI.Xaml.*` types will not compile in WinUI 3 projects.
2. **Do not use `Window.Current`.** WinUI 3 does not have a static `Window.Current` property. Track your window reference manually in the `App` class and pass it via DI or a static property.
3. **Do not use `CoreDispatcher`.** Replace `CoreDispatcher.RunAsync()` with `DispatcherQueue.TryEnqueue()`. `CoreDispatcher` is a UWP API not available in WinUI 3.
4. **Do not assume MSIX is required.** WinUI 3 supports unpackaged deployment via `<WindowsPackageType>None</WindowsPackageType>`. Only use MSIX when you need app identity, Store distribution, or Windows integration features that require it.
5. **Do not forget `x:Bind` defaults to `OneTime`.** Unlike `{Binding}` which defaults to `OneWay`, `x:Bind` defaults to `OneTime`. Always specify `Mode=OneWay` or `Mode=TwoWay` for properties that change after initial binding.
6. **Do not target Windows 10 builds below 19041.** Windows App SDK 1.6+ requires a minimum of build 19041 (version 2004). Targeting lower builds causes runtime failures.
7. **Do not use Widgets or Mica in unpackaged apps.** These features require MSIX packaged deployment with app identity. Attempting to use them in unpackaged mode fails silently or throws.
8. **Do not mix CommunityToolkit.Mvvm with manual INotifyPropertyChanged.** Use `[ObservableProperty]` consistently. Mixing source-generated and hand-written implementations causes subtle binding bugs.
9. **Do not forget the Host builder lifecycle.** Call `_host.StartAsync()` in `OnLaunched` and `_host.StopAsync()` when the window closes. Forgetting lifecycle management causes DI-registered `IHostedService` instances to never start or stop.

---

## Prerequisites

- .NET 8.0+ with Windows desktop workload
- Windows App SDK 1.6+ (auto-referenced via `UseWinUI`)
- Windows 10 version 2004 (build 19041) or later
- Visual Studio 2022+ with Windows App SDK workload, or VS Code with C# Dev Kit
- For widgets: Windows 11 (build 22000+)

---

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [UWP to WinUI Migration](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/)
- [MSIX Packaging](https://learn.microsoft.com/en-us/windows/msix/)
- [Windows App SDK Deployment Guide](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-overview)
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
