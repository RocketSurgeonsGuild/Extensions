{
    Task<Order> CreateAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}

// Avoid: one large interface with unrelated responsibilities
public interface IOrderRepository : IOrderReader, IOrderWriter { }

```text

---

## CancellationToken Conventions

Accept `CancellationToken` as the last parameter in async methods. Use `default` as the default value for optional
tokens:

```csharp

public async Task<Order> GetOrderAsync(int id, CancellationToken ct = default)
{
    return await _repo.GetByIdAsync(id, ct);
}

```text

Always forward the token to downstream async calls. Never ignore a received `CancellationToken`.

---

## XML Documentation

Add XML docs to public API surfaces. Keep them concise:

```csharp

/// <summary>
/// Retrieves an order by its unique identifier.
/// </summary>
/// <param name="id">The order identifier.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The order, or <see langword="null"/> if not found.</returns>
public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);

```text

Do not add XML docs to:

- Private or internal members (unless it's a library's `InternalsVisibleTo` API)
- Self-evident members (e.g., `public string Name { get; }`)
- Test methods

---

## Analyzer Enforcement

Configure these analyzers in `Directory.Build.props` or `.editorconfig` to enforce standards automatically:

```xml

<PropertyGroup>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <AnalysisLevel>latest-all</AnalysisLevel>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>

```text

Key `.editorconfig` rules for C# style:

```ini

[*.cs]
csharp_style_namespace_declarations = file_scoped:warning
csharp_prefer_braces = true:warning
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
dotnet_style_require_accessibility_modifiers = always:warning
csharp_style_prefer_pattern_matching = true:suggestion

```csharp

See [skill:dotnet-add-analyzers] for full analyzer configuration.

---

## Knowledge Sources

Conventions in this skill are grounded in publicly available content from:

- **Microsoft Framework Design Guidelines** -- The canonical reference for .NET naming, type design, and API surface
  conventions. Source: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
- **C# Language Design Notes (Mads Torgersen et al.)** -- Design rationale behind C# language features that affect
  coding standards. Key decisions relevant to this skill: file-scoped namespaces (reducing nesting for readability),
  pattern matching over type checks (expressiveness), `required` members (compile-time initialization safety), and `var`
  usage guidelines (readability-first). The language design team explicitly chose these features to reduce ceremony
  while maintaining safety. Source: https://github.com/dotnet/csharplang/tree/main/meetings

> **Note:** This skill applies publicly documented design rationale. It does not represent or speak for the named
> sources.



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

- [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [C# Identifier Naming Rules](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [.editorconfig for .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)
- [C# Language Design Notes](https://github.com/dotnet/csharplang/tree/main/meetings)
````
