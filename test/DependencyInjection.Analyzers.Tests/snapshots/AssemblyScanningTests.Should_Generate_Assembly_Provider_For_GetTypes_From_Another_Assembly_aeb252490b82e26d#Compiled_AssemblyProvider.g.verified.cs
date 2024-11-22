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
            // FilePath: Input0.cs Expression: grnrT2DhTaKAqIuAg3cQ/g==
            case 14:
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("FxResources.Microsoft.Extensions.DependencyInjection.Abstractions.SR");
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities);
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ActivatorUtilitiesUpdateHandler");
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorInfoEx");
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorMatcher");
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+FactoryParameterContext");
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+StackAllocatedObjects");
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceCollection);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsKeyedService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceProviderIsService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScope);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.IServiceScopeFactory);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ISupportRequiredService);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.KeyedService);
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.KeyedService+AnyKeyObj");
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollection);
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ServiceCollection+ServiceCollectionDebugView");
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceLifetime);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions);
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions);
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.Internal.ParameterDefaultValue");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.BytePolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DateTimeOffsetPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DateTimePolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DoublePolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.EnumPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.GuidPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.IntPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.LongPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.RegexPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.SBytePolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.ShortPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.StringPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.UIntPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.ULongPolyfill");
                yield return RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.UShortPolyfill");
                yield return typeof(global::Program);
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
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions);
                yield return typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions);
                yield return typeof(global::System.ComponentModel.CancelEventArgs);
                yield return typeof(global::System.ComponentModel.IChangeTracking);
                yield return typeof(global::System.ComponentModel.IEditableObject);
                yield return typeof(global::System.ComponentModel.IRevertibleChangeTracking);
                yield return typeof(global::System.IServiceProvider);
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("System.SR");
                yield return MicrosoftExtensionsDependencyInjectionAbstractions.GetType("System.ThrowHelper");
                break;
        }
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly);
    private Assembly _MicrosoftExtensionsDependencyInjectionAbstractions;
    private Assembly MicrosoftExtensionsDependencyInjectionAbstractions => _MicrosoftExtensionsDependencyInjectionAbstractions ??= _context.LoadFromAssemblyName(new AssemblyName("Microsoft.Extensions.DependencyInjection.Abstractions, Version=version, Culture=neutral, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f33a29044fa9d740c9b3213a93e57c84b472c84e0b8a0e1ae48e67a9f8f6de9d5f7f3d52ac23e48ac51801f1dc950abe901da34d2a9e3baadb141a17c77ef3c565dd5ee5054b91cf63bb3c6ab83f72ab3aafe93d0fc3c2348b764fafb0b1c0733de51459aeab46580384bf9d74c4e28164b7cde247f891ba07891c9d872ad2bb"));

    private Assembly _RocketSurgeryDependencyInjectionExtensions;
    private Assembly RocketSurgeryDependencyInjectionExtensions => _RocketSurgeryDependencyInjectionExtensions ??= _context.LoadFromAssemblyName(new AssemblyName("Rocket.Surgery.DependencyInjection.Extensions, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
#nullable restore
