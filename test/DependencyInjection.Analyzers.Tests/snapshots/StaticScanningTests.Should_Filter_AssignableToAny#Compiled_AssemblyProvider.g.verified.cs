//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoiR3pTbzFaSUozUEIwYzRjWXc0NEY4Zz09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOltdLCJkIjpbXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W10sImwiOlt7ImkiOnRydWUsInQiOlt7ImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZSIsInUiOmZhbHNlfSx7ImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZUIiLCJ1IjpmYWxzZX1dfV19LCJzIjp7IlNlcnZpY2VUeXBlRGVzY3JpcHRvcnMiOlt7IklkZW50aWZpZXIiOiJzIiwiVHlwZURhdGEiOm51bGwsIlR5cGVGaWx0ZXIiOm51bGx9LHsiSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH1dLCJMaWZldGltZSI6MX0sInoiOjF9")]
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
            // FilePath: Input0.cs Expression: GzSo1ZIJ3PB0c4cYw44F8g==
            case 20:
                services.Add(ServiceDescriptor.Scoped<global::Nested.ServiceA, global::Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::IService>(a => a.GetRequiredService<global::Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::Service, global::Service>());
                services.Add(ServiceDescriptor.Scoped<global::IService>(a => a.GetRequiredService<global::Service>()));
                services.Add(ServiceDescriptor.Scoped<global::IServiceB>(a => a.GetRequiredService<global::Service>()));
                services.Add(ServiceDescriptor.Scoped<global::ServiceB, global::ServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::IService>(a => a.GetRequiredService<global::ServiceB>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
