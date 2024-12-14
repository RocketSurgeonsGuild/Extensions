//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoicXMwVnNjXHUwMDJCN1l6L3JZOXJrZGlxYkx3PT0iLCJmIjoiSW5wdXQwLmNzIn0sImEiOnsiYSI6dHJ1ZSwiaSI6ZmFsc2UsIm0iOltdLCJuYSI6W10sImQiOltdfSwidCI6eyJiIjoxLCJjIjpbXSwiZCI6W10sImUiOlt7ImYiOnRydWUsInQiOlsyXX1dLCJmIjpbeyJmIjpmYWxzZSwidCI6Wy0yLDFdfV0sImciOltdLCJoIjpbXSwiaSI6W10sImoiOltdLCJrIjpbXSwibCI6W3siaSI6dHJ1ZSwidCI6W3siYSI6IlRlc3RQcm9qZWN0IiwidCI6IklPdGhlciIsInUiOmZhbHNlfV19XX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6InMiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH0seyJJZGVudGlmaWVyIjoiYyIsIlR5cGVEYXRhIjp7ImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZVx1MDA2MDEiLCJ1Ijp0cnVlfSwiVHlwZUZpbHRlciI6bnVsbH1dLCJMaWZldGltZSI6MX0sInoiOjF9")]
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
            // FilePath: Input0.cs Expression: qs0Vsc+7Yz/rY9rkdiqbLw==
            case 20:
                services.Add(ServiceDescriptor.Scoped<global::Nested.ServiceA, global::Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::IService<global::System.String>>(a => a.GetRequiredService<global::Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::Service, global::Service>());
                services.Add(ServiceDescriptor.Scoped<global::IService<global::System.Int32>>(a => a.GetRequiredService<global::Service>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
