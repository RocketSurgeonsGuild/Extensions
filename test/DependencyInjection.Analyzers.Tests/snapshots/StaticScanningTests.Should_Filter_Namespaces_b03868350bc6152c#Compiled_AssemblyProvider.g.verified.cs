//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjMyLCJhIjoiR1x1MDAyQmNMVG5FempKbmJOck51MFVJXHUwMDJCQlE9PSIsImYiOiJJbnB1dDAuY3MifSwiYSI6eyJhIjp0cnVlLCJpIjpmYWxzZSwibSI6W10sIm5hIjpbXSwiZCI6W119LCJ0Ijp7ImIiOjEsImMiOlt7ImYiOjIsIm4iOlsiVGVzdFByb2plY3QuQSJdfV0sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W10sImwiOltdfSwicyI6eyJTZXJ2aWNlVHlwZURlc2NyaXB0b3JzIjpbeyJJZGVudGlmaWVyIjoicyIsIlR5cGVEYXRhIjpudWxsLCJUeXBlRmlsdGVyIjpudWxsfSx7IklkZW50aWZpZXIiOiJpIiwiVHlwZURhdGEiOm51bGwsIlR5cGVGaWx0ZXIiOm51bGx9XSwiTGlmZXRpbWUiOjF9LCJ6IjoxfQ==")]
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
            // FilePath: Input0.cs Expression: G+cLTnEzjJnbNrNu0UI+BQ==
            case 32:
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.C.ServiceC, global::TestProject.A.C.ServiceC>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.C.ServiceC>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.Nested.ServiceA, global::TestProject.A.Nested.ServiceA>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.Nested.ServiceA>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.Service, global::TestProject.A.Service>());
                services.Add(ServiceDescriptor.Scoped<global::TestProject.A.IService>(a => a.GetRequiredService<global::TestProject.A.Service>()));
                services.Add(ServiceDescriptor.Scoped<global::TestProject.B.IServiceB>(a => a.GetRequiredService<global::TestProject.A.Service>()));
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
