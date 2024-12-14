//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIxLCJhIjoiOEpzSWZXR3R5eHdicHdXZlBydGNiUT09IiwiZiI6IkNvbXBpbGVkVHlwZVByb3ZpZGVyU2VydmljZUNvbGxlY3Rpb25FeHRlbnNpb25zLmNzIn0sImEiOnsiYSI6dHJ1ZSwiaSI6ZmFsc2UsIm0iOltdLCJuYSI6W10sImQiOltdfSwidCI6eyJiIjoxLCJjIjpbXSwiZCI6W10sImUiOlt7ImYiOnRydWUsInQiOlsyXX1dLCJmIjpbeyJmIjpmYWxzZSwidCI6Wy0yLDFdfV0sImciOltdLCJoIjpbXSwiaSI6W3siaSI6dHJ1ZSwiYSI6IlJvY2tldC5TdXJnZXJ5LkRlcGVuZGVuY3lJbmplY3Rpb24uRXh0ZW5zaW9ucyIsImIiOiJSb2NrZXQuU3VyZ2VyeS5EZXBlbmRlbmN5SW5qZWN0aW9uLlNlcnZpY2VSZWdpc3RyYXRpb25BdHRyaWJ1dGUiLCJ1IjpmYWxzZX0seyJpIjp0cnVlLCJhIjoiUm9ja2V0LlN1cmdlcnkuRGVwZW5kZW5jeUluamVjdGlvbi5FeHRlbnNpb25zIiwiYiI6IlJvY2tldC5TdXJnZXJ5LkRlcGVuZGVuY3lJbmplY3Rpb24uU2VydmljZVJlZ2lzdHJhdGlvbkF0dHJpYnV0ZVx1MDA2MDIiLCJ1Ijp0cnVlfSx7ImkiOnRydWUsImEiOiJSb2NrZXQuU3VyZ2VyeS5EZXBlbmRlbmN5SW5qZWN0aW9uLkV4dGVuc2lvbnMiLCJiIjoiUm9ja2V0LlN1cmdlcnkuRGVwZW5kZW5jeUluamVjdGlvbi5TZXJ2aWNlUmVnaXN0cmF0aW9uQXR0cmlidXRlXHUwMDYwMyIsInUiOnRydWV9LHsiaSI6dHJ1ZSwiYSI6IlJvY2tldC5TdXJnZXJ5LkRlcGVuZGVuY3lJbmplY3Rpb24uRXh0ZW5zaW9ucyIsImIiOiJSb2NrZXQuU3VyZ2VyeS5EZXBlbmRlbmN5SW5qZWN0aW9uLlNlcnZpY2VSZWdpc3RyYXRpb25BdHRyaWJ1dGVcdTAwNjA0IiwidSI6dHJ1ZX1dLCJqIjpbXSwiayI6W10sImwiOltdfSwicyI6eyJTZXJ2aWNlVHlwZURlc2NyaXB0b3JzIjpbeyJJZGVudGlmaWVyIjoicyIsIlR5cGVEYXRhIjpudWxsLCJUeXBlRmlsdGVyIjpudWxsfV0sIkxpZmV0aW1lIjowfSwieiI6MH0=")]
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
            // FilePath: CompiledTypeProviderServiceCollectionExtensions.cs Expression: 8JsIfWGtyxwbpwWfPrtcbQ==
            case 21:
                services.Add(ServiceDescriptor.Transient<global::Nested.ServiceA, global::Nested.ServiceA>());
                services.Add(ServiceDescriptor.Transient<global::IService>(a => a.GetRequiredService<global::Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::Service, global::Service>());
                services.Add(ServiceDescriptor.Scoped<global::IServiceB>(a => a.GetRequiredService<global::Service>()));
                services.Add(ServiceDescriptor.Singleton<global::ServiceB, global::ServiceB>());
                services.Add(ServiceDescriptor.Singleton<global::IService>(a => a.GetRequiredService<global::ServiceB>()));
                services.Add(ServiceDescriptor.Singleton<global::IServiceB>(a => a.GetRequiredService<global::ServiceB>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
