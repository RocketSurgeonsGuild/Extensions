﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledTypeProviderGenerator/CompiledTypeProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CA5351, CS8618, CS8669, IL2026, IL2072
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes","{scrubbed}")]
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
        return items;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: CompiledTypeProviderServiceCollectionExtensions.cs Expression: 8PKITxXbtylCwtC8RjJaLg==
            case 21:
                switch (System.IO.Path.GetFileName(filePath))
                {
                    // FilePath: CompiledTypeProviderServiceCollectionExtensions.cs Expression: 8PKITxXbtylCwtC8RjJaLg==
                    case "CompiledTypeProviderServiceCollectionExtensions.cs":
                        services.Add(ServiceDescriptor.Transient<global::Nested.ServiceA, global::Nested.ServiceA>());
                        services.Add(ServiceDescriptor.Transient<global::IService>(a => a.GetRequiredService<global::Nested.ServiceA>()));
                        services.Add(ServiceDescriptor.Scoped<global::Service, global::Service>());
                        services.Add(ServiceDescriptor.Scoped<global::IServiceB>(a => a.GetRequiredService<global::Service>()));
                        services.Add(ServiceDescriptor.Singleton<global::ServiceB, global::ServiceB>());
                        services.Add(ServiceDescriptor.Singleton<global::IService>(a => a.GetRequiredService<global::ServiceB>()));
                        services.Add(ServiceDescriptor.Singleton<global::IServiceB>(a => a.GetRequiredService<global::ServiceB>()));
                        break;
                }

                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CA5351, CS8618, CS8669, IL2026, IL2072
#nullable restore
