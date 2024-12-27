//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
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
            // FilePath: Input0.cs Expression: +ZwL1kukDVL/SHup/rQyqQ==
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
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!, TestAssembly.GetType("TestAssembly.Nested+MyRecord")!));
                services.Add(ServiceDescriptor.Scoped(typeof(global::System.IEquatable<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Nested+Validator")!, TestAssembly.GetType("TestAssembly.Nested+Validator")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IValidator>(a => (global::TestAssembly.IValidator)a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+Validator")!)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestAssembly.IValidator<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+Validator")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Request, global::TestAssembly.Request>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IRequest<global::TestAssembly.Response>>(a => a.GetRequiredService<global::TestAssembly.Request>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.RequestHandler, global::TestAssembly.RequestHandler>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IRequestHandler<global::TestAssembly.Request, global::TestAssembly.Response>>(a => a.GetRequiredService<global::TestAssembly.RequestHandler>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Response, global::TestAssembly.Response>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.ServiceB, global::TestAssembly.ServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.ServiceB>()));
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
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!, TestAssembly.GetType("TestAssembly.Nested+MyRecord")!));
                services.Add(ServiceDescriptor.Scoped(typeof(global::System.IEquatable<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Nested+Validator")!, TestAssembly.GetType("TestAssembly.Nested+Validator")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IValidator>(a => (global::TestAssembly.IValidator)a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+Validator")!)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestAssembly.IValidator<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+Validator")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Request, global::TestAssembly.Request>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IRequest<global::TestAssembly.Response>>(a => a.GetRequiredService<global::TestAssembly.Request>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.RequestHandler, global::TestAssembly.RequestHandler>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IRequestHandler<global::TestAssembly.Request, global::TestAssembly.Response>>(a => a.GetRequiredService<global::TestAssembly.RequestHandler>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Response, global::TestAssembly.Response>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.ServiceB, global::TestAssembly.ServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.ServiceB>()));
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
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!, TestAssembly.GetType("TestAssembly.Nested+MyRecord")!));
                services.Add(ServiceDescriptor.Scoped(typeof(global::System.IEquatable<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Nested+Validator")!, TestAssembly.GetType("TestAssembly.Nested+Validator")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IValidator>(a => (global::TestAssembly.IValidator)a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+Validator")!)));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestAssembly.IValidator<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Nested+Validator")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Request, global::TestAssembly.Request>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IRequest<global::TestAssembly.Response>>(a => a.GetRequiredService<global::TestAssembly.Request>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.RequestHandler, global::TestAssembly.RequestHandler>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IRequestHandler<global::TestAssembly.Request, global::TestAssembly.Response>>(a => a.GetRequiredService<global::TestAssembly.RequestHandler>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Response, global::TestAssembly.Response>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.ServiceB, global::TestAssembly.ServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.ServiceB>()));
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _TestAssembly;
    private Assembly TestAssembly => _TestAssembly ??= _context.LoadFromAssemblyName(new AssemblyName("TestAssembly, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669, IL2026, IL2072
#nullable restore
