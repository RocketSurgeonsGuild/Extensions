using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class Diagnostics
{
    private const string Category = "Dependency Injection";

    public static DiagnosticDescriptor MustBeAnExpression { get; } = new DiagnosticDescriptor(
        "RSGD0001",
        "Must be a expression",
        "Methods that will be analyzed statically must be an expression, blocks and variables are not allowed",
        Category,
        DiagnosticSeverity.Error,
        true
    );

    public static DiagnosticDescriptor MustBeTypeOf { get; } = new DiagnosticDescriptor(
        "RSGD0002",
        "Must use typeof",
        "Method must be called with typeof, variables are not allowed",
        Category,
        DiagnosticSeverity.Error,
        true
    );

    public static DiagnosticDescriptor UnhandledSymbol { get; } = new DiagnosticDescriptor(
        "RSGD0003",
        "Symbol could not be handled",
        "The indicated symbol could not be handled correctly",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor NamespaceMustBeAString { get; } = new DiagnosticDescriptor(
        "RSGD0004",
        "Namespace must be a string",
        "The given namespace must be a constant string",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor DuplicateServiceDescriptorAttribute { get; } = new DiagnosticDescriptor(
        "RSGD0005",
        "Duplicate service descriptor attribute",
        "Cannot have more than one service descriptor attribute for a given type",
        Category,
        DiagnosticSeverity.Warning,
        true
    );
}
