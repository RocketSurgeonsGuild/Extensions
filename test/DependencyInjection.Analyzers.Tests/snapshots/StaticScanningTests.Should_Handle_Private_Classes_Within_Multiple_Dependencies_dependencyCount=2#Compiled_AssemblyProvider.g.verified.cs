﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

[assembly: System.Reflection.AssemblyMetadata("AssemblyProvider.ServiceDescriptorTypes", "eyJsIjp7ImwiOjE1LCJhIjoiTXBWblF3dU1yeDgyYjdRL3hkNk9CUT09IiwiZiI6IklucHV0MC5jcyJ9LCJhIjp7ImEiOnRydWUsImkiOmZhbHNlLCJtIjpbXSwibmEiOltdLCJkIjpbXX0sInQiOnsiYiI6MSwiYyI6W10sImQiOltdLCJlIjpbeyJmIjp0cnVlLCJ0IjpbMl19XSwiZiI6W3siZiI6ZmFsc2UsInQiOlstMiwxXX1dLCJnIjpbXSwiaCI6W10sImkiOltdLCJqIjpbXSwiayI6W3siaSI6dHJ1ZSwiYSI6IlJvb3REZXBlbmRlbmN5UHJvamVjdCIsInQiOiJSb290RGVwZW5kZW5jeVByb2plY3QuSVNlcnZpY2UiLCJ1IjpmYWxzZX1dLCJsIjpbXX0sInMiOnsiU2VydmljZVR5cGVEZXNjcmlwdG9ycyI6W3siSWRlbnRpZmllciI6ImkiLCJUeXBlRGF0YSI6bnVsbCwiVHlwZUZpbHRlciI6bnVsbH1dLCJMaWZldGltZSI6MH0sInoiOjB9")]
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
            // FilePath: Input0.cs Expression: MpVnQwuMrx82b7Q/xd6OBQ==
            case 15:
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency0Project.GetType("Dependency1Project.Service0")!));
                services.Add(ServiceDescriptor.Singleton(typeof(global::RootDependencyProject.IService), Dependency1Project.GetType("Dependency1Project.Service1")!));
                break;
        }

        return services;
    }

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _Dependency0Project;
    private Assembly Dependency0Project => _Dependency0Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency0Project, Version=version, Culture=neutral, PublicKeyToken=null"));

    private Assembly _Dependency1Project;
    private Assembly Dependency1Project => _Dependency1Project ??= _context.LoadFromAssemblyName(new AssemblyName("Dependency1Project, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
