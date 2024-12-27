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
        var items = new List<Assembly>();
        return items;
    }

    IEnumerable<Type> ICompiledTypeProvider.GetTypes(Func<IReflectionTypeSelector, IEnumerable<Type>> selector, int lineNumber, string filePath, string argumentExpression)
    {
        var items = new List<Type>();
        return items;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: Y77bSqlY24wpl6S/OrgAOg==
            case 20:
                services.Add(ServiceDescriptor.Singleton(DependencyProjectC.GetType("DependencyProjectC.HardReferenceA")!, DependencyProjectC.GetType("DependencyProjectC.HardReferenceA")!));
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IService>(a => (global::RootDependencyProject.IService)a.GetRequiredService(DependencyProjectC.GetType("DependencyProjectC.HardReferenceA")!)));
                services.Add(ServiceDescriptor.Singleton<global::DependencyProjectC.ServiceC, global::DependencyProjectC.ServiceC>());
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IService>(a => a.GetRequiredService<global::DependencyProjectC.ServiceC>()));
                services.Add(ServiceDescriptor.Singleton(DependencyProjectD.GetType("DependencyProjectD.HardReferenceA")!, DependencyProjectD.GetType("DependencyProjectD.HardReferenceA")!));
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IService>(a => (global::RootDependencyProject.IService)a.GetRequiredService(DependencyProjectD.GetType("DependencyProjectD.HardReferenceA")!)));
                services.Add(ServiceDescriptor.Singleton(DependencyProjectD.GetType("DependencyProjectD.HardReferenceC")!, DependencyProjectD.GetType("DependencyProjectD.HardReferenceC")!));
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IService>(a => (global::RootDependencyProject.IService)a.GetRequiredService(DependencyProjectD.GetType("DependencyProjectD.HardReferenceC")!)));
                services.Add(ServiceDescriptor.Singleton<global::DependencyProjectD.ServiceD, global::DependencyProjectD.ServiceD>());
                services.Add(ServiceDescriptor.Singleton<global::RootDependencyProject.IService>(a => a.GetRequiredService<global::DependencyProjectD.ServiceD>()));
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _DependencyProjectC;
    private Assembly DependencyProjectC => _DependencyProjectC ??= _context.LoadFromAssemblyName(new AssemblyName("DependencyProjectC, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _DependencyProjectD;
    private Assembly DependencyProjectD => _DependencyProjectD ??= _context.LoadFromAssemblyName(new AssemblyName("DependencyProjectD, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
