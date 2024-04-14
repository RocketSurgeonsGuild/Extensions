using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class StatementGeneration
{
    public static InvocationExpressionSyntax GenerateServiceFactory(
        Compilation compilation,
        NameSyntax strategyName,
        NameSyntax serviceCollectionName,
        INamedTypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        ExpressionSyntax lifetime,
        bool useAssemblyLoad
    )
    {
        var serviceTypeExpression = GetTypeOfExpression(compilation, serviceType, implementationType, useAssemblyLoad);
        var isAccessible = compilation.IsSymbolAccessibleWithin(implementationType, compilation.Assembly);

        if (isAccessible)
        {
            var implementationTypeExpression = SimpleLambdaExpression(Parameter(Identifier("_")))
               .WithExpressionBody(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("_"),
                            GenericName("GetRequiredService")
                               .WithTypeArgumentList(
                                    TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(Helpers.GetGenericDisplayName(implementationType))))
                                )
                        )
                    )
                );

            return GenerateServiceType(strategyName, serviceCollectionName, serviceTypeExpression, implementationTypeExpression, lifetime);
        }
        else
        {
            var implementationTypeExpression = SimpleLambdaExpression(Parameter(Identifier("_")))
               .WithExpressionBody(
                    BinaryExpression(
                        SyntaxKind.AsExpression,
                        InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("_"), IdentifierName("GetRequiredService"))
                            )
                           .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(Argument(GetTypeOfExpression(compilation, implementationType, serviceType, useAssemblyLoad)))
                                )
                            ),
                        IdentifierName(Helpers.GetGenericDisplayName(serviceType))
                    )
                );
            return GenerateServiceType(strategyName, serviceCollectionName, serviceTypeExpression, implementationTypeExpression, lifetime);
        }
    }

    public static InvocationExpressionSyntax GenerateServiceType(
        Compilation compilation,
        NameSyntax strategyName,
        NameSyntax serviceCollectionName,
        INamedTypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        ExpressionSyntax lifetime,
        bool useAssemblyLoad
    )
    {
        var serviceTypeExpression = GetTypeOfExpression(compilation, serviceType, implementationType, useAssemblyLoad);
        var implementationTypeExpression = GetTypeOfExpression(compilation, implementationType, serviceType, useAssemblyLoad);
        return GenerateServiceType(strategyName, serviceCollectionName, serviceTypeExpression, implementationTypeExpression, lifetime);
    }

    public static IEnumerable<MemberDeclarationSyntax> AssemblyDeclaration(IAssemblySymbol symbol)
    {
        var name = Helpers.AssemblyVariableName(symbol);
        var assemblyName = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(symbol.Identity.GetDisplayName(true)));

        yield return FieldDeclaration(
                VariableDeclaration(IdentifierName("AssemblyName"))
                   .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier($"_{name}"))
                        )
                    )
            )
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)));
        yield return PropertyDeclaration(IdentifierName("AssemblyName"), Identifier(name))
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithExpressionBody(
                         ArrowExpressionClause(
                             AssignmentExpression(
                                 SyntaxKind.CoalesceAssignmentExpression,
                                 IdentifierName(Identifier($"_{name}")),
                                 ObjectCreationExpression(IdentifierName("AssemblyName"))
                                    .WithArgumentList(
                                         ArgumentList(SingletonSeparatedList(Argument(assemblyName)))
                                     )
                             )
                         )
                     )
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public static bool RemoveImplicitGenericConversion(
        Compilation compilation,
        INamedTypeSymbol assignableToType,
        INamedTypeSymbol type
    )
    {
        if (SymbolEqualityComparer.Default.Equals(compilation.ObjectType, type)) return true;
        if (SymbolEqualityComparer.Default.Equals(assignableToType, type))
            return true;
        if (assignableToType.Arity <= 0 || !assignableToType.IsUnboundGenericType) return !compilation.HasImplicitConversion(type, assignableToType);

        var matchingBaseTypes = Helpers
                               .GetBaseTypes(compilation, type)
                               .Select(z => z.IsGenericType ? z.IsUnboundGenericType ? z : z.ConstructUnboundGenericType() : null!)
                               .Where(z => z is { })
                               .Where(symbol => compilation.HasImplicitConversion(symbol, assignableToType));
        if (matchingBaseTypes.Any())
        {
            return false;
        }

        var matchingInterfaces = type
                                .AllInterfaces
                                .Select(z => z.IsGenericType ? z.IsUnboundGenericType ? z : z.ConstructUnboundGenericType() : null!)
                                .Where(z => z is { })
                                .Where(symbol => compilation.HasImplicitConversion(symbol, assignableToType));
        return !matchingInterfaces.Any();
    }

    public static string GetGenericDisplayName(ISymbol? symbol)
    {
        if (symbol == null || IsRootNamespace(symbol))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(symbol.MetadataName);
        if (symbol is INamedTypeSymbol namedTypeSymbol && ( namedTypeSymbol.IsOpenGenericType() || namedTypeSymbol.IsGenericType ))
        {
            sb = new(symbol.Name);
            if (namedTypeSymbol.IsOpenGenericType())
            {
                sb.Append('<');
                for (var i = 1; i < namedTypeSymbol.Arity - 1; i++)
                    sb.Append(',');
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

    public static bool IsOpenGenericType(this INamedTypeSymbol type)
    {
        return type.IsGenericType && ( type.IsUnboundGenericType || type.TypeArguments.All(z => z.TypeKind == TypeKind.TypeParameter) );
    }

    private static readonly MemberAccessExpressionSyntax Describe = MemberAccessExpression(
        SyntaxKind.SimpleMemberAccessExpression,
        IdentifierName("ServiceDescriptor"),
        IdentifierName("Describe")
    );

    private static InvocationExpressionSyntax GenerateServiceType(
        NameSyntax strategyName,
        NameSyntax serviceCollectionName,
        ExpressionSyntax serviceTypeExpression,
        ExpressionSyntax implementationTypeExpression,
        ExpressionSyntax lifetime
    )
    {
        return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    strategyName,
                    IdentifierName("Apply")
                )
            )
           .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        new[]
                        {
                            Argument(serviceCollectionName),
                            Argument(
                                InvocationExpression(Describe)
                                   .WithArgumentList(
                                        ArgumentList(
                                            SeparatedList(
                                                new[]
                                                {
                                                    Argument(serviceTypeExpression),
                                                    Argument(implementationTypeExpression),
                                                    Argument(lifetime),
                                                }
                                            )
                                        )
                                    )
                            ),
                        }
                    )
                )
            );
    }

    private static ExpressionSyntax GetTypeOfExpression(
        Compilation compilation,
        INamedTypeSymbol type,
        INamedTypeSymbol? relatedType,
        bool useAssemblyLoad
    )
    {
        if (type.IsUnboundGenericType && relatedType != null)
        {
            if (relatedType.IsGenericType && relatedType.Arity == type.Arity)
            {
                type = type.Construct(relatedType.TypeArguments.ToArray());
            }
            else
            {
                var baseType = Helpers.GetBaseTypes(compilation, type).FirstOrDefault(z => z.IsGenericType && compilation.HasImplicitConversion(z, type));
                if (baseType == null)
                {
                    // ReSharper disable once AccessToModifiedClosure
                    baseType = type.AllInterfaces.FirstOrDefault(z => z.IsGenericType && compilation.HasImplicitConversion(z, type));
                }

                if (baseType != null)
                {
                    type = type.Construct(baseType.TypeArguments.ToArray());
                }
            }
        }

        if (compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
        {
            return TypeOfExpression(ParseTypeName(Helpers.GetGenericDisplayName(type)));
        }

        if (type.IsGenericType && !type.IsOpenGenericType())
        {
            var result = compilation.IsSymbolAccessibleWithin(type.ConstructUnboundGenericType(), compilation.Assembly);
            if (result)
            {
                var name = ParseTypeName(type.ConstructUnboundGenericType().ToDisplayString());
                if (name is GenericNameSyntax genericNameSyntax)
                {
                    name = genericNameSyntax.WithTypeArgumentList(
                        TypeArgumentList(
                            SeparatedList<TypeSyntax>(
                                genericNameSyntax.TypeArgumentList.Arguments.Select(_ => OmittedTypeArgument()).ToArray()
                            )
                        )
                    );
                }

                return InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, TypeOfExpression(name), IdentifierName("MakeGenericType"))
                    )
                   .WithArgumentList(
                        ArgumentList(
                            SeparatedList(
                                type.TypeArguments
                                    .Select(t => Argument(GetTypeOfExpression(compilation, ( t as INamedTypeSymbol )!, null, useAssemblyLoad)))
                            )
                        )
                    );
            }
        }

        return GetPrivateType(compilation, type, useAssemblyLoad);
    }

    private static InvocationExpressionSyntax GetPrivateType(Compilation compilation, INamedTypeSymbol type, bool useAssemblyLoad)
    {
        var expression = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    InvocationExpression(
                            useAssemblyLoad
                                ? MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("Assembly"),
                                    IdentifierName("Load")
                                )
                                : MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("context"),
                                    IdentifierName("LoadFromAssemblyName")
                                )
                        )
                       .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(IdentifierName(Helpers.AssemblyVariableName(type.ContainingAssembly)))
                                )
                            )
                        ),
                    IdentifierName("GetType")
                )
            )
           .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Helpers.GetFullMetadataName(type))))
                    )
                )
            );
        if (type.IsGenericType && !type.IsOpenGenericType())
        {
            return InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName("MakeGenericType"))
                )
               .WithArgumentList(
                    ArgumentList(
                        SeparatedList(
                            type.TypeArguments
                                .Select(t => Argument(GetTypeOfExpression(compilation, ( t as INamedTypeSymbol )!, null, useAssemblyLoad)))
                        )
                    )
                );
        }

        return expression;
    }
}