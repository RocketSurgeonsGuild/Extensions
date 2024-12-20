//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/Compiled_AssemblyProvider.g.cs
#nullable enable
#pragma warning disable CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;
using System.Runtime.Loader;

[assembly: Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute(typeof(CompiledTypeProvider))]
[System.CodeDom.Compiler.GeneratedCode("Rocket.Surgery.DependencyInjection.Analyzers", "version"), System.Runtime.CompilerServices.CompilerGenerated, System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
file class CompiledTypeProvider : ICompiledTypeProvider
{
    IEnumerable<Assembly> ICompiledTypeProvider.GetAssemblies(Action<IReflectionAssemblySelector> action, int lineNumber, string filePath, string argumentExpression)
    {
        switch (lineNumber)
        {
            // FilePath: Input0.cs Expression: w65l7hG8nk6L10nOXDqyzg==
            case 14:
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.ActivatorUtilities).Assembly;
                yield return mscorlib;
                yield return netstandard;
                yield return typeof(global::Program).Assembly;
                yield return typeof(global::Microsoft.Extensions.DependencyInjection.CompiledTypeProviderServiceCollectionExtensions).Assembly;
                yield return System;
                yield return typeof(global::System.IServiceProvider).Assembly;
                yield return SystemCore;
                yield return SystemPrivateCoreLib;
                yield return SystemRuntime;
                yield return TestProject;
                break;
        }

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

    private AssemblyLoadContext _context = AssemblyLoadContext.GetLoadContext(typeof(CompiledTypeProvider).Assembly)!;
    private Assembly _mscorlib;
    private Assembly mscorlib => _mscorlib ??= _context.LoadFromAssemblyName(new AssemblyName("mscorlib, Version=version, Culture=neutral, PublicKey=00000000000000000400000000000000"));

    private Assembly _netstandard;
    private Assembly netstandard => _netstandard ??= _context.LoadFromAssemblyName(new AssemblyName("netstandard, Version=version, Culture=neutral, PublicKey=00240000048000009400000006020000002400005253413100040000010001004b86c4cb78549b34bab61a3b1800e23bfeb5b3ec390074041536a7e3cbd97f5f04cf0f857155a8928eaa29ebfd11cfbbad3ba70efea7bda3226c6a8d370a4cd303f714486b6ebc225985a638471e6ef571cc92a4613c00b8fa65d61ccee0cbe5f36330c9a01f4183559f1bef24cc2917c6d913e3a541333a1d05d9bed22b38cb"));

    private Assembly _System;
    private Assembly System => _System ??= _context.LoadFromAssemblyName(new AssemblyName("System, Version=version, Culture=neutral, PublicKey=00000000000000000400000000000000"));

    private Assembly _SystemCore;
    private Assembly SystemCore => _SystemCore ??= _context.LoadFromAssemblyName(new AssemblyName("System.Core, Version=version, Culture=neutral, PublicKey=00000000000000000400000000000000"));

    private Assembly _SystemPrivateCoreLib;
    private Assembly SystemPrivateCoreLib => _SystemPrivateCoreLib ??= _context.LoadFromAssemblyName(new AssemblyName("System.Private.CoreLib, Version=version, Culture=neutral, PublicKey=00240000048000009400000006020000002400005253413100040000010001008d56c76f9e8649383049f383c44be0ec204181822a6c31cf5eb7ef486944d032188ea1d3920763712ccb12d75fb77e9811149e6148e5d32fbaab37611c1878ddc19e20ef135d0cb2cff2bfec3d115810c3d9069638fe4be215dbf795861920e5ab6f7db2e2ceef136ac23d5dd2bf031700aec232f6c6b1c785b4305c123b37ab"));

    private Assembly _SystemRuntime;
    private Assembly SystemRuntime => _SystemRuntime ??= _context.LoadFromAssemblyName(new AssemblyName("System.Runtime, Version=version, Culture=neutral, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293"));

    private Assembly _TestProject;
    private Assembly TestProject => _TestProject ??= _context.LoadFromAssemblyName(new AssemblyName("TestProject, Version=version, Culture=neutral, PublicKeyToken=null"));
}
#pragma warning restore CA1002, CA1034, CA1822, CS0105, CS1573, CS8618, CS8669
#nullable restore
