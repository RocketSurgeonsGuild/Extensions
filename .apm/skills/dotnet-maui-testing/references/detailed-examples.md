
    try
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        var sidePanel = wait.Until(d =>
            d.FindElement(MobileBy.AccessibilityId("side-panel")));

        Assert.True(sidePanel.Displayed);
    }
    finally
    {
        // Restore portrait
        _driver.Orientation = ScreenOrientation.Portrait;
    }
}

```text

---

## XHarness Test Execution

XHarness is a command-line tool for running tests on devices and emulators across platforms. It handles app
installation, test execution, and result collection.

### Running Tests with XHarness

```bash

# Install XHarness
dotnet tool install --global Microsoft.DotNet.XHarness.CLI

# Run on Android emulator
xharness android test \
    --app bin/Release/net8.0-android/com.myapp-Signed.apk \
    --package-name com.myapp \
    --instrumentation devicerunner.AndroidInstrumentation \
    --output-directory test-results/android

# Run on iOS simulator
xharness apple test \
    --app bin/Release/net8.0-ios/MyApp.app \
    --target ios-simulator-64 \
    --output-directory test-results/ios

# Run with specific device
xharness android test \
    --app app.apk \
    --package-name com.myapp \
    --device-id emulator-5554 \
    --output-directory test-results

```text

### XHarness with Device Runner

For xUnit tests running directly on device, add the device runner NuGet package:

```xml

<PackageReference Include="Microsoft.DotNet.XHarness.TestRunners.Xunit" Version="1.*" />

```xml

```csharp

// In the MAUI test app's MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.UseVisualRunner(); // XHarness visual test runner
    return builder.Build();
}

```text

---

## Key Principles

- **Use `AutomationId` for all testable elements.** It is the cross-platform equivalent of `data-testid` and maps to the
  native accessibility identifier on every platform.
- **Run tests against real emulators/simulators, not just unit tests.** MAUI rendering, navigation, and platform
  services behave differently than in-memory tests.
 - **Use explicit waits, never implicit waits or delays.** `WebDriverWait` with a condition is reliable; avoid `Thread.Sleep`.
  and implicit waits hide timing issues.
- **Tag platform-specific tests with `[Trait]` and `Assert.SkipWhen`.** xUnit v3's native skip support allows running
  the correct tests per platform in CI without failures from unsupported features.
- **Apply the page object model for maintainability.** MAUI apps have complex navigation flows; page objects keep tests
  readable as the app grows.

---

## Agent Gotchas

1. **Do not use `FindElement` without a wait strategy.** Elements may not be available immediately after navigation.
   Always use `WebDriverWait` for elements that appear after async operations or page transitions.
2. **Do not hardcode emulator/simulator names.** Use environment variables or test configuration so CI can specify the
   available device. Different CI environments have different emulators installed.
3. **Do not forget to set `AutomationId` on MAUI controls.** Without it, Appium falls back to platform-specific
   selectors (XPath, class name) that differ across Android, iOS, and Windows -- breaking cross-platform tests.
4. **Do not run iOS tests on non-macOS machines.** iOS simulators require Xcode, which is macOS-only. Use
   platform-conditional test skipping or separate CI pipelines per platform.
5. **Do not leave the Appium server unmanaged.** Start Appium as a fixture or CI service, not manually. Forgotten Appium
   processes cause port conflicts and test hangs.

---

## References

- [Appium Documentation](https://appium.io/docs/en/latest/)
- [Appium .NET Client](https://github.com/appium/dotnet-client)
- [.NET MAUI Testing](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/uitest)
- [XHarness](https://github.com/dotnet/xharness)
- [UIAutomator2 Driver](https://github.com/appium/appium-uiautomator2-driver)
- [XCUITest Driver](https://github.com/appium/appium-xcuitest-driver)
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
