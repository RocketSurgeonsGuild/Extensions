//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjMyLCJhIjoieVx1MDAyQlhSY3dQaEcyUlNcdTAwMkJsb3ZyMklOR3c9PSIsImYiOiJJbnB1dDAuY3MifSwiYSI6eyJhIjp0cnVlLCJpIjpmYWxzZSwibSI6W10sIm5hIjpbXSwiZCI6W119LCJ0Ijp7ImIiOjEsImMiOlt7ImYiOjIsIm4iOlsiVGVzdFByb2plY3QuQSIsIlRlc3RQcm9qZWN0LkIiXX1dLCJkIjpbXSwiZSI6W3siZiI6dHJ1ZSwidCI6WzJdfV0sImYiOlt7ImYiOmZhbHNlLCJ0IjpbLTIsMV19XSwiZyI6W10sImgiOltdLCJpIjpbXSwiaiI6W10sImsiOltdLCJsIjpbXX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6InMiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH0seyJJZGVudGlmaWVyIjoiaSIsIlR5cGVEYXRhIjpudWxsLCJUeXBlRmlsdGVyIjpudWxsfV0sIkxpZmV0aW1lIjoxfSwieiI6MX0=")]
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
            // FilePath: Input0.cs Expression: y+XRcwPhG2RS+lovr2INGw==
            case 32:
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.C.ServiceC, global::TestProject.A.C.ServiceC>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.C.ServiceC>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.Nested.ServiceA, global::TestProject.A.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.Service, global::TestProject.A.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.B.IServiceB>(a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.B.ServiceB, global::TestProject.B.ServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.B.ServiceB>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
