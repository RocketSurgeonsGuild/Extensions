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
        var isImplementationAccessible = compilation.IsSymbolAccessibleWithin(implementationType, compilation.Assembly);

        if (isImplementationAccessible)
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

            return GenerateServiceType(
                compilation,
                serviceType,
                serviceTypeExpression,
                implementationType,
                implementationTypeExpression,
                lifetime
            );
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
            return GenerateServiceType(
                compilation,
                serviceType,
                serviceTypeExpression,
                implementationType,
                implementationTypeExpression,
                lifetime
            );
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
        return GenerateServiceType(
            compilation,
            serviceType,
            serviceTypeExpression,
            implementationType,
            implementationTypeExpression,
            lifetime
        );
    }

    public static InvocationExpressionSyntax GenerateServiceType(
        Compilation compilation,
        INamedTypeSymbol serviceType,
        ExpressionSyntax serviceTypeExpression,
        INamedTypeSymbol implementationType,
        ExpressionSyntax implementationTypeExpression,
        string lifetime
    )
    {
        var isServiceTypeAccessible = compilation.IsSymbolAccessibleWithin(serviceType, compilation.Assembly);
        var isImplementationTypeAccessible = compilation.IsSymbolAccessibleWithin(implementationType, compilation.Assembly);
        return ( isServiceTypeAccessible, serviceTypeExpression, isImplementationTypeAccessible, implementationTypeExpression )
               switch
               {
                   (true, TypeOfExpressionSyntax serviceTypeOfExpression, true, TypeOfExpressionSyntax implementationTypeOfExpression)
                       when serviceType is { IsUnboundGenericType: false } && implementationType is { IsUnboundGenericType: false } => InvocationExpression(
                           MemberAccessExpression(
                               SyntaxKind.SimpleMemberAccessExpression,
                               IdentifierName("ServiceDescriptor"),
                               GenericName(lifetime)
                                  .WithTypeArgumentList(
                                       TypeArgumentList(
                                           SeparatedList(
                                               [
                                                   serviceTypeOfExpression.Type,
                                                   implementationTypeOfExpression.Type,
                                               ]
                                           )
                                       )
                                   )
                           )
                       ),
                   (true, TypeOfExpressionSyntax { Type: not GenericNameSyntax { IsUnboundGenericName: true } } serviceTypeOfExpression, _, { })
                       when serviceType is { IsUnboundGenericType: false } => InvocationExpression(
                               MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   IdentifierName("ServiceDescriptor"),
                                   GenericName(lifetime)
                                      .WithTypeArgumentList(TypeArgumentList(SeparatedList([serviceTypeOfExpression.Type])))
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
                           ),
                   _ => InvocationExpression(
                           MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("ServiceDescriptor"), IdentifierName(lifetime))
                       )
                      .WithArgumentList(ArgumentList(SeparatedList([Argument(serviceTypeExpression), Argument(implementationTypeExpression!)]))),
               };
    }

    public static bool IsOpenGenericType(this INamedTypeSymbol type) =>
        type.IsGenericType && ( type.IsUnboundGenericType || type.TypeArguments.All(z => z.TypeKind == TypeKind.TypeParameter) );
}
