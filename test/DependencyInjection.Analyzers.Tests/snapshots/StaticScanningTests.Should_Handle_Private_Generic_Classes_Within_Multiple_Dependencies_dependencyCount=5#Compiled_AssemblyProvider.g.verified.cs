//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8601, CS8602, CS8603, CS8604, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes","{scrubbed}")]
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
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IRequestHandler<, >).MakeGenericType(Dependency1Project.GetType("Dependency1Project.Request1"), Dependency1Project.GetType("Dependency1Project.Response1")), Dependency1Project.GetType("Dependency1Project.RequestHandler1")));
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IRequestHandler<global::Dependency1Project.Request2, global::Dependency1Project.Response2>, global::Dependency1Project.RequestHandler2>());
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IRequestHandler<, >).MakeGenericType(Dependency3Project.GetType("Dependency1Project.Request3"), Dependency3Project.GetType("Dependency1Project.Response3")), Dependency3Project.GetType("Dependency1Project.RequestHandler3")));
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IRequestHandler<global::Dependency1Project.Request4, global::Dependency1Project.Response4>, global::Dependency1Project.RequestHandler4>());
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly);
    private Assembly _Dependency1Project;
    private Assembly Dependency1Project => _Dependency1Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency1Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency3Project;
    private Assembly Dependency3Project => _Dependency3Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency3Project, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8601, CS8602, CS8603, CS8604, CS8618, CS8669
#nullable restore
