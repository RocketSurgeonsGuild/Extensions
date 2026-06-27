app.Run();

[JsonSerializable(typeof(Product))]
internal partial class AppJsonContext : JsonSerializerContext { }

record Product(int Id, string Name);

```json

### CreateSlimBuilder vs CreateBuilder

| Method | AOT Support | Includes |
|--------|-------------|----------|
| `WebApplication.CreateSlimBuilder()` | Full | Minimal services, no MVC, no Razor |
| `WebApplication.CreateBuilder()` | Partial | Full feature set, some features need reflection |

Use `CreateSlimBuilder` for Native AOT applications. It excludes features that require runtime code generation.

### .NET 10 ASP.NET Core AOT Improvements

.NET 10 brings improvements across the ASP.NET Core and runtime Native AOT stack. Target `net10.0` to benefit automatically.

```xml

<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <PublishAot>true</PublishAot>
</PropertyGroup>

```text

**Request Delegate Generator improvements:** The source generator that creates request delegates for Minimal API endpoints handles more parameter binding scenarios in .NET 10, including additional `TypedResults` return types and complex binding patterns. This reduces the need for manual workarounds that were required in .NET 8/9 when the generator could not produce AOT-safe code for certain endpoint signatures.

**Reduced linker warning surface:** Many ASP.NET Core framework APIs that previously emitted trim/AOT warnings (IL2xxx/IL3xxx) have been annotated or refactored for AOT compatibility. Projects upgrading from .NET 9 to .NET 10 will see fewer false-positive linker warnings when publishing with `PublishAot`.

**OpenAPI in the `webapiaot` template:** The `webapiaot` project template now includes OpenAPI document generation via `Microsoft.AspNetCore.OpenApi` by default, so AOT-published APIs get auto-generated API documentation without additional setup.

**Runtime NativeAOT code generation:** The .NET 10 runtime improves AOT code generation for struct arguments, enhances loop inversion optimizations, and improves method devirtualization -- resulting in better throughput for AOT-published applications without code changes.

**Blazor Server and SignalR:** Blazor Server and SignalR remain **not supported** with Native AOT in .NET 10. Blazor WebAssembly AOT (client-side compilation) is a separate concern covered by [skill:dotnet-aot-wasm]. For Blazor Server apps, continue using JIT deployment.

**Compatibility snapshot (.NET 10):**

| Feature | AOT Support |
|---------|-------------|
| gRPC | Fully supported |
| Minimal APIs | Partially supported (most scenarios work) |
| MVC | Not supported |
| Blazor Server | Not supported |
| SignalR | Not supported |
| JWT Authentication | Fully supported |
| CORS, HealthChecks, OutputCaching | Fully supported |
| WebSockets, StaticFiles | Fully supported |

---

## Agent Gotchas

1. **Do not use `PublishAot` in library projects.** Libraries use `IsAotCompatible` (which auto-enables the AOT analyzer). `PublishAot` is for applications that produce standalone executables.
2. **Do not use legacy RD.xml for type preservation.** RD.xml is a .NET Native/UWP format that is silently ignored by modern .NET AOT. Use ILLink descriptor XML files and `[DynamicDependency]` attributes instead.
3. **Do not use `[DllImport]` in new AOT code.** Use `[LibraryImport]` (.NET 7+) which generates marshalling at compile time. `[DllImport]` may require runtime marshalling that is not available in AOT.
4. **Do not use `WebApplication.CreateBuilder()` for AOT APIs.** Use `CreateSlimBuilder()` which excludes reflection-heavy features. `CreateBuilder()` includes MVC infrastructure that is not AOT-compatible.
5. **Do not use `dotnet publish --no-actual-publish` for analysis.** That flag does not exist. Use `dotnet build /p:EnableAotAnalyzer=true /p:EnableTrimAnalyzer=true` to get diagnostic warnings without publishing.
6. **Do not assume MVC controllers work with Native AOT.** MVC relies on reflection for model binding, action filters, and routing. Use Minimal APIs for AOT-published web applications.

---

## References

- [Native AOT deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [ASP.NET Core Native AOT](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot)
- [ILLink descriptor format](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trimming-options#descriptor-format)
- [LibraryImport source generation](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation)
- [Optimize AOT deployments](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/optimizing)
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
