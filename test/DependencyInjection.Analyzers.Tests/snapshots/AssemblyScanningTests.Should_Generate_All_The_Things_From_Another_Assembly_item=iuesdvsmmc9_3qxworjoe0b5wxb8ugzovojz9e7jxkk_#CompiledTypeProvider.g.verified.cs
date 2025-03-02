//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledTypeProviderGenerator/CompiledTypeProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CA5351, CS8618, CS8669, IL2026, IL2072
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

[assembly: Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute(typeof(CompiledTypeProvider), "{scrubbed}")]
[System.CodeDom.Compiler.GeneratedCode("Rocket.Surgery.DependencyInjection.Analyzers", "version"), System.Runtime.CompilerServices.CompilerGenerated, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
file class CompiledTypeProvider : ICompiledTypeProvider
{
    IEnumerable<Assembly> ICompiledTypeProvider.GetAssemblies(Action<IReflectionAssemblySelector> action, int lineNumber, string filePath, string argumentExpression)
    {
        var items = new List<Assembly>();
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: 8IGLe4J/MYHkM0PrcVVuwg==
            case 16:
                items.Add(OtherProject);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions).Assembly);
                items.Add(typeof(global::System.IServiceProvider).Assembly);
                items.Add(typeof(global::TestAssembly.IService).Assembly);
                items.Add(TestProject);
                break;
            // FilePath: Input0.cs Expression: Z9rFHKTvVtL4pjKVS65ieA==
            case 17:
                items.Add(OtherProject);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions).Assembly);
                items.Add(typeof(global::System.IServiceProvider).Assembly);
                items.Add(typeof(global::TestAssembly.IService).Assembly);
                items.Add(TestProject);
                break;
            // FilePath: Input0.cs Expression: EroujZtERsWBDGh52iQ5LQ==
            case 18:
                items.Add(OtherProject);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions).Assembly);
                items.Add(typeof(global::System.IServiceProvider).Assembly);
                items.Add(typeof(global::TestAssembly.IService).Assembly);
                items.Add(TestProject);
                break;
            // FilePath: Input0.cs Expression: LyqSv6vMcdITU/VUyq5Gfg==
            case 19:
                items.Add(OtherProject);
                break;
            // FilePath: Input0.cs Expression: ugfaFnP+kRqgwoQVip7fpQ==
            case 20:
                items.Add(OtherProject);
                items.Add(TestProject);
                break;
            // FilePath: Input0.cs Expression: r7EAhFclBVT54DTDSLqJjw==
            case 21:
                items.Add(OtherProject);
                items.Add(TestProject);
                break;
        }

        return items;
    }

    IEnumerable<Type> ICompiledTypeProvider.GetTypes(Func<IReflectionTypeSelector, IEnumerable<Type>> selector, int lineNumber, string filePath, string argumentExpression)
    {
        var items = new List<Type>();
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: Xt+gXX8VI+/o1zAmcdZPUw==
            case 42:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ExcludeFromCompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: ca8FK2k2biWcJo/tQ2D+0Q==
            case 102:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ExcludeFromCompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: aZistx/1kY6goq1k0APviA==
            case 122:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ExcludeFromCompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: 6eINCg7X1s1Fy3K8elffJw==
            case 132:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: riCLI8zEdrtpr1RFumwoCQ==
            case 147:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: CWFPPpqrLI3buzj44KDzTw==
            case 158:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: kccf/OaTnMCVdm/9GexI6A==
            case 169:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: UQMB36l1GXcwnwk07khcPA==
            case 200:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: zZSsZdtVJis8QO5n4pQYbg==
            case 211:
                items.Add(OtherProject.GetType("Program")!);
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(TestAssembly.GetType("TestAssembly.GenericService")!);
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!);
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(TestAssembly.GetType("TestAssembly.Nested+Validator")!);
                items.Add(TestAssembly.GetType("TestAssembly.Request")!);
                items.Add(TestAssembly.GetType("TestAssembly.RequestHandler")!);
                items.Add(TestAssembly.GetType("TestAssembly.Response")!);
                items.Add(typeof(global::TestAssembly.Service));
                items.Add(TestAssembly.GetType("TestAssembly.ServiceB")!);
                break;
            // FilePath: Input0.cs Expression: TmpOe7kqtaI4TN48hoMaWA==
            case 22:
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                break;
            // FilePath: Input0.cs Expression: Epy+1yb5HdlK1Hu9cBVEWQ==
            case 32:
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                break;
            // FilePath: Input0.cs Expression: 61ZCBretH0dWUW7zHYRfQQ==
            case 52:
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                break;
            // FilePath: Input0.cs Expression: ZZVauT3fwrJrb/rmRz/z5g==
            case 62:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ExcludeFromCompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(typeof(global::TestAssembly.Service));
                break;
            // FilePath: Input0.cs Expression: mF75dG+Dv3CzvhneQ0PZMQ==
            case 72:
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                break;
            // FilePath: Input0.cs Expression: qBIVSXgRbSfIUDtbJ2U3ow==
            case 82:
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                break;
            // FilePath: Input0.cs Expression: OEcoVmZS+d2TfT1y2s87jg==
            case 92:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Info")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Metadata")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(TestAssembly.GetType("MyAssembly")!);
                items.Add(TestAssembly.GetType("MyAssembly+Info")!);
                items.Add(TestAssembly.GetType("MyAssembly+Metadata")!);
                items.Add(TestAssembly.GetType("MyAssembly+Project")!);
                items.Add(typeof(global::TestAssembly.Nested));
                break;
            // FilePath: Input0.cs Expression: jbfZ/SBhNLTKU8V8KCyblQ==
            case 112:
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScoped`2")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`1")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`6")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`5")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`4")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`3")!);
                items.Add(RocketSurgeryDependencyInjectionExtensions.GetType("Rocket.Surgery.DependencyInjection.ExecuteScopedOptional`2")!);
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                break;
            // FilePath: Input0.cs Expression: 3jHicIrYcndhq8Cd5yrd0Q==
            case 222:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                break;
            // FilePath: Input0.cs Expression: STdPgg0BzqNex1xYlTuWWA==
            case 232:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                break;
            // FilePath: Input0.cs Expression: zAB6003nIMDyeZHIzhkT8w==
            case 242:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                break;
            // FilePath: Input0.cs Expression: G43gcXk/3DHqdCYy9nmEKg==
            case 252:
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions));
                items.Add(typeof(global::Microsoft.Extensions.DependencyInjection.ScopedServiceDependencyInjectionExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ExcludeFromCompiledTypeProviderAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeInfoFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.Compiled.TypeKindFilter));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScoped<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.IExecuteScopedOptional<, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ScopedServiceOptionalExtensions));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<>));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<,, >));
                items.Add(typeof(global::Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute<, >));
                items.Add(typeof(global::TestAssembly.GenericServiceB));
                items.Add(typeof(global::TestAssembly.IGenericService<>));
                items.Add(typeof(global::TestAssembly.IOther));
                items.Add(typeof(global::TestAssembly.IRequest<>));
                items.Add(typeof(global::TestAssembly.IRequestHandler<, >));
                items.Add(typeof(global::TestAssembly.IService));
                items.Add(typeof(global::TestAssembly.IServiceB));
                items.Add(typeof(global::TestAssembly.IValidator));
                items.Add(typeof(global::TestAssembly.IValidator<>));
                items.Add(typeof(global::TestAssembly.Nested));
                items.Add(typeof(global::TestAssembly.Nested.GenericServiceA));
                items.Add(typeof(global::TestAssembly.Nested.ServiceA));
                items.Add(typeof(global::TestAssembly.Service));
                break;
        }

        return items;
    }

    Microsoft.Extensions.DependencyInjection.IServiceCollection ICompiledTypeProvider.Scan(Microsoft.Extensions.DependencyInjection.IServiceCollection services, Action<IServiceDescriptorAssemblySelector> selector, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: lk0ALym+V2W6xQDXO/AkYw==
            case 262:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                break;
            // FilePath: Input0.cs Expression: CSc+6hElAjxc8UqHDR2nGg==
            case 268:
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Singleton(TestAssembly.GetType("TestAssembly.ServiceB")!, TestAssembly.GetType("TestAssembly.ServiceB")!));
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IService>(a => (global::TestAssembly.IService)a.GetRequiredService(TestAssembly.GetType("TestAssembly.ServiceB")!)));
                break;
            // FilePath: Input0.cs Expression: fKfq36ZCDNfEou+qyyLEbw==
            case 274:
                break;
            // FilePath: Input0.cs Expression: RF0U3ve7NmkBVhOpOoW7bQ==
            case 279:
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Singleton(TestAssembly.GetType("TestAssembly.ServiceB")!, TestAssembly.GetType("TestAssembly.ServiceB")!));
                services.Add(ServiceDescriptor.Singleton<global::TestAssembly.IService>(a => (global::TestAssembly.IService)a.GetRequiredService(TestAssembly.GetType("TestAssembly.ServiceB")!)));
                break;
            // FilePath: Input0.cs Expression: H/jKai1h1B6jnFlFcyi7Tg==
            case 285:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.ServiceB")!, TestAssembly.GetType("TestAssembly.ServiceB")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => (global::TestAssembly.IService)a.GetRequiredService(TestAssembly.GetType("TestAssembly.ServiceB")!)));
                break;
            // FilePath: Input0.cs Expression: Cig2XJLApDe8z9wMh/IDUA==
            case 291:
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.GenericService")!, TestAssembly.GetType("TestAssembly.GenericService")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => (global::TestAssembly.IGenericService<global::System.Int32>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => (global::TestAssembly.IGenericService<global::System.String>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => (global::TestAssembly.IOther)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Decimal>>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                break;
            // FilePath: Input0.cs Expression: GC4f7Eu/9wIch8wpSP2wyQ==
            case 296:
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.GenericService")!, TestAssembly.GetType("TestAssembly.GenericService")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => (global::TestAssembly.IGenericService<global::System.Int32>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => (global::TestAssembly.IGenericService<global::System.String>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => (global::TestAssembly.IOther)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Decimal>>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                break;
            // FilePath: Input0.cs Expression: a8a5Us+LBXHvnbZedXasXw==
            case 302:
                services.Add(ServiceDescriptor.Singleton(typeof(global::TestAssembly.IRequestHandler<, >).MakeGenericType(TestAssembly.GetType("TestAssembly.Request")!, TestAssembly.GetType("TestAssembly.Response")!)!, TestAssembly.GetType("TestAssembly.RequestHandler")!));
                break;
            // FilePath: Input0.cs Expression: xbYAbKX/QnnClXoKl6q/Aw==
            case 307:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                break;
            // FilePath: Input0.cs Expression: e8KAWIaBqxCKvrQ5PGf0qg==
            case 313:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.ServiceB")!, TestAssembly.GetType("TestAssembly.ServiceB")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => (global::TestAssembly.IService)a.GetRequiredService(TestAssembly.GetType("TestAssembly.ServiceB")!)));
                break;
            // FilePath: Input0.cs Expression: QR7ilMUHdBOqlpGLxKKLDQ==
            case 319:
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.ServiceA, global::TestAssembly.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.ServiceB")!, TestAssembly.GetType("TestAssembly.ServiceB")!));
                break;
            // FilePath: Input0.cs Expression: nNzkffztFbvyPeXlZPkOkQ==
            case 325:
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.GenericService")!, TestAssembly.GetType("TestAssembly.GenericService")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => (global::TestAssembly.IGenericService<global::System.Int32>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => (global::TestAssembly.IGenericService<global::System.String>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Decimal>>(a => a.GetRequiredService<global::TestAssembly.GenericServiceB>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => a.GetRequiredService<global::TestAssembly.Nested.GenericServiceA>()));
                break;
            // FilePath: Input0.cs Expression: HH9aVmBgFQ5bSAugUwbZsw==
            case 331:
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestAssembly.IValidator<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Nested+MyRecord")!)!, TestAssembly.GetType("TestAssembly.Nested+Validator")!));
                break;
            // FilePath: Input0.cs Expression: nGdrAptlVCEX9H6kHx0auw==
            case 336:
                break;
            // FilePath: Input0.cs Expression: iOVNY6dfLzra3vCDdW4AxA==
            case 341:
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.GenericService")!, TestAssembly.GetType("TestAssembly.GenericService")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.GenericServiceB, global::TestAssembly.GenericServiceB>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Nested.GenericServiceA, global::TestAssembly.Nested.GenericServiceA>());
                break;
            // FilePath: Input0.cs Expression: jm/nkIDlrjpFn5pT15jCnw==
            case 347:
                break;
            // FilePath: Input0.cs Expression: MeX+mF+ej5803TONO0SaMw==
            case 366:
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.GenericService")!, TestAssembly.GetType("TestAssembly.GenericService")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.Int32>>(a => (global::TestAssembly.IGenericService<global::System.Int32>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IGenericService<global::System.String>>(a => (global::TestAssembly.IGenericService<global::System.String>)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IOther>(a => (global::TestAssembly.IOther)a.GetRequiredService(TestAssembly.GetType("TestAssembly.GenericService")!)));
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
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Request")!, TestAssembly.GetType("TestAssembly.Request")!));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestAssembly.IRequest<>).MakeGenericType(TestAssembly.GetType("TestAssembly.Response")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.Request")!)));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.RequestHandler")!, TestAssembly.GetType("TestAssembly.RequestHandler")!));
                services.Add(ServiceDescriptor.Scoped(typeof(global::TestAssembly.IRequestHandler<, >).MakeGenericType(TestAssembly.GetType("TestAssembly.Request")!, TestAssembly.GetType("TestAssembly.Response")!)!, a => a.GetRequiredService(TestAssembly.GetType("TestAssembly.RequestHandler")!)));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.Response")!, TestAssembly.GetType("TestAssembly.Response")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.Service, global::TestAssembly.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IServiceB>(a => a.GetRequiredService<global::TestAssembly.Service>()));
                services.Add(ServiceDescriptor.Scoped(TestAssembly.GetType("TestAssembly.ServiceB")!, TestAssembly.GetType("TestAssembly.ServiceB")!));
                services.Add(ServiceDescriptor.Scoped<global::TestAssembly.IService>(a => (global::TestAssembly.IService)a.GetRequiredService(TestAssembly.GetType("TestAssembly.ServiceB")!)));
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _OtherProject;
    private Assembly OtherProject => _OtherProject ??= _context.LoadFromAssemblyName(new AssemblyName("OtherProject, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _RocketSurgeryDependencyInjectionExtensions;
    private Assembly RocketSurgeryDependencyInjectionExtensions => _RocketSurgeryDependencyInjectionExtensions ??= _context.LoadFromAssemblyName(new AssemblyName("Rocket.Surgery.DependencyInjection.Extensions, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _TestAssembly;
    private Assembly TestAssembly => _TestAssembly ??= _context.LoadFromAssemblyName(new AssemblyName("TestAssembly, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _TestProject;
    private Assembly TestProject => _TestProject ??= _context.LoadFromAssemblyName(new AssemblyName("TestProject, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CA5351, CS8618, CS8669, IL2026, IL2072
#nullable restore
