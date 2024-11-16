using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal interface ICompiledTypeFilter<TSymbol>
{
    bool IsMatch(Compilation compilation, TSymbol targetType);
}