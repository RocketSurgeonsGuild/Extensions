using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal record IncludeSystemAssembliesDescriptor : IAssemblyDescriptor
{
    public override string ToString() => "IncludeSystemAssemblies";
}
