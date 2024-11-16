using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal record CompiledAssemblyFilter
    (ImmutableArray<IAssemblyDescriptor> AssemblyDescriptors) : ICompiledTypeFilter<IAssemblySymbol>
{
    internal static readonly HashSet<string> _coreAssemblies =
    [
        "mscorlib",
        "netstandard",
        "System",
        "System.Core",
        "System.Runtime",
        "System.Private.CoreLib",
    ];

    private readonly bool _includeSystemAssemblies = AssemblyDescriptors.OfType<IncludeSystemAssembliesDescriptor>().Any();
    private readonly bool _allAssemblies = AssemblyDescriptors.OfType<AllAssemblyDescriptor>().Any();

    public bool IsMatch(Compilation compilation, IAssemblySymbol targetType)
    {
        if (!_includeSystemAssemblies && _coreAssemblies.Contains(targetType.Name)) return false;
        if (_allAssemblies) return true;

        return AssemblyDescriptors
           .Any(
                filter => filter switch
                          {
                              AssemblyDescriptor { Assembly: var assembly, } => SymbolEqualityComparer.Default.Equals(assembly, targetType),
                              AssemblyDependenciesDescriptor { Assembly: var assembly, } => targetType
                                                                                           .Modules.SelectMany(z => z.ReferencedAssemblySymbols)
                                                                                           .Any(
                                                                                                reference => SymbolEqualityComparer.Default.Equals(
                                                                                                    assembly,
                                                                                                    reference
                                                                                                )
                                                                                            ),
                              _ => false,
                          }
            );
    }
}
