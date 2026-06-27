        => EmailRegex().IsMatch(email);
}

```text

**Key rules:**

- Method must be `static partial` returning `Regex`
- Place on `partial class` (or `partial struct`)
- Replaces `new Regex(...)` with zero allocation at runtime
- Supports all `RegexOptions` except `RegexOptions.Compiled` (which is ignored -- the source generator replaces it)

### `[LoggerMessage]` (net6.0+)

High-performance structured logging with zero-allocation at log-disabled levels.

```csharp

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information,
        Message = "Processing order {OrderId} for customer {CustomerId}")]
    public static partial void OrderProcessing(
        this ILogger logger, int orderId, string customerId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to process order {OrderId}")]
    public static partial void OrderProcessingFailed(
        this ILogger logger, int orderId, Exception exception);
}

// Usage
logger.OrderProcessing(order.Id, order.CustomerId);

```text

**Key rules:**

- Methods must be `static partial` in a `partial class`
- Parameters matching `{Placeholder}` in the message are logged as structured data
- `Exception` parameter is logged automatically (do not include in message template)
- Event IDs are auto-assigned if not specified; specify explicit IDs for stable telemetry

### System.Text.Json Source Generation (net6.0+)

AOT-compatible JSON serialization. Eliminates runtime reflection.

```csharp

[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(List<Order>))]
[JsonSerializable(typeof(Customer))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class AppJsonContext : JsonSerializerContext;

```json

#### Registration in ASP.NET Core

```csharp

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

// Or for Minimal APIs
app.MapGet("/orders/{id}", async (int id, IOrderService service) =>
{
    var order = await service.GetByIdAsync(id);
    return order is not null
        ? Results.Ok(order)
        : Results.NotFound();
});

```text

#### Manual Serialization

```csharp

// Serialize
var json = JsonSerializer.Serialize(order, AppJsonContext.Default.Order);

// Deserialize
var order = JsonSerializer.Deserialize(json, AppJsonContext.Default.Order);

// With stream
await JsonSerializer.SerializeAsync(stream, orders,
    AppJsonContext.Default.ListOrder);

```json

**Key rules:**

- Register all types that need serialization in `[JsonSerializable]` attributes
- Use `TypeInfoResolverChain` (net8.0+) to combine multiple contexts
- Required for Native AOT -- reflection-based serialization is trimmed
- See [skill:dotnet-csharp-modern-patterns] for related C# features used in generated code

### `[JsonSerializable]` with Polymorphism (net7.0+)

```csharp

[JsonDerivedType(typeof(CreditCardPayment), "credit")]
[JsonDerivedType(typeof(BankTransferPayment), "bank")]
public abstract class Payment
{
    public decimal Amount { get; init; }
}

public class CreditCardPayment : Payment
{
    public required string CardLast4 { get; init; }
}

public class BankTransferPayment : Payment
{
    public required string AccountNumber { get; init; }
}

[JsonSerializable(typeof(Payment))]
public partial class PaymentJsonContext : JsonSerializerContext;

```json

---

## Generator Reference: Packaging and Consumption

### Referencing a Generator in a Consuming Project

```xml

<ItemGroup>
  <ProjectReference Include="..\MyGenerator\MyGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>

```csharp

### NuGet Package Layout

When shipping a generator as a NuGet package, place the assembly under `analyzers/dotnet/cs/`:

```text

MyGenerator.nupkg
  analyzers/
    dotnet/
      cs/
        MyGenerator.dll
  lib/
    netstandard2.0/
      _._   (empty placeholder if no runtime dependency)

```text

```xml

<!-- In the generator .csproj -->
<PropertyGroup>
  <IncludeBuildOutput>false</IncludeBuildOutput>
  <DevelopmentDependency>true</DevelopmentDependency>
</PropertyGroup>

<ItemGroup>
  <None Include="$(OutputPath)\$(AssemblyName).dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs" />
</ItemGroup>

```text

---

## Debugging Source Generators

```csharp

// Add to Initialize() for attach-debugger workflow
#if DEBUG
if (!System.Diagnostics.Debugger.IsAttached)
{
    System.Diagnostics.Debugger.Launch();
}
#endif

```text

Alternatively, emit generated files to disk for inspection:

```xml

<!-- In the consuming project -->
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>

```text

Add `Generated/` to `.gitignore`.

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

- [Source Generator Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)
- [Incremental Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [GeneratedRegex source generator](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators)
- [Compile-time logging source generation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator)
- [System.Text.Json source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
- [.NET Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
````
