using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal record AssemblyDependenciesDescriptor(IAssemblySymbol Assembly) : IAssemblyDescriptor
{
    public override string ToString() => "CompiledAssemblyDependencies of " + Assembly.Name;
}
