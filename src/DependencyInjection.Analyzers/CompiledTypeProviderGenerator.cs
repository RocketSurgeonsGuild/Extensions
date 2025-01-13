using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public static class Constants
{
    public const string CompiledTypeProviderCacheFileName = "CompiledTypeProvider.ctpjson";
}

/// <summary>
///     Source generate used for scanning assemblies for registrations
/// </summary>
[Generator]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CompiledTypeProviderGenerator : IIncrementalGenerator
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
#pragma warning disable RS1035
    private static string? GetCacheDirectory(AnalyzerConfigOptionsProvider options)
    {

/* Unmerged change from project 'Rocket.Surgery.DependencyInjection.Analyzers.roslyn4.8'
Before:
        var directory = options.GlobalOptions.TryGetValue("build_property.IntermediateOutputPath", out var intermediateOutputPath)
After:
        var directory = ( options.GlobalOptions.TryGetValue("build_property.IntermediateOutputPath", out var intermediateOutputPath) )
*/
        var directory = 
/* Unmerged change from project 'Rocket.Surgery.DependencyInjection.Analyzers.roslyn4.8'
Before:
        if (directory is null) return null;
        if (!Path.IsPathRooted(directory) && options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDirectory))
            directory = Path.Combine(projectDirectory, directory);
        var cacheDirectory = Path.Combine(directory, "ctp");
        if (!Directory.Exists(cacheDirectory)) Directory.CreateDirectory(cacheDirectory);
After:
        if (directory is null)
        {
            return null;
        }

        if (!Path.IsPathRooted(directory) && options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDirectory))
        {
            directory = Path.Combine(projectDirectory, directory);
        }

        var cacheDirectory = Path.Combine(directory, "ctp");
        if (!Directory.Exists(cacheDirectory))
        {
            _ = Directory.CreateDirectory(cacheDirectory);
*/
( options.GlobalOptions.TryGetValue("build_property.IntermediateOutputPath", out var intermediateOutputPath) )
            ? intermediateOutputPath
            : null;
        if (directory is null)
        {
            return null;
        }

        if (!Path.IsPathRooted(directory) && options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDirectory))
        {
            directory = Path.Combine(projectDirectory, directory);
        }

        var cacheDirectory = Path.Combine(directory, "ctp");
        if (!Directory.Exists(cacheDirectory))
        {
            _ = Directory.CreateDirectory(cacheDirectory);
        }

        return cacheDirectory;
    }
#pragma warning restore RS1035
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
                                   .AdditionalTextsProvider.Where(z => Path.GetFileName(z.Path).Equals(Constants.CompiledTypeProviderCacheFileName, StringComparison.OrdinalIgnoreCase))
                                   .Select(
                                        (text, _) =>
                                        {
                                            var source = text.GetText()?.ToString();

/* Unmerged change from project 'Rocket.Surgery.DependencyInjection.Analyzers.roslyn4.8'
Before:
                                            if (source is not { Length: > 100 })
                                            {
                                                return new(
                                                    ImmutableDictionary<string, CompiledAssemblyProviderData>.Empty,
                                                    ImmutableHashSet<string>.Empty,
                                                    ImmutableDictionary<string, GeneratedLocationAssemblyResolvedSourceCollection>.Empty
                                                );
                                            }

                                            return JsonSerializer.Deserialize(
                                                source,
                                                JsonSourceGenerationContext.Default.GeneratedAssemblyProviderData
                                            )!;
After:
                                            return ( source is not { Length: > 100 } )
                                                ? new(
                                                    ImmutableDictionary<string, CompiledAssemblyProviderData>.Empty,
                                                    [],
                                                    ImmutableDictionary<string, GeneratedLocationAssemblyResolvedSourceCollection>.Empty
                                                )
                                                : JsonSerializer.Deserialize(
                                                source,
                                                JsonSourceGenerationContext.Default.GeneratedAssemblyProviderData
                                            );
*/
                                            return ( source is not { Length: > 100 } )
                                                ?   new(
                                                    ImmutableDictionary<string, CompiledAssemblyProviderData>.Empty,
                                                    [],
                                                    ImmutableDictionary<string, GeneratedLocationAssemblyResolvedSourceCollection>.Empty
                                                )  
                                                :  JsonSerializer.Deserialize(
                                                source,
                                                JsonSourceGenerationContext.Default.GeneratedAssemblyProviderData
                                            );
                                        }
                                    )
                                   .Collect()
                                   .Select(
                                        (z, _) => z.SingleOrDefault()
                                         ?? new(
                                                ImmutableDictionary<string, CompiledAssemblyProviderData>.Empty,
                                                [],
                                                ImmutableDictionary<string, GeneratedLocationAssemblyResolvedSourceCollection>.Empty
                                            )
                                    );
        context.RegisterImplementationSourceOutput(
            context
               .CompilationProvider
               .Combine(context.AnalyzerConfigOptionsProvider)
               .Combine(collectionProvider)
               .Combine(generatedJsonProvider)
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
                HashSet<string> excludedAssemblies = ( request.options.GlobalOptions.TryGetValue("build_property.ExcludeAssemblyFromCTP", out var assemblies) )
                    ? [.. assemblies.Split([';', ','], StringSplitOptions.RemoveEmptyEntries)]
                    : [];
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

                var assemblySymbols = request
                                     .compilation
                                     .References
                                     .Select(request.compilation.GetAssemblyOrModuleSymbol)
                                     .Concat([request.compilation.Assembly])
                                     .Select(
                                          symbol =>
                                          {
                                              if (symbol is IAssemblySymbol assemblySymbol)
                                              {
                                                  return assemblySymbol;
                                              }

                                              if (symbol is IModuleSymbol moduleSymbol)
                                              {
                                                  return moduleSymbol.ContainingAssembly;
                                              }

                                              // ReSharper disable once NullableWarningSuppressionIsUsed
                                              return null!;
                                          }
                                      )
                                     .Where(z => z is { })
                                     .Where(z => excludedAssemblies.All(a => !z.MetadataName.StartsWith(a, StringComparison.OrdinalIgnoreCase)))
                                     .GroupBy(z => z.MetadataName, z => z, (s, symbols) => (Key: s, Symbol: symbols.First()))
                                     .ToImmutableDictionary(z => z.Key, z => z.Symbol);

                var resultingData = new ResultingAssemblyProviderData();

                var config = new AssemblyProviderConfiguration(
                    context,
                    request.compilation,
                    request.options,
                    request.additionalFiles,
                    resultingData
                );

                var (InternalAssemblyRequests, InternalReflectionRequests, ReflectionSources, InternalServiceDescriptorRequests, ServiceDescriptorSources) = config.FromAssemblyAttributes(
                    ref assemblySymbols,
                    reflectionRequests,
                    serviceDescriptorRequests,
                    diagnostics
                );

                assemblyRequests = assemblyRequests.AddRange(InternalAssemblyRequests);
                reflectionRequests = reflectionRequests.AddRange(InternalReflectionRequests);
                serviceDescriptorRequests = serviceDescriptorRequests.AddRange(InternalServiceDescriptorRequests);

                var assemblySources = AssemblyCollection.ResolveSources(
                    config,
                    request.compilation,
                    diagnostics,
                    assemblyRequests,
                    assemblySymbols
                );
                var reflectionSources = ReflectionCollection.ResolveSources(
                    config,
                    request.compilation,
                    diagnostics,
                    reflectionRequests,
                    request.compilation.Assembly
                );
                var serviceDescriptorSources = ServiceDescriptorCollection.ResolveSources(
                    config,
                    request.compilation,
                    diagnostics,
                    serviceDescriptorRequests,
                    request.compilation.Assembly
                );

                reflectionSources = reflectionSources.AddRange(ReflectionSources);
                serviceDescriptorSources = serviceDescriptorSources.AddRange(ServiceDescriptorSources);

                privateAssemblies.UnionWith(joinAssemblies(assemblySymbols, assemblySources));
                privateAssemblies.UnionWith(joinAssemblies(assemblySymbols, reflectionSources));
                privateAssemblies.UnionWith(joinAssemblies(assemblySymbols, serviceDescriptorSources));

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
                    Path.ChangeExtension(Constants.CompiledTypeProviderCacheFileName, ".g.cs"),
                    cu.NormalizeWhitespace().SyntaxTree.GetRoot().GetText(Encoding.UTF8)
                );

                if (!( GetCacheDirectory(request.options) is { } cacheDirectory ))
                {
                    return;
#pragma warning restore RS1035
                }

                var generatedData = resultingData.ToGeneratedAssemblyProviderData();
                var json = JsonSerializer.Serialize(generatedData, JsonSourceGenerationContext.Default.GeneratedAssemblyProviderData);
                var path = Path.Combine(cacheDirectory, Constants.CompiledTypeProviderCacheFileName);
#pragma warning disable RS1035
                File.WriteAllText(path, json);

                return;

                static IEnumerable<IAssemblySymbol> joinAssemblies(IEnumerable<KeyValuePair<string, IAssemblySymbol>> assemblies, IEnumerable<ResolvedSourceLocation> sources) => sources.SelectMany(z => z.PrivateAssemblies).Join(assemblies, z => z, z => z.Key, (_, a) => a.Value);
            }
        );
    }
}
