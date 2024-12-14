using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal class TypeSymbolVisitor
    (Compilation compilation, ICompiledTypeFilter<IAssemblySymbol> assemblyFilter, ICompiledTypeFilter<INamedTypeSymbol> typeFilter)
    : TypeSymbolVisitorBase(compilation, assemblyFilter, typeFilter)
{
    private readonly HashSet<INamedTypeSymbol> _types = new(SymbolEqualityComparer.Default);

    protected override bool FoundNamedType(INamedTypeSymbol symbol)
    {
        _types.Add(symbol);
        return false;
    }

    public ImmutableList<INamedTypeSymbol> GetTypes() => _types.ToImmutableList();
}

internal static class TypeSymbolVisitorExtensions
{
    public static ImmutableList<INamedTypeSymbol> GetTypes(this TypeSymbolVisitor visitor, Compilation compilation)
    {
        GetReferencedTypes(visitor, compilation);
        GetCompilationTypes(visitor, compilation);
        return visitor.GetTypes(compilation);
    }

    public static TypeSymbolVisitor GetReferencedTypes(this TypeSymbolVisitor visitor, Compilation compilation)
    {
        foreach (var symbol in compilation.References.Select(compilation.GetAssemblyOrModuleSymbol))
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

        return visitor;
    }

    public static TypeSymbolVisitor GetCompilationTypes(this TypeSymbolVisitor visitor, Compilation compilation)
    {
        compilation.Assembly.Accept(visitor);
        return visitor;
    }
}
