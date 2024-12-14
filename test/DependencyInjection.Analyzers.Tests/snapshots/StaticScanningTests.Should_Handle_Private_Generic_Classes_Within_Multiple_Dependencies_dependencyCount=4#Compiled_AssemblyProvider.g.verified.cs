//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjE1LCJhIjoiaS9ROVlmYnJhbmd0aG1PXHUwMDJCY2JFM2FnPT0iLCJmIjoiSW5wdXQwLmNzIn0sImEiOnsiYSI6dHJ1ZSwiaSI6ZmFsc2UsIm0iOltdLCJuYSI6W10sImQiOltdfSwidCI6eyJiIjoxLCJjIjpbXSwiZCI6W10sImUiOlt7ImYiOnRydWUsInQiOlsyXX1dLCJmIjpbeyJmIjpmYWxzZSwidCI6Wy0yLDFdfV0sImciOltdLCJoIjpbXSwiaSI6W10sImoiOltdLCJrIjpbeyJpIjp0cnVlLCJhIjoiUm9vdERlcGVuZGVuY3lQcm9qZWN0IiwidCI6IlJvb3REZXBlbmRlbmN5UHJvamVjdC5JUmVxdWVzdEhhbmRsZXJcdTAwNjAyIiwidSI6dHJ1ZX1dLCJsIjpbXX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH1dLCJMaWZldGltZSI6MH0sInoiOjB9")]
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
            // FilePath: Input0.cs Expression: i/Q9YfbrangthmO+cbE3ag==
            case 15:
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IRequestHandler<global::Dependency1Project.Request0, global::Dependency1Project.Response0>, global::Dependency1Project.RequestHandler0>());
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IRequestHandler<, >).MakeGenericType(Dependency1Project.GetType("Dependency1Project.Request1")!, Dependency1Project.GetType("Dependency1Project.Response1")!)!, Dependency1Project.GetType("Dependency1Project.RequestHandler1")!));
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IRequestHandler<global::Dependency1Project.Request2, global::Dependency1Project.Response2>, global::Dependency1Project.RequestHandler2>());
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IRequestHandler<, >).MakeGenericType(Dependency3Project.GetType("Dependency1Project.Request3")!, Dependency3Project.GetType("Dependency1Project.Response3")!)!, Dependency3Project.GetType("Dependency1Project.RequestHandler3")!));
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _Dependency1Project;
    private Assembly Dependency1Project => _Dependency1Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency1Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency3Project;
    private Assembly Dependency3Project => _Dependency3Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency3Project, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
