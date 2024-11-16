using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct AssemblyDependenciesDescriptor(IAssemblySymbol Assembly) : IAssemblyDescriptor
{
    public override string ToString()
    {
        return "CompiledAssemblyDependencies of " + Assembly.Name;
    }
}
