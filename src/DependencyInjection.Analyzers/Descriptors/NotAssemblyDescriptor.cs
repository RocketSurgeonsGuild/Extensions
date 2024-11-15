using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct NotAssemblyDescriptor(IAssemblySymbol Assembly) : IAssemblyDescriptor
{
    public override string ToString()
    {
        return "CompiledAssembly of " + Assembly.Name;
    }
}
