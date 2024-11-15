using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal abstract class TypeSymbolVisitorBase
    (Compilation compilation, ICompiledTypeFilter<IAssemblySymbol> assemblyFilter, ICompiledTypeFilter<INamedTypeSymbol> typeFilter) : SymbolVisitor
{
    private bool _abort;

    private void Accept<T>(IEnumerable<T> members)
        where T : ISymbol?
    {
        foreach (var member in members)
        {
            if (_abort) return;
            member?.Accept(this);
        }
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        if (_abort) return;
        Accept(symbol.GetMembers());
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        if (_abort) return;
        if (!assemblyFilter.IsMatch(compilation, symbol)) return;
        symbol.GlobalNamespace.Accept(this);
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        if (_abort) return;
        if (symbol.CanBeReferencedByName && typeFilter.IsMatch(compilation, symbol)) _abort = FoundNamedType(symbol);

        if (_abort) return;
        Accept(symbol.GetMembers());
    }

    protected abstract bool FoundNamedType(INamedTypeSymbol symbol);
}