//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoiV0dyS1E2TlhUS1ltNVdNQ0p2eXRWQT09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOltdLCJkIjpbXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6dHJ1ZSwiYSI6IlRlc3RQcm9qZWN0IiwidCI6IklTZXJ2aWNlXHUwMDYwMSIsInUiOnRydWV9XSwibCI6W3siaSI6dHJ1ZSwidCI6W3siYSI6IlRlc3RQcm9qZWN0IiwidCI6IklPdGhlciIsInUiOmZhbHNlfV19XX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6eyJiIjoxLCJjIjpbXSwiZCI6W10sImUiOltdLCJmIjpbXSwiZyI6W10sImgiOltdLCJpIjpbXSwiaiI6W10sImsiOlt7ImkiOnRydWUsImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZVx1MDA2MDEiLCJ1Ijp0cnVlfV0sImwiOltdfX1dLCJMaWZldGltZSI6MX0sInoiOjF9")]
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
            // FilePath: Input0.cs Expression: WGrKQ6NXTKYm5WMCJvytVA==
            case 20:
                services.Add(ServiceDescriptor.Scoped<global::IService<global::System.String>, global::Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::IService<global::System.Int32>, global::Service>());
                services.Add(ServiceDescriptor.Scoped<global::IService<global::System.String>, global::Service>());
                services.Add(ServiceDescriptor.Scoped<global::IService<global::System.Decimal>, global::ServiceB>());
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
