using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal class TypeSymbolVisitor
    (Compilation compilation, ICompiledTypeFilter<IAssemblySymbol> assemblyFilter, ICompiledTypeFilter<INamedTypeSymbol> typeFilter)
    : TypeSymbolVisitorBase(compilation, assemblyFilter, typeFilter)
{
    public static ImmutableArray<INamedTypeSymbol> GetTypes(
        Compilation compilation,
        ICompiledTypeFilter<IAssemblySymbol> assemblyFilter,
        ICompiledTypeFilter<INamedTypeSymbol> typeFilter
    )
    {
        var visitor = new TypeSymbolVisitor(compilation, assemblyFilter, typeFilter);
        foreach (var symbol in compilation.References.Select(compilation.GetAssemblyOrModuleSymbol).Concat([compilation.Assembly,]))
        {
            switch (symbol)
            {
                case IAssemblySymbol:
                    symbol.Accept(visitor);
                    break;
                case IModuleSymbol:
                    symbol.Accept(visitor);
                    break;
            }
        }

        return visitor.GetTypes();
    }

    private readonly HashSet<INamedTypeSymbol> _types = new(SymbolEqualityComparer.Default);

    protected override bool FoundNamedType(INamedTypeSymbol symbol)
    {
        _types.Add(symbol);
        return false;
    }

    public ImmutableArray<INamedTypeSymbol> GetTypes()
    {
        return _types.ToImmutableArray();
    }
}