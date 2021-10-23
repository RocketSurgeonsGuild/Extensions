using System.Reflection;
using System.Runtime.Loader;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public class CollectibleTestAssemblyLoadContext : AssemblyLoadContext, IDisposable
{
    public CollectibleTestAssemblyLoadContext() : base(
#if NETCOREAPP3_1 || NET5_0
        true
#endif

    )
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }

    public void Dispose()
    {
#if NETCOREAPP3_1
            Unload();
#endif
    }
}
