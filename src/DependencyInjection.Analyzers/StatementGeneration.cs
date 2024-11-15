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

            return GenerateServiceType(serviceTypeExpression, implementationTypeExpression, lifetime);
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
            return GenerateServiceType(serviceTypeExpression, implementationTypeExpression, lifetime);
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
        return GenerateServiceType(serviceTypeExpression, implementationTypeExpression, lifetime);
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

    private static InvocationExpressionSyntax GenerateServiceType(
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
