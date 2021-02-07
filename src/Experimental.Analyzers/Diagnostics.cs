using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Experimental.Analyzers
{
    public static class Diagnostics
    {
        private const string Category = "Auto Implement";


        public static DiagnosticDescriptor MustBePartial  { get; } = new DiagnosticDescriptor(
            "LPAD0001",
            "Type must be made partial",
            "Type {0} must be made partial.",
            Category,
            DiagnosticSeverity.Error,
            true
        );

        // public static DiagnosticDescriptor MustBeAnExpression { get; } = new DiagnosticDescriptor(
        //     "RSGD0001",
        //     "Must be a expression",
        //     "Methods that will be analyzed statically must be an expression, blocks and variables are not allowed",
        //     Category,
        //     DiagnosticSeverity.Error,
        //     true
        // );
    }
}
