using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public class Constants
{
    public const string AssemblyJsonExtension = ".gadjson";
    public const string SkipExtension = ".gadskip";
    public const string PartialExtension = ".gadpartial";
}

/// <summary>
///     Source generate used for scanning assemblies for registrations
/// </summary>
[Generator]
public class CompiledServiceScanningGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hasAssemblyLoadContext =
            context.CompilationProvider.Select((compilation, _) => compilation.GetTypeByMetadataName("System.Runtime.Loader.AssemblyLoadContext") is { });
        var assembliesSyntaxProvider = AssemblyCollection.Create(context.SyntaxProvider, hasAssemblyLoadContext);
        var reflectionSyntaxProvider = ReflectionCollection.Create(context.SyntaxProvider, hasAssemblyLoadContext);
        var serviceDescriptorSyntaxProvider = ServiceDescriptorCollection.Create(context.SyntaxProvider, hasAssemblyLoadContext);
        var collectionProvider = assembliesSyntaxProvider
                                .Combine(reflectionSyntaxProvider)
                                .Combine(serviceDescriptorSyntaxProvider)
                                .Select((z, _) => (assemblies: z.Left.Left, reflection: z.Left.Right, serviceDescriptors: z.Right));
        var generatedJsonProvider = context
                                   .AdditionalTextsProvider.Where(z => z.Path.EndsWith(Constants.AssemblyJsonExtension, StringComparison.OrdinalIgnoreCase))
                                   .Select(
                                        (text, _) =>
                                        {
                                            var source = text.GetText()?.ToString();
                                            if (source is not { Length: > 100 })
                                            {
                                                return (path: Path.GetFileName(text.Path), source: new([], [], []));
                                            }

                                            return (path: Path.GetFileName(text.Path),
                                                     source: JsonSerializer.Deserialize(
                                                         source,
                                                         JsonSourceGenerationContext.Default.CompiledAssemblyProviderData
                                                     )!
                                                );
                                        }
                                    )
                                   .Collect()
                                   .Select((z, _) => z.ToFrozenDictionary(static z => z.path, static z => z.source));
        var skipProvider = context
                          .AdditionalTextsProvider.Where(z => z.Path.EndsWith(Constants.SkipExtension, StringComparison.OrdinalIgnoreCase))
                          .Select((z, _) => Path.GetFileName(z.Path))
                          .Collect()
                          .Select((array, _) => array.ToFrozenSet());
        var partialProvider = context
                             .AdditionalTextsProvider.Where(z => z.Path.EndsWith(Constants.PartialExtension, StringComparison.OrdinalIgnoreCase))
                             .Collect()
                             .Select((z, _) => z.ToFrozenDictionary(static z => Path.GetFileName(z.Path), static z =>
                                                                                                              JsonSerializer.Deserialize(
                                                                                                                  z.GetText()?.ToString() ?? "",
                                                                                                                  JsonSourceGenerationContext.Default.SavedSourceLocation
                                                                                                              )!
                                                                                                              ));
        var additionalFilesProvider = generatedJsonProvider
                                     .Combine(skipProvider)
                                     .Combine(partialProvider)
                                     .Select((z, _) => (generatedJson: z.Left.Left, skip: z.Left.Right, partial: z.Right));
        context.RegisterImplementationSourceOutput(
            context
               .CompilationProvider
               .Combine(context.AnalyzerConfigOptionsProvider)
               .Combine(collectionProvider)
               .Combine(additionalFilesProvider)
               .Select(
                    (tuple, _) => (
                        compilation: tuple.Left.Left.Left,
                        options: tuple.Left.Left.Right,
                        tuple.Left.Right.assemblies,
                        tuple.Left.Right.reflection,
                        tuple.Left.Right.serviceDescriptors,
                        additionalFiles: tuple.Right
                    )
                ),
            static (context, request) =>
            {
                var privateAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
                var diagnostics = new HashSet<Diagnostic>();
                var assemblyRequests = AssemblyCollection.GetAssemblyItems(request.compilation, diagnostics, request.assemblies, context.CancellationToken);
                var reflectionRequests = ReflectionCollection.GetReflectionItems(
                    request.compilation,
                    diagnostics,
                    request.reflection,
                    context.CancellationToken
                );
                var serviceDescriptorRequests = ServiceDescriptorCollection.GetServiceDescriptorItems(
                    request.compilation,
                    diagnostics,
                    request.serviceDescriptors,
                    context.CancellationToken
                );
                var attributes = AssemblyProviderConfiguration.ToAssemblyAttributes(assemblyRequests, reflectionRequests, serviceDescriptorRequests).ToArray();

                var config = new AssemblyProviderConfiguration(context, request.compilation, request.options, request.additionalFiles.generatedJson, request.additionalFiles.skip, request.additionalFiles.partial);

                var assemblySymbols = request.compilation
                                             .References.Select(request.compilation.GetAssemblyOrModuleSymbol)
                                             .Concat([request.compilation.Assembly])
                                             .Select(
                                                  symbol =>
                                                  {
                                                      if (symbol is IAssemblySymbol assemblySymbol) return assemblySymbol;

                                                      if (symbol is IModuleSymbol moduleSymbol) return moduleSymbol.ContainingAssembly;

                                                      // ReSharper disable once NullableWarningSuppressionIsUsed
                                                      return null!;
                                                  }
                                              )
                                             .Where(z => z is { })
                                             .GroupBy(z => z.MetadataName, z => z, (s, symbols) => (Key: s, Symbol: symbols.First()))
                                             .ToImmutableDictionary(z => z.Key, z => z.Symbol);

                var resolvedData = config.FromAssemblyAttributes(
                    assemblySymbols,
                    reflectionRequests,
                    serviceDescriptorRequests,
                    diagnostics
                );

                assemblyRequests = assemblyRequests.AddRange(resolvedData.InternalAssemblyRequests);
                reflectionRequests = reflectionRequests.AddRange(resolvedData.InternalReflectionRequests);
                serviceDescriptorRequests = serviceDescriptorRequests.AddRange(resolvedData.InternalServiceDescriptorRequests);

                var assemblySources = AssemblyCollection.ResolveSources(
                    request.compilation,
                    diagnostics,
                    assemblyRequests
                );
                var reflectionSources = ReflectionCollection.ResolveSources(
                    request.compilation,
                    diagnostics,
                    reflectionRequests,
                    request.compilation.Assembly
                );
                var serviceDescriptorSources = ServiceDescriptorCollection.ResolveSources(
                    request.compilation,
                    diagnostics,
                    serviceDescriptorRequests,
                    request.compilation.Assembly
                );

                reflectionSources = reflectionSources.AddRange(resolvedData.ReflectionSources);
                serviceDescriptorSources = serviceDescriptorSources.AddRange(resolvedData.ServiceDescriptorSources);

                privateAssemblies.UnionWith(JoinAssemblies(assemblySymbols, assemblySources));
                privateAssemblies.UnionWith(JoinAssemblies(assemblySymbols, reflectionSources));
                privateAssemblies.UnionWith(JoinAssemblies(assemblySymbols, serviceDescriptorSources));

                static IEnumerable<IAssemblySymbol> JoinAssemblies(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, IAssemblySymbol>> assemblies, System.Collections.Generic.IEnumerable<ResolvedSourceLocation> sources)
                {
                    return sources.SelectMany(z => z.PrivateAssemblies).Join(assemblies, z => z, z => z.Key, (_, a) => a.Value);
                }

                var cu = CompilationUnit()
                   .WithUsings(
                        List(
                            [
                                UsingDirective(ParseName("System")),
                                UsingDirective(ParseName("System.Collections.Generic")),
                                UsingDirective(ParseName("System.Reflection")),
                                UsingDirective(ParseName("Microsoft.Extensions.DependencyInjection")),
                                UsingDirective(ParseName("Rocket.Surgery.DependencyInjection")),
                                UsingDirective(ParseName("Rocket.Surgery.DependencyInjection.Compiled")),
                            ]
                        )
                    );

                var assemblyProvider = AssemblyProviderBuilder.GetAssemblyProvider(
                    context,
                    request.compilation,
                    assemblySources,
                    reflectionSources,
                    serviceDescriptorSources,
                    privateAssemblies
                );
                if (privateAssemblies.Any())
                {
                    cu = cu.AddUsings(UsingDirective(ParseName("System.Runtime.Loader")));
                }

                MemberDeclarationSyntax[] members = [assemblyProvider];

                cu = cu
                    .AddSharedTrivia()
                    .AddAttributeLists(attributes)
                    .AddAttributeLists(
                         AttributeList(
                                 SingletonSeparatedList(
                                     Attribute(
                                         ParseName("Rocket.Surgery.DependencyInjection.Compiled.CompiledTypeProviderAttribute"),
                                         AttributeArgumentList(
                                             SingletonSeparatedList(
                                                 AttributeArgument(TypeOfExpression(ParseName(assemblyProvider.Identifier.Text)))
                                             )
                                         )
                                     )
                                 )
                             )
                            .WithTarget(AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)))
                     )
                    .AddMembers(members);

                foreach (var diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }

                context.AddSource(
                    "Compiled_AssemblyProvider.g.cs",
                    cu.NormalizeWhitespace().SyntaxTree.GetRoot().GetText(Encoding.UTF8)
                );
            }
        );
    }
}
