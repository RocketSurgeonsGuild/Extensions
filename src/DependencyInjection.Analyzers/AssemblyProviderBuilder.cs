using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class AssemblyProviderBuilder
{
    public static TypeDeclarationSyntax GetAssemblyProvider(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableList<ResolvedSourceLocation> assemblyRequests,
        ImmutableList<ResolvedSourceLocation> reflectionRequests,
        ImmutableList<ResolvedSourceLocation> serviceDescriptorRequests,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        var resolvedAssemblyDetails = ( assemblyRequests is { Count: > 0 } )
            ? GenerateMethodBody(AssembliesMethod, assemblyRequests)
            : AssembliesMethod;

        var resolvedReflectionDetails = ( reflectionRequests is { Count: > 0 } )
            ? GenerateMethodBody(TypesMethod, reflectionRequests)
            : TypesMethod;

        var resolvedServiceDescriptorDetails = ( serviceDescriptorRequests is { Count: > 0 } )
            ? GenerateMethodBody(ScanMethod, serviceDescriptorRequests)
            : ScanMethod;

        var privateMembers = privateAssemblies
                            .OrderBy(z => z.ToDisplayString())
                            .SelectMany(StatementGeneration.AssemblyDeclaration)
                            .ToList();
        if (privateAssemblies.Any())
        {
            privateMembers.Insert(
                0,
                FieldDeclaration(
                        VariableDeclaration(IdentifierName("AssemblyLoadContext"))
                           .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier("_context"))
                                       .WithInitializer(
                                            EqualsValueClause(
                                                PostfixUnaryExpression(
                                                    SyntaxKind.SuppressNullableWarningExpression,
                                                    InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("AssemblyLoadContext"),
                                                                IdentifierName("GetLoadContext")
                                                            )
                                                        )
                                                       .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList(
                                                                    Argument(
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            TypeOfExpression(IdentifierName("CompiledTypeProvider")),
                                                                            IdentifierName("Assembly")
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                )
                                            )
                                        )
                                )
                            )
                    )
                   .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            );
        }

        return ClassDeclaration("CompiledTypeProvider")
              .AddAttributeLists(Helpers.CompilerGeneratedAttributes)
              .WithModifiers(TokenList(Token(SyntaxKind.FileKeyword)))
              .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName("ICompiledTypeProvider")))))
              .AddMembers(resolvedAssemblyDetails, resolvedReflectionDetails, resolvedServiceDescriptorDetails)
              .AddMembers([.. privateMembers]);
    }

    private static MethodDeclarationSyntax GenerateMethodBody(MethodDeclarationSyntax baseMethod, IReadOnlyList<ResolvedSourceLocation> locations) => baseMethod.WithBody(
            Block(
                SwitchGenerator.GenerateSwitchStatement(
                    [
                        ..locations
                         .GroupBy(z => z.Location)
                         .Select(z => new ResolvedSourceLocation(z.Key, z.Aggregate("", (s, location) => s + "\n" + location.Expression), []))
                    ]
                )
            )
        )
       .AddBodyStatements([.. baseMethod.Body?.Statements ?? []]);


    private static readonly MethodDeclarationSyntax TypesMethod =
        MethodDeclaration(
                GenericName(Identifier("IEnumerable"))
                   .WithTypeArgumentList(
                        TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("Type")))
                    ),
                Identifier("GetTypes")
            )
           .WithExplicitInterfaceSpecifier(
                ExplicitInterfaceSpecifier(IdentifierName("ICompiledTypeProvider"))
            )
           .AddParameterListParameters(
                Parameter(Identifier("selector"))
                   .WithType(
                        GenericName(Identifier("Func"))
                           .AddTypeArgumentListArguments(
                                IdentifierName(IReflectionTypeSelector),
                                GenericName(Identifier("IEnumerable"))
                                   .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(IdentifierName("Type"))
                                        )
                                    )
                            )
                    ),
                Parameter(Identifier("lineNumber"))
                   .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
                Parameter(Identifier("filePath"))
                   .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                Parameter(Identifier("argumentExpression"))
                   .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            )
           .WithBody(
                Block(SingletonList<StatementSyntax>(YieldStatement(SyntaxKind.YieldBreakStatement)))
            );

    private static readonly MethodDeclarationSyntax ScanMethod =
        MethodDeclaration(ParseName("Microsoft.Extensions.DependencyInjection.IServiceCollection"), Identifier("Scan"))
           .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName("ICompiledTypeProvider")))
           .AddParameterListParameters(
                Parameter(Identifier("services")).WithType(ParseName("Microsoft.Extensions.DependencyInjection.IServiceCollection")),
                Parameter(Identifier("selector"))
                   .WithType(GenericName(Identifier("Action")).AddTypeArgumentListArguments(IdentifierName(IServiceDescriptorAssemblySelector))),
                Parameter(Identifier("lineNumber")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
                Parameter(Identifier("filePath")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                Parameter(Identifier("argumentExpression")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            )
           .WithBody(Block(ReturnStatement(IdentifierName("services"))));

    private const string IReflectionTypeSelector = nameof(IReflectionTypeSelector);
    private const string IServiceDescriptorAssemblySelector = nameof(IServiceDescriptorAssemblySelector);
    private const string IReflectionAssemblySelector = nameof(IReflectionAssemblySelector);

    private static readonly MethodDeclarationSyntax AssembliesMethod =
        MethodDeclaration(
                GenericName(Identifier("IEnumerable"))
                   .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("Assembly")))),
                Identifier("GetAssemblies")
            )
           .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName("ICompiledTypeProvider")))
           .AddParameterListParameters(
                Parameter(Identifier("action"))
                   .WithType(
                        GenericName(Identifier("Action"))
                           .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(IReflectionAssemblySelector)
                                    )
                                )
                            )
                    ),
                Parameter(Identifier("lineNumber")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
                Parameter(Identifier("filePath")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                Parameter(Identifier("argumentExpression")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            )
           .WithBody(Block(SingletonList<StatementSyntax>(YieldStatement(SyntaxKind.YieldBreakStatement))));
}
