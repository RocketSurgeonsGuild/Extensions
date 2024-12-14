//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoiYVx1MDAyQmI4ZXFYY0JXaWY5SkduSzR1cTVnPT0iLCJmIjoiSW5wdXQwLmNzIn0sImEiOnsiYSI6dHJ1ZSwiaSI6ZmFsc2UsIm0iOltdLCJuYSI6W10sImQiOltdfSwidCI6eyJiIjoxLCJjIjpbXSwiZCI6W10sImUiOlt7ImYiOnRydWUsInQiOlsyXX1dLCJmIjpbeyJmIjpmYWxzZSwidCI6Wy0yLDFdfV0sImciOltdLCJoIjpbXSwiaSI6W10sImoiOltdLCJrIjpbeyJpIjp0cnVlLCJhIjoiVGVzdFByb2plY3QiLCJ0IjoiSVNlcnZpY2VCIiwidSI6ZmFsc2V9XSwibCI6W3siaSI6dHJ1ZSwidCI6W3siYSI6IlRlc3RQcm9qZWN0IiwidCI6IklTZXJ2aWNlIiwidSI6ZmFsc2V9LHsiYSI6IlRlc3RQcm9qZWN0IiwidCI6IklTZXJ2aWNlQiIsInUiOmZhbHNlfV19XX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6eyJiIjoxLCJjIjpbXSwiZCI6W10sImUiOltdLCJmIjpbXSwiZyI6W10sImgiOltdLCJpIjpbXSwiaiI6W10sImsiOlt7ImkiOnRydWUsImEiOiJUZXN0UHJvamVjdCIsInQiOiJJU2VydmljZUIiLCJ1IjpmYWxzZX1dLCJsIjpbXX19XSwiTGlmZXRpbWUiOjF9LCJ6IjoxfQ==")]
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
            // FilePath: Input0.cs Expression: a+b8eqXcBWif9JGnK4uq5g==
            case 20:
                services.Add(ServiceDescriptor.Scoped<global::IServiceB, global::Service>());
                break;
        }

        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
