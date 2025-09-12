using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal class TypeSymbolVisitor
    (Compilation compilation, ICompiledTypeFilter<IAssemblySymbol> assemblyFilter, ICompiledTypeFilter<INamedTypeSymbol> typeFilter)
    : TypeSymbolVisitorBase(compilation, assemblyFilter, typeFilter)
{
    public ImmutableList<INamedTypeSymbol> GetTypes() => [.. _types];

    protected override bool FoundNamedType(INamedTypeSymbol symbol)
    {
        _types.Add(symbol);
        return false;
    }

    private readonly HashSet<INamedTypeSymbol> _types = new(SymbolEqualityComparer.Default);
}

internal static class TypeSymbolVisitorExtensions
{
    public static TypeSymbolVisitor GetReferencedTypes(this TypeSymbolVisitor visitor, Compilation compilation)
    {
        foreach (var symbol in compilation.References.Select(compilation.GetAssemblyOrModuleSymbol))
        {
            switch (symbol)
            {
                case IAssemblySymbol:
                    {
                        symbol.Accept(visitor);
                        break;
                    }

                case IModuleSymbol:
                    {
                        symbol.Accept(visitor);
                        break;
                    }

                default:
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

    public static TypeSymbolVisitor GetReferencedTypes(this TypeSymbolVisitor visitor, IAssemblySymbol assemblySymbol)
    {
        assemblySymbol.Accept(visitor);
        return visitor;
    }
}
