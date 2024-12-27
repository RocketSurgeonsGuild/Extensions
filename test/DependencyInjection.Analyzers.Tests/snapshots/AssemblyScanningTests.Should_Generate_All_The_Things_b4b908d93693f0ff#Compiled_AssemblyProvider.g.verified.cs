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

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ReflectionTypes","{scrubbed}")]
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
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: 4knsgcWNnVMzYv5vk8vSXg==
            case 16:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities));
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ActivatorUtilitiesUpdateHandler")!);
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorInfoEx")!);
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorMatcher")!);
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+FactoryParameterContext")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IServiceCollection));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderFactory<>));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsKeyedService));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ISupportRequiredService));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.KeyedService));
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.KeyedService+AnyKeyObj")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory<>));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollection));
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ServiceCollection+ServiceCollectionDebugView")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceLifetime));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                break;
        }

        return items;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _MicrosoftExtensionsDependencyInjectionAbstractions;
    private Assembly MicrosoftExtensionsDependencyInjectionAbstractions => _MicrosoftExtensionsDependencyInjectionAbstractions ??= _context.LoadFromAssemblyName(new AssemblyName("Microsoft.Extensions.DependencyInjection.Abstractions, Version=version, Culture=neutral, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f33a29044fa9d740c9b3213a93e57c84b472c84e0b8a0e1ae48e67a9f8f6de9d5f7f3d52ac23e48ac51801f1dc950abe901da34d2a9e3baadb141a17c77ef3c565dd5ee5054b91cf63bb3c6ab83f72ab3aafe93d0fc3c2348b764fafb0b1c0733de51459aeab46580384bf9d74c4e28164b7cde247f891ba07891c9d872ad2bb"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
