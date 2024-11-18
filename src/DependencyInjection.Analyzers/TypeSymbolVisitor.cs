using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal class TypeSymbolVisitor(Compilation compilation) : SymbolVisitor
{
    public static ImmutableArray<INamedTypeSymbol> GetTypes(Compilation compilation)
    {
        var visitor = new TypeSymbolVisitor(compilation);
        visitor.VisitNamespace(compilation.GlobalNamespace);
        foreach (var symbol in compilation.References.Select(compilation.GetAssemblyOrModuleSymbol).Where(z => z != null))
            symbol?.Accept(visitor);
        return visitor.GetTypes();
    }

    public static ImmutableArray<INamedTypeSymbol> GetTypes(Compilation compilation, IEnumerable<IAssemblySymbol> symbols)
    {
        var visitor = new TypeSymbolVisitor(compilation);
        visitor.Accept(symbols);
        return visitor.GetTypes();
    }

    private readonly List<INamedTypeSymbol> _types = [];

    private void Accept<T>(IEnumerable<T> members)
        where T : ISymbol?
    {
        foreach (var member in members)
        {
            member?.Accept(this);
        }
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        Accept(symbol.GetMembers());
    }

    public override void VisitAssembly(IAssemblySymbol symbol)
    {
        symbol.GlobalNamespace.Accept(this);
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        if (symbol.TypeKind is TypeKind.Class or TypeKind.Delegate or TypeKind.Struct)
        {
            if (symbol.IsAbstract || !symbol.CanBeReferencedByName) return;
            if (Helpers
               .GetBaseTypes(compilation, symbol)
               .Contains(compilation.GetTypeByMetadataName("System.Attribute"), SymbolEqualityComparer.Default)) return;
            _types.Add(symbol);
        }

        Accept(symbol.GetMembers());
    }

    public ImmutableArray<INamedTypeSymbol> GetTypes() => _types.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToImmutableArray();
}
