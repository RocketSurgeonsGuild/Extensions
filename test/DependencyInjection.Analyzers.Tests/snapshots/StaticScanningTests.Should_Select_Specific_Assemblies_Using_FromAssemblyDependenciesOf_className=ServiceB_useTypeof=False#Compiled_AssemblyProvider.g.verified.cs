//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjIwLCJhIjoiOXhVQU1WOWQ5am5xdlhnd05wSVh2UT09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOmZhbHNlLCJpIjpmYWxzZSwibSI6W10sIm5hIjpbXSwiZCI6WyJEZXBlbmRlbmN5UHJvamVjdEIiXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6dHJ1ZSwiYSI6IlJvb3REZXBlbmRlbmN5UHJvamVjdCIsInQiOiJSb290RGVwZW5kZW5jeVByb2plY3QuSVNlcnZpY2UiLCJ1IjpmYWxzZX1dLCJsIjpbXX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6InMiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH0seyJJZGVudGlmaWVyIjoiaSIsIlR5cGVEYXRhIjpudWxsLCJUeXBlRmlsdGVyIjpudWxsfV0sIkxpZmV0aW1lIjowfSwieiI6MH0=")]
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
