//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ReflectionTypes", "eyJsIjp7ImwiOjE0LCJhIjoiT0FmM3pOaDNMTVIwYlRSQnpqSGlCQT09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOlsiUm9ja2V0LlN1cmdlcnkuRGVwZW5kZW5jeUluamVjdGlvbi5FeHRlbnNpb25zIl0sImQiOltdfSwidCI6eyJiIjoxLCJjIjpbeyJmIjozLCJuIjpbIkpldEJyYWlucy5Bbm5vdGF0aW9ucyIsIlBvbHlmaWxscyIsIlN5c3RlbSJdfV0sImQiOlt7ImkiOmZhbHNlLCJmIjowLCJuIjpbIlBvbHlmaWxsIl19XSwiZSI6W10sImYiOltdLCJnIjpbeyJpIjp0cnVlLCJhIjoiU3lzdGVtLlByaXZhdGUuQ29yZUxpYiIsImIiOiJTeXN0ZW0uQ29tcG9uZW50TW9kZWwuRWRpdG9yQnJvd3NhYmxlQXR0cmlidXRlIiwidSI6ZmFsc2V9XSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6ZmFsc2UsImEiOiJSb2NrZXQuU3VyZ2VyeS5EZXBlbmRlbmN5SW5qZWN0aW9uLkV4dGVuc2lvbnMiLCJ0IjoiUm9ja2V0LlN1cmdlcnkuRGVwZW5kZW5jeUluamVjdGlvbi5Db21waWxlZC5JQ29tcGlsZWRUeXBlUHJvdmlkZXIiLCJ1IjpmYWxzZX1dLCJsIjpbXX19")]
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
        return services;
    }
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
