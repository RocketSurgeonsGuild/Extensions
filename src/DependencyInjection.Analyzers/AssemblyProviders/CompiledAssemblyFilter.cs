using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal class CompiledAssemblyFilter(ImmutableList<IAssemblyDescriptor> assemblyDescriptors, SourceLocation? sourceLocation = null): ICompiledTypeFilter<IAssemblySymbol>
{
    public ImmutableList<IAssemblyDescriptor> AssemblyDescriptors { get; } = assemblyDescriptors;

    internal static readonly HashSet<string> coreAssemblies =
    [
        "mscorlib",
        "netstandard",
        "System",
        "System.Core",
        "System.Runtime",
        "System.Private.CoreLib",
    ];

    private readonly bool _includeSystemAssemblies = assemblyDescriptors.OfType<IncludeSystemAssembliesDescriptor>().Any();
    private readonly bool _allAssemblies = assemblyDescriptors.OfType<AllAssemblyDescriptor>().Any();

    public string Hash => sourceLocation?.ExpressionHash ?? Guid.NewGuid().ToString("N");

    public bool IsMatch(Compilation compilation, IAssemblySymbol targetType)
    {
        if (!_includeSystemAssemblies && coreAssemblies.Contains(targetType.Name)) return false;
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
