using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class AssemblyCollection
{
    public static IncrementalValueProvider<ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)>> Create(
        SyntaxValueProvider valueProvider,
        IncrementalValueProvider<bool> hasAssemblyLoadContext
    )
    {
        return valueProvider
              .CreateSyntaxProvider((node, _) => IsValidMethod(node), (syntaxContext, _) => GetMethod(syntaxContext))
              .Combine(hasAssemblyLoadContext)
              .Where(z => z is { Right: true, Left: { method: { }, selector: { } } })
              .Select((tuple, _) => tuple.Left)
              .Collect();
    }

    public static void Collect(
        SourceProductionContext context,
        CollectRequest request
    )
    {
        ( var discoveredAssemblyRequests, var discoveredReflectionRequests, var discoveredServiceDescriptorRequests ) =
            AssemblyProviderConfiguration.FromAssemblyAttributes(request.Compilation);

        var assemblyRequests = GetAssemblyDetails(context, request.Compilation, request.GetAssemblies);
        var reflectionRequests = ReflectionCollection.GetTypeDetails(context, request.Compilation, request.GetTypes);
        var serviceDescriptorRequests = ServiceDescriptorCollection.GetTypeDetails(context, request.Compilation, request.ScanResults);
        var attributes = AssemblyProviderConfiguration.ToAssemblyAttributes(assemblyRequests, reflectionRequests, serviceDescriptorRequests).ToArray();

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
                 )
                .AddAttributeLists(attributes);

        var privateAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
        var assemblyProvider = GetAssemblyProvider(
            context,
            request.Compilation,
            assemblyRequests.AddRange(discoveredAssemblyRequests),
            reflectionRequests.AddRange(discoveredReflectionRequests),
            serviceDescriptorRequests.AddRange(discoveredServiceDescriptorRequests),
            privateAssemblies
        );
        if (privateAssemblies.Any()) cu = cu.AddUsings(UsingDirective(ParseName("System.Runtime.Loader")));
        MemberDeclarationSyntax[] members = [assemblyProvider];

        cu = cu
            .AddSharedTrivia()
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


        context.AddSource(
            "Compiled_AssemblyProvider.g.cs",
            cu.NormalizeWhitespace().SyntaxTree.GetRoot().GetText(Encoding.UTF8)
        );
    }

    public static MethodDeclarationSyntax Execute(
        Request request
    )
    {
        if (!request.Items.Any()) return AssembliesMethod;
        var compilation = request.Compilation;

        var assemblySymbols = compilation
                             .References.Select(compilation.GetAssemblyOrModuleSymbol)
                             .Concat([compilation.Assembly])
                             .Select(
                                  symbol => symbol switch
                                            {
                                                IAssemblySymbol assemblySymbol => assemblySymbol,
                                                IModuleSymbol moduleSymbol     => moduleSymbol.ContainingAssembly,
                                                _                              => null!,
                                            }
                              )
                             .Where(z => z is { })
                             .ToImmutableHashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);

        var results = new List<(SourceLocation location, BlockSyntax block)>();

        foreach (var item in request.Items)
        {
            try
            {
                var filterAssemblies = assemblySymbols
                                      .Where(z => item.AssemblyFilter.IsMatch(compilation, z))
                                      .ToArray();

                if (filterAssemblies.Length == 0) continue;
                results.Add(( item.Location, GenerateDescriptors(compilation, filterAssemblies, request.PrivateAssemblies) ));
            }
            catch (Exception e)
            {
                request.Context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnhandledException, null, e.Message, e.StackTrace, e.GetType().Name));
            }
        }

        return results.Count == 0 ? AssembliesMethod : AssembliesMethod.WithBody(Block(SwitchGenerator.GenerateSwitchStatement(results)));
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel ) GetMethod(
        GeneratorSyntaxContext context
    )
    {
        var baseData = GetMethod(context.Node);
        if (baseData.method is null
         || baseData.selector is null
         || context.SemanticModel.GetTypeInfo(baseData.selector).ConvertedType is not INamedTypeSymbol
            {
                TypeArguments: [{ Name: IReflectionAssemblySelector }, ..],
            })
            return default;

        return ( baseData.method, baseData.selector, semanticModel: context.SemanticModel );
    }

    public static (InvocationExpressionSyntax method, ExpressionSyntax selector ) GetMethod(SyntaxNode node) =>
        node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: "GetAssemblies",
            },
            ArgumentList.Arguments: [{ Expression: { } expression }],
        } invocationExpressionSyntax
            ? ( invocationExpressionSyntax, expression )
            : default;

    private static bool IsValidMethod(SyntaxNode node) => GetMethod(node) is { method: { }, selector: { } };

    private static BlockSyntax GenerateDescriptors(Compilation compilation, IEnumerable<IAssemblySymbol> assemblies, HashSet<IAssemblySymbol> privateAssemblies)
    {
        var block = Block();
        foreach (var assembly in assemblies.OrderBy(z => z.ToDisplayString()))
        {
            // TODO: Make this always use the load context?
            if (AssemblyProviders.StatementGeneration.GetAssemblyExpression(compilation, assembly) is not { } assemblyExpression)
            {
                privateAssemblies.Add(assembly);
                block = block.AddStatements(
                    YieldStatement(SyntaxKind.YieldReturnStatement, AssemblyProviders.StatementGeneration.GetPrivateAssembly(assembly))
                );
                continue;
            }

            block = block.AddStatements(YieldStatement(SyntaxKind.YieldReturnStatement, assemblyExpression));
        }

        return block;
    }

    private static readonly MethodDeclarationSyntax AssembliesMethod =
        MethodDeclaration(
                GenericName(Identifier("IEnumerable"))
                   .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName("Assembly")))),
                Identifier("GetAssemblies")
            )
           .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName("ICompiledTypeProvider")))
           .AddParameterListParameters(
                Parameter(Identifier("action"))
                   .WithType(
                        GenericName(Identifier("Action"))
                           .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName("IReflectionAssemblySelector")
                                    )
                                )
                            )
                    ),
                Parameter(Identifier("lineNumber")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
                Parameter(Identifier("filePath")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                Parameter(Identifier("argumentExpression")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            )
           .WithBody(Block(SingletonList<StatementSyntax>(YieldStatement(SyntaxKind.YieldBreakStatement))));

    private static ImmutableArray<Item> GetAssemblyDetails(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<(InvocationExpressionSyntax expression, ExpressionSyntax selector, SemanticModel semanticModel)> results
    )
    {
        var items = ImmutableArray.CreateBuilder<Item>();
        foreach (var tuple in results)
        {
            try
            {
                ( var methodCallSyntax, var selector, var semanticModel ) = tuple;

                var assemblies = new List<IAssemblyDescriptor>();
                var typeFilters = new List<ITypeFilterDescriptor>();
                var serviceDescriptors = new List<IServiceTypeDescriptor>();
                var lifetime = 2;
                var classFilter = ClassFilter.All;


                DataHelpers.HandleInvocationExpressionSyntax(
                    context,
                    compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                    selector,
                    assemblies,
                    typeFilters,
                    serviceDescriptors,
                    ref lifetime,
                    ref classFilter,
                    context.CancellationToken
                );

                var assemblyFilter = new CompiledAssemblyFilter(assemblies.ToImmutableArray());

                var source = Helpers.CreateSourceLocation(methodCallSyntax, context.CancellationToken);
                // disallow list?
                if (source.FileName == "ConventionContextHelpers.cs") continue;

                var i = new Item(source, assemblyFilter);
                items.Add(i);
            }
            catch (MustBeAnExpressionException e)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAnExpression, e.Location));
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnhandledException, null, e.Message, e.StackTrace, e.GetType().Name));
            }
        }

        return items.ToImmutable();
    }

    private static TypeDeclarationSyntax GetAssemblyProvider(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<Item> getAssemblies,
        ImmutableArray<ReflectionCollection.Item> reflectionItems,
        ImmutableArray<ServiceDescriptorCollection.Item> implementationItems,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        var getAssembliesMethod = Execute(new(context, compilation, getAssemblies, privateAssemblies));
        var reflectionMethod = ReflectionCollection.Execute(new(context, compilation, reflectionItems, privateAssemblies));
        var serviceDescriptorMethod = ServiceDescriptorCollection.Execute(new(context, compilation, implementationItems, privateAssemblies));
        var privateMembers = privateAssemblies
                            .OrderBy(z => z.ToDisplayString())
                            .SelectMany(AssemblyProviders.StatementGeneration.AssemblyDeclaration)
                            .ToList();
        if (privateAssemblies.Any())
        {
            privateMembers.Insert(
                0,
                FieldDeclaration(
                        VariableDeclaration(IdentifierName("AssemblyLoadContext"))
                           .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier("_context"))
                                       .WithInitializer(
                                            EqualsValueClause(
                                                InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("AssemblyLoadContext"),
                                                            IdentifierName("GetLoadContext")
                                                        )
                                                    )
                                                   .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList(
                                                                Argument(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        TypeOfExpression(IdentifierName("CompiledTypeProvider")),
                                                                        IdentifierName("Assembly")
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                            )
                                        )
                                )
                            )
                    )
                   .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            );
        }

        return ClassDeclaration("CompiledTypeProvider")
              .AddAttributeLists(Helpers.CompilerGeneratedAttributes)
              .WithModifiers(TokenList(Token(SyntaxKind.FileKeyword)))
              .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName("ICompiledTypeProvider")))))
              .AddMembers(getAssembliesMethod, reflectionMethod, serviceDescriptorMethod)
              .AddMembers(privateMembers.ToArray());
    }

    private const string IReflectionAssemblySelector = nameof(IReflectionAssemblySelector);

    public record CollectRequest
    (
        Compilation Compilation,
        ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)> GetAssemblies,
        ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)> GetTypes,
        ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)> ScanResults
    );

    public record Request(SourceProductionContext Context, Compilation Compilation, ImmutableArray<Item> Items, HashSet<IAssemblySymbol> PrivateAssemblies);

    public record Item(SourceLocation Location, CompiledAssemblyFilter AssemblyFilter);
}
