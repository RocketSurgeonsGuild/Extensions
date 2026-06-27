---
name: dotnet-accessibility
category: ui-frameworks
subcategory: maui
description: Implements accessible .NET UI. SemanticProperties, ARIA, AutomationPeer, testing per platform.
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

# dotnet-accessibility

Cross-platform accessibility patterns for .NET UI frameworks: semantic markup, keyboard navigation, focus management,
color contrast, and screen reader integration. In-depth coverage for Blazor (HTML ARIA), MAUI (SemanticProperties), and
WinUI (AutomationProperties / UI Automation). Brief guidance with cross-references for WPF, Uno Platform, and TUI
frameworks.

## Scope

- Cross-platform accessibility principles (semantic markup, keyboard nav, focus, contrast)
- Blazor accessibility (HTML ARIA attributes, screen reader patterns)
- MAUI accessibility (SemanticProperties, platform-specific setup)
- WinUI accessibility (AutomationProperties, UI Automation, AutomationPeer)
- WPF, Uno Platform, and TUI accessibility guidance with cross-references

## Out of scope

- Framework project setup -- see individual framework skills
- Legal compliance advice (references WCAG but not legal guidance)
- UI framework selection -- see [skill:dotnet-ui-chooser]

Cross-references: [skill:dotnet-blazor-patterns] for Blazor hosting and render modes, [skill:dotnet-blazor-components]
for Blazor component lifecycle, [skill:dotnet-maui-development] for MAUI patterns, [skill:dotnet-winui] for WinUI 3
patterns, [skill:dotnet-wpf-modern] for WPF on .NET 8+, [skill:dotnet-uno-platform] for Uno Platform patterns,
[skill:dotnet-terminal-gui] for Terminal.Gui, [skill:dotnet-spectre-console] for Spectre.Console,
[skill:dotnet-ui-chooser] for framework selection.

---

## Cross-Platform Principles

These principles apply across all .NET UI frameworks. Framework-specific implementations follow in subsequent sections.

### Semantic Markup

Provide meaningful names and descriptions for all interactive and informational elements. Screen readers rely on
semantic metadata -- not visual appearance -- to convey UI structure.

- Every interactive control must have an accessible name (text label, ARIA label, or automation property)
- Images and icons must have text alternatives describing their purpose
- Decorative elements should be hidden from the accessibility tree
- Group related controls logically so screen readers announce them in context

### Keyboard Navigation

All functionality must be operable via keyboard alone. Users who cannot use a mouse, pointer, or touch depend entirely
on keyboard interaction.

- Maintain a logical tab order that follows the visual reading flow
- Provide visible focus indicators on all interactive elements
- Support standard keyboard patterns: Tab/Shift+Tab for navigation, Enter/Space for activation, Escape to dismiss, arrow
  keys within composite controls
- Avoid keyboard traps -- users must be able to navigate away from every control

### Focus Management

Programmatic focus management ensures screen readers announce context changes correctly.

- Move focus to newly revealed content (dialogs, expanded panels, inline notifications)
- Return focus to the triggering element when dismissing overlays
- Avoid stealing focus unexpectedly during background updates
- Set initial focus on the primary action when a page or dialog loads

### Color Contrast

Ensure text and interactive elements meet WCAG contrast ratios.

| Element Type                        | Minimum Ratio (WCAG AA) | Enhanced Ratio (WCAG AAA) |
| ----------------------------------- | ----------------------- | ------------------------- |
| Normal text (< 18pt)                | 4.5:1                   | 7:1                       |
| Large text (>= 18pt or 14pt bold)   | 3:1                     | 4.5:1                     |
| UI components and graphical objects | 3:1                     | 3:1                       |

- Do not rely on color alone to convey information (use icons, patterns, or text labels as supplements)
- Support high-contrast themes and system color overrides
- Test with color blindness simulation tools

---

## Blazor Accessibility (In-Depth)

Blazor renders HTML, so standard web accessibility patterns apply. Use native HTML semantics and ARIA attributes to
build accessible Blazor apps.

### Semantic HTML and ARIA

````razor

@* Use semantic HTML elements for structure *@
<nav aria-label="Main navigation">
    <ul>
        <li><a href="/products">Products</a></li>
        <li><a href="/about">About</a></li>
    </ul>
</nav>

<main>
    <h1>Product Catalog</h1>

    @* Image with alt text *@
    <img src="hero.png" alt="Product showcase displaying three featured items" />

    @* Decorative image hidden from accessibility tree *@
    <img src="divider.svg" alt="" role="presentation" />

    @* Button with accessible name from content *@
    <button @onclick="AddToCart">Add to Cart</button>

    @* Icon button requires aria-label *@
    <button @onclick="ToggleFavorite" aria-label="Add to favorites">
        <span class="icon-heart" aria-hidden="true"></span>
    </button>
</main>

```text

### Keyboard Event Handling

```razor

<div role="listbox"
     tabindex="0"
     aria-label="Product list"
     aria-activedescendant="@_activeId"
     @onkeydown="HandleKeyDown"
     @onkeydown:preventDefault>
    @foreach (var product in Products)
    {
        <div id="@($"product-{product.Id}")"
             role="option"
             aria-selected="@(product.Id == SelectedId)"
             @onclick="() => Select(product)">
            @product.Name
        </div>
    }
</div>

@code {
    private string _activeId = "";

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                MoveSelection(1);
                break;
            case "ArrowUp":
                MoveSelection(-1);
                break;
            case "Enter":
            case " ":
                ConfirmSelection();
                break;
        }
    }
}

```text

### Live Regions

Announce dynamic content changes to screen readers without moving focus:

```razor

@* Polite: announced after current speech finishes *@
<div aria-live="polite" aria-atomic="true">
    @if (_statusMessage is not null)
    {
        <p>@_statusMessage</p>
    }
</div>

@* Assertive: interrupts current speech (use sparingly) *@
<div aria-live="assertive" role="alert">
    @if (_errorMessage is not null)
    {
        <p>@_errorMessage</p>
    }
</div>

```text

### Form Accessibility

```razor

<EditForm Model="@_model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />

    <div>
        <label for="product-name">Product name</label>
        <InputText id="product-name"
                   @bind-Value="_model.Name"
                   aria-describedby="name-error"
                   aria-invalid="@(_nameInvalid ? "true" : null)" />
        <ValidationMessage For="() => _model.Name" id="name-error" />
    </div>

    <div>
        <label for="quantity">Quantity</label>
        <InputNumber id="quantity"
                     @bind-Value="_model.Quantity"
                     aria-describedby="quantity-help"
                     min="1" max="100" />
        <span id="quantity-help">Enter a value between 1 and 100</span>
    </div>

    <button type="submit">Submit Order</button>
</EditForm>

```text

For Blazor hosting models and render mode configuration, see [skill:dotnet-blazor-patterns]. For component lifecycle and
EditForm patterns, see [skill:dotnet-blazor-components].

---

## MAUI Accessibility (In-Depth)

MAUI provides the `SemanticProperties` attached properties as the recommended accessibility API. These map to native
platform accessibility APIs (VoiceOver on iOS/macOS, TalkBack on Android, Narrator on Windows).

### SemanticProperties

```xml

<!-- Description: primary screen reader announcement -->
<Image Source="product.png"
       SemanticProperties.Description="Product photo showing a blue widget" />

<!-- Hint: additional context about an action -->
<Button Text="Add to Cart"
        SemanticProperties.Hint="Adds the current product to your shopping cart" />

<!-- HeadingLevel: enables heading-based navigation -->
<Label Text="Order Summary"
       SemanticProperties.HeadingLevel="Level1" />
<Label Text="Items"
       SemanticProperties.HeadingLevel="Level2" />

```text

**Key APIs:**

- `SemanticProperties.Description` -- short text the screen reader announces (equivalent to `accessibilityLabel` on iOS,
  `contentDescription` on Android)
- `SemanticProperties.Hint` -- additional purpose context (equivalent to `accessibilityHint` on iOS)
- `SemanticProperties.HeadingLevel` -- marks headings (Level1 through Level9); Android and iOS only support a single
  heading level, Windows supports all 9

**Platform warning:** Do not set `SemanticProperties.Description` on a `Label` -- it overrides the `Text` property for
screen readers, creating a mismatch between visual and spoken text. Do not set `SemanticProperties.Description` on
`Entry` or `Editor` on Android -- use `Placeholder` or `SemanticProperties.Hint` instead, because Description conflicts
with TalkBack actions.

### Legacy AutomationProperties

`AutomationProperties` are the older Xamarin.Forms API, superseded by `SemanticProperties` in MAUI. Use
`SemanticProperties` for new code.

| Legacy API                       | Replacement                                                 |
| -------------------------------- | ----------------------------------------------------------- |
| `AutomationProperties.Name`      | `SemanticProperties.Description`                            |
| `AutomationProperties.HelpText`  | `SemanticProperties.Hint`                                   |
| `AutomationProperties.LabeledBy` | Bind `SemanticProperties.Description` to the label's `Text` |

`AutomationProperties.IsInAccessibleTree` and `AutomationProperties.ExcludedWithChildren` remain useful for controlling
accessibility tree inclusion.

### Programmatic Focus and Announcements

```csharp

// Move screen reader focus to a specific element
myLabel.SetSemanticFocus();

// Announce text to the screen reader without moving focus
SemanticScreenReader.Default.Announce("Item added to cart successfully.");

```text

### Accessible Custom Controls

When building custom controls, ensure accessibility metadata is set:

```csharp

public class RatingControl : ContentView
{
    private int _rating;

    public int Rating
    {
        get => _rating;
        set
        {
            _rating = value;
            SemanticProperties.SetDescription(this,
                $"Rating: {value} out of 5 stars");
            SemanticScreenReader.Default.Announce(
                $"Rating changed to {value} stars");
        }
    }
}

```text

For MAUI project structure, MVVM patterns, and platform services, see [skill:dotnet-maui-development].

---

## WinUI Accessibility (In-Depth)

WinUI 3 / Windows App SDK builds on the Microsoft UI Automation framework. Built-in controls include automation support
by default. Custom controls need automation peers.

### AutomationProperties

```xml

<!-- Name: primary accessible name for screen readers -->
<Image Source="ms-appx:///Assets/product.png"
       AutomationProperties.Name="Product photo showing a blue widget" />

<!-- HelpText: supplementary description -->
<Button Content="Add to Cart"
        AutomationProperties.HelpText="Adds the current product to your shopping cart" />

<!-- LabeledBy: associates a label with a control -->
<TextBlock x:Name="QuantityLabel" Text="Quantity:" />
<NumberBox AutomationProperties.LabeledBy="{x:Bind QuantityLabel}"
           Value="{x:Bind ViewModel.Quantity, Mode=TwoWay}" />

<!-- Hide decorative elements from accessibility tree -->
<Image Source="ms-appx:///Assets/divider.png"
       AutomationProperties.AccessibilityView="Raw" />

```text

### Custom Automation Peers

For custom controls, implement an `AutomationPeer` to expose the control to UI Automation clients:

```csharp

// Custom control
public sealed class StarRating : Control
{
    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int),
            typeof(StarRating), new PropertyMetadata(0, OnValueChanged));

    private static void OnValueChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (FrameworkElementAutomationPeer
                .FromElement((StarRating)d) is StarRatingAutomationPeer peer)
        {
            peer.RaiseValueChanged((int)e.OldValue, (int)e.NewValue);
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer()
        => new StarRatingAutomationPeer(this);
}

// Automation peer (using Microsoft.UI.Xaml.Automation.Provider)
public sealed class StarRatingAutomationPeer
    : FrameworkElementAutomationPeer, IRangeValueProvider
{

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
