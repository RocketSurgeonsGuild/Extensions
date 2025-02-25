//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledTypeProviderGenerator/CompiledTypeProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CA5351, CS8618, CS8669, IL2026, IL2072
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

[assembly: Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute(typeof(CompiledTypeProvider), "{scrubbed}")]
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
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: kccf/OaTnMCVdm/9GexI6A==
            case 16:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
        }

        return items;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _OtherProject;
    private Assembly OtherProject => _OtherProject ??= _context.LoadFromAssemblyName(new AssemblyName("OtherProject, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _RocketSurgeryDependencyInjectionExtensions;
    private Assembly RocketSurgeryDependencyInjectionExtensions => _RocketSurgeryDependencyInjectionExtensions ??= _context.LoadFromAssemblyName(new AssemblyName("Rocket.Surgery.DependencyInjection.Extensions, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _TestAssembly;
    private Assembly TestAssembly => _TestAssembly ??= _context.LoadFromAssemblyName(new AssemblyName("TestAssembly, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CA5351, CS8618, CS8669, IL2026, IL2072
#nullable restore
