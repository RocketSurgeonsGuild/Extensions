﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class ReflectionCollection
{
    public static IncrementalValueProvider<ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)>> Create(
        SyntaxValueProvider valueProvider,
        IncrementalValueProvider<bool> hasAssemblyLoadContext
    )
    {
        return valueProvider
              .CreateSyntaxProvider((node, _) => IsValidMethod(node), (syntaxContext, _) => GetTypesMethod(syntaxContext))
              .Combine(hasAssemblyLoadContext)
              .Where(z => z is { Right: true, Left: { method: { }, selector: { }, }, })
              .Select((tuple, _) => tuple.Left)
              .Collect();
    }

    public static MethodDeclarationSyntax Execute(Request request)
    {
        if (!request.Items.Any()) return TypesMethod;
        var compilation = request.Compilation;

        var results = new List<(SourceLocation location, BlockSyntax block)>();
        foreach (var item in request.Items)
        {
            var reducedTypes = AssemblyProviders.TypeSymbolVisitor.GetTypes(compilation, item.AssemblyFilter, item.TypeFilter);
            if (reducedTypes.Length == 0) continue;
            var localBlock = GenerateDescriptors(compilation, reducedTypes, request.PrivateAssemblies);
            results.Add(( item.Location, localBlock ));
        }

        return results.Count == 0 ? TypesMethod : TypesMethod.WithBody(Block(SwitchGenerator.GenerateSwitchStatement(results)));
    }

    private static bool IsValidMethod(SyntaxNode node)
    {
        return GetTypesMethod(node) is { method: { }, selector: { } };
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel ) GetTypesMethod(GeneratorSyntaxContext context)
    {
        var baseData = GetTypesMethod(context.Node);
        if (baseData.method is null
         || baseData.selector is null
         || context.SemanticModel.GetTypeInfo(baseData.selector).ConvertedType is not INamedTypeSymbol
            {
                TypeArguments: [{ Name: IReflectionAssemblySelector, }, ..,],
            })
            return default;

        return ( baseData.method, baseData.selector, semanticModel: context.SemanticModel );
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector ) GetTypesMethod(SyntaxNode node)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: "GetTypes",
            },
            ArgumentList.Arguments: [.., { Expression: { } expression, },],
        } invocationExpressionSyntax
            ? ( invocationExpressionSyntax, expression )
            : default;
    }

    internal static ImmutableArray<Item> GetTypeDetails(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<(InvocationExpressionSyntax expression, ExpressionSyntax selector, SemanticModel semanticModel)> results
    )
    {
        var items = ImmutableArray.CreateBuilder<Item>();
        foreach (var tuple in results)
        {
            try
            {
                ( var methodCallSyntax, var selector, _ ) = tuple;

                var assemblies = new List<IAssemblyDescriptor>();
                var typeFilters = new List<ITypeFilterDescriptor>();
                var classFilter = ClassFilter.All;
                var lifetime = 2;

                DataHelpers.HandleInvocationExpressionSyntax(
                    context,
                    compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                    selector,
                    assemblies,
                    typeFilters,
                    new(),
                    ref lifetime,
                    compilation.ObjectType,
                    ref classFilter,
                    context.CancellationToken
                );

                var assemblyFilter = new CompiledAssemblyFilter(assemblies.ToImmutableArray());
                var typeFilter = new CompiledTypeFilter(classFilter, typeFilters.ToImmutableArray());

                var source = Helpers.CreateSourceLocation(methodCallSyntax, context.CancellationToken);

                var i = new Item(source, assemblyFilter, typeFilter);
                items.Add(i);
            }
            catch (MustBeAnExpressionException e)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAnExpression, e.Location));
            }
        }

        return items.ToImmutable();
    }

    private static BlockSyntax GenerateDescriptors(Compilation compilation, IEnumerable<INamedTypeSymbol> types, HashSet<IAssemblySymbol> privateAssemblies)
    {
        var block = Block();
        foreach (var type in types.OrderBy(z => z.ToDisplayString()))
        {
            block = block.AddStatements(
                YieldStatement(SyntaxKind.YieldReturnStatement, AssemblyProviders.StatementGeneration.GetTypeOfExpression(compilation, type))
            );
            if (compilation.IsSymbolAccessibleWithin(type, compilation.Assembly)) continue;
            privateAssemblies.Add(type.ContainingAssembly);
        }

        return block;
    }


    private static readonly MethodDeclarationSyntax TypesMethod = MethodDeclaration(
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
                                                                                      IdentifierName(IReflectionAssemblySelector),
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

    public record Request(SourceProductionContext Context, Compilation Compilation, ImmutableArray<Item> Items, HashSet<IAssemblySymbol> PrivateAssemblies);

    public record Item(SourceLocation Location, CompiledAssemblyFilter AssemblyFilter, CompiledTypeFilter TypeFilter);

    private const string IReflectionAssemblySelector = nameof(IReflectionAssemblySelector);
}