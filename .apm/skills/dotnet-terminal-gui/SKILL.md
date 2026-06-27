---
name: dotnet-terminal-gui
category: developer-experience
subcategory: cli
description: Builds full TUI apps. Terminal.Gui v2 -- views, layout (Pos/Dim), menus, dialogs, bindings, themes.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-terminal-gui

Terminal.Gui v2 for building full terminal user interfaces with windows, menus, dialogs, views, layout, event handling,
color themes, and mouse support. Cross-platform across Windows, macOS, and Linux terminals.

**Version assumptions:** .NET 8.0+ baseline. Terminal.Gui 2.0.0-alpha (v2 Alpha is the active development line for new
projects -- API is stable with comprehensive features; breaking changes possible before Beta but core architecture is
solid). v1.x (1.19.0) is in maintenance mode with no new features.

For detailed code examples (views, menus, dialogs, events, themes, complete editor), see `examples.md` in this skill
directory.

## Scope

- Terminal.Gui v2 application lifecycle and initialization
- Views, layout (Pos/Dim), menus, dialogs, event handling
- Data binding, color themes, mouse support

## Out of scope

- Rich console output (tables, progress bars, prompts) -- see [skill:dotnet-spectre-console]
- CLI command-line parsing -- see [skill:dotnet-system-commandline]
- CLI application architecture and distribution -- see [skill:dotnet-cli-architecture] and
  [skill:dotnet-cli-distribution]

Cross-references: [skill:dotnet-spectre-console] for rich console output alternative,
[skill:dotnet-csharp-async-patterns] for async TUI patterns, [skill:dotnet-native-aot] for AOT compilation
considerations, [skill:dotnet-system-commandline] for CLI parsing, [skill:dotnet-csharp-dependency-injection] for DI in
TUI apps, [skill:dotnet-accessibility] for TUI accessibility limitations and screen reader considerations.

---

## Package Reference

````xml

<ItemGroup>
  <PackageReference Include="Terminal.Gui" Version="2.0.0-alpha.*" />
</ItemGroup>

```xml

---

## Application Lifecycle

Terminal.Gui v2 uses an instance-based model with `IApplication` and `IDisposable` for proper resource cleanup.

### Basic Application

```csharp

using Terminal.Gui;

using IApplication app = Application.Create().Init();

var window = new Window
{
    Title = "My TUI App",
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

var label = new Label
{
    Text = "Hello, Terminal.Gui!",
    X = Pos.Center(),
    Y = Pos.Center()
};
window.Add(label);

app.Run(window);

```text

---

## Layout System

Terminal.Gui v2 unifies layout into a single model. Position is controlled by `Pos` (X, Y) and size by `Dim` (Width, Height), both relative to the SuperView's content area.

### Pos Types (Positioning)

```csharp

view.X = 5;                          // Absolute
view.X = Pos.Percent(25);            // 25% from left
view.X = Pos.Center();               // Centered
view.X = Pos.AnchorEnd(10);          // 10 from right edge
view.X = Pos.Right(otherView) + 1;   // Relative to another view
view.Y = Pos.Bottom(otherView) + 1;
view.X = Pos.Align(Alignment.End);   // Align groups
view.X = Pos.Func(() => CalculateX());

```text

### Dim Types (Sizing)

```csharp

view.Width = 40;                       // Absolute
view.Width = Dim.Percent(50);          // 50% of parent
view.Width = Dim.Fill();               // Fill remaining space
view.Width = Dim.Auto();               // Size based on content
view.Width = Dim.Auto(minimumContentDim: 20);
view.Width = Dim.Width(otherView);     // Relative to another view
view.Width = Dim.Func(() => CalculateWidth());

```text

### Frame vs. Viewport

- **Frame** -- outermost rectangle: location and size relative to SuperView
- **Viewport** -- visible portion of content area: acts as a scrollable portal into the view's content

---

## Cross-Platform Considerations

| Feature | Windows Terminal | macOS Terminal.app | Linux (xterm/gnome) |
|---|---|---|---|
| TrueColor (24-bit) | Yes | Yes | Yes (most) |
| Mouse support | Yes | Yes | Yes |
| Unicode/emoji | Yes | Yes | Varies |
| Key modifiers | Full | Limited | Full |

### Platform-Specific Notes

- **macOS Terminal.app** -- limited modifier key support; iTerm2 and WezTerm provide better support
- **SSH sessions** -- terminal capabilities depend on the client terminal, not the server
- **Windows Console Host** -- legacy conhost has limited Unicode support; Windows Terminal provides full support
- **tmux/screen** -- may intercept key combinations; set `TERM=xterm-256color`

---

## Agent Gotchas

1. **Do not use v1 static lifecycle pattern.** v2 uses instance-based `Application.Create().Init()` with `IDisposable`. Always wrap in a `using` statement.
2. **Do not use `View.AutoSize`.** Removed in v2. Use `Dim.Auto()` instead.
3. **Do not confuse Frame with Viewport.** Frame is the outer rectangle; Viewport is the visible content area (supports scrolling).
4. **Do not use `Button.Clicked`.** Replaced by `Button.Accepting` in v2.
5. **Do not call UI operations from background threads.** Terminal.Gui is single-threaded. Use `Application.Invoke()` to marshal calls back to the UI thread.
6. **Do not forget `RequestStop()` to close windows.** Calling `Dispose()` directly corrupts terminal state.
7. **Do not hardcode terminal dimensions.** Use `Dim.Fill()`, `Dim.Percent()`, and `Pos.Center()` for responsive layouts.
8. **Do not ignore terminal state on crash.** Wrap `app.Run()` in try/catch and ensure the `using` block disposes the application.
9. **Do not use `ScrollView`.** Removed in v2. All views now support scrolling natively via `SetContentSize()`.
10. **Do not use `NStack.ustring`.** Removed in v2. Use standard `System.String`.
11. **Do not use `StatusItem`.** Removed in v2. Use `Shortcut` objects with `StatusBar.Add()` instead.

---

## Prerequisites

- **NuGet package:** `Terminal.Gui` 2.0.0-alpha (v2) or 1.19.x (v1 maintenance)
- **Target framework:** net8.0 or later
- **Terminal:** Any terminal emulator supporting ANSI escape sequences

---

## References

- [Terminal.Gui GitHub](https://github.com/gui-cs/Terminal.Gui)
- [Terminal.Gui v2 Documentation](https://gui-cs.github.io/Terminal.Gui/)
- [Terminal.Gui NuGet](https://www.nuget.org/packages/Terminal.Gui)
- [Terminal.Gui v2 What's New](https://gui-cs.github.io/Terminal.Gui/docs/newinv2)
- [v1 to v2 Migration Guide](https://github.com/gui-cs/Terminal.Gui/blob/v2_develop/docfx/docs/migratingfromv1.md)
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
