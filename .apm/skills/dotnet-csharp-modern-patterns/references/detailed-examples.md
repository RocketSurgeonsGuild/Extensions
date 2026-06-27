    private string _name = "";
    public partial string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}

```text

> **net9.0+ only.** See [skill:dotnet-csharp-source-generators] for generator patterns.

---

## `nameof` for Unbound Generic Types (C# 14, net10.0+)

```csharp

string name = nameof(List<>);      // "List"
string name2 = nameof(Dictionary<,>); // "Dictionary"

```csharp

Useful in logging, diagnostics, and reflection scenarios.

> **net10.0+ only.**

---

## Polyfill Guidance for Multi-Targeting

When targeting multiple TFMs, newer language features may not compile on older targets. Use these approaches:

1. **PolySharp** -- Polyfills compiler-required types (`IsExternalInit`, `RequiredMemberAttribute`, etc.) so language
   features like `init`, `required`, and `record` work on older TFMs.
2. **Polyfill** -- Polyfills runtime APIs (e.g., `string.Contains(char)` for netstandard2.0).
3. **Conditional compilation** -- Use `#if` for features that cannot be polyfilled:

```csharp

#if NET10_0_OR_GREATER
    // Use field keyword
    public double Value { get => field; set => field = Math.Max(0, value); }
#else
    private double _value;
    public double Value { get => _value; set => _value = Math.Max(0, value); }
#endif

```text

See [skill:dotnet-multi-targeting] for comprehensive polyfill guidance.

---

## Knowledge Sources

Feature guidance in this skill is grounded in publicly available language design rationale from:

- **C# Language Design Notes (Mads Torgersen et al.)** -- Design decisions behind each C# version's features. Key
  rationale relevant to this skill: primary constructors (reducing boilerplate for DI-heavy services), collection
  expressions (unifying collection initialization syntax), `field` keyword (eliminating backing field ceremony), and
  extension blocks (grouping extensions by target type). Each feature balances expressiveness with safety -- e.g.,
  primary constructor parameters are intentionally mutable captures (not readonly) to keep the feature simple; use
  explicit readonly fields when immutability is needed. Source: https://github.com/dotnet/csharplang/tree/main/meetings
- **C# Language Proposals Repository** -- Detailed specifications and design rationale for accepted and proposed
  features. Source: https://github.com/dotnet/csharplang/tree/main/proposals

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

- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/)
- [What's new in C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)
- [What's new in C# 13](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [C# Language Design Notes](https://github.com/dotnet/csharplang/tree/main/meetings)
- [.NET Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
````
