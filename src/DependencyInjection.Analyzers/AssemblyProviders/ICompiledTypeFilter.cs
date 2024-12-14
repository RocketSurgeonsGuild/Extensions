using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal interface ICompiledTypeFilter<in TSymbol>
{
    string Hash { get; }
    bool IsMatch(Compilation compilation, TSymbol targetType);
}
