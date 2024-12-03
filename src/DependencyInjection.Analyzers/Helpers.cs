using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class Helpers
{
    public static CompilationUnitSyntax AddSharedTrivia(this CompilationUnitSyntax source) =>
        source
           .WithLeadingTrivia(
                Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)),
                Trivia(
                    PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)
                       .WithErrorCodes(SeparatedList(List(DisabledWarnings.Value)))
                )
            )
           .WithTrailingTrivia(
                Trivia(
                    PragmaWarningDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true)
                       .WithErrorCodes(SeparatedList(List(DisabledWarnings.Value)))
                ),
                Trivia(NullableDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true)),
                CarriageReturnLineFeed
            );

    public static INamedTypeSymbol? GetUnboundGenericType(INamedTypeSymbol symbol)
    {
        return symbol switch
               {
                   { IsGenericType: true, IsUnboundGenericType: true } => symbol, { IsGenericType: true } => symbol.ConstructUnboundGenericType(), _ => default,
               };
    }

    public static string GetTypeOfName(ISymbol? symbol)
    {
        if (symbol == null || IsRootNamespace(symbol)) return string.Empty;

        var sb = new StringBuilder(symbol.MetadataName);
        if (symbol is INamedTypeSymbol namedTypeSymbol && ( namedTypeSymbol.IsOpenGenericType() || namedTypeSymbol.IsGenericType ))
        {
            sb = new(symbol.Name);
            if (namedTypeSymbol.IsOpenGenericType())
            {
                sb.Append('<');
                for (var i = 1; i < namedTypeSymbol.Arity; i++)
                {
                    sb.Append(',');
                }

                sb.Append('>');
            }
            else
            {
                sb.Append('<');
                for (var index = 0; index < namedTypeSymbol.TypeArguments.Length; index++)
                {
                    var argument = namedTypeSymbol.TypeArguments[index];
                    sb.Append(GetTypeOfName(argument));
                    if (index < namedTypeSymbol.TypeArguments.Length - 1)
                        sb.Append(',');
                }

                sb.Append('>');
            }
        }

        var last = symbol;

        var workingSymbol = symbol.ContainingSymbol;

        while (!IsRootNamespace(workingSymbol))
        {
            if (workingSymbol is ITypeSymbol && last is ITypeSymbol)
                sb.Insert(0, '.');
            else
                sb.Insert(0, '.');

            sb.Insert(0, workingSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Trim());
            //sb.Insert(0, symbol.MetadataName);
            workingSymbol = workingSymbol.ContainingSymbol;
        }

        sb.Insert(0, "global::");
        return sb.ToString();

        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol? s;
            return ( s = symbol as INamespaceSymbol ) != null && s.IsGlobalNamespace;
        }
    }

    public static string GetGenericDisplayName(ISymbol? symbol)
    {
        if (symbol == null || IsRootNamespace(symbol)) return string.Empty;

        var sb = new StringBuilder(symbol.MetadataName);
        if (symbol is INamedTypeSymbol namedTypeSymbol && ( namedTypeSymbol.IsOpenGenericType() || namedTypeSymbol.IsGenericType ))
        {
            sb = new(symbol.Name);
            if (namedTypeSymbol.IsOpenGenericType())
            {
                sb.Append('<');
                for (var i = 1; i < namedTypeSymbol.Arity; i++)
                {
                    sb.Append(',');
                }

                sb.Append('>');
            }
            else
            {
                sb.Append('<');
                for (var index = 0; index < namedTypeSymbol.TypeArguments.Length; index++)
                {
                    var argument = namedTypeSymbol.TypeArguments[index];
                    sb.Append(GetGenericDisplayName(argument));
                    if (index < namedTypeSymbol.TypeArguments.Length - 1)
                        sb.Append(',');
                }

                sb.Append('>');
            }
        }

        var last = symbol;

        var workingSymbol = symbol.ContainingSymbol;

        while (!IsRootNamespace(workingSymbol))
        {
            if (workingSymbol is ITypeSymbol && last is ITypeSymbol)
                sb.Insert(0, '+');
            else
                sb.Insert(0, '.');

            sb.Insert(0, workingSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Trim());
            //sb.Insert(0, symbol.MetadataName);
            workingSymbol = workingSymbol.ContainingSymbol;
        }

        sb.Insert(0, "global::");
        return sb.ToString();

        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol? s;
            return ( s = symbol as INamespaceSymbol ) != null && s.IsGlobalNamespace;
        }
    }


    public static INamedTypeSymbol GetClosedGenericConversion(
        Compilation compilation,
        INamedTypeSymbol assignableToType,
        INamedTypeSymbol assignableFromType
    )
    {
        if (assignableToType is not { IsUnboundGenericType: true, Arity: > 0 })
            return assignableToType;

        if (GetUnboundGenericType(assignableFromType) is { } unboundFromType && compilation.HasImplicitConversion(assignableToType, unboundFromType))
        {
            // TODO:
            return assignableToType;
//            return assignableToType.Construct(assignableFromType.TypeArguments.ToArray());
        }

        var matchingInterfaces = assignableFromType
                                .AllInterfaces
                                .Where(symbol => symbol is { } && compilation.HasImplicitConversion(GetUnboundGenericType(symbol), assignableToType));
        return matchingInterfaces.FirstOrDefault() ?? assignableToType;
    }


    public static bool HasImplicitGenericConversion(
        Compilation compilation,
        INamedTypeSymbol assignableToType,
        INamedTypeSymbol assignableFromType
    )
    {
        if (SymbolEqualityComparer.Default.Equals(compilation.ObjectType, assignableToType)) return false;
        if (SymbolEqualityComparer.Default.Equals(compilation.ObjectType, assignableFromType)) return false;
        if (SymbolEqualityComparer.Default.Equals(assignableToType, assignableFromType))
            return true;
        if (compilation.HasImplicitConversion(assignableFromType, assignableToType)) return true;
        if (assignableToType is not { Arity: > 0, IsUnboundGenericType: true }) return false;
        if (GetUnboundGenericType(assignableFromType) is { } unboundAssignableFromType
         && compilation.HasImplicitConversion(assignableToType, unboundAssignableFromType))
            return true;

        var matchingBaseTypes = GetBaseTypes(compilation, assignableFromType)
                               .Select(GetUnboundGenericType)
                               .Where(symbol => symbol is { } && compilation.HasImplicitConversion(symbol, assignableToType));
        if (matchingBaseTypes.Any()) return true;

        var matchingInterfaces = assignableFromType
                                .AllInterfaces
                                .Select(GetUnboundGenericType)
                                .Where(symbol => symbol is { } && compilation.HasImplicitConversion(symbol, assignableToType));
        return matchingInterfaces.Any();
    }

    public static string GetFullMetadataName(ISymbol? symbol)
    {
        if (symbol == null || IsRootNamespace(symbol))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(symbol.MetadataName);

        var last = symbol;

        var workingSymbol = symbol.ContainingSymbol;

        while (!IsRootNamespace(workingSymbol))
        {
            if (workingSymbol is ITypeSymbol && last is ITypeSymbol)
            {
                sb.Insert(0, '+');
            }
            else
            {
                sb.Insert(0, '.');
            }

            sb.Insert(0, workingSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Trim());
            //sb.Insert(0, symbol.MetadataName);
            workingSymbol = workingSymbol.ContainingSymbol;
        }

        return sb.ToString();

        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol? s;
            return ( s = symbol as INamespaceSymbol ) != null && s.IsGlobalNamespace;
        }
    }


    public static string AssemblyVariableName(IAssemblySymbol symbol) => SpecialCharacterRemover.Replace(symbol.Identity.GetDisplayName(true), "");

    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(Compilation compilation, INamedTypeSymbol namedTypeSymbol)
    {
        while (namedTypeSymbol.BaseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.BaseType, compilation.ObjectType)) yield break;
            yield return namedTypeSymbol.BaseType;
            namedTypeSymbol = namedTypeSymbol.BaseType;
        }
    }

    public static TypeSyntax? ExtractSyntaxFromMethod(
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name is GenericNameSyntax genericNameSyntax)
        {
            if (genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
            {
                return genericNameSyntax.TypeArgumentList.Arguments[0];
            }
        }

        if (name is SimpleNameSyntax)
        {
            if (expression.ArgumentList.Arguments.Count == 1 && expression.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax typeOfExpression)
            {
                return typeOfExpression.Type;
            }
        }

        return null;
    }

    internal static AttributeListSyntax CompilerGeneratedAttributes =
        AttributeList(
            SeparatedList(
                [
                    Attribute(ParseName("System.CodeDom.Compiler.GeneratedCode"))
                       .WithArgumentList(
                            AttributeArgumentList(
                                SeparatedList(
                                    [
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(typeof(Helpers).Assembly.GetName().Name)
                                            )
                                        ),
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(typeof(Helpers).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "generated")
                                            )
                                        ),
                                    ]
                                )
                            )
                        ),
                    Attribute(ParseName("System.Runtime.CompilerServices.CompilerGenerated")),
                    Attribute(ParseName("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")),
                ]
            )
        );

    internal static AttributeListSyntax AddAssemblyAttribute(string key, string? value) =>
        AttributeList(
                SingletonSeparatedList(
                    Attribute(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Reflection")), IdentifierName("AssemblyMetadata")))
                       .WithArgumentList(
                            AttributeArgumentList(
                                SeparatedList(
                                    [
                                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(key))),
                                        AttributeArgument(
                                            value is null
                                                ? LiteralExpression(SyntaxKind.NullLiteralExpression)
                                                : LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))
                                        ),
                                    ]
                                )
                            )
                        )
                )
            )
           .WithTarget(AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)));

    internal static SourceLocation CreateSourceLocation(InvocationExpressionSyntax methodCallSyntax, CancellationToken cancellationToken)
    {
        if (methodCallSyntax is { Expression: MemberAccessExpressionSyntax memberAccess, ArgumentList.Arguments: [{ Expression: { } argumentExpression }] }) { }
        else if (methodCallSyntax is
                 { Expression: MemberAccessExpressionSyntax memberAccess2, ArgumentList.Arguments: [_, { Expression: { } argumentExpression2 }] })
        {
            memberAccess = memberAccess2;
            argumentExpression = argumentExpression2;
        }
        else
        {
            throw new InvalidOperationException("Invalid method call syntax");
        }

        var hasher = MD5.Create();
        var expression = argumentExpression.ToFullString().Replace("\r", "");
        expression = string.Join("", expression.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(z => z.Trim()));
        var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(expression));

        var source = new SourceLocation(
            memberAccess
               .Name
               .SyntaxTree.GetText(cancellationToken)
               .Lines.First(z => z.Span.IntersectsWith(memberAccess.Name.Span))
               .LineNumber
          + 1,
            memberAccess.SyntaxTree.FilePath,
            Convert.ToBase64String(hash)
        );
        return source;
    }


    private static readonly string[] _disabledWarnings =
    [
        "CA1002",
        "CA1034",
        "CA1822",
        "CS0105",
        "CS1573",
        "CS8601",
        "CS8602",
        "CS8603",
        "CS8604",
        "CS8618",
        "CS8669",
    ];

    private static readonly Lazy<ImmutableArray<ExpressionSyntax>> DisabledWarnings = new(
        () => _disabledWarnings.Select(z => (ExpressionSyntax)IdentifierName(z)).ToImmutableArray()
    );

    private static readonly Regex SpecialCharacterRemover = new("[^\\w\\d]", RegexOptions.Compiled);
}
