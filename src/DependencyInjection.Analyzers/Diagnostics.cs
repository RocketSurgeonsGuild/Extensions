using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class Diagnostics
{
    public static DiagnosticDescriptor MustBeAnExpression { get; } = new(
        "RSGD0001",
        "Must be a expression",
        "Methods that will be analyzed statically must be an expression, blocks and variables are not allowed",
        Category,
        DiagnosticSeverity.Error,
        true
    );

    public static DiagnosticDescriptor MustBeTypeOf { get; } = new(
        "RSGD0002",
        "Must use typeof",
        "Method must be called with typeof, variables are not allowed",
        Category,
        DiagnosticSeverity.Error,
        true
    );

    public static DiagnosticDescriptor UnhandledSymbol { get; } = new(
        "RSGD0003",
        "Symbol could not be handled",
        "The indicated symbol could not be handled correctly",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor NamespaceMustBeAString { get; } = new(
        "RSGD0004",
        "Namespace must be a string",
        "The given namespace must be a constant string",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor DuplicateServiceDescriptorAttribute { get; } = new(
        "RSGD0005",
        "Duplicate service descriptor attribute",
        "Cannot have more than one service descriptor attribute for a given type",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor MustBeAString { get; } = new(
        "RSGD0005",
        "Value must be a string",
        "The given value must be a constant string",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor UnhandledException { get; } = new(
        "RSGD0006",
        "Unhandled exception",
        "Unhandled exception has occured {2} {0} -- Stack Trace: {1} {3}",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    public static DiagnosticDescriptor CouldNotFindServiceType { get; } = new(
        "RSGD0007",
        "Could not find service type",
        "Could not find service type {0} {1}",
        Category,
        DiagnosticSeverity.Warning,
        true
    );

    private const string Category = "Dependency Injection";
}
