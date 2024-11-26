﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

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
            // FilePath: {SolutionDirectory}src\DependencyInjection.Extensions\CompiledTypeProviderServiceCollectionExtensions.cs Expression: 8JsIfWGtyxwbpwWfPrtcbQ==
            case 20:
                services.Add(ServiceDescriptor.Singleton(typeof(global::Service), typeof(global::Service)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::IService), a => a.GetRequiredService<global::Service>()));
                services.Add(ServiceDescriptor.Transient(typeof(global::IServiceB), a => a.GetRequiredService<global::Service>()));
                services.Add(ServiceDescriptor.Singleton(typeof(global::ServiceA), typeof(global::ServiceA)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::IService), a => a.GetRequiredService<global::ServiceA>()));
                services.Add(ServiceDescriptor.Singleton(typeof(global::IServiceC), a => a.GetRequiredService<global::ServiceA>()));
                services.Add(ServiceDescriptor.Singleton(typeof(global::ServiceB), typeof(global::ServiceB)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::IService), a => a.GetRequiredService<global::ServiceB>()));
                services.Add(ServiceDescriptor.Transient(typeof(global::IServiceB), a => a.GetRequiredService<global::ServiceB>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
#nullable restore