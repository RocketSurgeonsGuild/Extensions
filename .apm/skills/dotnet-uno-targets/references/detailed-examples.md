
The Linux target uses the Skia renderer with a GTK host window. All XAML rendering is done by Skia -- GTK provides only the window and input handling.

### Debugging

- **VS Code with C# Dev Kit:** Remote debugging on Linux
- **JetBrains Rider:** Full Linux debugging support
- **Console logging:** `dotnet run` outputs logs to terminal

### Packaging/Distribution

```bash

# Publish self-contained for Linux
dotnet publish -f net8.0-desktop -c Release \
  --self-contained \
  -r linux-x64

# Package as AppImage, Flatpak, or Snap

```text

Distribution: AppImage (portable, no install), Flatpak (sandboxed), Snap (Ubuntu Store), DEB/RPM packages.

### Platform Gotchas

- **GTK dependencies:** The app requires GTK3 libraries at runtime. Package or document the dependency: `libgtk-3-0`, `libskia*`
- **Skia rendering:** All rendering is Skia-based. Native GTK widgets are not used. This ensures pixel-perfect cross-platform rendering but means the app does not follow the host GTK theme
- **Font rendering:** Ensure fonts are available on the target system. Embed fonts in the app or declare font dependencies
- **Display scaling:** HiDPI support works via Skia; test with `GDK_SCALE=2` environment variable

### AOT/Trimming

```xml

<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0-desktop'">
  <PublishTrimmed>true</PublishTrimmed>
  <PublishAot>true</PublishAot>
</PropertyGroup>

```text

AOT on Linux produces a native binary. Self-contained deployment avoids requiring a system-wide .NET runtime.

### Behavior Differences

- **Navigation:** Keyboard-driven (Alt+Left for back). No gesture-based navigation
- **Authentication:** Uses system browser for OAuth flows. No platform-specific auth APIs
- **Debugging:** Standard .NET debugging. No platform-specific debugger tools

---

## Embedded (Skia/Framebuffer)

### Project Setup

The Embedded target uses the Skia renderer with a framebuffer backend, enabling headless or kiosk-style rendering without a windowing system.

```bash

# Build for embedded (same TFM as desktop)
dotnet build -f net8.0-desktop -r linux-arm64

# Run directly on embedded device
dotnet run -f net8.0-desktop

```text

The embedded target shares the `net8.0-desktop` TFM with Linux desktop. Platform-specific configuration selects the framebuffer backend.

```csharp

// Program.cs -- framebuffer host configuration
public static void Main(string[] args)
{
    SkiaHostBuilder.Create()
        .UseFrameBuffer()  // Use framebuffer instead of GTK
        .App(() => new App())
        .Build()
        .Run();
}

```text

### Debugging

- **SSH remote debugging:** Use VS Code Remote or `dotnet-trace` for remote profiling
- **Serial console:** Redirect logs to serial output for headless debugging
- **Remote logging:** Configure Serilog with network sink for remote log aggregation

### Packaging/Distribution

```bash

# Publish self-contained for ARM64
dotnet publish -f net8.0-desktop -c Release \
  --self-contained \
  -r linux-arm64

# Deploy binary directly to device filesystem
scp -r ./publish/* pi@device:/opt/myapp/

```text

Direct deployment to device filesystem. No app store or package manager.

### Platform Gotchas

- **No windowing system:** No X11/Wayland. The app renders directly to the Linux framebuffer (`/dev/fb0`)
- **Input handling:** Touch input via Linux input events (`/dev/input/event*`). No mouse cursor by default
- **Limited resources:** Embedded devices often have constrained RAM and CPU. Profile memory usage carefully
- **No browser:** OAuth flows that require a browser are not available. Use device-code flow or pre-provisioned tokens
- **Display resolution:** Must match the framebuffer resolution. Set via kernel boot parameters or `fbset`

### AOT/Trimming

**AOT is strongly recommended for embedded** due to limited resources and startup time requirements.

```xml

<PropertyGroup Condition="'$(RuntimeIdentifier)' == 'linux-arm64'">
  <PublishTrimmed>true</PublishTrimmed>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>

```text

`InvariantGlobalization` reduces binary size by removing ICU data (~28MB). Only use if the app does not need locale-specific formatting.

### Behavior Differences

- **Navigation:** Touch or hardware button only. No keyboard shortcuts or gesture bars
- **Authentication:** Device-code flow or pre-provisioned credentials. No browser-based OAuth
- **Debugging:** Remote only. No local IDE. Use SSH debugging or remote logging

---

## Cross-Target Behavior Differences

### Navigation

| Target | Back Navigation | Deep Linking | Gesture Navigation |
|--------|----------------|--------------|-------------------|
| Web/WASM | Browser back button / Alt+Left | URL-based routing | None |
| iOS | Swipe from left edge | URL schemes / Universal Links | Full gesture support |
| Android | Hardware/software back button | Intent filters / App Links | System back gesture (Android 13+) |
| macOS | Cmd+[ / toolbar back | URL schemes | None |
| Windows | Alt+Left / title bar back | Protocol activation | None |
| Linux | Alt+Left / keyboard | None | None |
| Embedded | Hardware button only | None | Touch only |

### Authentication

| Target | OAuth Flow | Token Storage | Biometric |
|--------|-----------|---------------|-----------|
| Web/WASM | Browser redirect/popup | Browser secure storage | WebAuthn (limited) |
| iOS | `ASWebAuthenticationSession` | Keychain | Touch ID / Face ID |
| Android | Chrome Custom Tabs | Android Keystore | BiometricPrompt |
| macOS | `ASWebAuthenticationSession` | Keychain | Touch ID |
| Windows | System browser / WAM | Credential Manager | Windows Hello |
| Linux | System browser | Secret Service API | None |
| Embedded | Device-code flow | File-based (encrypt) | None |

### Debugging Tools

| Target | IDE Debugger | Hot Reload | Profiling |
|--------|-------------|-----------|-----------|
| Web/WASM | VS / VS Code (CDP) | Yes | Browser DevTools |
| iOS | VS (Pair to Mac) / VS Code / Rider | Yes | Xcode Instruments |
| Android | VS / VS Code | Yes | Android Profiler |
| macOS | VS (Pair to Mac) / VS Code / Rider | Yes | Xcode Instruments |
| Windows | Visual Studio | Yes (+ Live Visual Tree) | VS Diagnostic Tools |
| Linux | VS Code / Rider | Yes | dotnet-counters / dotnet-trace |
| Embedded | VS Code Remote | Yes (SSH) | dotnet-trace (remote) |

---

## Agent Gotchas

1. **Do not hardcode TFM versions in detection logic.** Use version-agnostic globs (`net*-ios`, `net*-android`) to handle both .NET 8 and future .NET versions.
2. **Do not assume all targets share the same debugging workflow.** iOS requires provisioning; Android requires ADB; WASM uses browser DevTools. Each target has distinct tooling.
3. **Do not use JIT-dependent patterns on iOS.** iOS prohibits JIT compilation. Code that uses `Reflection.Emit`, `Expression.Compile()`, or dynamic assembly loading will fail at runtime.
4. **Do not forget platform-specific permissions.** Android runtime permissions, iOS entitlements, macOS sandbox permissions, and WASM CORS policy are all different and must be handled per-target.
5. **Do not deploy untrimmed WASM apps.** Untrimmed WASM bundles exceed 30MB. Always enable trimming and consider AOT for production WASM deployments.
6. **Do not assume file system access on all targets.** WASM has no filesystem; iOS/macOS have sandbox restrictions; embedded may have read-only storage. Use Uno Storage abstractions.
7. **Do not use the same authentication flow for all targets.** WASM uses browser redirects, mobile uses native auth sessions, embedded uses device-code flow. Auth must be configured per-target.

---

## Prerequisites

- .NET 8.0+ with platform workloads: `dotnet workload install ios android maccatalyst wasm-tools`
- Platform SDKs: Xcode (iOS/macOS), Android SDK (Android), GTK3 (Linux)
- Uno Platform 5.x+

---

## References

- [Uno Platform Getting Started](https://platform.uno/docs/articles/get-started.html)
- [Uno Platform Targets](https://platform.uno/docs/articles/getting-started/requirements.html)
- [WASM Deployment](https://platform.uno/docs/articles/features/using-il-linker-webassembly.html)
- [iOS Deployment](https://learn.microsoft.com/en-us/dotnet/maui/ios/deployment/)
- [Android Deployment](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/)
- [Linux with Skia/GTK](https://platform.uno/docs/articles/get-started-with-linux.html)
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
