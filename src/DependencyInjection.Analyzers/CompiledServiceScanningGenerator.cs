using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

/// <summary>
///     Source generate used for scanning assemblies for registrations
/// </summary>
[Generator]
public class CompiledServiceScanningGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hasAssemblyLoadContext =
            context.CompilationProvider.Select((compilation, _) => compilation.GetTypeByMetadataName("System.Runtime.Loader.AssemblyLoadContext") is { });
        var assembliesSyntaxProvider = AssemblyCollection.Create(context.SyntaxProvider, hasAssemblyLoadContext);
        var reflectionSyntaxProvider = ReflectionCollection.Create(context.SyntaxProvider, hasAssemblyLoadContext);
        var serviceDescriptorSyntaxProvider = ServiceDescriptorCollection.Create(context.SyntaxProvider, hasAssemblyLoadContext);
        context.RegisterImplementationSourceOutput(
            context
               .CompilationProvider
               .Combine(assembliesSyntaxProvider)
               .Combine(reflectionSyntaxProvider)
               .Combine(serviceDescriptorSyntaxProvider)
               .Select(
                    (tuple, token) => ( compilation: tuple.Left.Left.Left,
                                        assemblies: tuple.Left.Left.Right,
                                        reflection: tuple.Left.Right,
                                        serviceDescriptors: tuple.Right
                        )
                ),
            static (context, results) => AssemblyCollection.Collect(
                context,
                new(results.compilation, results.assemblies, results.reflection, results.serviceDescriptors)
            )
        );
    }
}
