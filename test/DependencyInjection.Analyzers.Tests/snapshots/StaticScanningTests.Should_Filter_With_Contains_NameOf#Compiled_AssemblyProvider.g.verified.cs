//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoiUUJuWnhOWVJhTUxoXHUwMDJCdG5yTDVOZG5nPT0iLCJmIjoiSW5wdXQwLmNzIn0sImEiOnsiYSI6dHJ1ZSwiaSI6ZmFsc2UsIm0iOltdLCJuYSI6W10sImQiOltdfSwidCI6eyJiIjoxLCJjIjpbXSwiZCI6W3siaSI6dHJ1ZSwiZiI6MiwibiI6WyJGYWN0b3J5Il19XSwiZSI6W3siZiI6dHJ1ZSwidCI6WzJdfV0sImYiOlt7ImYiOmZhbHNlLCJ0IjpbLTIsMV19XSwiZyI6W10sImgiOltdLCJpIjpbXSwiaiI6W10sImsiOlt7ImkiOnRydWUsImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZSIsInUiOmZhbHNlfSx7ImkiOnRydWUsImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZUIiLCJ1IjpmYWxzZX1dLCJsIjpbXX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6InMiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH0seyJJZGVudGlmaWVyIjoiaSIsIlR5cGVEYXRhIjpudWxsLCJUeXBlRmlsdGVyIjpudWxsfV0sIkxpZmV0aW1lIjoxfSwieiI6MX0=")]
[assembly: Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute(typeof(CompiledTypeProvider))]
[System.CodeDom.Compiler.GeneratedCode("Rocket.Surgery.DependencyInjection.Analyzers", "version"), System.Runtime.CompilerServices.CompilerGenerated, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
file class CompiledTypeProvider : ICompiledTypeProvider
{
    IEnumerable<Assembly> ICompiledTypeProvider.GetAssemblies(Action<IReflectionAssemblySelector> action, int lineNumber, string filePath, string argumentExpression)
    {
        yield break;
    }

    IEnumerable<Type> ICompiledTypeProvider.GetTypes(Func<IReflectionTypeSelector, IEnumerable<Type>> selector, int lineNumber, string filePath, string argumentExpression)
    {
        yield break;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: QBnZxNYRaMLh+tnrL5Ndng==
            case 20:
                services.Add(ServiceDescriptor.Scoped<global::ServFactoryice, global::ServFactoryice>());
                services.Add(ServiceDescriptor.Scoped<global::IService>(a => a.GetRequiredService<global::ServFactoryice>()));
                services.Add(ServiceDescriptor.Scoped<global::IServiceB>(a => a.GetRequiredService<global::ServFactoryice>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
