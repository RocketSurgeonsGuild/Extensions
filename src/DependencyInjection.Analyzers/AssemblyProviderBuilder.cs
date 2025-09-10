using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class AssemblyProviderBuilder
{
    public static TypeDeclarationSyntax GetAssemblyProvider(
        ImmutableList<ResolvedSourceLocation> assemblyRequests,
        ImmutableList<ResolvedSourceLocation> reflectionRequests,
        ImmutableList<ResolvedSourceLocation> serviceDescriptorRequests,
        HashSet<IAssemblySymbol> privateAssemblies,
        out string cacheHash
    )
    {
        using var hasher = MD5.Create();

        static void addStringToHash(ICryptoTransform hasher, string textToHash)
        {
            var inputBuffer = Encoding.UTF8.GetBytes(textToHash);
            hasher.TransformBlock(inputBuffer, 0, inputBuffer.Length, inputBuffer, 0);
        }

        static IEnumerable<T> dohash<T>(ICryptoTransform hasher, IEnumerable<T> enumerable, JsonTypeInfo<T> jsonTypeInfo)
        {
            foreach (var item in enumerable)
            {
                addStringToHash(hasher, JsonSerializer.Serialize(item, jsonTypeInfo));
                yield return item;
            }
        }

        var resolvedAssemblyDetails = assemblyRequests is { Count: > 0 }
            ? GenerateMethodBody(AssembliesMethod, dohash(hasher, assemblyRequests, JsonSourceGenerationContext.Default.ResolvedSourceLocation))
            : AssembliesMethod;

        var resolvedReflectionDetails = reflectionRequests is { Count: > 0 }
            ? GenerateMethodBody(TypesMethod, dohash(hasher, reflectionRequests, JsonSourceGenerationContext.Default.ResolvedSourceLocation))
            : TypesMethod;

        var resolvedServiceDescriptorDetails = serviceDescriptorRequests is { Count: > 0 }
            ? GenerateMethodBody(ScanMethod, dohash(hasher, serviceDescriptorRequests, JsonSourceGenerationContext.Default.ResolvedSourceLocation))
            : ScanMethod;

        var privateMembers = privateAssemblies
                            .OrderBy(
                                 z =>
                                 {
                                     addStringToHash(hasher, z.MetadataName);
                                     return z.MetadataName;
                                 }
                             )
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

        _ = hasher.TransformFinalBlock([], 0, 0);
        cacheHash = Convert.ToBase64String(hasher.Hash);
        return ClassDeclaration("CompiledTypeProvider")
              .AddAttributeLists(Helpers.CompilerGeneratedAttributes)
              .WithModifiers(TokenList(Token(SyntaxKind.FileKeyword)))
              .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName("ICompiledTypeProvider")))))
              .AddMembers(resolvedAssemblyDetails, resolvedReflectionDetails, resolvedServiceDescriptorDetails)
              .AddMembers([.. privateMembers]);
    }

    private static MethodDeclarationSyntax GenerateMethodBody(MethodDeclarationSyntax baseMethod, IEnumerable<ResolvedSourceLocation> locations)
    {
        StatementSyntax[] item = [.. baseMethod.Body?.Statements ?? []];
        var returnStatement = item.OfType<ReturnStatementSyntax>().Single();

        return baseMethod
           .WithBody(
                Block(
                    SyntaxList.Create(
                        [
                            ..item.Except([returnStatement]),
                            SwitchGenerator.GenerateSwitchStatement(
                                [
                                    ..locations
                                     .GroupBy(z => z.Location)
                                     .Select(
                                          z => z.Aggregate(
                                              new ResolvedSourceLocation(z.First().Location, "", [], null),
                                              (location, sourceLocation) => new(
                                                  location.Location,
                                                  location.Expression + "\n" + sourceLocation.Expression,
                                                  [..location.PrivateAssemblies, ..sourceLocation.PrivateAssemblies],
                                                  null
                                              )
                                          )
                                      ),
                                ]
                            ),
                            returnStatement,
                        ]
                    )
                )
            );
    }

    private static StatementSyntax GetCollectionVariable(TypeSyntax type) => LocalDeclarationStatement(
        VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
           .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(Identifier("items"))
                       .WithInitializer(
                            EqualsValueClause(
                                ObjectCreationExpression(
                                        GenericName(Identifier("List"))
                                           .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(type)))
                                    )
                                   .WithArgumentList(ArgumentList())
                            )
                        )
                )
            )
    );

    private const string IReflectionAssemblySelector = nameof(IReflectionAssemblySelector);

    private const string IReflectionTypeSelector = nameof(IReflectionTypeSelector);
    private const string IServiceDescriptorAssemblySelector = nameof(IServiceDescriptorAssemblySelector);

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
                Block(
                    GetCollectionVariable(IdentifierName("Type")),
                    ReturnStatement(IdentifierName("items"))
                )
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
           .WithBody(
                Block(
                    GetCollectionVariable(IdentifierName("Assembly")),
                    ReturnStatement(IdentifierName("items"))
                )
            );
}
