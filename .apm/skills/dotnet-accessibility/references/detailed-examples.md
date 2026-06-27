{
    private StarRating Owner => (StarRating)base.Owner;

    public StarRatingAutomationPeer(StarRating owner) : base(owner) { }

    protected override string GetClassNameCore() => nameof(StarRating);
    protected override string GetNameCore()
        => $"Rating: {Owner.Value} out of 5 stars";
    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Slider;

    // IRangeValueProvider
    public double Value => Owner.Value;
    public double Minimum => 0;
    public double Maximum => 5;
    public double SmallChange => 1;
    public double LargeChange => 1;
    public bool IsReadOnly => false;

    public void SetValue(double value)
        => Owner.Value = (int)Math.Clamp(value, Minimum, Maximum);

    public void RaiseValueChanged(int oldValue, int newValue)
    {
        RaisePropertyChangedEvent(
            RangeValuePatternIdentifiers.ValueProperty,
            (double)oldValue, (double)newValue);
    }
}

```text

### Keyboard Accessibility in WinUI

WinUI XAML controls provide built-in keyboard support. Ensure custom controls follow the same patterns:

```xml

<!-- TabIndex controls navigation order -->
<TextBox Header="First name" TabIndex="1" />
<TextBox Header="Last name" TabIndex="2" />
<Button Content="Submit" TabIndex="3" />

<!-- AccessKey provides keyboard shortcuts (Alt + key) -->
<Button Content="Save" AccessKey="S" />
<Button Content="Delete" AccessKey="D" />

```text

For WinUI project setup, XAML patterns, and Windows integration, see [skill:dotnet-winui].

---

## WPF Accessibility (Brief)

WPF on .NET 8+ uses the same UI Automation framework as WinUI. The APIs are nearly identical with namespace differences.

- `AutomationProperties.Name`, `AutomationProperties.HelpText`, `AutomationProperties.LabeledBy` work the same as in
  WinUI
- Custom controls override `OnCreateAutomationPeer()` and return a `FrameworkElementAutomationPeer` subclass
- WPF Fluent theme (.NET 9+) includes high-contrast support automatically
- Use `AutomationProperties.LiveSetting` for live region announcements

```xml

<!-- WPF accessibility follows the same pattern as WinUI -->
<Image Source="product.png"
       AutomationProperties.Name="Product photo" />

<TextBlock x:Name="StatusLabel"
           AutomationProperties.LiveSetting="Polite"
           Text="{Binding StatusText}" />

```text

For WPF development patterns on .NET 8+, see [skill:dotnet-wpf-modern].

---

## Uno Platform Accessibility (Brief)

Uno Platform follows UWP/WinUI `AutomationProperties` patterns since its API surface is WinUI-compatible.

- `AutomationProperties.Name`, `AutomationProperties.HelpText`, `AutomationProperties.LabeledBy` work cross-platform
- Custom `AutomationPeer` implementations follow the WinUI pattern
- On WebAssembly, Uno maps `AutomationProperties` to HTML ARIA attributes automatically
- Platform-specific behavior may vary -- test on each target (Windows, iOS, Android, WASM)

For Uno Platform development patterns, see [skill:dotnet-uno-platform]. For per-target deployment and testing, see
[skill:dotnet-uno-targets].

---

## TUI Accessibility (Brief)

Terminal UI frameworks have inherent accessibility limitations. Screen reader support depends on the terminal emulator
and operating system.

**Terminal.Gui (v2):**

- Screen readers can read terminal text content via the terminal emulator's accessibility support
- No programmatic accessibility API equivalent to ARIA or AutomationProperties
- Logical tab order follows the `TabIndex` property on views
- High contrast is managed by terminal color themes, not the app

**Spectre.Console:**

- Output-only library -- screen readers read terminal text buffer directly
- Use plain text fallbacks for complex visual elements (tables, trees) when accessibility is critical
- `AnsiConsole.Profile.Capabilities` can detect terminal features but not screen reader presence

**Honest constraint:** TUI apps cannot programmatically control screen reader behavior. Terminal emulators provide
varying levels of accessibility support. For applications where accessibility is a hard requirement, consider a GUI
framework (Blazor, MAUI, WinUI) instead.

For Terminal.Gui patterns, see [skill:dotnet-terminal-gui]. For Spectre.Console patterns, see
[skill:dotnet-spectre-console].

---

## Accessibility Testing Tools

### Per-Platform Testing

| Platform     | Primary Tool                                                                             | Secondary Tools                                         |
| ------------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| Windows      | [Accessibility Insights for Windows](https://accessibilityinsights.io/)                  | Narrator (Win+Ctrl+Enter), Inspect.exe (Windows SDK)    |
| Web (Blazor) | [axe-core](https://github.com/dequelabs/axe-core) / axe DevTools                         | Lighthouse (Chrome), WAVE, NVDA, VoiceOver (macOS)      |
| Android      | [Accessibility Scanner](https://support.google.com/accessibility/android/answer/6376570) | TalkBack, Android Studio Layout Inspector               |
| iOS / macOS  | Accessibility Inspector (Xcode)                                                          | VoiceOver (built-in), XCUITest accessibility assertions |

### Automated Testing Integration

```csharp

// Blazor: integrate axe-core with Playwright for automated accessibility testing
// Requires: Deque.AxeCore.Playwright NuGet package
// Install: dotnet add package Deque.AxeCore.Playwright
var axeResults = await new Deque.AxeCore.Playwright.AxeBuilder(page)
    .AnalyzeAsync();

// Check for violations
Assert.Empty(axeResults.Violations);

// WinUI/WPF: use Accessibility Insights for Windows CLI in CI pipelines
// Requires: AccessibilityInsights.CLI (available via Microsoft Store or direct download)

```text

### Manual Testing Checklist

1. **Keyboard-only navigation** -- tab through entire app without mouse; verify all functionality is reachable
2. **Screen reader walkthrough** -- enable Narrator/VoiceOver/TalkBack and navigate the full workflow
3. **High contrast** -- enable system high-contrast theme and verify all content remains visible
4. **Zoom/scaling** -- increase text size to 200% and verify layout does not break or clip content
5. **Color contrast** -- verify all text and interactive elements meet WCAG AA ratios (4.5:1 for text, 3:1 for large
   text and UI components)

---

## WCAG Reference

This skill references the
[Web Content Accessibility Guidelines (WCAG)](https://www.w3.org/WAI/standards-guidelines/wcag/) as the global
accessibility standard. WCAG 2.1 is the current baseline; WCAG 2.2 adds additional criteria for mobile and cognitive
accessibility.

**Four principles (POUR):**

1. **Perceivable** -- information must be presentable in ways all users can perceive
2. **Operable** -- UI components must be operable by all users
3. **Understandable** -- information and UI operation must be understandable
4. **Robust** -- content must be robust enough to work with assistive technologies

**Conformance levels:** A (minimum), AA (recommended target for most apps), AAA (enhanced). Most legal requirements and
industry standards target WCAG 2.1 Level AA.

**Note:** This skill provides technical implementation guidance. It does not constitute legal advice regarding
accessibility compliance requirements, which vary by jurisdiction and application type.

---

## Agent Gotchas

1. **Do not set `SemanticProperties.Description` on MAUI `Label` controls.** It overrides the `Text` property for screen
   readers, causing a mismatch between visual and spoken content. Labels are already accessible via their `Text`
   property.
2. **Do not set `SemanticProperties.Description` on MAUI `Entry`/`Editor` on Android.** Use `Placeholder` or
   `SemanticProperties.Hint` instead -- `Description` conflicts with TalkBack actions on these controls.
3. **Do not use `AutomationProperties.Name` or `AutomationProperties.HelpText` for new MAUI code.** Use
   `SemanticProperties` instead (the MAUI-native API). `AutomationProperties.IsInAccessibleTree` and
   `ExcludedWithChildren` remain valid for controlling accessibility tree inclusion.
4. **Do not omit `aria-label` on icon-only Blazor buttons.** Buttons without visible text content are invisible to
   screen readers unless `aria-label` or `aria-labelledby` is set.
5. **Do not use `aria-live="assertive"` for routine status updates.** Assertive interrupts the screen reader
   immediately. Use `aria-live="polite"` for non-critical updates; reserve assertive for errors and time-critical
   alerts.
6. **Do not assume TUI apps are accessible by default.** Terminal screen reader support varies dramatically by emulator
   and OS. Always provide alternative output formats for critical accessibility scenarios.
7. **Do not hardcode colors without verifying contrast ratios.** Use tools (Accessibility Insights, Lighthouse) to
   verify WCAG AA compliance. System high-contrast themes must also be tested.
8. **Do not forget `AccessKey` on frequently used WinUI/WPF buttons.** Access keys (Alt+key shortcuts) are essential for
   keyboard-dependent users and are trivial to add.

---

## Prerequisites

- .NET 8.0+ (baseline for all frameworks)
- Framework-specific SDKs: MAUI workload, Windows App SDK (WinUI), Blazor project template
- Testing tools: Accessibility Insights (Windows), axe-core (web), Xcode Accessibility Inspector (macOS/iOS)
- Screen readers for manual testing: Narrator (Windows), VoiceOver (macOS/iOS), TalkBack (Android), NVDA (Windows, free)

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

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [WCAG 2.2 Guidelines](https://www.w3.org/TR/WCAG22/)
- [MAUI Accessibility (SemanticProperties)](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/accessibility)
- [WinUI Accessibility Overview](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/accessibility-overview)
- [Blazor Accessibility](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/accessibility)
- [UI Automation Overview](https://learn.microsoft.com/en-us/windows/desktop/WinAuto/uiauto-uiautomationoverview)
- [Accessibility Insights](https://accessibilityinsights.io/)
- [axe-core (Deque)](https://github.com/dequelabs/axe-core)
````
