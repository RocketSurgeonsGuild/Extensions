
        var html = _renderer.RenderOrderConfirmation(model);

        return Verify(html, extension: "html");
    }

    [Fact]
    public Task PasswordReset_MatchesSnapshot()
    {
        var model = new PasswordResetModel
        {
            UserName = "alice",
            ResetLink = "https://example.com/reset?token=test-token"
        };

        var html = _renderer.RenderPasswordReset(model);

        return Verify(html, extension: "html")
            .ScrubLinesWithReplace(line =>
                Regex.Replace(line, @"token=[^""&]+", "token={scrubbed}"));
    }
}

```text

---

## Custom Converters

Custom converters control how specific types are serialized for verification. Use them for domain types that need a readable, stable representation.

### Writing a Custom Converter

```csharp

public class MoneyConverter : WriteOnlyJsonConverter<Money>
{
    public override void Write(VerifyJsonWriter writer, Money value)
    {
        writer.WriteStartObject();
        writer.WriteMember(value, value.Amount, "Amount");
        writer.WriteMember(value, value.Currency.Code, "Currency");
        writer.WriteEndObject();
    }
}

```text

Register in the module initializer:

```csharp

[ModuleInitializer]
public static void Init()
{
    VerifierSettings.AddExtraSettings(settings =>
        settings.Converters.Add(new MoneyConverter()));
}

```text

### Converter for Complex Domain Types

```csharp

public class AddressConverter : WriteOnlyJsonConverter<Address>
{
    public override void Write(VerifyJsonWriter writer, Address value)
    {
        // Single-line summary for compact snapshots
        writer.WriteValue($"{value.Street}, {value.City}, {value.State} {value.Zip}");
    }
}

public class DateRangeConverter : WriteOnlyJsonConverter<DateRange>
{
    public override void Write(VerifyJsonWriter writer, DateRange value)
    {
        writer.WriteStartObject();
        writer.WriteMember(value, value.Start.ToString("yyyy-MM-dd"), "Start");
        writer.WriteMember(value, value.End.ToString("yyyy-MM-dd"), "End");
        writer.WriteMember(value, value.Duration.Days, "DurationDays");
        writer.WriteEndObject();
    }
}

```text

Usage in tests:

```csharp

[Fact]
public Task Customer_WithAddress_MatchesSnapshot()
{
    var customer = new Customer
    {
        Name = "Alice Johnson",
        Address = new Address("123 Main St", "Springfield", "IL", "62701"),
        MemberSince = new DateRange(
            new DateTime(2020, 1, 15),
            new DateTime(2025, 1, 15))
    };

    return Verify(customer);
}

```text

Produces:

```txt

{
  Name: Alice Johnson,
  Address: 123 Main St, Springfield, IL 62701,
  MemberSince: {
    Start: 2020-01-15,
    End: 2025-01-15,
    DurationDays: 1827
  }
}

```text

---

## Snapshot File Organization

### Default Naming

Verify names snapshot files based on the test class and method:

```text

TestClassName.MethodName.verified.txt

```text

Files are placed next to the test source file by default.

### Unique Directory

Move verified files into a dedicated directory to reduce clutter:

```csharp

// ModuleInitializer.cs
[ModuleInitializer]
public static void Init()
{
    Verifier.DerivePathInfo(
        (sourceFile, projectDirectory, type, method) =>
            new PathInfo(
                directory: Path.Combine(projectDirectory, "Snapshots"),
                typeName: type.Name,
                methodName: method.Name));
}

```text

### Parameterized Tests

For `[Theory]` tests, Verify appends parameter values to the file name:

```csharp

[Theory]
[InlineData("en-US")]
[InlineData("de-DE")]
[InlineData("ja-JP")]
public Task FormatCurrency_ByLocale_MatchesSnapshot(string locale)
{
    var formatted = currencyFormatter.Format(1234.56m, locale);
    return Verify(formatted)
        .UseParameters(locale);
}

```text

Creates separate files:

```text

FormatCurrencyTests.FormatCurrency_ByLocale_MatchesSnapshot_locale=en-US.verified.txt
FormatCurrencyTests.FormatCurrency_ByLocale_MatchesSnapshot_locale=de-DE.verified.txt
FormatCurrencyTests.FormatCurrency_ByLocale_MatchesSnapshot_locale=ja-JP.verified.txt

```text

---

## Workflow: Accepting Changes

When a snapshot test fails, Verify creates a `.received.txt` file alongside the `.verified.txt` file. Review the diff and accept or reject:

### Diff Tool Integration

Verify launches a diff tool automatically when a test fails. Configure the preferred tool:

```csharp

[ModuleInitializer]
public static void Init()
{
    // Verify auto-detects installed diff tools
    // Override if needed:
    DiffTools.UseOrder(DiffTool.VisualStudioCode, DiffTool.Rider);
}

```text

### CLI Acceptance

Install the Verify CLI tool (one-time setup), then accept pending changes after review:

```bash

# Install the Verify CLI tool (one-time)
dotnet tool install -g verify.tool

# Accept all received files in the solution
verify accept

# Accept for a specific test project
verify accept --project tests/MyApp.Tests

```text

### CI Behavior

In CI, Verify should fail tests without launching a diff tool. Set the environment variable:

```yaml

env:
  DiffEngine_Disabled: true

```yaml

Or in the module initializer:

```csharp

[ModuleInitializer]
public static void Init()
{
    if (Environment.GetEnvironmentVariable("CI") is not null)
    {
        DiffRunner.Disabled = true;
    }
}

```text

---

## Key Principles

- **Snapshot test complex outputs, not simple values.** If the expected value fits in a single `Assert.Equal`, prefer that over a snapshot. Snapshots shine for multi-field objects, API responses, and rendered content.
- **Scrub all non-deterministic values.** Dates, GUIDs, timestamps, and machine-specific values must be scrubbed or ignored. Unscrubbed snapshots cause flaky tests.
- **Commit `.verified.txt` files to source control.** These are the approved baselines. Never add `.received.txt` files -- they represent unapproved changes.
- **Review snapshot diffs carefully.** Accepting a snapshot change without review can silently approve regressions. Treat snapshot diffs like code review.
- **Use custom converters for domain readability.** Default JSON serialization may be verbose or unclear for domain types. Converters produce focused, human-readable snapshots.
- **Keep snapshots focused.** Verify only the parts that matter. Use `IgnoreMember` to exclude volatile or irrelevant fields rather than verifying the entire object graph.

---

## Agent Gotchas

1. **Do not forget `[UsesVerify]` on the test class.** Without this attribute, `Verify()` calls compile but fail at runtime with an initialization error. Every test class using Verify must have this attribute.
2. **Do not commit `.received.txt` files.** These represent test failures and unapproved changes. Add `*.received.*` to `.gitignore` to prevent accidental commits.
3. **Do not skip `UseParameters()` in parameterized tests.** Without it, all parameter combinations write to the same snapshot file, overwriting each other. Always call `UseParameters()` with the theory data values.
4. **Do not scrub values that are part of the contract.** If an API always returns a specific date format or a known GUID, verify those values rather than scrubbing them. Only scrub values that are genuinely non-deterministic between runs.
5. **Do not use snapshot testing for rapidly evolving APIs.** During early development when the API shape changes frequently, snapshot tests create excessive churn. Wait until the API stabilizes.
6. **Do not hardcode Verify package versions across different test frameworks.** `Verify.Xunit`, `Verify.NUnit`, and `Verify.MSTest` have independent version lines. Always use version ranges (e.g., `20.*`) rather than pinning to a specific version.

---

## References

- [Verify GitHub repository](https://github.com/VerifyTests/Verify)
- [Verify documentation](https://github.com/VerifyTests/Verify/blob/main/docs/readme.md)
- [Verify.Http for HTTP response testing](https://github.com/VerifyTests/Verify.Http)
- [Scrubbing and filtering](https://github.com/VerifyTests/Verify/blob/main/docs/scrubbers.md)
- [Custom converters](https://github.com/VerifyTests/Verify/blob/main/docs/converters.md)
- [DiffEngine (diff tool integration)](https://github.com/VerifyTests/DiffEngine)
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
