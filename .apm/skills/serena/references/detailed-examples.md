```bash
# Find references and assess impact
serena find_referencing_symbols --name 'deprecated_function'
# If 50+ references, plan careful migration
# If 0 references, safe to remove

```text

### 5. Combine with git diff

After insertions, verify changes:

```bash
serena insert_after_symbol --name 'MyClass' --code '...'
git diff  # Review actual changes before committing

```text

## Supported Languages

Serena supports 30+ languages through LSP integration:

**Tier 1 (Fully tested):**

- Python, JavaScript, TypeScript, Rust, Go, Java, C/C++

**Tier 2 (Community tested):**

- C#, Ruby, PHP, Kotlin, Swift, Scala

**Tier 3 (Experimental):**

- Haskell, Elixir, Clojure, Erlang, Julia, R, and more

For the complete list and setup instructions, see
[Serena language support docs](https://oraios.github.io/serena/languages).

## Limitations

### When NOT to Use Serena

1. **Searching text/comments**: Use ripgrep instead

   ```bash
   # WRONG TOOL: Serena doesn't search comments
   serena find_symbol --name "TODO"

   # RIGHT TOOL: Use ripgrep for text
   rg "TODO"
````

1. **Generated code**: LSP may not index auto-generated files
   - Use ripgrep for build artifacts, generated code

1. **Very large codebases**: Symbol indexing can be slow
   - Use ripgrep for initial broad searches
   - Use serena for precise follow-up

1. **Dynamic languages without types**: Limited semantic info
   - Python without type hints has reduced precision
   - JavaScript without TypeScript has fewer guarantees

### Known Edge Cases

- **Ambiguous symbols**: Multiple symbols with same name may require manual disambiguation
- **Macro-generated code**: C/C++ macros may confuse LSP
- **Circular dependencies**: May affect reference tracking accuracy
- **Incomplete projects**: Missing dependencies can reduce LSP effectiveness

## Performance Considerations

### Token Efficiency

Serena is designed for **token-efficient code navigation**:

````bash
# Traditional approach (inefficient)
execute_command("cat entire_file.py")  # 1000+ tokens
# [Search for symbol manually in output]

# Serena approach (efficient)
serena find_symbol --name 'MyClass'  # 50 tokens
# [Get precise location immediately]

```text

### Speed Characteristics

- **Symbol lookup**: Near-instant (LSP indexed)
- **Reference finding**: Fast (O(log n) with indexing)
- **Code insertion**: Instant (direct file modification)

**Comparison with alternatives:**

- Ripgrep: Faster for text search (no semantic understanding)
- AST-grep: Comparable speed (syntax vs semantic)
- Serena: Slower initial startup (LSP indexing), faster precise queries

## Troubleshooting

### Symbol Not Found

If `find_symbol` returns no results:

1. **Verify symbol exists**: Use ripgrep to confirm

   ```bash
   rg "class MyClass" --type py
````

1. **Check language server**: Ensure LSP is configured for the language

   ```bash
   serena status  # Check LSP server status
   ```

1. **Try case variations**: Symbol names are case-sensitive

   ```bash
   serena find_symbol --name 'myclass'  # Try different cases
   ```

1. **Rebuild index**: Force LSP to re-index

   ```bash
   serena reindex  # Rebuild symbol index
   ```

### Too Many References

If `find_referencing_symbols` returns hundreds of results:

1. **Use file-search first**: Narrow scope with ripgrep

   ```bash
   rg "MyClass" src/services/  # Limit to specific directory
   serena find_referencing_symbols --name 'MyClass' --path src/services/
   ```

1. **Filter by reference type**: Focus on specific usage patterns

   ```bash
   # Look for imports only
   rg "from .* import.*MyClass" --type py
   ```

1. **Prioritize recent changes**: Check git history first

   ```bash
   git log --all -p -S 'MyClass' --since="1 week ago"
   ```

### Insertion Failures

If `insert_after_symbol` fails:

1. **Verify symbol exists**: Find it first
2. **Check syntax**: Ensure inserted code is valid
3. **Review indentation**: Match surrounding code style
4. **Test incrementally**: Insert small changes first

## Resources

- [Serena GitHub](https://github.com/oraios/serena)
- [Serena Documentation](https://oraios.github.io/serena)
- [LSP Specification](https://microsoft.github.io/language-server-protocol/)
- [Solid-LSP (Serena's foundation)](https://github.com/oraios/solid-lsp)
