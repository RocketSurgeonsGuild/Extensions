//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

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
            // FilePath: Input0.cs Expression: 4NoScwJSnwDs2efx8zADEg==
            case 14:
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderFactory<>);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory<>);
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2");
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
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>);
                break;
        }
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly);
    private Assembly _RocketSurgeryDependencyInjectionExtensions;
    private Assembly RocketSurgeryDependencyInjectionExtensions => _RocketSurgeryDependencyInjectionExtensions ??= _context.LoadFromAssemblyName(new AssemblyName("Rocket.Surgery.DependencyInjection.Extensions, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
#nullable restore
