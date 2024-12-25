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
        yield break;
    }

    IEnumerable<Type> ICompiledTypeProvider.GetTypes(Func<IReflectionTypeSelector, IEnumerable<Type>> selector, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: k2UXW1Y4n0CiziE30f9ykg==
            case 14:
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceCollection);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderFactory<>);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsKeyedService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ISupportRequiredService);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >);
                break;
        }

        yield break;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
