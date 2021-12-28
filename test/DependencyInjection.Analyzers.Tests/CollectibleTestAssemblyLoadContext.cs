using System.Reflection;
using System.Runtime.Loader;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

internal class CollectibleTestAssemblyLoadContext : AssemblyLoadContext, IDisposable
{
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
