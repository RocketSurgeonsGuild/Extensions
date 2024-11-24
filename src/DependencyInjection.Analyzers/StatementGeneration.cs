using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class StatementGeneration
{
    public static InvocationExpressionSyntax GenerateServiceFactory(
        Compilation compilation,
        INamedTypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        string lifetime
    )
    {
        var serviceTypeExpression = AssemblyProviders.StatementGeneration.GetTypeOfExpression(compilation, serviceType);
        var isAccessible = compilation.IsSymbolAccessibleWithin(implementationType, compilation.Assembly);

        if (isAccessible)
        {
            var implementationTypeExpression = SimpleLambdaExpression(Parameter(Identifier("a")))
               .WithExpressionBody(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("a"),
                            GenericName("GetRequiredService")
                               .WithTypeArgumentList(
                                    TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(Helpers.GetGenericDisplayName(implementationType))))
                                )
                        )
                    )
                );

            return GenerateServiceType(serviceType, serviceTypeExpression, implementationType, implementationTypeExpression, lifetime);
        }
        else
        {
            var implementationTypeExpression = SimpleLambdaExpression(Parameter(Identifier("a")))
               .WithExpressionBody(
                    BinaryExpression(
                        SyntaxKind.AsExpression,
                        InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("a"), IdentifierName("GetRequiredService"))
                            )
                           .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(Argument(AssemblyProviders.StatementGeneration.GetTypeOfExpression(compilation, implementationType)))
                                )
                            ),
                        IdentifierName(Helpers.GetGenericDisplayName(serviceType))
                    )
                );
            return GenerateServiceType(serviceType, serviceTypeExpression, implementationType, implementationTypeExpression, lifetime);
        }
    }

    public static InvocationExpressionSyntax GenerateServiceType(
        Compilation compilation,
        INamedTypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        string lifetime
    )
    {
        var serviceTypeExpression = AssemblyProviders.StatementGeneration.GetTypeOfExpression(compilation, serviceType);
        var implementationTypeExpression = AssemblyProviders.StatementGeneration.GetTypeOfExpression(compilation, implementationType);
        return GenerateServiceType(serviceType, serviceTypeExpression, implementationType, implementationTypeExpression, lifetime);
    }

    public static InvocationExpressionSyntax GenerateServiceType(
        INamedTypeSymbol serviceType,
        ExpressionSyntax serviceTypeExpression,
        INamedTypeSymbol implementationType,
        ExpressionSyntax implementationTypeExpression,
        string lifetime
    )
    {
        switch ( serviceTypeExpression, implementationTypeExpression )
        {
            case (TypeOfExpressionSyntax serviceTypeOfExpression, TypeOfExpressionSyntax implementationTypeOfExpression)
                when !serviceType.IsUnboundGenericType && !implementationType.IsUnboundGenericType:
                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("ServiceDescriptor"),
                        GenericName(lifetime)
                           .WithTypeArgumentList(
                                TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        [
                                            serviceTypeOfExpression.Type,
                                            implementationTypeOfExpression.Type,
                                        ]
                                    )
                                )
                            )
                    )
                );
            case (TypeOfExpressionSyntax { Type: not GenericNameSyntax { IsUnboundGenericName: true } } serviceTypeOfExpression, { })
                when !serviceType.IsUnboundGenericType:
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("ServiceDescriptor"),
                            GenericName(lifetime)
                               .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>([serviceTypeOfExpression.Type])))
                        )
                    )
                   .WithArgumentList(
                        ArgumentList(
                            SeparatedList(
                                [
                                    Argument(implementationTypeExpression),
                                ]
                            )
                        )
                    );
            default:
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("ServiceDescriptor"),
                            IdentifierName(lifetime)
                        )
                    )
                   .WithArgumentList(
                        ArgumentList(
                            SeparatedList(
                                [
                                    Argument(serviceTypeExpression),
                                    Argument(implementationTypeExpression!),
                                ]
                            )
                        )
                    );
        }
    }

    public static bool IsOpenGenericType(this INamedTypeSymbol type)
    {
        return type.IsGenericType && ( type.IsUnboundGenericType || type.TypeArguments.All(z => z.TypeKind == TypeKind.TypeParameter) );
    }

    private static MemberAccessExpressionSyntax DescribeLifetime(string lifetime) => MemberAccessExpression(
        SyntaxKind.SimpleMemberAccessExpression,
        IdentifierName("ServiceDescriptor"),
        IdentifierName(lifetime)
    );

    private static InvocationExpressionSyntax GenerateServiceType_(
        ExpressionSyntax serviceTypeExpression,
        ExpressionSyntax implementationTypeExpression,
        string lifetime
    )
    {
        return InvocationExpression(DescribeLifetime(lifetime))
           .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        new[]
                        {
                            Argument(serviceTypeExpression),
                            Argument(implementationTypeExpression),
                        }
                    )
                )
            );
    }
}
