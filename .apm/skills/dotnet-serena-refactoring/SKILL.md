---
name: dotnet-serena-refactoring
category: developer-experience
subcategory: serena
description: 'Symbol-level refactoring with automatic reference updates using Serena MCP'
targets: ['*']
tags: [dotnet, serena, refactoring, productivity, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
license: 'MIT'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-serena-refactoring'
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-serena-refactoring

Refactor .NET code safely and efficiently using Serena MCP's symbol-aware operations. Automatic reference updates,
precise targeting, and safe transformations.

## Trigger

Use when needing to:

- Rename methods, classes, or properties
- Extract methods or classes
- Move types between files
- Change method signatures
- Update inheritance hierarchies

## Why Symbol-Level Refactoring?

### Traditional Approach Problems

```text
# Manual refactoring issues:
1. Find all references manually (error-prone)
2. Edit each file individually (tedious)
3. Update imports/namespaces (easy to miss)
4. Verify no breaking changes (time-consuming)
5. Test everything (after the fact)
```

### Serena Advantages

```text
# Symbol-aware benefits:
1. Automatic reference discovery (complete)
2. Single precise edit (efficient)
3. Namespace handling (automatic)
4. Compile-time safety (pre-validated)
5. Integrated verification (built-in)
```

## Core Refactoring Patterns

### Pattern 1: Rename Symbol

**Scenario**: Rename a method

```text
# Step 1: Find all references
serena_find_referencing_symbols: "OrderService/ProcessOrder"

# Step 2: Replace method body (includes signature)
serena_replace_symbol_body: "OrderService/ProcessOrder"
New body:
  public async Task ProcessOrderAsync(Order order)
  {
    // ... implementation
  }

# Step 3: References are automatically tracked
# Step 4: Verify all call sites updated
```

**Key Point**: References are found symbolically, not textually. No regex needed.

### Pattern 2: Extract Method

**Scenario**: Extract logic into new method

```text
# Step 1: Identify code to extract
Read: "src/Services/OrderService.cs" (lines 45-60)

# Step 2: Find insertion point
serena_get_symbols_overview: "src/Services/OrderService.cs"
# Locate method OrderService/ProcessOrder

# Step 3: Insert new method after existing
serena_insert_after_symbol: "OrderService/ProcessOrder"
New method:
  private decimal CalculateDiscount(Order order)
  {
    // extracted logic
  }

# Step 4: Replace original with call
serena_replace_content: "OrderService/ProcessOrder"
Pattern: "// lines 45-60"
Replacement: "CalculateDiscount(order)"
```

### Pattern 3: Move Type to New File

**Scenario**: Move class to separate file

```text
# Step 1: Get symbol info
serena_find_symbol: "OrderService"

# Step 2: Create new file
Write: src/Services/OrderService.cs
Content: (extracted class)

# Step 3: Remove from original
serena_replace_content: "src/Services/OrderProcessing.cs"
Pattern: "class OrderService.*?^}"
Replacement: ""

# Step 4: Verify namespace imports
serena_get_symbols_overview: "src/Services/OrderService.cs"
```

### Pattern 4: Change Method Signature

**Scenario**: Add parameter to method

```text
# Step 1: Find references
serena_find_referencing_symbols: "OrderService/CreateOrder"

# Step 2: Update signature
serena_replace_symbol_body: "OrderService/CreateOrder"
New signature:
  public async Task<Order> CreateOrderAsync(
    OrderRequest request,
    CancellationToken cancellationToken = default)

# Step 3: Update all callers
# Each reference location needs parameter added
serena_replace_content: "Caller1/CreateOrder"
Pattern: "CreateOrder(request)"
Replacement: "CreateOrder(request, CancellationToken.None)"
```

## Common Refactoring Scenarios

### Scenario 1: Interface Extraction

```text
# Goal: Extract interface from concrete class

# Step 1: Analyze class
serena_get_symbols_overview: "src/Services/OrderService.cs"

# Step 2: Create interface
Write: src/Interfaces/IOrderService.cs
Content: Interface with public methods

# Step 3: Update class to implement interface
serena_replace_content: "OrderService"
Pattern: "class OrderService"
Replacement: "class OrderService : IOrderService"

# Step 4: Find all usages
serena_find_referencing_symbols: "OrderService"

# Step 5: Update to use interface where appropriate
# Manual review: which references should use interface?
```

### Scenario 2: Base Class Creation

```text
# Goal: Extract common code into base class

# Step 1: Find similar classes
serena_find_symbol: "UserService"
serena_find_symbol: "OrderService"

# Step 2: Compare structures
serena_get_symbols_overview: "UserService"
serena_get_symbols_overview: "OrderService"

# Step 3: Create base class
Write: src/Services/BaseService.cs

# Step 4: Update derived classes
serena_replace_content: "UserService"
Pattern: "class UserService"
Replacement: "class UserService : BaseService"
```

### Scenario 3: Namespace Reorganization

```text
# Goal: Move types to new namespace

# Step 1: Find all types in namespace
serena_find_symbol: "MyApp.Services.*"

# Step 2: Update namespace declarations
serena_replace_content: "OrderService"
Pattern: "namespace MyApp.Services"
Replacement: "namespace MyApp.Application.Services"

# Step 3: Update using statements
# Find all files referencing old namespace
serena_find_referencing_symbols: "MyApp.Services"
```

## Safety Guidelines

### Always Do These Steps

1. **Find references first**

   ```text
   serena_find_referencing_symbols: "TargetSymbol"
   # Review before modifying
   ```

2. **Make backup or use git**

   ```text
   Bash: git stash
   # Or work on branch
   ```

3. **Verify compilable after changes**

   ```text
   Bash: dotnet build
   ```

4. **Run tests**

   ```text
   Bash: dotnet test
   ```

### Avoid These Mistakes

1. **Don't rename without checking references**
   - May break external consumers
   - Could miss string-based references

2. **Don't change public API without versioning consideration**
   - Breaking changes require major version bump
   - Consider backward compatibility

3. **Don't ignore compile errors**
   - Serena helps but doesn't guarantee correctness
   - Always verify with compiler

## Performance Considerations

### Large Codebases

```text
# For very large refactors:

1. **Batch operations**
   - Group related changes
   - Apply in single commit

2. **Incremental approach**
   - Refactor one subsystem at a time
   - Verify between steps

3. **Use branches**
   - Feature branches for major refactors
   - Easy rollback if issues
```

### Complex Dependencies

```text
# When dependencies are complex:

1. **Map dependency graph first**
   serena_find_referencing_symbols: "TargetType"
   # Multiple levels deep

2. **Identify breaking changes**
   - Public API surface
   - Cross-project dependencies
   - External consumers

3. **Plan migration strategy**
   - Deprecation periods
   - Adapter patterns
   - Feature flags
```

## Integration with Other Skills

- [skill:dotnet-serena-code-navigation] - Navigate before refactoring
- [skill:dotnet-solid-principles] - Apply during refactoring
- [skill:dotnet-csharp-coding-standards] - Maintain conventions
- [skill:dotnet-testing-strategy] - Verify after refactoring
- [skill:dotnet-version-detection] - Handle framework differences

## Troubleshooting

### Issue: References Not Found

**Cause**: Dynamic/runtime references (reflection, DI containers)

**Solution**:

```text
# Search for string references
Grep: "OrderService"
# Review matches carefully
# May need manual updates
```

### Issue: Compilation Errors After Refactor

**Cause**: Symbol operations succeeded but broke dependencies

**Solution**:

```text
# Step 1: Review error messages
Bash: dotnet build 2>&1 | head -20

# Step 2: Fix each error
# May need traditional Edit for complex cases

# Step 3: Re-verify
Bash: dotnet build
```

### Issue: Partial Refactoring Applied

**Cause**: Network error or process interruption

**Solution**:

```text
# Check git status
Bash: git status

# Either:
# a) Complete manually with remaining references
# b) Revert and retry
Bash: git checkout -- .
```

## Best Practices Summary

1. **Plan before acting** - Understand the full scope
2. **Start small** - Practice on private methods first
3. **Test immediately** - Don't wait to verify
4. **Commit often** - Small, focused commits
5. **Document changes** - Update XML docs, comments
6. **Review impact** - Consider downstream consumers

## Quick Reference Card

```text
Rename:        serena_replace_symbol_body
Extract:       serena_insert_after_symbol + serena_replace_content
Move:          Write + serena_replace_content
Signature:     serena_replace_symbol_body + update callers
Find refs:     serena_find_referencing_symbols
Navigate:      serena_find_symbol + serena_get_symbols_overview
```

## Example: Complete Refactoring Session

```text
User: "Extract validation logic from OrderService into Validator class"

Agent:
1. serena_get_symbols_overview: "src/Services/OrderService.cs"
2. Read: identify validation methods (lines 30-45)
3. Write: src/Validators/OrderValidator.cs
4. serena_insert_after_symbol: "OrderService/ProcessOrder"
   Insert: call to new validator
5. serena_replace_content: remove old validation
6. Bash: dotnet build
7. Bash: dotnet test

Result: Clean extraction with working code
```
