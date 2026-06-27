        ctx.UpdateTarget(new Markup("[green]Ready![/]"));
        await Task.Delay(1000);
        ctx.UpdateTarget(
            new Panel("Final result: [bold]42[/]")
                .Header("Done")
                .Border(BoxBorder.Rounded));
    });

```text

---

## Spectre.Console.Cli Framework

Spectre.Console.Cli provides a structured command-line parsing framework with command hierarchies, typed settings, validation, and automatic help generation.

### Basic Command App

```csharp

using Spectre.Console.Cli;

var app = new CommandApp<GreetCommand>();
return app.Run(args);

// Command with typed settings
public sealed class GreetSettings : CommandSettings
{
    [CommandArgument(0, "<name>")]
    [Description("The person to greet")]
    public string Name { get; init; } = string.Empty;

    [CommandOption("-c|--count")]
    [Description("Number of times to greet")]
    [DefaultValue(1)]
    public int Count { get; init; }

    [CommandOption("--shout")]
    [Description("Greet in uppercase")]
    public bool Shout { get; init; }
}

public sealed class GreetCommand : Command<GreetSettings>
{
    public override int Execute(CommandContext context, GreetSettings settings)
    {
        for (int i = 0; i < settings.Count; i++)
        {
            var greeting = $"Hello, {settings.Name}!";
            AnsiConsole.MarkupLine(settings.Shout
                ? $"[bold]{greeting.ToUpperInvariant()}[/]"
                : greeting);
        }
        return 0;  // exit code
    }
}

```text

### Command Hierarchy with Branches

```csharp

var app = new CommandApp();
app.Configure(config =>
{
    config.AddBranch<RemoteSettings>("remote", remote =>
    {
        remote.AddCommand<RemoteAddCommand>("add")
            .WithDescription("Add a remote");
        remote.AddCommand<RemoteRemoveCommand>("remove")
            .WithDescription("Remove a remote");
    });

    config.AddCommand<CloneCommand>("clone")
        .WithDescription("Clone a repository");
});

return app.Run(args);

// Shared settings for the branch -- inherited by subcommands
public class RemoteSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Verbose output")]
    public bool Verbose { get; init; }
}

// Subcommand settings inherit from branch settings
public sealed class RemoteAddSettings : RemoteSettings
{
    [CommandArgument(0, "<name>")]
    public string Name { get; init; } = string.Empty;

    [CommandArgument(1, "<url>")]
    public string Url { get; init; } = string.Empty;
}

public sealed class RemoteAddCommand : Command<RemoteAddSettings>
{
    public override int Execute(CommandContext context, RemoteAddSettings settings)
    {
        if (settings.Verbose)
        {
            AnsiConsole.MarkupLine($"[dim]Adding remote...[/]");
        }
        AnsiConsole.MarkupLine($"Added remote [green]{settings.Name}[/] -> {settings.Url}");
        return 0;
    }
}

```text

### Settings Validation

```csharp

public sealed class DeploySettings : CommandSettings
{
    [CommandArgument(0, "<environment>")]
    public string Environment { get; init; } = string.Empty;

    [CommandOption("--timeout")]
    [DefaultValue(30)]
    public int TimeoutSeconds { get; init; }

    public override ValidationResult Validate()
    {
        var validEnvs = new[] { "dev", "staging", "prod" };
        if (!validEnvs.Contains(Environment, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Error(
                $"Environment must be one of: {string.Join(", ", validEnvs)}");
        }

        if (TimeoutSeconds <= 0)
        {
            return ValidationResult.Error("Timeout must be positive");
        }

        return ValidationResult.Success();
    }
}

```text

### Async Commands

```csharp

public sealed class FetchCommand : AsyncCommand<FetchSettings>
{
    public override async Task<int> ExecuteAsync(
        CommandContext context, FetchSettings settings)
    {
        await AnsiConsole.Status()
            .StartAsync("Fetching data...", async ctx =>
            {
                await Task.Delay(2000); // simulate work
            });

        AnsiConsole.MarkupLine("[green]Done![/]");
        return 0;
    }
}

```text

### Dependency Injection with ITypeRegistrar

```csharp

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

// Set up DI container
var services = new ServiceCollection();
services.AddSingleton<IGreetingService, GreetingService>();
services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);

var registrar = new TypeRegistrar(services);
var app = new CommandApp<GreetCommand>(registrar);
return app.Run(args);

// TypeRegistrar bridges Microsoft DI to Spectre.Console.Cli
public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

    public void Register(Type service, Type implementation)
        => services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation)
        => services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory)
        => services.AddSingleton(service, _ => factory());
}

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object? Resolve(Type? type)
        => type is null ? null : provider.GetService(type);
}

// Command receives services via constructor injection
public sealed class GreetCommand(IGreetingService greetingService) : Command<GreetSettings>
{
    public override int Execute(CommandContext context, GreetSettings settings)
    {
        var message = greetingService.GetGreeting(settings.Name);
        AnsiConsole.MarkupLine(message);
        return 0;
    }
}

```text

---

## Testable Console Output

Spectre.Console provides `IAnsiConsole` for testable output instead of writing directly to the real console.

```csharp

// Production: use AnsiConsole.Console (the real console)
IAnsiConsole console = AnsiConsole.Console;

// Testing: use a recording console
var console = AnsiConsole.Create(new AnsiConsoleSettings
{
    Out = new AnsiConsoleOutput(new StringWriter())
});

// Use the abstraction instead of static AnsiConsole methods
console.MarkupLine("[green]Testable output[/]");
console.Write(new Table().AddColumn("Col").AddRow("Val"));

```text

---

## Agent Gotchas

1. **Do not use `AnsiConsole.Markup*` in redirected output.** When stdout is redirected (piped to a file or another process), ANSI escape codes corrupt the output. Check `AnsiConsole.Profile.Capabilities.Ansi` before using markup, or use `IAnsiConsole` with appropriate settings. See [skill:dotnet-csharp-async-patterns] for async pipeline patterns.
2. **Do not assume ANSI support in CI environments.** CI runners (GitHub Actions, Azure Pipelines) may not support ANSI escape codes. Set `TERM=dumb` or use `AnsiConsole.Create()` with `ColorSystemSupport.NoColors` for CI-safe output. Spectre.Console auto-detects capabilities, but explicit configuration prevents flaky rendering.
3. **Do not mix `AnsiConsole` static calls with `IAnsiConsole` instance calls.** Static `AnsiConsole.Write()` always targets the real console. When using DI with `IAnsiConsole`, consistently use the injected instance. Mixing the two produces duplicated or interleaved output.
4. **Do not modify a renderable from a background thread during `Live()`.** Live displays are not thread-safe. Mutate the target renderable only inside the `Start`/`StartAsync` callback, then call `ctx.Refresh()`. Concurrent mutations cause corrupted terminal output.
5. **Do not use prompts in non-interactive contexts.** `TextPrompt`, `SelectionPrompt`, and `ConfirmationPrompt` block waiting for user input. In CI or automated scripts, use environment variables or command-line arguments for input instead of prompts. Check `AnsiConsole.Profile.Capabilities.Interactive` before prompting.
6. **Do not confuse Spectre.Console.Cli with System.CommandLine.** They are independent frameworks with different APIs. Spectre.Console.Cli uses `CommandSettings` classes with `[CommandArgument]`/`[CommandOption]` attributes, while System.CommandLine uses `Option<T>` and `Argument<T>` builder pattern. Do not mix APIs. For System.CommandLine, see [skill:dotnet-system-commandline].
7. **Do not forget `ctx.Refresh()` after modifying live display content.** Changes to tables, trees, or panels inside a `Live()` callback are not rendered until `ctx.Refresh()` is called. Omitting it produces stale displays.
8. **Do not hardcode color values without fallback.** Terminals with limited color support silently degrade TrueColor values. Use named colors (`Color.Green`) when possible and test with `NO_COLOR=1` environment variable to verify graceful degradation.

---

## Prerequisites

- **NuGet packages:** `Spectre.Console` 0.54.0 for rich output; add `Spectre.Console.Cli` 0.53.1 for CLI framework
- **Target framework:** net8.0 or later (also supports netstandard2.0)
- **Terminal:** Any terminal emulator supporting ANSI escape sequences. Windows Terminal, iTerm2, or modern Linux terminal recommended for best experience (TrueColor, Unicode). Console output degrades gracefully on limited terminals.
- **For DI with Spectre.Console.Cli:** `Microsoft.Extensions.DependencyInjection` package for the `ITypeRegistrar`/`ITypeResolver` bridge

---

## References

- [Spectre.Console GitHub](https://github.com/spectreconsole/spectre.console) -- source code, issues, samples
- [Spectre.Console Documentation](https://spectreconsole.net/) -- official guides and API reference
- [Spectre.Console NuGet](https://www.nuget.org/packages/Spectre.Console) -- package downloads and version history
- [Spectre.Console.Cli NuGet](https://www.nuget.org/packages/Spectre.Console.Cli) -- CLI framework package
- [Spectre.Console Examples](https://github.com/spectreconsole/spectre.console/tree/main/examples) -- official example projects
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
