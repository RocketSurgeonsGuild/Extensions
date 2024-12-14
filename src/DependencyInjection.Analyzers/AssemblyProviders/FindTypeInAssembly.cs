using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal class FindTypeInAssembly(Compilation compilation, ICompiledTypeFilter<IAssemblySymbol> assemblyFilter) : TypeSymbolVisitorBase(
    compilation,
    assemblyFilter,
    new CompiledTypeFilter(ClassFilter.PublicOnly, ImmutableList<ITypeFilterDescriptor>.Empty)
)
{
    public static INamedTypeSymbol? FindType(Compilation compilation, IAssemblySymbol assemblySymbol)
    {
        var visitor = new FindTypeInAssembly(
            compilation,
            new CompiledAssemblyFilter(ImmutableList.Create<IAssemblyDescriptor>(new AssemblyDescriptor(assemblySymbol)))
        );
        visitor.Visit(assemblySymbol);
        return visitor._type;
    }

    private INamedTypeSymbol? _type;

    protected override bool FoundNamedType(INamedTypeSymbol symbol)
    {
        _type = symbol;
        return true;
    }
}
