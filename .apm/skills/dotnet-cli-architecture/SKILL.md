---
name: dotnet-cli-architecture
category: developer-experience
subcategory: cli
description: Structures CLI app layers. Command/handler/service separation, clig.dev principles, exit codes.
license: MIT
targets: ['*']
tags: [architecture, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for architecture tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-cli-architecture

Layered CLI application architecture for .NET: command/handler/service separation following clig.dev principles,
configuration precedence (appsettings → environment variables → CLI arguments), structured logging in CLI context, exit
code conventions, stdin/stdout/stderr patterns, and testing CLI applications via in-process invocation with output
capture.

**Version assumptions:** .NET 8.0+ baseline. Patterns apply to CLI tools built with System.CommandLine 2.0 and generic
host.

## Scope

- Layered command/handler/service architecture for CLI apps
- clig.dev principles for .NET (stdout/stderr, exit codes, NO_COLOR)
- Configuration precedence (appsettings, env vars, CLI args)
- Structured logging in CLI context
- Stdin/stdout/stderr patterns and machine-readable output
- Testing CLI applications via in-process invocation

## Out of scope

- System.CommandLine API details (RootCommand, Option<T>, SetAction) -- see [skill:dotnet-system-commandline]
- Native AOT compilation and publish pipeline -- see [skill:dotnet-native-aot]
- CLI distribution and packaging -- see [skill:dotnet-cli-distribution] and [skill:dotnet-cli-packaging]
- General CI/CD patterns -- see [skill:dotnet-gha-patterns] and [skill:dotnet-ado-patterns]
- DI container internals -- see [skill:dotnet-csharp-dependency-injection]
- General testing strategies -- see [skill:dotnet-testing-strategy]

Cross-references: [skill:dotnet-system-commandline] for System.CommandLine 2.0 API, [skill:dotnet-native-aot] for AOT
publishing CLI tools, [skill:dotnet-csharp-dependency-injection] for DI patterns, [skill:dotnet-csharp-configuration]
for configuration integration, [skill:dotnet-testing-strategy] for general testing patterns.

---

## clig.dev Principles for .NET CLI Tools

The [Command Line Interface Guidelines](https://clig.dev/) provide language-agnostic principles for well-behaved CLI
tools. These translate directly to .NET patterns.

### Core Principles

| Principle                             | Implementation                                                 |
| ------------------------------------- | -------------------------------------------------------------- |
| Human-first output by default         | Use `Console.Out` for data, `Console.Error` for diagnostics    |
| Machine-readable output with `--json` | Add a `--json` global option that switches output format       |
| Stderr for status/diagnostics         | Logging, progress bars, and prompts go to stderr               |
| Stdout for data only                  | Piped output (`mycli list \| jq .`) must not contain log noise |
| Non-zero exit on failure              | Return specific exit codes (see conventions below)             |
| Fail early, fail loudly               | Validate inputs before doing work                              |
| Respect `NO_COLOR`                    | Check `Environment.GetEnvironmentVariable("NO_COLOR")`         |
| Support `--verbose` and `--quiet`     | Global options controlling output verbosity                    |

### Stdout vs Stderr in .NET

````csharp

// Data output -- goes to stdout (can be piped)
Console.Out.WriteLine(JsonSerializer.Serialize(result, jsonContext.Options));

// Status/diagnostic output -- goes to stderr (user sees it, pipe ignores it)
Console.Error.WriteLine("Processing 42 files...");

// With ILogger (when using hosting)
// ILogger writes to stderr via console provider by default
logger.LogInformation("Connected to {Endpoint}", endpoint);

```text

---

## Layered Command → Handler → Service Architecture

Separate CLI concerns into three layers:

```bash

┌─────────────────────────────────────┐
│  Commands (System.CommandLine)      │  Parse args, wire options
│  ─ RootCommand, Command, Option<T>  │
├─────────────────────────────────────┤
│  Handlers (orchestration)           │  Coordinate services, format output
│  ─ ICommandHandler implementations  │
├─────────────────────────────────────┤
│  Services (business logic)          │  Pure logic, no CLI concerns
│  ─ Interfaces + implementations     │
└─────────────────────────────────────┘

```text

### Why Three Layers

- **Commands** know about CLI syntax (options, arguments, subcommands) but not business logic
- **Handlers** bridge CLI inputs to service calls and format results for output
- **Services** contain domain logic and are reusable outside the CLI (tests, libraries, APIs)

### Example Structure

```text

src/
  MyCli/
    MyCli.csproj
    Program.cs                    # RootCommand + CommandLineBuilder
    Commands/
      SyncCommandDefinition.cs    # Command, options, arguments
    Handlers/
      SyncHandler.cs              # ICommandHandler, orchestrates services
    Services/
      ISyncService.cs             # Business logic interface
      SyncService.cs              # Implementation (no CLI awareness)
    Output/
      ConsoleFormatter.cs         # Table/JSON output formatting

```csharp

### Command Definition Layer

```csharp

// Commands/SyncCommandDefinition.cs
public static class SyncCommandDefinition
{
    public static readonly Option<Uri> SourceOption = new(
        "--source", "Source endpoint URL") { IsRequired = true };

    public static readonly Option<bool> DryRunOption = new(
        "--dry-run", "Preview changes without applying");

    public static Command Create()
    {
        var command = new Command("sync", "Synchronize data from source");
        command.AddOption(SourceOption);
        command.AddOption(DryRunOption);
        return command;
    }
}

```bash

### Handler Layer

```csharp

// Handlers/SyncHandler.cs
public class SyncHandler : ICommandHandler
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncHandler> _logger;

    public SyncHandler(ISyncService syncService, ILogger<SyncHandler> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    // Bound by naming convention from options
    public Uri Source { get; set; } = null!;
    public bool DryRun { get; set; }

    public int Invoke(InvocationContext context) =>
        InvokeAsync(context).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var ct = context.GetCancellationToken();

        _logger.LogInformation("Syncing from {Source}", Source);

        var result = await _syncService.SyncAsync(Source, DryRun, ct);

        if (result.HasErrors)
        {
            context.Console.Error.Write($"Sync failed: {result.ErrorMessage}\n");
            return ExitCodes.SyncFailed;
        }

        context.Console.Out.Write($"Synced {result.ItemCount} items.\n");
        return ExitCodes.Success;
    }
}

```text

### Service Layer

```csharp

// Services/ISyncService.cs -- no CLI dependency
public interface ISyncService
{
    Task<SyncResult> SyncAsync(Uri source, bool dryRun, CancellationToken ct);
}

// Services/SyncService.cs
public class SyncService : ISyncService
{
    private readonly HttpClient _httpClient;

    public SyncService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SyncResult> SyncAsync(
        Uri source, bool dryRun, CancellationToken ct)
    {
        // Pure business logic -- testable without CLI infrastructure
        var data = await _httpClient.GetFromJsonAsync<SyncData>(source, ct);
        // ...
        return new SyncResult(ItemCount: data.Items.Length);
    }
}

```text

---

## Configuration Precedence

CLI tools use a specific configuration precedence (lowest to highest priority):

1. **Compiled defaults** -- hardcoded fallback values
2. **appsettings.json** -- shipped with the tool
3. **appsettings.{Environment}.json** -- environment-specific overrides
4. **Environment variables** -- set by shell or CI
5. **CLI arguments** -- explicit user input (highest priority)

### Implementation with Generic Host

```csharp

var builder = new CommandLineBuilder(rootCommand)
    .UseHost(_ => Host.CreateDefaultBuilder(args), host =>
    {
        host.ConfigureAppConfiguration((ctx, config) =>
        {
            // Layers 2-3 handled by CreateDefaultBuilder:
            //   appsettings.json, appsettings.{env}.json, env vars

            // Layer 4: User-specific config file
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".mycli", "config.json");
            if (File.Exists(configPath))
            {
                config.AddJsonFile(configPath, optional: true);
            }
        });

        // Layer 5: CLI args override everything
        // System.CommandLine options take precedence via handler binding
    })
    .UseDefaults()
    .Build();

```bash

### User-Level Configuration

Many CLI tools support user-level config (e.g., `~/.mycli/config.json`, `~/.config/mycli/config.yaml`). Follow platform
conventions:

| Platform      | Location                          |
| ------------- | --------------------------------- |
| Linux/macOS   | `~/.config/mycli/` or `~/.mycli/` |
| Windows       | `%APPDATA%\mycli\`                |
| XDG-compliant | `$XDG_CONFIG_HOME/mycli/`         |

---

## Structured Logging in CLI Context

### Configuring Logging for CLI

CLI tools need different logging than web apps: logs go to stderr, and verbosity is controlled by flags.

```csharp

host.ConfigureLogging((ctx, logging) =>
{
    logging.ClearProviders();
    logging.AddConsole(options =>
    {
        // Write to stderr, not stdout
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });
});

```text

### Verbosity Mapping

Map `--verbose`/`--quiet` flags to log levels:

```csharp

public static class VerbosityMapping
{
    public static LogLevel ToLogLevel(bool verbose, bool quiet) => (verbose, quiet) switch
    {
        (true, _) => LogLevel.Debug,
        (_, true) => LogLevel.Warning,
        _ => LogLevel.Information  // default
    };
}

// In host configuration
host.ConfigureLogging((ctx, logging) =>
{
    var level = VerbosityMapping.ToLogLevel(verbose, quiet);
    logging.SetMinimumLevel(level);
});

```text

---

## Exit Code Conventions

### Standard Exit Codes

```csharp

public static class ExitCodes
{
    public const int Success = 0;
    public const int GeneralError = 1;
    public const int InvalidUsage = 2;    // Bad arguments or options
    public const int IoError = 3;         // File not found, permission denied
    public const int NetworkError = 4;    // Connection failed, timeout
    public const int AuthError = 5;       // Authentication/authorization failure

    // Tool-specific codes start at 10+
    public const int SyncFailed = 10;
    public const int ValidationFailed = 11;
}

```text

### Guidelines

- **0** = success (always)
- **1** = general/unspecified error
- **2** = invalid usage (bad arguments) -- System.CommandLine returns this for parse errors automatically
- **3-9** = reserved for common categories
- **10+** = tool-specific error codes
- Never use exit codes > 125 (reserved by shells; 126 = not executable, 127 = not found, 128+N = killed by signal N)

### Propagating Exit Codes

```csharp

public async Task<int> InvokeAsync(InvocationContext context)
{
    try
    {
        await _service.ProcessAsync(context.GetCancellationToken());
        return ExitCodes.Success;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Network error");
        context.Console.Error.Write($"Error: {ex.Message}\n");
        return ExitCodes.NetworkError;
    }
    catch (UnauthorizedAccessException ex)
    {
        context.Console.Error.Write($"Permission denied: {ex.Message}\n");
        return ExitCodes.IoError;
    }
}

```text

---

## Stdin/Stdout/Stderr Patterns


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
