using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal record AllAssemblyDescriptor : IAssemblyDescriptor
{
    public override string ToString() => "All";
}
