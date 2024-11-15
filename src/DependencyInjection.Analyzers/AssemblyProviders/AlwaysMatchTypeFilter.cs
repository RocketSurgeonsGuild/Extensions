using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal class AlwaysMatchTypeFilter<TSymbol> : ICompiledTypeFilter<TSymbol>
{
    public bool IsMatch(Compilation compilation, TSymbol targetType)
    {
        return true;
    }
}
