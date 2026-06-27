---
name: dotnet-spectre-console
category: developer-experience
subcategory: cli
description: Renders rich console output. Spectre.Console tables, trees, progress, prompts, live displays.
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

# dotnet-spectre-console

Spectre.Console for building rich console output (tables, trees, progress bars, prompts, markup, live displays) and
Spectre.Console.Cli for structured command-line application parsing. Cross-platform across Windows, macOS, and Linux
terminals.

**Version assumptions:** .NET 8.0+ baseline. Spectre.Console 0.54.0 (latest stable). Spectre.Console.Cli 0.53.1 (latest
stable). Both packages target net8.0+ and netstandard2.0.

## Scope

- Spectre.Console rich output: markup, tables, trees, progress bars, prompts, live displays
- Spectre.Console.Cli command-line application framework

## Out of scope

- Full TUI applications (windows, menus, dialogs, views) -- see [skill:dotnet-terminal-gui]
- System.CommandLine parsing -- see [skill:dotnet-system-commandline]
- CLI application architecture and distribution -- see [skill:dotnet-cli-architecture] and
  [skill:dotnet-cli-distribution]

Cross-references: [skill:dotnet-terminal-gui] for full TUI alternative, [skill:dotnet-system-commandline] for
System.CommandLine scope boundary, [skill:dotnet-cli-architecture] for CLI structure,
[skill:dotnet-csharp-async-patterns] for async patterns, [skill:dotnet-csharp-dependency-injection] for DI with
Spectre.Console.Cli, [skill:dotnet-accessibility] for TUI accessibility limitations and screen reader considerations.

---

## Package References

````xml

<ItemGroup>
  <!-- Rich console output: markup, tables, trees, progress, prompts, live displays -->
  <PackageReference Include="Spectre.Console" Version="0.54.0" />

  <!-- CLI command framework (adds command parsing, settings, DI support) -->
  <PackageReference Include="Spectre.Console.Cli" Version="0.53.1" />
</ItemGroup>

```bash

Spectre.Console.Cli has a dependency on Spectre.Console -- install both only when you need the CLI framework. For rich output only, Spectre.Console alone is sufficient.

---

## Markup and Styling

Spectre.Console uses a BBCode-inspired markup syntax for styled console output.

### Basic Markup

```csharp

using Spectre.Console;

// Styled text with markup tags
AnsiConsole.MarkupLine("[bold red]Error:[/] File not found.");
AnsiConsole.MarkupLine("[green]Success![/] Build completed in [blue]2.3s[/].");
AnsiConsole.MarkupLine("[underline]https://example.com[/]");
AnsiConsole.MarkupLine("[dim italic]This is subtle text[/]");

// Nested styles
AnsiConsole.MarkupLine("[bold [red on white]Warning:[/] check config[/]");

// Escape brackets with double brackets
AnsiConsole.MarkupLine("Use [[bold]] for bold text.");

```text

### Figlet Text

```csharp

AnsiConsole.Write(
    new FigletText("Hello!")
        .Color(Color.Green)
        .Centered());

```text

### Rule (Horizontal Line)

```csharp

// Simple rule
AnsiConsole.Write(new Rule());

// Titled rule
AnsiConsole.Write(new Rule("[yellow]Section Title[/]"));

// Aligned rule
AnsiConsole.Write(new Rule("[blue]Left Aligned[/]").LeftJustified());

```text

---

## Tables

```csharp

var table = new Table();

// Add columns
table.AddColumn("Name");
table.AddColumn(new TableColumn("Age").Centered());
table.AddColumn(new TableColumn("City").RightAligned());

// Add rows
table.AddRow("Alice", "30", "Seattle");
table.AddRow("[green]Bob[/]", "25", "Portland");
table.AddRow("Charlie", "35", "Vancouver");

// Styling
table.Border(TableBorder.Rounded);
table.BorderColor(Color.Grey);
table.Title("[underline]Team Members[/]");
table.Caption("[dim]Updated daily[/]");

// Column configuration
table.Columns[0].PadLeft(2);
table.Columns[0].NoWrap();

AnsiConsole.Write(table);

```text

### Nested Tables

```csharp

var innerTable = new Table()
    .AddColumn("Detail")
    .AddColumn("Value")
    .AddRow("Role", "Developer")
    .AddRow("Level", "Senior");

var outerTable = new Table()
    .AddColumn("Name")
    .AddColumn("Info")
    .AddRow("Alice", innerTable);

AnsiConsole.Write(outerTable);

```text

---

## Trees

```csharp

var tree = new Tree("Solution");

// Add nodes
var srcNode = tree.AddNode("[yellow]src[/]");
var apiNode = srcNode.AddNode("Api");
apiNode.AddNode("Controllers/");
apiNode.AddNode("Program.cs");

var libNode = srcNode.AddNode("Library");
libNode.AddNode("Services/");

var testNode = tree.AddNode("[blue]tests[/]");
testNode.AddNode("Api.Tests/");

// Styling
tree.Style = Style.Parse("dim");

AnsiConsole.Write(tree);

```text

---

## Panels

```csharp

var panel = new Panel("This is [green]important[/] content.")
    .Header("[bold]Notice[/]")
    .Border(BoxBorder.Rounded)
    .BorderColor(Color.Blue)
    .Padding(2, 1)    // horizontal, vertical
    .Expand();         // fill available width

AnsiConsole.Write(panel);

```text

### Composing Renderables with Columns

```csharp

AnsiConsole.Write(new Columns(
    new Panel("Left panel").Expand(),
    new Panel("Right panel").Expand()));

```csharp

---

## Progress Displays

### Progress Bars

```csharp

await AnsiConsole.Progress()
    .AutoClear(false)       // keep completed tasks visible
    .HideCompleted(false)
    .Columns(
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn())
    .StartAsync(async ctx =>
    {
        var downloadTask = ctx.AddTask("[green]Downloading[/]", maxValue: 100);
        var extractTask = ctx.AddTask("[blue]Extracting[/]", maxValue: 100);

        while (!ctx.IsFinished)
        {
            await Task.Delay(50);
            downloadTask.Increment(1.5);

            if (downloadTask.Value > 50)
            {
                extractTask.Increment(0.8);
            }
        }
    });

```text

### Status Spinners

```csharp

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .SpinnerStyle(Style.Parse("green bold"))
    .StartAsync("Processing...", async ctx =>
    {
        await Task.Delay(1000);
        ctx.Status("Compiling...");
        ctx.Spinner(Spinner.Known.Star);
        await Task.Delay(1000);
        ctx.Status("Publishing...");
        await Task.Delay(1000);
    });

```text

---

## Prompts

### Text Prompt

```csharp

// Simple typed input
var name = AnsiConsole.Ask<string>("What's your [green]name[/]?");
var age = AnsiConsole.Ask<int>("What's your [green]age[/]?");

// With default value
var city = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [green]city[/]:")
        .DefaultValue("Seattle")
        .ShowDefaultValue());

// Secret input (password)
var password = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [green]password[/]:")
        .Secret());

// With validation
var email = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [green]email[/]:")
        .Validate(input =>
            input.Contains('@') && input.Contains('.')
                ? ValidationResult.Success()
                : ValidationResult.Error("[red]Invalid email address[/]")));

// Optional (allow empty)
var nickname = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [green]nickname[/] (optional):")
        .AllowEmpty());

```text

### Confirmation Prompt

```csharp

bool proceed = AnsiConsole.Confirm("Continue with deployment?");

```csharp

### Selection Prompt

```csharp

var fruit = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Pick a [green]fruit[/]:")
        .PageSize(10)
        .EnableSearch()
        .WrapAround()
        .AddChoices("Apple", "Banana", "Orange", "Mango", "Grape"));

// Grouped choices
var country = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select [green]destination[/]:")
        .AddChoiceGroup("Europe", "France", "Italy", "Spain")
        .AddChoiceGroup("Asia", "Japan", "Thailand", "Vietnam"));

```text

### Multi-Selection Prompt

```csharp

var toppings = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>()
        .Title("Choose [green]toppings[/]:")
        .PageSize(10)
        .Required()
        .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
        .AddChoices("Cheese", "Pepperoni", "Mushrooms", "Olives", "Onions"));

```text

---

## Live Displays

Live displays update in-place for dynamic content that changes over time.

```csharp

var table = new Table()
    .AddColumn("Time")
    .AddColumn("Status");

await AnsiConsole.Live(table)
    .AutoClear(false)
    .Overflow(VerticalOverflow.Ellipsis)
    .Cropping(VerticalOverflowCropping.Bottom)
    .StartAsync(async ctx =>
    {
        table.AddRow(DateTime.Now.ToString("T"), "[yellow]Starting...[/]");
        ctx.Refresh();
        await Task.Delay(1000);

        table.AddRow(DateTime.Now.ToString("T"), "[green]Processing...[/]");
        ctx.Refresh();
        await Task.Delay(1000);

        table.AddRow(DateTime.Now.ToString("T"), "[blue]Complete![/]");
        ctx.Refresh();
    });

```text

### Replacing the Target

```csharp

await AnsiConsole.Live(new Markup("[yellow]Initializing...[/]"))
    .StartAsync(async ctx =>
    {
        await Task.Delay(1000);
        ctx.UpdateTarget(new Markup("[green]Ready![/]"));

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
