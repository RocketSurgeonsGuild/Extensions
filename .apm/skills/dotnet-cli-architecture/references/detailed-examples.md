
### Reading from Stdin

Support piped input as an alternative to file arguments:

```csharp

public async Task<int> InvokeAsync(InvocationContext context)
{
    string input;

    if (InputFile is not null)
    {
        input = await File.ReadAllTextAsync(InputFile.FullName);
    }
    else if (Console.IsInputRedirected)
    {
        // Read from stdin: echo '{"data":1}' | mycli process
        input = await Console.In.ReadToEndAsync();
    }
    else
    {
        context.Console.Error.Write("Error: Provide input via --file or stdin.\n");
        return ExitCodes.InvalidUsage;
    }

    var result = _processor.Process(input);
    context.Console.Out.Write(JsonSerializer.Serialize(result));
    return ExitCodes.Success;
}

```json

### Machine-Readable Output

```csharp

// Global --json option for machine-readable output
var jsonOption = new Option<bool>("--json", "Output as JSON");
rootCommand.AddGlobalOption(jsonOption);

// In handler
if (useJson)
{
    Console.Out.WriteLine(JsonSerializer.Serialize(result, jsonContext.Options));
}
else
{
    // Human-friendly table format
    ConsoleFormatter.WriteTable(result, context.Console);
}

```text

### Progress to Stderr

```csharp

// Progress reporting goes to stderr (does not pollute piped stdout)
await foreach (var item in _service.StreamAsync(ct))
{
    Console.Error.Write($"\rProcessing {item.Index}/{total}...");
    Console.Out.WriteLine(item.ToJson());
}
Console.Error.WriteLine();  // Clear progress line

```json

---

## Testing CLI Applications

### In-Process Invocation with CommandLineBuilder

Test the full CLI pipeline without spawning a child process:

```csharp

public class CliTestHarness
{
    private readonly RootCommand _rootCommand;
    private readonly Action<IServiceCollection>? _configureServices;

    public CliTestHarness(Action<IServiceCollection>? configureServices = null)
    {
        _rootCommand = Program.BuildRootCommand();
        _configureServices = configureServices;
    }

    public async Task<(int ExitCode, string Stdout, string Stderr)> InvokeAsync(
        string commandLine)
    {
        var console = new TestConsole();

        var builder = new CommandLineBuilder(_rootCommand)
            .UseHost(_ => Host.CreateDefaultBuilder(), host =>
            {
                if (_configureServices is not null)
                {
                    host.ConfigureServices(_configureServices);
                }
            })
            .UseDefaults()
            .Build();

        var exitCode = await builder.InvokeAsync(commandLine, console);

        return (exitCode, console.Out.ToString()!, console.Error.ToString()!);
    }
}

```text

### Testing with Service Mocks

```csharp

[Fact]
public async Task Sync_WithValidSource_ReturnsZero()
{
    var fakeSyncService = new FakeSyncService(
        new SyncResult(ItemCount: 5));

    var harness = new CliTestHarness(services =>
    {
        services.AddSingleton<ISyncService>(fakeSyncService);
    });

    var (exitCode, stdout, stderr) = await harness.InvokeAsync(
        "sync --source https://api.example.com");

    Assert.Equal(0, exitCode);
    Assert.Contains("Synced 5 items", stdout);
}

[Fact]
public async Task Sync_WithMissingSource_ReturnsNonZero()
{
    var harness = new CliTestHarness();

    var (exitCode, _, stderr) = await harness.InvokeAsync("sync");

    Assert.NotEqual(0, exitCode);
    Assert.Contains("--source", stderr);  // Parse error mentions missing option
}

```text

### Exit Code Assertion

```csharp

[Theory]
[InlineData("sync --source https://valid.example.com", 0)]
[InlineData("sync", 2)]  // Missing required option
[InlineData("invalid-command", 1)]
public async Task ExitCode_MatchesExpected(string args, int expectedExitCode)
{
    var harness = new CliTestHarness();
    var (exitCode, _, _) = await harness.InvokeAsync(args);
    Assert.Equal(expectedExitCode, exitCode);
}

```text

### Testing Output Format

```csharp

[Fact]
public async Task List_WithJsonFlag_OutputsValidJson()
{
    var harness = new CliTestHarness(services =>
    {
        services.AddSingleton<IItemRepository>(
            new FakeItemRepository([new Item(1, "Widget")]));
    });

    var (exitCode, stdout, _) = await harness.InvokeAsync("list --json");

    Assert.Equal(0, exitCode);
    var items = JsonSerializer.Deserialize<Item[]>(stdout);
    Assert.NotNull(items);
    Assert.Single(items);
}

[Fact]
public async Task List_StderrContainsLogs_StdoutContainsDataOnly()
{
    var harness = new CliTestHarness();
    var (_, stdout, stderr) = await harness.InvokeAsync("list --json --verbose");

    // Stdout must be valid JSON (no log noise)
    // xUnit: just call it -- if it throws, the test fails
    var doc = JsonDocument.Parse(stdout);
    Assert.NotNull(doc);

    // Stderr contains diagnostic output
    Assert.Contains("Connected to", stderr);
}

```text

---

## Agent Gotchas

1. **Do not write diagnostic output to stdout.** Logs, progress, and errors go to stderr. Stdout is reserved for data
   output that can be piped. A CLI tool that mixes logs into stdout breaks shell pipelines.
2. **Do not hardcode exit code 1 for all errors.** Use distinct exit codes for different failure categories (I/O,
   network, auth, validation). Callers and scripts rely on exit codes to determine what went wrong.
3. **Do not put business logic in command handlers.** Handlers should orchestrate calls to injected services and format
   output. Business logic in handlers cannot be reused or unit-tested independently.
4. **Do not test CLI tools only via process spawning.** Use in-process invocation with `CommandLineBuilder` and
   `TestConsole` for fast, reliable tests. Reserve process-level tests for smoke testing the published binary.
5. **Do not ignore `Console.IsInputRedirected` when accepting stdin.** Without checking, the tool may hang waiting for
   input when invoked without piped data.
6. **Do not use exit codes above 125.** Codes 126-255 have special meanings in Unix shells (126 = not executable, 127 =
   not found, 128+N = killed by signal N). Tool-specific codes should be in the 1-125 range.

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

- [Command Line Interface Guidelines (clig.dev)](https://clig.dev/)
- [System.CommandLine overview](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [12 Factor CLI Apps](https://medium.com/@jdxcode/12-factor-cli-apps-dd3c227a0e46)
- [Generic Host in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [Console logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter)
````
