using System.Collections.Immutable;
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
              .Where(z => z is { Right: true, Left: { method: { }, selector: { } } })
              .Select((tuple, _) => tuple.Left)
              .Collect();
    }

    public static ResolvedSourceLocation? ResolveSource(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        Item item,
        HashSet<IAssemblySymbol> privateAssemblies,
        Func<Compilation, TypeSymbolVisitor, TypeSymbolVisitor> visitorFactory
    )
    {
        try
        {
            var pa = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
            var visitor = visitorFactory(compilation, new(compilation, item.AssemblyFilter, item.TypeFilter));
            var reducedTypes = visitor.GetTypes();
            if (reducedTypes.Count == 0) return null;
            var localBlock = GenerateDescriptors(compilation, reducedTypes, pa)
                            .NormalizeWhitespace()
                            .ToFullString();
            privateAssemblies.UnionWith(pa);
            return new(item.Location, localBlock, pa.Select(z => z.MetadataName).ToImmutableHashSet());
        }
        catch (Exception e)
        {
            diagnostics.Add(
                Diagnostic.Create(
                    Diagnostics.UnhandledException,
                    null,
                    e.Message,
                    e.StackTrace.Replace("\r", "").Replace("\n", ""),
                    e.GetType().Name,
                    e.ToString()
                )
            );
            return null;
        }
    }

    public static ImmutableList<ResolvedSourceLocation> ResolveSources(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        IReadOnlyList<Item> items,
        HashSet<IAssemblySymbol> privateAssemblies,
        Func<Compilation, TypeSymbolVisitor, TypeSymbolVisitor> visitorFactory
    )
    {
        if (!items.Any()) return [];
        var results = new List<ResolvedSourceLocation>();
        foreach (var item in items)
        {
            var resolved = ResolveSource(compilation, diagnostics, item, privateAssemblies, visitorFactory);
            if (resolved is { }) results.Add(resolved);
        }

        return results.ToImmutableList();
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel ) GetTypesMethod(GeneratorSyntaxContext context)
    {
        var baseData = GetTypesMethod(context.Node);
        if (baseData.method is null
         || baseData.selector is null
         || context.SemanticModel.GetTypeInfo(baseData.selector).ConvertedType is not INamedTypeSymbol
            {
                TypeArguments: [{ Name: IReflectionTypeSelector }, ..],
            })
            return default;

        return ( baseData.method, baseData.selector, semanticModel: context.SemanticModel );
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector ) GetTypesMethod(SyntaxNode node) =>
        node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "GetTypes" },
            ArgumentList.Arguments: [.., { Expression: { } expression }],
        } invocationExpressionSyntax
            ? ( invocationExpressionSyntax, expression )
            : default;

    internal static ImmutableList<Item> GetReflectionItems(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        IReadOnlyList<(InvocationExpressionSyntax expression, ExpressionSyntax selector, SemanticModel semanticModel)> results,
        CancellationToken cancellationToken
    )
    {
        var items = ImmutableList.CreateBuilder<Item>();
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
                    diagnostics,
                    compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                    selector,
                    assemblies,
                    typeFilters,
                    new(),
                    ref lifetime,
                    ref classFilter,
                    cancellationToken
                );

                var source = Helpers.CreateSourceLocation(methodCallSyntax, cancellationToken);
                var assemblyFilter = new CompiledAssemblyFilter(assemblies.ToImmutableList(), source);
                var typeFilter = new CompiledTypeFilter(classFilter, typeFilters.ToImmutableList(), source);


                var i = new Item(source, assemblyFilter, typeFilter);
                items.Add(i);
            }
            catch (MustBeAnExpressionException e)
            {
                diagnostics.Add(Diagnostic.Create(Diagnostics.MustBeAnExpression, e.Location));
            }
            catch (Exception e)
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.UnhandledException,
                        null,
                        e.Message,
                        e.StackTrace.Replace("\r", "").Replace("\n", ""),
                        e.GetType().Name,
                        e.ToString()
                    )
                );
            }
        }

        return items.ToImmutable();
    }

    private static bool IsValidMethod(SyntaxNode node) => GetTypesMethod(node) is { method: { }, selector: { } };

    private static BlockSyntax GenerateDescriptors(Compilation compilation, IEnumerable<INamedTypeSymbol> types, HashSet<IAssemblySymbol> privateAssemblies)
    {
        var block = Block();
        foreach (var type in types.OrderBy(z => z.ToDisplayString()))
        {
            block = block.AddStatements(
                YieldStatement(SyntaxKind.YieldReturnStatement, StatementGeneration.GetTypeOfExpression(compilation, type))
            );
            if (compilation.IsSymbolAccessibleWithin(type, compilation.Assembly)) continue;
            privateAssemblies.Add(type.ContainingAssembly);
        }

        return block;
    }


    private const string IReflectionTypeSelector = nameof(IReflectionTypeSelector);

    public record Item(SourceLocation Location, CompiledAssemblyFilter AssemblyFilter, CompiledTypeFilter TypeFilter);
}
