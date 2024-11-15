using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct IncludeSystemAssembliesDescriptor : IAssemblyDescriptor
{
    public override string ToString()
    {
        return "IncludeSystemAssemblies";
    }
}
