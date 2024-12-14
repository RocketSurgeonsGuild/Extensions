//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjExLCJhIjoiQTZ5ankxL3NEZkNaWXJ5Sjg0MUhFUT09IiwiZiI6IklucHV0MS5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOltdLCJkIjpbXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6dHJ1ZSwiYSI6IlRlc3RQcm9qZWN0IiwidCI6IklTZXJ2aWNlIiwidSI6ZmFsc2V9XSwibCI6W119LCJzIjp7IlNlcnZpY2VUeXBlRGVzY3JpcHRvcnMiOlt7IklkZW50aWZpZXIiOiJzIiwiVHlwZURhdGEiOm51bGwsIlR5cGVGaWx0ZXIiOm51bGx9LHsiSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH1dLCJMaWZldGltZSI6MH0sInoiOjB9")]
[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjExLCJhIjoiZmhOdi8xc25UMUVnTXhJVXQycWg3UT09IiwiZiI6IklucHV0Mi5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOltdLCJkIjpbXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6dHJ1ZSwiYSI6IlRlc3RQcm9qZWN0IiwidCI6IklTZXJ2aWNlQiIsInUiOmZhbHNlfV0sImwiOltdfSwicyI6eyJTZXJ2aWNlVHlwZURlc2NyaXB0b3JzIjpbeyJJZGVudGlmaWVyIjoicyIsIlR5cGVEYXRhIjpudWxsLCJUeXBlRmlsdGVyIjpudWxsfSx7IklkZW50aWZpZXIiOiJpIiwiVHlwZURhdGEiOm51bGwsIlR5cGVGaWx0ZXIiOm51bGx9XSwiTGlmZXRpbWUiOjF9LCJ6IjoxfQ==")]
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
            // FilePath: Input1.cs Expression: A6yjy1/sDfCZYryJ841HEQ==
            case 11:
                switch (System.IO.Path.GetFileName(filePath))
                {
                    // FilePath: Input1.cs Expression: A6yjy1/sDfCZYryJ841HEQ==
                    case "Input1.cs":
                        services.Add(ServiceDescriptor.Singleton<global::Service, global::Service>());
                        services.Add(ServiceDescriptor.Singleton<global::IService>(a => a.GetRequiredService<global::Service>()));
                        break;
                    // FilePath: Input2.cs Expression: fhNv/1snT1EgMxIUt2qh7Q==
                    case "Input2.cs":
                        services.Add(ServiceDescriptor.Scoped<global::ServiceB, global::ServiceB>());
                        services.Add(ServiceDescriptor.Scoped<global::IServiceB>(a => a.GetRequiredService<global::ServiceB>()));
                        break;
                }

                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
