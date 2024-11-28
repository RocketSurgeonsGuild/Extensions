using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
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
        var serviceTypeExpression = GetTypeOfExpression(compilation, serviceType);
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
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(IdentifierName(Helpers.GetTypeOfName(implementationType)))
                                    )
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
                                    SingletonSeparatedList(Argument(GetTypeOfExpression(compilation, implementationType, serviceType)))
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
        var serviceTypeExpression = GetTypeOfExpression(compilation, serviceType);
        var implementationTypeExpression = GetTypeOfExpression(compilation, implementationType, serviceType);
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
        return ( isServiceTypeAccessible, serviceTypeExpression, implementationTypeExpression ) switch
               {
                   (true, TypeOfExpressionSyntax { Type: { } serviceTypeSyntax }, TypeOfExpressionSyntax { Type: { } implementationTypeSyntax })
                       when !IsOpenGenericType(serviceType)
                    && !IsOpenGenericType(implementationType)
                    && compilation.IsSymbolAccessibleWithin(implementationType, compilation.Assembly)
                       => InvocationExpression(
                           MemberAccessExpression(
                               SyntaxKind.SimpleMemberAccessExpression,
                               IdentifierName("ServiceDescriptor"),
                               GenericName(lifetime)
                                  .WithTypeArgumentList(TypeArgumentList(SeparatedList([serviceTypeSyntax, implementationTypeSyntax])))
                           )
                       ),
                   (true, TypeOfExpressionSyntax { Type: { } serviceTypeSyntax }, SimpleLambdaExpressionSyntax { ExpressionBody: { } })
                       when !IsOpenGenericType(serviceType) =>
                       InvocationExpression(
                               MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   IdentifierName("ServiceDescriptor"),
                                   GenericName(lifetime).WithTypeArgumentList(TypeArgumentList(SeparatedList([serviceTypeSyntax])))
                               )
                           )
                          .WithArgumentList(ArgumentList(SeparatedList([Argument(implementationTypeExpression)]))),
                   _ => InvocationExpression(
                           MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("ServiceDescriptor"), IdentifierName(lifetime))
                       )
                      .WithArgumentList(ArgumentList(SeparatedList([Argument(serviceTypeExpression), Argument(implementationTypeExpression!)]))),
               };
    }

    public static bool IsOpenGenericType(this INamedTypeSymbol type) =>
        type.IsGenericType && ( type.IsUnboundGenericType || type.TypeArguments.All(z => z.TypeKind == TypeKind.TypeParameter) );

    public static IEnumerable<MemberDeclarationSyntax> AssemblyDeclaration(IAssemblySymbol symbol)
    {
        var name = AssemblyVariableName(symbol);
        var assemblyName = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(symbol.Identity.GetDisplayName(true)));

        yield return FieldDeclaration(
                VariableDeclaration(IdentifierName("Assembly"))
                   .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier($"_{name}"))
                        )
                    )
            )
           .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        yield return PropertyDeclaration(IdentifierName("Assembly"), Identifier(name))
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                    .WithExpressionBody(
                         ArrowExpressionClause(
                             AssignmentExpression(
                                 SyntaxKind.CoalesceAssignmentExpression,
                                 IdentifierName(Identifier($"_{name}")),
                                 InvocationExpression(
                                         MemberAccessExpression(
                                             SyntaxKind.SimpleMemberAccessExpression,
                                             IdentifierName("_context"),
                                             IdentifierName("LoadFromAssemblyName")
                                         )
                                     )
                                    .WithArgumentList(
                                         ArgumentList(
                                             SingletonSeparatedList(
                                                 Argument(
                                                     ObjectCreationExpression(IdentifierName("AssemblyName"))
                                                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(assemblyName))))
                                                 )
                                             )
                                         )
                                     )
                             )
                         )
                     )
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }


    public static ExpressionSyntax? GetAssemblyExpression(Compilation compilation, IAssemblySymbol assembly) =>
        FindTypeInAssembly.FindType(compilation, assembly) is { } keyholdType
            ? MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                GetTypeOfExpression(compilation, keyholdType),
                IdentifierName("Assembly")
            )
            : null;

    public static ExpressionSyntax GetTypeOfExpression(Compilation compilation, INamedTypeSymbol type)
    {
        if (type.IsGenericType && type.IsOpenGenericType()) return getPrivateType(compilation, type);

        return !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly) && type.IsGenericType && !type.IsOpenGenericType()
            ? InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, getPrivateType(compilation, type.ConstructUnboundGenericType()), IdentifierName("MakeGenericType")))
               .WithArgumentList(
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    ArgumentList(SeparatedList(type.TypeArguments.Select(t => Argument(getPrivateType(compilation, ( t as INamedTypeSymbol )!)))))
                )
            : getPrivateType(compilation, type);

        static ExpressionSyntax getPrivateType(Compilation compilation, INamedTypeSymbol type)
        {
            return compilation.IsSymbolAccessibleWithin(type, compilation.Assembly) ? TypeOfExpression(ParseTypeName(Helpers.GetTypeOfName(type))) : InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, GetPrivateAssembly(type.ContainingAssembly), IdentifierName("GetType"))
                )
               .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Helpers.GetFullMetadataName(type)))))
                    )
                );
        }
    }


    public static NameSyntax GetPrivateAssembly(IAssemblySymbol type) => IdentifierName(AssemblyVariableName(type));

    public static string AssemblyVariableName(IAssemblySymbol symbol) => SpecialCharacterRemover.Replace(symbol.MetadataName, "");

    public static ExpressionSyntax GetTypeOfExpression(
        Compilation compilation,
        INamedTypeSymbol type,
        INamedTypeSymbol relatedType
    )
    {
        if (!type.IsUnboundGenericType)
        {
            return GetTypeOfExpression(compilation, type);
        }

        if (relatedType.IsGenericType && relatedType.Arity == type.Arity)
        {
            type = type.Construct([.. relatedType.TypeArguments]);
        }
        else
        {
            var baseType = Helpers
                          .GetBaseTypes(compilation, type)
                          .FirstOrDefault(z => z.IsGenericType && compilation.HasImplicitConversion(z, relatedType));
            // ReSharper disable once AccessToModifiedClosure
            baseType ??= type.AllInterfaces.FirstOrDefault(z => z.IsGenericType && compilation.HasImplicitConversion(z, relatedType));

            if (baseType is { })
            {
                type = type.Construct([.. baseType.TypeArguments]);
            }
        }

        return GetTypeOfExpression(compilation, type);
    }

    private static readonly Regex SpecialCharacterRemover = new("[^\\w\\d]", RegexOptions.Compiled);
}
