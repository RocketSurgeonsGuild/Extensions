---
name: dotnet-serena-analysis-patterns
category: developer-experience
subcategory: serena
description: 'Code analysis patterns using Serena MCP for architecture validation and pattern detection'
targets: ['*']
tags: [dotnet, serena, analysis, architecture, patterns, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
license: 'MIT'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-serena-analysis-patterns'
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-serena-analysis-patterns

Analyze .NET codebases using Serena MCP's symbol-aware capabilities. Detect patterns, validate architecture, and
understand code structure at the semantic level.

## Trigger

Use when needing to:

- Validate architectural patterns
- Detect code smells and anti-patterns
- Analyze dependencies and coupling
- Understand project structure
- Review for maintainability issues

## Analysis Approach

### Traditional Analysis Limitations

```text
# Text-based analysis problems:
- Regex can't understand semantics
- False positives on similar names
- Misses relationships between symbols
- No type information
- Limited cross-file understanding
```

### Symbol-Aware Analysis Benefits

```text
# Serena advantages:
- Precise symbol identification
- Type hierarchy understanding
- Cross-reference tracking
- Semantic relationships
- Automated dependency mapping
```

## Architecture Pattern Validation

### Pattern 1: Layered Architecture Validation

**Goal**: Verify proper layer separation

```text
# Step 1: Find domain layer types
serena_find_symbol: "MyApp.Domain.*"

# Step 2: Check domain references infrastructure
serena_find_referencing_symbols: "MyApp.Infrastructure.*"
# Should NOT reference Domain types in wrong direction

# Step 3: Validate dependency direction
# Domain -> Application -> Infrastructure
# NOT: Infrastructure -> Domain
```

**Anti-pattern detection**:

```text
# Find infrastructure referencing domain
coupling = []
for infra_type in infrastructure_types:
    refs = serena_find_referencing_symbols(infra_type)
    for ref in refs:
        if "Domain" in ref:
            coupling.append((infra_type, ref))

if coupling:
    print("Violation: Infrastructure depends on Domain")
```

### Pattern 2: Repository Pattern Validation

**Goal**: Verify repository abstractions

```text
# Step 1: Find repositories
repositories = serena_find_symbol: "*Repository"

# Step 2: Check for interface
for repo in repositories:
    overview = serena_get_symbols_overview(repo)
    if "I" + repo.name not in overview:
        print(f"Warning: {repo.name} lacks interface")

# Step 3: Verify usage through abstraction
refs = serena_find_referencing_symbols: "OrderRepository"
for ref in refs:
    if ref.type == "OrderRepository" and not ref.is_interface:
        print(f"Warning: Direct repository usage at {ref.location}")
```

### Pattern 3: Dependency Injection Validation

**Goal**: Verify proper DI registration

```text
# Step 1: Find service registrations
registrations = serena_find_symbol: "Program/ConfigureServices"

# Step 2: Check for interface-implementation pairs
services = [
    ("IOrderService", "OrderService"),
    ("IRepository", "Repository"),
]

for interface, impl in services:
    impl_refs = serena_find_referencing_symbols(impl)
    interface_refs = serena_find_referencing_symbols(interface)

    # Should reference interface, not implementation
    if len(impl_refs) > len(interface_refs):
        print(f"Warning: {impl} used directly instead of {interface}")
```

## Code Smell Detection

### Smell 1: God Class Detection

```text
# Find classes with too many members
threshold = 20  # methods + properties

for file in project_files:
    overview = serena_get_symbols_overview(file)
    for type in overview.types:
        member_count = len(type.methods) + len(type.properties)
        if member_count > threshold:
            print(f"God Class: {type.name} has {member_count} members")
```

### Smell 2: Feature Envy

```text
# Method uses more features of other class than its own

for method in methods:
    own_type = method.parent_type
    other_refs = 0
    own_refs = 0

    for ref in method.references:
        if ref.symbol.parent == own_type:
            own_refs += 1
        else:
            other_refs += 1

    if other_refs > own_refs * 2:
        print(f"Feature Envy: {method.name} in {own_type}")
```

### Smell 3: Circular Dependencies

```text
# Build dependency graph
dependencies = {}

for type in all_types:
    refs = serena_find_referencing_symbols(type)
    dependencies[type] = refs

# Detect cycles
def find_cycles(graph, start, visited=None, path=None):
    if visited is None:
        visited = set()
    if path is None:
        path = []

    if start in path:
        cycle_start = path.index(start)
        return path[cycle_start:]

    if start in visited:
        return None

    visited.add(start)
    path.append(start)

    for neighbor in graph.get(start, []):
        cycle = find_cycles(graph, neighbor, visited, path)
        if cycle:
            return cycle

    path.pop()
    return None

# Check each type
for type in all_types:
    cycle = find_cycles(dependencies, type)
    if cycle:
        print(f"Circular dependency: {' -> '.join(cycle)}")
```

### Smell 4: Shotgun Surgery

```text
# Changes require modifications in many places

for type in all_types:
    change_impact = 0

    # Check how many places reference this type
    refs = serena_find_referencing_symbols(type)
    change_impact = len(refs)

    if change_impact > 10:
        print(f"Shotgun Surgery risk: {type} affects {change_impact} locations")
```

## Dependency Analysis

### Coupling Metrics

```text
# Calculate coupling between modules

modules = {
    "Domain": "MyApp.Domain.*",
    "Application": "MyApp.Application.*",
    "Infrastructure": "MyApp.Infrastructure.*"
}

coupling_matrix = {}

for source_name, source_pattern in modules.items():
    coupling_matrix[source_name] = {}

    for target_name, target_pattern in modules.items():
        if source_name == target_name:
            continue

        # Count references from source to target
        count = 0
        source_types = serena_find_symbol(source_pattern)
        for type in source_types:
            refs = serena_find_referencing_symbols(type)
            for ref in refs:
                if target_pattern in ref:
                    count += 1

        coupling_matrix[source_name][target_name] = count

# Print coupling matrix
for source, targets in coupling_matrix.items():
    print(f"{source} depends on:")
    for target, count in targets.items():
        print(f"  {target}: {count} references")
```

### Afferent/Efferent Coupling

```text
# Ca (Afferent Coupling) - who depends on me
ca = len(serena_find_referencing_symbols(type))

# Ce (Efferent Coupling) - who do I depend on
ce = 0
overview = serena_get_symbols_overview(type)
for ref in overview.references:
    ce += 1

# Instability = Ce / (Ca + Ce)
instability = ce / (ca + ce) if (ca + ce) > 0 else 0
print(f"Instability of {type}: {instability:.2f}")
```

## Architecture Metrics

### Lines of Code per Type

```text
# Estimate complexity by symbol count

for file in project_files:
    overview = serena_get_symbols_overview(file)

    for type in overview.types:
        # Count lines (approximate)
        loc = len(type.methods) * 10  # avg 10 lines per method

        if loc > 500:
            print(f"Large type: {type.name} (~{loc} LOC)")
```

### Inheritance Depth

```text
# Find deep inheritance hierarchies

def get_inheritance_depth(type, depth=0):
    if not type.base_type:
        return depth
    return get_inheritance_depth(type.base_type, depth + 1)

for type in all_types:
    depth = get_inheritance_depth(type)
    if depth > 3:
        print(f"Deep hierarchy: {type.name} (depth {depth})")
```

## Security Analysis

### Hardcoded Secrets Detection

```text
# Find string literals that look like secrets

for file in project_files:
    overview = serena_get_symbols_overview(file)

    for member in overview.members:
        if member.is_string_literal:
            value = member.value
            # Check for API keys, passwords, etc.
            patterns = [
                r"[a-zA-Z0-9]{32,}",  # API keys
                r"password\s*=\s*['\"]",  # Password assignments
                r"connection.*string",  # Connection strings
            ]
            for pattern in patterns:
                if re.search(pattern, value, re.IGNORECASE):
                    print(f"Potential secret: {file}:{member.line}")
```

### SQL Injection Vulnerabilities

```text
# Find string concatenation in SQL queries

sql_methods = serena_find_symbol("*Repository/ExecuteSql")

for method in sql_methods:
    overview = serena_get_symbols_overview(method)

    # Check for string interpolation or concatenation
    if "string.Format" in overview or "$" in overview:
        refs = serena_find_referencing_symbols(method)
        print(f"SQL injection risk: {method} ({len(refs)} call sites)")
```

## Performance Analysis

### Async/Await Patterns

```text
# Find sync-over-async anti-patterns

async_methods = serena_find_symbol("*Async")

for method in async_methods:
    overview = serena_get_symbols_overview(method)

    # Check for .Result or .Wait()
    if ".Result" in overview or ".Wait()" in overview:
        print(f"Sync-over-async: {method}")
```

### Allocation Hotspots

```text
# Find methods likely to allocate heavily

for method in all_methods:
    overview = serena_get_symbols_overview(method)

    # LINQ, string manipulation in loops
    linq_count = overview.count_pattern("\.Select\(|\.Where\(|\.ToList\(")
    string_count = overview.count_pattern("\+\"|\$\"")

    if linq_count > 3 or string_count > 5:
        print(f"Allocation hotspot: {method}")
```

## Integration with Other Skills

- [skill:dotnet-serena-code-navigation] - Navigate before analyzing
- [skill:dotnet-solid-principles] - Validate SOLID compliance
- [skill:dotnet-security-owasp] - Security-focused analysis
- [skill:dotnet-performance-patterns] - Performance analysis
- [skill:dotnet-csharp-code-smells] - Code smell catalog

## Analysis Report Template

```markdown
## Code Analysis Report

### Executive Summary

- Total Types: X
- Total Methods: Y
- Critical Issues: Z
- Warnings: W

### Architecture Health

- Layer violations: N
- Circular dependencies: N
- Coupling violations: N

### Code Quality

- God classes: N
- High complexity: N
- Feature envy: N

### Recommendations

1. [Priority] Address circular dependencies in...
2. [Medium] Refactor God class...
3. [Low] Extract interface for...

### Detailed Findings

[See full report]
```

## Best Practices

1. **Analyze regularly** - Weekly architecture reviews
2. **Fix incrementally** - Don't try to fix everything at once
3. **Track trends** - Monitor metrics over time
4. **Focus on critical** - Address high-impact issues first
5. **Document decisions** - Why certain patterns were chosen

## Quick Analysis Checklist

```text
□ Check layer dependencies
□ Find circular references
□ Identify God classes
□ Validate DI registrations
□ Review public API surface
□ Check for code smells
□ Analyze test coverage
□ Review security hotspots
```
