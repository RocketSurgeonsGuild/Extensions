﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
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
        var items = new List<Assembly>();
        return items;
    }

    IEnumerable<Type> ICompiledTypeProvider.GetTypes(Func<IReflectionTypeSelector, IEnumerable<Type>> selector, int lineNumber, string filePath, string argumentExpression)
    {
        var items = new List<Type>();
        switch (System.IO.Path.GetFileName(filePath))
        {
            // FilePath: Input0.cs Expression: D2aB+ba3m7tjD2l6ZnawMg==
            case "Input0.cs":
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorInfoEx")!);
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+ConstructorMatcher")!);
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ActivatorUtilities+FactoryParameterContext")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.AsyncServiceScope));
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.KeyedService+AnyKeyObj")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ObjectFactory<>));
                items.Add(MicrosoftExtensionsDependencyInjectionAbstractions.GetType("Microsoft.Extensions.DependencyInjection.ServiceCollection+ServiceCollectionDebugView")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ServiceLifetime));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
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
