---
name: dotnet-serena-code-navigation
category: developer-experience
subcategory: serena
description: 'Efficient code navigation using Serena MCP symbol operations instead of text search'
targets: ['*']
tags: [dotnet, serena, navigation, productivity, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
license: 'MIT'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-serena-code-navigation'
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-serena-code-navigation

Navigate .NET codebases efficiently using Serena MCP symbol operations. Move beyond text search to precise symbol-level
navigation.

## Trigger

Use when needing to:

- Find class/method definitions across files
- Understand project structure and dependencies
- Navigate complex inheritance hierarchies
- Jump between related symbols quickly

## The Problem with Text Search

Traditional approaches have limitations:

```text
# Text search problems:
- "class UserService" finds comments, strings, unrelated mentions
- Line numbers change, breaking bookmarks
- Can't distinguish method overloads
- Misses inherited members
- No semantic understanding
```

## Serena-First Navigation

### 1. Finding Definitions

**Serena approach:**

```text
serena_find_symbol: "OrderService/ProcessOrder"
```

**Text search fallback:**

```text
Grep: "public.*void.*ProcessOrder"
Read: src/Services/OrderService.cs
```

### 2. Understanding File Structure

**Serena approach:**

```text
serena_get_symbols_overview: "src/Services/OrderService.cs"
```

Returns:

- All types in file
- Methods and properties
- Inheritance relationships
- Namespace organization

**Text search fallback:**

```text
Read: src/Services/OrderService.cs
Grep: "^    (public|private|protected)"
```

### 3. Tracking References

**Serena approach:**

```text
serena_find_referencing_symbols: "OrderService"
```

Shows:

- All usages across codebase
- Dependency direction
- Impact analysis

**Text search fallback:**

```text
Grep: "OrderService"
# Manual filtering of false positives
```

## Common Navigation Patterns

### Pattern 1: Jump to Implementation

```text
# Scenario: Find where interface method is implemented

serena_find_symbol: "IOrderRepository/SaveAsync"
# Get implementing types
serena_find_referencing_symbols: "IOrderRepository"
# Navigate to concrete implementation
```

### Pattern 2: Understand Call Hierarchy

```text
# Scenario: Who calls this method?

serena_find_referencing_symbols: "OrderService/ProcessOrder"
# Returns all callers with context
```

### Pattern 3: Navigate Inheritance

```text
# Scenario: Find all implementations of an abstract method

serena_find_symbol: "BaseService/Execute"
serena_find_referencing_symbols: "BaseService"
# Review each derived type
```

## When to Use Serena vs Text Search

| Scenario                  | Serena          | Text Search |
| ------------------------- | --------------- | ----------- |
| Find definition by name   | ✅ Primary      | Fallback    |
| Understand file structure | ✅ Primary      | Fallback    |
| Track references          | ✅ Primary      | Fallback    |
| Search by pattern/content | ❌ Not suitable | ✅ Primary  |
| Read full file context    | ❌ Not suitable | ✅ Primary  |
| Regex-based search        | ❌ Not suitable | ✅ Primary  |

## Practical Examples

### Example 1: Refactoring a Method

```text
# Step 1: Find all references
serena_find_referencing_symbols: "OrderService/CalculateTotal"

# Step 2: Understand signature
serena_get_symbols_overview: "src/Services/OrderService.cs"

# Step 3: Replace with new signature
serena_replace_symbol_body: "OrderService/CalculateTotal"
```

### Example 2: Understanding Dependencies

```text
# Step 1: Get entry point structure
serena_get_symbols_overview: "src/Program.cs"

# Step 2: Follow service registration
serena_find_symbol: "Program/ConfigureServices"
serena_find_referencing_symbols: "IOrderService"

# Step 3: Analyze dependency chain
serena_get_symbols_overview: "src/Services/OrderService.cs"
```

## Best Practices

### 1. Always Check Symbol Exists

```text
# Good
result = serena_find_symbol: "UserService"
if not found:
    # Try text search or different name
    Grep: "UserService"
```

### 2. Combine with Traditional Tools

```text
# Use Serena for navigation
serena_find_symbol: "OrderService/ProcessOrder"

# Use Read for understanding implementation
Read: src/Services/OrderService.cs (lines 45-80)
```

### 3. Cache Symbol Information

```text
# In agent memory or notes:
"OrderService defined in src/Services/OrderService.cs:15"
"ProcessOrder has 5 references"
```

## Troubleshooting

### Issue: Symbol Not Found

**Cause:** LSP server not running or file not indexed

**Solution:**

```text
# Fallback to text search
Grep: "class OrderService"
Read: found_file.cs
```

### Issue: Outdated Symbol Info

**Cause:** File changed since last index

**Solution:**

```text
# Re-trigger indexing by opening file
Read: src/Services/OrderService.cs

# Or use text search for recent changes
Grep: "public void NewMethod"
```

### Issue: Partial Matches

**Cause:** Symbol name ambiguous

**Solution:**

```text
# Use fully qualified name
serena_find_symbol: "MyApp.Services.OrderService"

# Or search with namespace context
serena_get_symbols_overview: "src/Services/OrderService.cs"
```

## Performance Tips

1. **Use symbol overview first** - Faster than reading entire file
2. **Cache symbol locations** - Avoid repeated searches
3. **Follow references incrementally** - Don't load entire graph at once
4. **Combine operations** - Get overview, then dive into specific symbols

## Integration with Other Skills

- [skill:dotnet-solution-navigation] - Project structure basics
- [skill:dotnet-csharp-coding-standards] - C# conventions
- [skill:dotnet-serena-refactoring] - Symbol-level refactoring
- [skill:dotnet-serena-analysis-patterns] - Code analysis with symbols

## Example Agent Session

```text
User: "I need to understand how Order processing works"

Agent:
1. serena_find_symbol: "OrderProcessingService"
2. serena_get_symbols_overview: "src/Services/OrderProcessingService.cs"
3. serena_find_referencing_symbols: "OrderProcessingService"
4. serena_find_symbol: "OrderProcessingService/ProcessOrder"
5. Read: specific implementation details

Result: Complete understanding with minimal file reads
```

## Migration from Text Search

### Before (Text-Heavy)

```text
Read: src/Program.cs
Grep: "OrderService"
Read: src/Services/OrderService.cs
Grep: "ProcessOrder"
Read: found references
```

### After (Serena-First)

```text
serena_find_symbol: "OrderService"
serena_get_symbols_overview: "src/Services/OrderService.cs"
serena_find_referencing_symbols: "OrderService/ProcessOrder"
Read: specific implementations only
```

**Result**: 60% fewer file reads, precise targeting, better accuracy.
