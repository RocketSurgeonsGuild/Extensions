using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class AssemblyCollection
{
    public static IncrementalValueProvider<ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)>> Create(
        SyntaxValueProvider valueProvider,
        IncrementalValueProvider<bool> hasAssemblyLoadContext
    )
    {
        return valueProvider
              .CreateSyntaxProvider((node, _) => IsValidMethod(node), (syntaxContext, _) => GetMethod(syntaxContext))
              .Combine(hasAssemblyLoadContext)
              .Where(z => z is { Right: true, Left: { method: { }, selector: { } } })
              .Select((tuple, _) => tuple.Left)
              .Collect();
    }

    public static ImmutableList<ResolvedSourceLocation> ResolveSources(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        IReadOnlyList<Item> items,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        var assemblySymbols = compilation
                             .References.Select(compilation.GetAssemblyOrModuleSymbol)
                             .Concat([compilation.Assembly])
                             .Select(
                                  symbol => symbol switch
                                            {
                                                IAssemblySymbol assemblySymbol => assemblySymbol,
                                                IModuleSymbol moduleSymbol     => moduleSymbol.ContainingAssembly,
                                                _                              => null!,
                                            }
                              )
                             .Where(z => z is { })
                             .ToImmutableHashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);

        var results = new List<ResolvedSourceLocation>();
        foreach (var item in items)
        {
            var pa = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
            try
            {
                var filterAssemblies = assemblySymbols
                                      .Where(z => item.AssemblyFilter.IsMatch(compilation, z))
                                      .ToArray();

                if (filterAssemblies.Length == 0)
                {
                    continue;
                }

                results.Add(
                    new(
                        item.Location,
                        GenerateDescriptors(compilation, filterAssemblies, pa).NormalizeWhitespace().ToFullString(),
                        pa.Select(z => z.MetadataName).ToImmutableHashSet()
                    )
                );
                privateAssemblies.UnionWith(pa);
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

        return results.ToImmutableList();
//        .WithBody(Block(SwitchGenerator.GenerateSwitchStatement(results)));
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel) GetMethod(
        GeneratorSyntaxContext context
    )
    {
        var (method, selector) = GetMethod(context.Node);
        if (method is null) return default;
        if (selector is null) return default;

        var convertType = context.SemanticModel.GetTypeInfo(selector).ConvertedType;
        return convertType is not INamedTypeSymbol { TypeArguments: [{ Name: IReflectionAssemblySelector }, ..] }
            ? default
            : ( method, selector, semanticModel: context.SemanticModel );
    }

    public static (InvocationExpressionSyntax? method, ExpressionSyntax? selector) GetMethod(SyntaxNode node) =>
        node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "GetAssemblies" },
            ArgumentList.Arguments: [{ Expression: { } expression }],
        } invocationExpressionSyntax
            ? ( invocationExpressionSyntax, expression )
            : default;

    private static bool IsValidMethod(SyntaxNode node) => GetMethod(node) is { method: { }, selector: { } };

    private static BlockSyntax GenerateDescriptors(Compilation compilation, IEnumerable<IAssemblySymbol> assemblies, HashSet<IAssemblySymbol> privateAssemblies)
    {
        var block = Block();
        foreach (var assembly in assemblies.OrderBy(z => z.ToDisplayString()))
        {
            // TODO: Make this always use the load context?
            if (StatementGeneration.GetAssemblyExpression(compilation, assembly) is not { } assemblyExpression)
            {
                privateAssemblies.Add(assembly);
                block = block.AddStatements(
                    YieldStatement(SyntaxKind.YieldReturnStatement, StatementGeneration.GetPrivateAssembly(assembly))
                );
                continue;
            }

            block = block.AddStatements(YieldStatement(SyntaxKind.YieldReturnStatement, assemblyExpression));
        }

        return block;
    }

    public static ImmutableList<Item> GetAssemblyItems(
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
                var (methodCallSyntax, selector, semanticModel) = tuple;

                var assemblies = new List<IAssemblyDescriptor>();
                var typeFilters = new List<ITypeFilterDescriptor>();
                var serviceDescriptors = new List<IServiceTypeDescriptor>();
                var lifetime = 2;
                var classFilter = ClassFilter.All;

                DataHelpers.HandleInvocationExpressionSyntax(
                    diagnostics,
                    compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                    selector,
                    assemblies,
                    typeFilters,
                    serviceDescriptors,
                    ref lifetime,
                    ref classFilter,
                    cancellationToken
                );

                var assemblyFilter = new CompiledAssemblyFilter([.. assemblies]);

                var source = Helpers.CreateSourceLocation(methodCallSyntax, cancellationToken);
                // disallow list?
                if (source.FileName == "ConventionContextHelpers.cs")
                {
                    continue;
                }

                var i = new Item(source, assemblyFilter);
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
                        e.StackTrace,
                        e.GetType().Name,
                        e.ToString()
                    )
                );
            }
        }

        return items.ToImmutable();
    }

    private const string IReflectionAssemblySelector = nameof(IReflectionAssemblySelector);

    public record Item(SourceLocation Location, CompiledAssemblyFilter AssemblyFilter);
}
