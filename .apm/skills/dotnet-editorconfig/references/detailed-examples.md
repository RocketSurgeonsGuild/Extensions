root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.cs]
# Namespace and using preferences
csharp_style_namespace_declarations = file_scoped:warning
csharp_using_directive_placement = outside_namespace:warning
dotnet_sort_system_directives_first = true

# Code style enforcement in build
dotnet_diagnostic.IDE0005.severity = warning
dotnet_diagnostic.IDE0161.severity = warning
dotnet_diagnostic.IDE0090.severity = suggestion

# CA rule adjustments
dotnet_diagnostic.CA1848.severity = warning
dotnet_diagnostic.CA2016.severity = warning

```text

### Test Project Overrides (tests/.editorconfig)

Place a separate `.editorconfig` in the `tests/` directory to relax rules that conflict with test readability. For
common per-project-type suppression patterns (ASP.NET Core apps, libraries, test projects), see
[skill:dotnet-add-analyzers].

```ini

[*.cs]
# Relax rules for test readability
dotnet_diagnostic.CA1707.severity = none          # Allow underscores in test names
dotnet_diagnostic.CA1822.severity = none          # Test methods often not static
dotnet_diagnostic.IDE0058.severity = none         # Expression value is never used

```text

---

## Generated Code Configuration

Source generators and scaffolding tools produce code that often triggers IDE/CA warnings. Use the
`generated_code = true` setting or file glob patterns to suppress analysis on generated files:

```ini

# Suppress warnings in generated code files
[*.g.cs]
generated_code = true

[*.generated.cs]
generated_code = true

```csharp

When `generated_code = true` is set, Roslyn treats the file as generated code and applies the
`GeneratedCodeAnalysisFlags` configured in each analyzer (most analyzers skip generated code by default). This is
particularly relevant when using source generators -- see [skill:dotnet-csharp-source-generators].

---

## References

- [EditorConfig for .NET code style rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)
- [Code quality rules (CA\*)](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/)
- [Code style rules (IDE\*)](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/)
- [Configuration options for code analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options)
- [Global AnalyzerConfig files](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files#global-analyzerconfig)
- [Naming rule configuration](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules)
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
