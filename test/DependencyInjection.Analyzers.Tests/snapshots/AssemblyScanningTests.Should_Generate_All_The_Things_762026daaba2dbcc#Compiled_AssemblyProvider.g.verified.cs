//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

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
            // FilePath: Input0.cs Expression: smHMPY82+06HwG0itiaMJw==
            case 16:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities));
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
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory<>));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollection));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceLifetime));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
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
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(typeof(global::TestAssembly.Service));
                break;
        }

        return items;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
