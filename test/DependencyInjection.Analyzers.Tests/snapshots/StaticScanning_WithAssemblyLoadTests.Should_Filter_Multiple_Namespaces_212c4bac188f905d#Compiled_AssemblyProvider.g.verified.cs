﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes","{scrubbed}")]
[assembly: Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute(typeof(CompiledTypeProvider))]
[System.CodeDom.Compiler.GeneratedCode("Rocket.Surgery.DependencyInjection.Analyzers", "version"), System.Runtime.CompilerServices.CompilerGenerated, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
file class CompiledTypeProvider : ICompiledTypeProvider
{
    IEnumerable<Assembly> ICompiledTypeProvider.GetAssemblies(Action<IReflectionAssemblySelector> action, int lineNumber, string filePath, string argumentExpression)
    {
        yield break;
    }

    IEnumerable<Type> ICompiledTypeProvider.GetTypes(Func<IReflectionAssemblySelector, IEnumerable<Type>> selector, int lineNumber, string filePath, string argumentExpression)
    {
        yield break;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: q1XDNowA0B+UjNLeBZPMAw==
            case 29:
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.A.Service), typeof(global::TestProject.A.Service)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.A.IService), a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.B.IServiceB), a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.A.ServiceA), typeof(global::TestProject.A.ServiceA)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.A.IService), a => a.GetRequiredService<global::TestProject.A.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.B.ServiceB), typeof(global::TestProject.B.ServiceB)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestProject.A.IService), a => a.GetRequiredService<global::TestProject.B.ServiceB>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8602, CS8603, CS8618, CS8669
#nullable restore