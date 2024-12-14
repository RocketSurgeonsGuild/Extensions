//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ReflectionTypes", "eyJsIjp7ImwiOjE0LCJhIjoibWp5OG4yc0daL0d6MlBLRzJ3ajc3UT09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOlsiUm9ja2V0LlN1cmdlcnkuRGVwZW5kZW5jeUluamVjdGlvbi5FeHRlbnNpb25zIl0sImQiOltdfSwidCI6eyJiIjoyLCJjIjpbeyJmIjozLCJuIjpbIkpldEJyYWlucy5Bbm5vdGF0aW9ucyIsIlBvbHlmaWxscyIsIlN5c3RlbSJdfV0sImQiOlt7ImkiOmZhbHNlLCJmIjowLCJuIjpbIlBvbHlmaWxsIl19XSwiZSI6W10sImYiOltdLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6ZmFsc2UsImEiOiJSb2NrZXQuU3VyZ2VyeS5EZXBlbmRlbmN5SW5qZWN0aW9uLkV4dGVuc2lvbnMiLCJ0IjoiUm9ja2V0LlN1cmdlcnkuRGVwZW5kZW5jeUluamVjdGlvbi5Db21waWxlZC5JQ29tcGlsZWRUeXBlUHJvdmlkZXIiLCJ1IjpmYWxzZX1dLCJsIjpbXX19")]
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
            // FilePath: Input0.cs Expression: mjy8n2sGZ/Gz2PKG2wj77Q==
            case 14:
                yield return typeof(global::Program);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceCollection);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderFactory<>);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsKeyedService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ISupportRequiredService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.KeyedService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory<>);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollection);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceLifetime);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter);
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
                yield return typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >);
                break;
        }
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
