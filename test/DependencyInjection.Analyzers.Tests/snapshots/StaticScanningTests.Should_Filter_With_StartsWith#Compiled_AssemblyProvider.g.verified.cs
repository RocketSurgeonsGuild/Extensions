//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoieTRmQmtrV3hBMk4yTDNzSGdkVmUxQT09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOltdLCJkIjpbXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOlt7ImkiOnRydWUsImYiOjAsIm4iOlsiRmFjdG9yeSJdfV0sImUiOlt7ImYiOnRydWUsInQiOlsyXX1dLCJmIjpbeyJmIjpmYWxzZSwidCI6Wy0yLDFdfV0sImciOltdLCJoIjpbXSwiaSI6W10sImoiOltdLCJrIjpbeyJpIjp0cnVlLCJhIjoiVGVzdFByb2plY3QiLCJ0IjoiSVNlcnZpY2UiLCJ1IjpmYWxzZX0seyJpIjp0cnVlLCJhIjoiVGVzdFByb2plY3QiLCJ0IjoiSVNlcnZpY2VCIiwidSI6ZmFsc2V9XSwibCI6W119LCJzIjp7IlNlcnZpY2VUeXBlRGVzY3JpcHRvcnMiOlt7IklkZW50aWZpZXIiOiJzIiwiVHlwZURhdGEiOm51bGwsIlR5cGVGaWx0ZXIiOm51bGx9LHsiSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH1dLCJMaWZldGltZSI6MX0sInoiOjF9")]
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
            // FilePath: Input0.cs Expression: y4fBkkWxA2N2L3sHgdVe1A==
            case 20:
                services.Add(ServiceDescriptor.Scoped<global::FactoryService, global::FactoryService>());
                services.Add(ServiceDescriptor.Scoped<global::IService>(a => a.GetRequiredService<global::FactoryService>()));
                services.Add(ServiceDescriptor.Scoped<global::IServiceB>(a => a.GetRequiredService<global::FactoryService>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
