﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
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
            // FilePath: Input0.cs Expression: kZPm/qtioIE2Q/XYleeTDg==
            case 16:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericService, global::TestAssembly.GenericService>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Decimal>>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericService, global::TestAssembly.GenericService>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Decimal>>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                break;
            // FilePath: {SolutionDirectory}src/DependencyInjection.Extensions/CompiledTypeProviderServiceCollectionExtensions.cs Expression: 8JsIfWGtyxwbpwWfPrtcbQ==
            case 21:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericService, global::TestAssembly.GenericService>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericService>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Decimal>>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
