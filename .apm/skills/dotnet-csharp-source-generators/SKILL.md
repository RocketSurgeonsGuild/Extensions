---
name: dotnet-csharp-source-generators
category: fundamentals
subcategory: coding-standards
description: Creates Roslyn source generators. IIncrementalGenerator, GeneratedRegex, LoggerMessage, STJ.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for csharp tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-csharp-source-generators

Guidance for both **creating** and **consuming** Roslyn source generators in .NET. Creating: `IIncrementalGenerator`,
syntax providers, semantic analysis, emit patterns, diagnostic reporting, testing with `CSharpGeneratorDriver`.
Consuming: `[GeneratedRegex]`, `[LoggerMessage]`, System.Text.Json source generation, `[JsonSerializable]`.

## Scope

- IIncrementalGenerator authoring and syntax providers
- Consuming built-in generators (GeneratedRegex, LoggerMessage, STJ)
- Diagnostic reporting and testing with CSharpGeneratorDriver
- NuGet packaging for analyzer/generator assemblies

## Out of scope

- Roslyn analyzers and code fix providers -- see [skill:dotnet-roslyn-analyzers]
- Modern C# language features -- see [skill:dotnet-csharp-modern-patterns]
- Naming conventions -- see [skill:dotnet-csharp-coding-standards]

Cross-references: [skill:dotnet-csharp-modern-patterns] for partial properties and related C# features,
[skill:dotnet-csharp-coding-standards] for naming conventions.

---

## Creating Source Generators

### Project Setup

Source generators are shipped as analyzers targeting `netstandard2.0`.

````xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" PrivateAssets="all" />
  </ItemGroup>
</Project>

```csharp

> **Always target `netstandard2.0`.** Generators load into the compiler process, which requires this TFM for
> compatibility. Use `LangVersion>latest` to write modern C# in the generator itself.

### `IIncrementalGenerator` (Preferred)

Always use `IIncrementalGenerator` over the legacy `ISourceGenerator`. Incremental generators are cache-aware and only
re-run when inputs change, making them significantly faster in IDE scenarios.

```csharp

[Generator]
public sealed class AutoNotifyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Filter syntax nodes to candidate fields
        var fieldDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "MyLib.AutoNotifyAttribute",
                predicate: static (node, _) => node is FieldDeclarationSyntax,
                transform: static (ctx, _) => GetFieldInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Step 2: Group fields by containing type, then emit one file per type
        context.RegisterSourceOutput(fieldDeclarations.Collect(),
            static (spc, fields) => Execute(fields, spc));
    }

    private static FieldInfo? GetFieldInfo(
        GeneratorAttributeSyntaxContext context)
    {
        var fieldSymbol = context.TargetSymbol as IFieldSymbol;
        if (fieldSymbol is null)
            return null;

        var containingType = fieldSymbol.ContainingType;

        // Use fully qualified type name to handle generic and nested types
        var fullTypeName = containingType.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

        return new FieldInfo(
            fieldSymbol.ContainingNamespace.IsGlobalNamespace
                ? ""
                : fieldSymbol.ContainingNamespace.ToDisplayString(),
            containingType.Name,
            fullTypeName,
            fieldSymbol.Name,
            fieldSymbol.Type.ToDisplayString());
    }

    private static void Execute(
        ImmutableArray<FieldInfo> fields,
        SourceProductionContext context)
    {
        // Group by fully qualified type name to emit one file per class
        foreach (var group in fields.GroupBy(f => f.FullTypeName))
        {
            var first = group.First();
            var ns = first.Namespace;
            var className = first.ClassName;
            var properties = new StringBuilder();

            foreach (var field in group)
            {
                var propertyName = GetPropertyName(field.FieldName);
                properties.AppendLine($$"""
                        public {{field.FieldType}} {{propertyName}}
                        {
                            get => {{field.FieldName}};
                            set
                            {
                                if (!global::System.Collections.Generic.EqualityComparer<{{field.FieldType}}>.Default.Equals({{field.FieldName}}, value))
                                {
                                    {{field.FieldName}} = value;
                                    PropertyChanged?.Invoke(this,
                                        new global::System.ComponentModel.PropertyChangedEventArgs(nameof({{propertyName}})));
                                }
                            }
                        }
                    """);
            }

            // Handle global namespace (no namespace declaration)
            var nsBlock = string.IsNullOrEmpty(ns) ? "" : $"namespace {ns};\n\n";

            var source = $$"""
                // <auto-generated/>
                #nullable enable

                {{nsBlock}}partial class {{className}}
                    : global::System.ComponentModel.INotifyPropertyChanged
                {
                    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

                {{properties}}
                }
                """;

            // Include namespace in hint name to avoid collisions across namespaces
            var hintPrefix = string.IsNullOrEmpty(ns) ? className : $"{ns}.{className}";
            context.AddSource($"{hintPrefix}.AutoNotify.g.cs", source);
        }
    }

    private static string GetPropertyName(string fieldName)
        => fieldName.TrimStart('_') is [var first, .. var rest]
            ? $"{char.ToUpperInvariant(first)}{rest}"
            : fieldName;
}

internal readonly record struct FieldInfo(
    string Namespace,
    string ClassName,
    string FullTypeName,
    string FieldName,
    string FieldType);

```text

> **Scope note:** This example targets top-level, non-generic classes for clarity. A production generator should also
> handle generic type parameters (emitting matching `partial class Foo<T>` declarations) and nested types (emitting
> nested partial class hierarchies). Report a diagnostic for unsupported shapes rather than emitting invalid code.

### Key Pipeline Design Rules

1. **Filter early** -- Use `ForAttributeWithMetadataName` or `CreateSyntaxProvider` with a tight predicate to minimize
   work.
2. **Transform to simple data** -- Extract only the data you need (strings, records) in the transform step. Never pass
   `ISymbol` or `SyntaxNode` through the pipeline (they hold the compilation alive and break caching).
3. **Use value equality** -- Pipeline outputs are compared by value. Use `record struct` or implement `IEquatable<T>`
   for custom types.
4. **Emit deterministic output** -- Same inputs must produce identical source. Use `// <auto-generated/>` and
   `#nullable enable` headers.

### Syntax Providers

```csharp

// ForAttributeWithMetadataName -- most common, filters by attribute
var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
    "MyLib.GenerateMapperAttribute",
    predicate: static (node, _) => node is ClassDeclarationSyntax,
    transform: static (ctx, _) => /* extract info */);

// CreateSyntaxProvider -- general-purpose, any syntax predicate
var candidates = context.SyntaxProvider.CreateSyntaxProvider(
    predicate: static (node, _) => node is MethodDeclarationSyntax m
        && m.Modifiers.Any(SyntaxKind.PartialKeyword),
    transform: static (ctx, _) => /* extract info */);

```text

### Diagnostic Reporting

Report errors and warnings through `SourceProductionContext` rather than throwing exceptions. To report
location-specific diagnostics, include a `Location` in your pipeline data (captured from the syntax node in the
transform step).

```csharp

private static readonly DiagnosticDescriptor InvalidFieldType = new(
    id: "AN001",
    title: "Invalid field type for AutoNotify",
    messageFormat: "Field '{0}' must be a non-pointer type",
    category: "AutoNotify",
    defaultSeverity: DiagnosticSeverity.Error,
    isEnabledByDefault: true);

// In the transform step, capture location:
var location = context.TargetNode.GetLocation();

// In the Execute method, report with location:
context.ReportDiagnostic(Diagnostic.Create(
    InvalidFieldType,
    location,       // captured from syntax node, not from projected data
    fieldName));

```text

> **Note:** `Location` is not value-equatable, so including it in your pipeline record breaks incremental caching. A
> common pattern is to carry it as a separate field that you exclude from equality, or report diagnostics in a
> `CreateSyntaxProvider` step before projecting to value types.

### Emit Patterns

```csharp

// Prefer raw string literals for templates (C# 11+, in the generator project)
var source = $$"""
    // <auto-generated/>
    #nullable enable

    namespace {{ns}};

    partial class {{className}}
    {
        {{generatedMembers}}
    }
    """;

context.AddSource($"{className}.g.cs", source);

```csharp

**File naming convention:** `{TypeName}.{Feature}.g.cs` -- the `.g.cs` suffix signals generated code and is excluded by
many linters.

### Post-Init Output (Static Source)

Use `RegisterPostInitializationOutput` for marker attributes and helper types that do not depend on user code:

```csharp

context.RegisterPostInitializationOutput(static ctx =>
{
    ctx.AddSource("AutoNotifyAttribute.g.cs", """
        // <auto-generated/>
        namespace MyLib;

        [System.AttributeUsage(System.AttributeTargets.Field)]
        internal sealed class AutoNotifyAttribute : System.Attribute { }
        """);
});

```text

---

## Testing Source Generators

Use `CSharpGeneratorDriver` to run generators in-memory and verify output.

```csharp

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

[Fact]
public void Generator_ProducesExpectedOutput()
{
    // Arrange
    var source = """
        using MyLib;

        namespace TestApp;

        public partial class ViewModel
        {
            [AutoNotify]
            private string _name = "";
        }
        """;

    var syntaxTree = CSharpSyntaxTree.ParseText(source);
    var references = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .Cast<MetadataReference>()
        .ToList();

    var compilation = CSharpCompilation.Create("TestAssembly",
        [syntaxTree],
        references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var generator = new AutoNotifyGenerator();

    // Act
    GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
    driver = driver.RunGeneratorsAndUpdateCompilation(
        compilation, out var outputCompilation, out var diagnostics);

    // Assert
    Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

    var runResult = driver.GetRunResult();
    Assert.Single(runResult.GeneratedTrees);

    var generatedSource = runResult.GeneratedTrees[0].GetText().ToString();
    Assert.Contains("public string Name", generatedSource);
}

```text

### Snapshot Testing (Verify)

For more robust testing, use the [Verify.SourceGenerators](https://github.com/VerifyTests/Verify.SourceGenerators)
package to snapshot-test generated output:

```csharp

[Fact]
public Task Generator_SnapshotTest()
{
    var source = """
        using MyLib;
        namespace TestApp;
        public partial class ViewModel
        {
            [AutoNotify]
            private string _name = "";
        }
        """;

    return TestHelper.Verify(source);
}

```text

---

## Consuming Built-In Source Generators

### `[GeneratedRegex]` (net7.0+)

Compile-time regex generation. Zero runtime compilation cost, AOT-compatible.

```csharp

public partial class Validators
{
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public static bool IsValidEmail(string email)
        => EmailRegex().IsMatch(email);

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
