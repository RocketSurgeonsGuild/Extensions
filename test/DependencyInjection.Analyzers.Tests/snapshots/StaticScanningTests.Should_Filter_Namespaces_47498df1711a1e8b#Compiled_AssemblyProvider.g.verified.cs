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

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes","{scrubbed}")]
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
            // FilePath: Input0.cs Expression: DD8TGJ4l6DlY3zR4q5u0mQ==
            case 29:
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("{CompiledTypeProvider}")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.Compiled.ICompiledTypeProvider>(a => a.GetRequiredService(RocketSurgeryDependencyInjectionExtensions.GetType("{CompiledTypeProvider}")) as global::Rocket.Surgery.DependencyInjection.Compiled.ICompiledTypeProvider));
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("FxResources.Microsoft.Extensions.DependencyInjection.Abstractions.SR"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("FxResources.Microsoft.Extensions.DependencyInjection.Abstractions.SR")));
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities, global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities>());
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ActivatorUtilitiesUpdateHandler"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ActivatorUtilitiesUpdateHandler")));
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorInfoEx"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorInfoEx")));
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute, global::Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions, global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions, global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute, global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.KeyedService, global::Microsoft.Extensions.DependencyInjection.KeyedService>());
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.KeyedService+AnyKeyObj"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.KeyedService+AnyKeyObj")));
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions, global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ServiceCollection, global::Microsoft.Extensions.DependencyInjection.ServiceCollection>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.IServiceCollection>(a => a.GetRequiredService<global::Microsoft.Extensions.DependencyInjection.ServiceCollection>()));
                services.Add(ServiceDescriptor.Scoped<global::System.Collections.Generic.ICollection<global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor>>(a => a.GetRequiredService<global::Microsoft.Extensions.DependencyInjection.ServiceCollection>()));
                services.Add(ServiceDescriptor.Scoped<global::System.Collections.Generic.IEnumerable<global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor>>(a => a.GetRequiredService<global::Microsoft.Extensions.DependencyInjection.ServiceCollection>()));
                services.Add(ServiceDescriptor.Scoped<global::System.Collections.Generic.IList<global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor>>(a => a.GetRequiredService<global::Microsoft.Extensions.DependencyInjection.ServiceCollection>()));
                services.Add(ServiceDescriptor.Scoped<global::System.Collections.IEnumerable>(a => a.GetRequiredService<global::Microsoft.Extensions.DependencyInjection.ServiceCollection>()));
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ServiceCollection+ServiceCollectionDebugView"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ServiceCollection+ServiceCollectionDebugView")));
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions, global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor, global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute, global::Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions, global::Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions, global::Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions>());
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.Internal.ParameterDefaultValue"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.Internal.ParameterDefaultValue")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.BytePolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.BytePolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DateTimeOffsetPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DateTimeOffsetPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DateTimePolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DateTimePolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DoublePolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.DoublePolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.EnumPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.EnumPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.GuidPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.GuidPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.IntPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.IntPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.LongPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.LongPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.Polyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.Polyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.RegexPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.RegexPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.SBytePolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.SBytePolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.ShortPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.ShortPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.StringPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.StringPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.UIntPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.UIntPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.ULongPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.ULongPolyfill")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.UShortPolyfill"), RocketSurgeryDependencyInjectionExtensions.GetType("Polyfills.UShortPolyfill")));
                services.Add(ServiceDescriptor.Scoped<global::Program, global::Program>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute, global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions, global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions>());
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")));
                services.Add(ServiceDescriptor.Scoped(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2"), RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >>(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")));
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute, global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions, global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions, global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute, global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>, global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >, global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >, global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >>());
                services.Add(ServiceDescriptor.Scoped<global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >, global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >>());
                services.Add(ServiceDescriptor.Scoped<global::System.ComponentModel.CancelEventArgs, global::System.ComponentModel.CancelEventArgs>());
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("System.SR"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("System.SR")));
                services.Add(ServiceDescriptor.Scoped(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("System.ThrowHelper"), MicrosoftExtensionsDependencyInjectionAbstractions.GetType("System.ThrowHelper")));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.C.ServiceC, global::TestProject.A.C.ServiceC>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.C.ServiceC>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.Service, global::TestProject.A.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.B.IServiceB>(a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.ServiceA, global::TestProject.A.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.B.ServiceB, global::TestProject.B.ServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.B.ServiceB>()));
                break;
        }

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
