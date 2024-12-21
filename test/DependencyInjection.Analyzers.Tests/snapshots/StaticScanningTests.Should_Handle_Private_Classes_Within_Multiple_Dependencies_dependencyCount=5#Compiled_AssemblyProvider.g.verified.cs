//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
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
            // FilePath: Input0.cs Expression: MpVnQwuMrx82b7Q/xd6OBQ==
            case 15:
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency0Project.GetType("Dependency1Project.Service0")!));
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency1Project.GetType("Dependency1Project.Service1")!));
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency2Project.GetType("Dependency1Project.Service2")!));
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency3Project.GetType("Dependency1Project.Service3")!));
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency4Project.GetType("Dependency1Project.Service4")!));
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _Dependency0Project;
    private Assembly Dependency0Project => _Dependency0Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency0Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency1Project;
    private Assembly Dependency1Project => _Dependency1Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency1Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency2Project;
    private Assembly Dependency2Project => _Dependency2Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency2Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency3Project;
    private Assembly Dependency3Project => _Dependency3Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency3Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency4Project;
    private Assembly Dependency4Project => _Dependency4Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency4Project, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
