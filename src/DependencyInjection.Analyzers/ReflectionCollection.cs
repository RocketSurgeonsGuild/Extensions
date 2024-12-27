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
    ) => valueProvider
        .CreateSyntaxProvider((node, _) => IsValidMethod(node), (syntaxContext, _) => GetTypesMethod(syntaxContext))
        .Combine(hasAssemblyLoadContext)
        .Where(z => z is { Right: true, Left: { method: { }, selector: { } } })
        .Select((tuple, _) => tuple.Left)
        .Collect();

    public static ResolvedSourceLocation? ResolveSource(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        Item item,
        IAssemblySymbol targetAssembly
    )
    {
        try
        {
            var pa = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
            var reducedTypes = new TypeSymbolVisitor(compilation, item.AssemblyFilter, item.TypeFilter)
                              .GetReferencedTypes(targetAssembly)
                              .GetTypes();
            if (reducedTypes.Count == 0)
            {
                return null;
            }

            var localBlock = GenerateDescriptors(compilation, reducedTypes, pa).NormalizeWhitespace().ToFullString().Replace("\r", "");
            return new(item.Location, localBlock, pa.Select(z => z.MetadataName).ToImmutableHashSet());
        }
        catch (Exception e)
        {
            _ = diagnostics.Add(
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
        IAssemblySymbol targetAssembly
    )
    {
        if (!items.Any())
        {
            return [];
        }

        var results = new List<ResolvedSourceLocation>();
        foreach (var item in items)
        {
            if (ResolveSource(compilation, diagnostics, item, targetAssembly) is not { } location)
            {
                continue;
            }

            results.Add(location);
        }

        return [.. results];
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel) GetTypesMethod(GeneratorSyntaxContext context)
    {
        ( var method, var selector ) = GetTypesMethod(context.Node);
        return method is null
         || selector is null
         || context.SemanticModel.GetTypeInfo(selector).ConvertedType is not INamedTypeSymbol
            {
                TypeArguments: [{ Name: IReflectionTypeSelector }, ..],
            }
                ? default
                : ( method, selector, semanticModel: context.SemanticModel );
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector) GetTypesMethod(SyntaxNode node) =>
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
                    [],
                    ref lifetime,
                    ref classFilter,
                    cancellationToken
                );

                var source = Helpers.CreateSourceLocation(SourceLocationKind.Reflection, methodCallSyntax, cancellationToken);
                var assemblyFilter = new CompiledAssemblyFilter([.. assemblies], source);
                var typeFilter = new CompiledTypeFilter(classFilter, [.. typeFilters], source);

                var i = new Item(source, assemblyFilter, typeFilter);
                items.Add(i);
            }
            catch (MustBeAnExpressionException e)
            {
                _ = diagnostics.Add(Diagnostic.Create(Diagnostics.MustBeAnExpression, e.Location));
            }
            catch (Exception e)
            {
                _ = diagnostics.Add(
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
                ExpressionStatement(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("items"), IdentifierName("Add")))
                       .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(StatementGeneration.GetTypeOfExpression(compilation, type)))))
                )
            );
            if (compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
            {
                continue;
            }

            _ = privateAssemblies.Add(type.ContainingAssembly);
        }

        return block;
    }


    private const string IReflectionTypeSelector = nameof(IReflectionTypeSelector);

    public record Item(SourceLocation Location, CompiledAssemblyFilter AssemblyFilter, CompiledTypeFilter TypeFilter);
}
