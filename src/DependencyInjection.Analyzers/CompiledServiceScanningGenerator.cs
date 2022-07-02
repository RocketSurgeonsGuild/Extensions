using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.Internals;
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
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax mae } ies
                      && mae.Name.Identifier.Text.EndsWith("ScanCompiled", StringComparison.Ordinal) && ies.ArgumentList.Arguments.Count is 1 or 2,
            (syntaxContext, _) => ( expression: ( (InvocationExpressionSyntax)syntaxContext.Node ).ArgumentList.Arguments[0].Expression,
                                    semanticModel: syntaxContext.SemanticModel )
        );

        var useAssemblyLoadContext = context.AnalyzerConfigOptionsProvider.Combine(context.CompilationProvider)
                                            .Select(
                                                 (tuple, _) =>
                                                 {
                                                     var (provider, compilation) = tuple;
                                                     return provider.GlobalOptions.TryGetValue("compiled_scan_assembly_load", out var v)
                                                         ? v.Equals("true", StringComparison.OrdinalIgnoreCase)
                                                         : compilation.GetTypeByMetadataName("System.Runtime.Loader.AssemblyLoadContext") is null;
                                                 }
                                             );
        // There is no way to post initialize here, as we have no idea if the AssemblyLoadContext will be available or not
//        context.RegisterPostInitializationOutput(
//            initializationContext => { initializationContext.AddSource("PartialCompiledServiceScanningExtensions.cs", staticScanPartialSourceText); }
//        );

        context.RegisterImplementationSourceOutput(
            useAssemblyLoadContext.Combine(syntaxProvider.Collect()),
            static (context, tuple) =>
            {
                if (tuple.Right.Length > 0)
                {
                    context.AddSource("CompiledServiceScanningExtensions.cs", tuple.Left ? StaticScanSourceText : StaticScanSourceTextWithAssemblyLoadContext);
                    return;
                }

                context.AddSource("PopulateExtensions.cs", tuple.Left ? PopulateSourceText : PopulateSourceTextWithAssemblyLoadContext);
            }
        );


        context.RegisterImplementationSourceOutput(
            syntaxProvider
               .Combine(
                    useAssemblyLoadContext
                       .Combine(context.ParseOptionsProvider.Select((options, token) => (CSharpParseOptions)options))
                       .Combine(context.CompilationProvider)
                       .Select(
                            (tuple, token) =>
                            {
                                var (_, compilation) = tuple;
                                var (useAssemblyLoad, parseOptions) = tuple.Left;
                                // This is required because post init cannpt tell us if AssemblyLoadContext is available or not.
                                return ( useAssemblyLoad, parseOptions, compilation: compilation
                                            .AddSyntaxTrees(
                                                 CSharpSyntaxTree.ParseText(
                                                     useAssemblyLoad ? StaticScanSourceText : StaticScanSourceTextWithAssemblyLoadContext, parseOptions,
                                                     "CompiledServiceScanningExtensions.cs", cancellationToken: token
                                                 ),
                                                 CSharpSyntaxTree.ParseText(
                                                     useAssemblyLoad ? PopulateSourceText : PopulateSourceTextWithAssemblyLoadContext, parseOptions,
                                                     "PopulateExtensions.cs", cancellationToken: token
                                                 )
                                             ) );
                            }
                        )
                )
               .Select(
                    (tuple, token) => ( tuple.Left.expression, tuple.Right.compilation,
                                        useAssemblyLoadContext: tuple.Right.useAssemblyLoad, tuple.Right.parseOptions )
                )
               .Select(
                    static (tuple, token) =>
                    {
                        var (rootExpression, compilation, useAssemblyLoadContext, parseOptions) = tuple;

                        var assemblies = new List<IAssemblyDescriptor>();
                        var typeFilters = new List<ITypeFilterDescriptor>();
                        var serviceTypes = new List<IServiceTypeDescriptor>();
                        var diagnostics = new List<Diagnostic>();
                        var classFilter = ClassFilter.All;
                        var lifetimeExpressionSyntax =
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("ServiceLifetime"),
                                IdentifierName("Transient")
                            );

                        DataHelpers.HandleInvocationExpressionSyntax(
                            diagnostics,
                            compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                            rootExpression,
                            assemblies,
                            typeFilters,
                            serviceTypes,
                            compilation.ObjectType,
                            ref classFilter,
                            ref lifetimeExpressionSyntax,
                            token
                        );

                        var containingMethod = rootExpression.Ancestors().OfType<MethodDeclarationSyntax>().First();

                        var methodCallSyntax = rootExpression.Ancestors()
                                                             .OfType<InvocationExpressionSyntax>()
                                                             .First(
                                                                  ies => ies.Expression is MemberAccessExpressionSyntax mae
                                                                      && mae.Name.ToString().EndsWith("ScanCompiled", StringComparison.Ordinal)
                                                              );


                        if (serviceTypes.Count == 0)
                        {
                            serviceTypes.Add(new SelfServiceTypeDescriptor());
                        }

                        return (
                            context: tuple,
                            data: (
                                diagnostics,
                                rootExpression,
                                filePath: rootExpression.SyntaxTree.FilePath,
                                containingMethod: containingMethod.Identifier.Text,
                                // line numbers here are 1 based
                                lineNumber: methodCallSyntax.SyntaxTree.GetText(token).Lines.First(z => z.Span.IntersectsWith(methodCallSyntax.Span)).LineNumber
                                          + 1,
                                assemblies,
                                typeFilters,
                                serviceTypes,
                                classFilter,
                                lifetimeExpressionSyntax
                            ) );
                    }
                )
               .Collect()
               .Select(
                    (array, token) =>
                    {
                        var d = array.FirstOrDefault();
                        return ( d.context.compilation, d.context.expression, d.context.useAssemblyLoadContext, d.context.parseOptions,
                                 d.data.diagnostics, groups: array.GroupBy(z => z.data.lineNumber, z => z.data) );
                    }
                )
           ,
            static (context, tuple) =>
            {
                if (tuple.diagnostics == null) return;
                Execute(
                    context, tuple.compilation, tuple.expression, tuple.useAssemblyLoadContext, tuple.parseOptions, tuple.diagnostics,
                    tuple.groups
                );
            }
        );
    }

    private static void Execute(
        SourceProductionContext context, Compilation compilation, ExpressionSyntax expression, bool useAssemblyLoadContext,
        ParseOptions parseOptions, List<Diagnostic> diagnostics,
        IEnumerable<IGrouping<int, (List<Diagnostic> diagnostics, ExpressionSyntax rootExpression, string filePath, string containingMethod, int lineNumber,
            List<IAssemblyDescriptor> assemblies, List<ITypeFilterDescriptor> typeFilters, List<IServiceTypeDescriptor> serviceTypes, ClassFilter classFilter,
            MemberAccessExpressionSyntax lifetimeExpressionSyntax)>> groups
    )
    {
        if (diagnostics.Any())
        {
            foreach (var diag in diagnostics)
            {
                context.ReportDiagnostic(diag);
            }

            return;
        }

        var allNamedTypes = TypeSymbolVisitor.GetTypes(compilation);

        var strategyName = IdentifierName("strategy");
        var serviceCollectionName = IdentifierName("services");
        var lineNumberIdentifier = IdentifierName("lineNumber");
        var block = Block();
#pragma warning disable RS1024
        var privateAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024

        var switchStatement = SwitchStatement(lineNumberIdentifier);
        foreach (var lineGrouping in groups)
        {
            var innerBlock = Block();
            var blocks = new List<(string filePath, string memberName, BlockSyntax block)>();
            foreach (var (_, _, filePath, memberName, _, assemblies, typeFilters, serviceTypes, classFilter, lifetime) in lineGrouping)
            {
                var types = NarrowListOfTypes(assemblies, allNamedTypes, compilation, classFilter, typeFilters);

                var localBlock = GenerateDescriptors(
                    context,
                    compilation,
                    types,
                    serviceTypes,
                    innerBlock,
                    strategyName,
                    serviceCollectionName,
                    lifetime,
                    privateAssemblies,
                    useAssemblyLoadContext
                );

                blocks.Add(( filePath, memberName, localBlock ));
            }

            static SwitchSectionSyntax createNestedSwitchSections<T>(
                IReadOnlyList<(string filePath, string memberName, BlockSyntax block)> blocks,
                NameSyntax identifier,
                Func<(string filePath, string memberName, BlockSyntax block), T> regroup,
                Func<IGrouping<T, (string filePath, string memberName, BlockSyntax block)>, SwitchSectionSyntax> next,
                Func<T, LiteralExpressionSyntax> literalFactory
            )
            {
                if (blocks.Count == 1)
                {
                    var (_, _, localBlock) = blocks[0];
                    return SwitchSection()
                          .AddStatements(localBlock.Statements.ToArray())
                          .AddStatements(BreakStatement());
                }

                var section = SwitchStatement(identifier);
                foreach (var item in blocks.GroupBy(regroup))
                {
                    section = section.AddSections(next(item).AddLabels(CaseSwitchLabel(literalFactory(item.Key))));
                }

                return SwitchSection().AddStatements(section, BreakStatement());
            }

            var lineSwitchSection = createNestedSwitchSections(
                    blocks,
                    IdentifierName("filePath"),
                    x => x.filePath,
                    generateFilePathSwitchStatement,
                    value =>
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(value)
                        )
                )
               .AddLabels(
                    CaseSwitchLabel(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(lineGrouping.Key)
                        )
                    )
                );

            static SwitchSectionSyntax generateFilePathSwitchStatement(IGrouping<string, (string filePath, string memberName, BlockSyntax block)> innerGroup)
            {
                return createNestedSwitchSections(
                    innerGroup.ToArray(),
                    IdentifierName("memberName"),
                    x => x.memberName,
                    generateMemberNameSwitchStatement,
                    value =>
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(value)
                        )
                );
            }

            static SwitchSectionSyntax generateMemberNameSwitchStatement(IGrouping<string, (string filePath, string memberName, BlockSyntax block)> innerGroup)
            {
                return SwitchSection()
                      .AddLabels(
                           CaseSwitchLabel(
                               LiteralExpression(
                                   SyntaxKind.StringLiteralExpression,
                                   Literal(innerGroup.Key)
                               )
                           )
                       )
                      .AddStatements(innerGroup.FirstOrDefault().block?.Statements.ToArray() ?? Array.Empty<StatementSyntax>())
                      .AddStatements(BreakStatement());
            }


            switchStatement = switchStatement.AddSections(lineSwitchSection);
        }


        {
            var root = CSharpSyntaxTree.ParseText(
                                            useAssemblyLoadContext ? PopulateSourceText : PopulateSourceTextWithAssemblyLoadContext,
                                            (CSharpParseOptions)parseOptions
                                        )
                                       .GetCompilationUnitRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            var newMethod = method
               .WithBody(block.AddStatements(switchStatement).AddStatements(method.Body!.Statements.ToArray()));

            root = root.ReplaceNode(method, newMethod);

            if (privateAssemblies.Any())
            {
                var @class = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
                var privateAssemblyNodes = privateAssemblies
                   .SelectMany(StatementGeneration.AssemblyDeclaration);
                root = root.ReplaceNode(@class, @class.AddMembers(privateAssemblyNodes.ToArray()));
            }

            context.AddSource("PopulateExtensions.cs", root.NormalizeWhitespace().GetText(Encoding.UTF8));
        }
    }

    private static BlockSyntax GenerateDescriptors(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> types,
        List<IServiceTypeDescriptor> serviceTypes,
        BlockSyntax innerBlock,
        IdentifierNameSyntax strategyName,
        IdentifierNameSyntax serviceCollectionName,
        ExpressionSyntax lifetime,
        HashSet<IAssemblySymbol> privateAssemblies,
        bool useAssemblyLoad
    )
    {
        var asSelf = serviceTypes.OfType<SelfServiceTypeDescriptor>().Any() || !serviceTypes.Any();
        var asImplementedInterfaces = serviceTypes.OfType<ImplementedInterfacesServiceTypeDescriptor>().Any();
        var asMatchingInterface = serviceTypes.OfType<MatchingInterfaceServiceTypeDescriptor>().Any();
        var asSpecificTypes = serviceTypes.OfType<CompiledServiceTypeDescriptor>().Select(z => z.Type).ToArray();
        var usingAttributes = serviceTypes.OfType<UsingAttributeServiceTypeDescriptor>().Any();
        var serviceDescriptorAttribute = compilation.GetTypeByMetadataName("Scrutor.ServiceDescriptorAttribute")!;
        var serviceRegistrationAttribute = compilation.GetTypeByMetadataName("Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute")!;

        foreach (var type in types)
        {
#pragma warning disable RS1024
            var emittedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024
            var typeIsOpenGeneric = type.IsOpenGenericType();
            if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
            {
                privateAssemblies.Add(type.ContainingAssembly);
            }

            if (usingAttributes)
            {
                var attributeDataElements = type.GetAttributes()
                                                .Where(
                                                     attribute => attribute.AttributeClass != null &&
                                                                  ( SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serviceDescriptorAttribute)
                                                                  ||
                                                                    SymbolEqualityComparer.Default.Equals(
                                                                        attribute.AttributeClass, serviceRegistrationAttribute
                                                                    ) )
                                                 )
                                                .ToArray();

                var duplicates = attributeDataElements
                                .Where(
                                     attribute => attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Type
                                 )
                                .GroupBy(attribute => attribute.ConstructorArguments[0].Value as INamedTypeSymbol, SymbolEqualityComparer.Default)
                                .SelectMany(grp => grp.Skip(1))
                                .ToArray();
                foreach (var duplicate in duplicates)
                {
                    // If there is no syntax it is probably not our code
                    if (duplicate.ApplicationSyntaxReference == null)
                        continue;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Diagnostics.DuplicateServiceDescriptorAttribute,
                            Location.Create(duplicate.ApplicationSyntaxReference.SyntaxTree, duplicate.ApplicationSyntaxReference.Span)
                        )
                    );
                }

                foreach (var attribute in attributeDataElements)
                {
                    if (attribute.AttributeClass == null)
                        continue;
                    var isServiceDescriptor = SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serviceDescriptorAttribute);
                    var isServiceRegistration = SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serviceRegistrationAttribute);
                    if (!( isServiceDescriptor || isServiceRegistration ))
                        continue;

                    INamedTypeSymbol? attributeServiceType = null;
                    var lifetimeValue = lifetime;

                    if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Enum)
                    {
                        var members = attribute.ConstructorArguments[0].Type!
                                               .GetMembers()
                                               .OfType<IFieldSymbol>();
                        if (attribute.ConstructorArguments[0].Value is int v)
                        {
                            var value = members
                               .First(z => z.ConstantValue is int i && i == v);
                            lifetimeValue = MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("ServiceLifetime"),
                                IdentifierName(value.Name)
                            );
                        }
                    }
                    else if (attribute.ConstructorArguments.Length == 2 && attribute.ConstructorArguments[1].Kind == TypedConstantKind.Enum)
                    {
                        var members = attribute.ConstructorArguments[1].Type!
                                               .GetMembers()
                                               .OfType<IFieldSymbol>();
                        if (attribute.ConstructorArguments[1].Value is int v)
                        {
                            var value = members
                               .First(z => z.ConstantValue is int i && i == v);
                            lifetimeValue = MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("ServiceLifetime"),
                                IdentifierName(value.Name)
                            );
                        }
                    }

                    if (attribute.ConstructorArguments.Length == 0 ||
                        ( isServiceRegistration && attribute.ConstructorArguments.Length == 1
                                                && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Enum ) ||
                        ( isServiceDescriptor && attribute.ConstructorArguments.Length == 2 && attribute.ConstructorArguments[1].Kind == TypedConstantKind.Enum
                       && attribute.ConstructorArguments[0].Value == null ))
                    {
                        if (!emittedTypes.Contains(type))
                        {
                            innerBlock = innerBlock.AddStatements(
                                ExpressionStatement(
                                    StatementGeneration.GenerateServiceType(
                                        compilation,
                                        strategyName,
                                        serviceCollectionName,
                                        type,
                                        type,
                                        lifetimeValue,
                                        useAssemblyLoad
                                    )
                                )
                            );

                            if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
                            {
                                privateAssemblies.Add(type.ContainingAssembly);
                            }

                            emittedTypes.Add(type);
                        }

                        foreach (var @interface in type.AllInterfaces)
                        {
                            if (!emittedTypes.Contains(@interface))
                            {
                                innerBlock = innerBlock.AddStatements(
                                    ExpressionStatement(
                                        StatementGeneration.GenerateServiceFactory(
                                            compilation,
                                            strategyName,
                                            serviceCollectionName,
                                            @interface,
                                            type,
                                            lifetimeValue,
                                            useAssemblyLoad
                                        )
                                    )
                                );
                                emittedTypes.Add(@interface);
                            }
                        }

                        foreach (var baseType in Helpers.GetBaseTypes(compilation, type))
                        {
                            if (!emittedTypes.Contains(baseType))
                            {
                                innerBlock = innerBlock.AddStatements(
                                    ExpressionStatement(
                                        StatementGeneration.GenerateServiceFactory(
                                            compilation,
                                            strategyName,
                                            serviceCollectionName,
                                            baseType,
                                            type,
                                            lifetimeValue,
                                            useAssemblyLoad
                                        )
                                    )
                                );
                                emittedTypes.Add(baseType);
                            }
                        }
                    }

                    if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Type)
                    {
                        attributeServiceType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
                    }

                    if (attribute.ConstructorArguments.Length == 2)
                    {
                        attributeServiceType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
                    }

                    if (attributeServiceType != null)
                    {
                        innerBlock = innerBlock.AddStatements(
                            ExpressionStatement(
                                asSelf
                                    ? StatementGeneration.GenerateServiceFactory(
                                        compilation,
                                        strategyName,
                                        serviceCollectionName,
                                        attributeServiceType,
                                        type,
                                        lifetimeValue,
                                        useAssemblyLoad
                                    )
                                    : StatementGeneration.GenerateServiceType(
                                        compilation,
                                        strategyName,
                                        serviceCollectionName,
                                        attributeServiceType,
                                        type,
                                        lifetimeValue,
                                        useAssemblyLoad
                                    )
                            )
                        );
                        if (!compilation.IsSymbolAccessibleWithin(attributeServiceType, compilation.Assembly))
                        {
                            privateAssemblies.Add(type.ContainingAssembly);
                        }

                        emittedTypes.Add(attributeServiceType);
                    }
                }
            }

            if (asSelf && !emittedTypes.Contains(type))
            {
                innerBlock = innerBlock.AddStatements(
                    ExpressionStatement(
                        StatementGeneration.GenerateServiceType(
                            compilation,
                            strategyName,
                            serviceCollectionName,
                            type,
                            type,
                            lifetime,
                            useAssemblyLoad
                        )
                    )
                );
                if (!compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
                {
                    privateAssemblies.Add(type.ContainingAssembly);
                }

                emittedTypes.Add(type);
            }

            if (asMatchingInterface)
            {
                var name = $"I{type.Name}";
                var @interface = type.AllInterfaces.FirstOrDefault(z => z.Name == name);
                if (@interface is not null && !emittedTypes.Contains(@interface))
                {
                    innerBlock = innerBlock.AddStatements(
                        ExpressionStatement(
                            typeIsOpenGeneric || !asSelf
                                ? StatementGeneration.GenerateServiceType(
                                    compilation,
                                    strategyName,
                                    serviceCollectionName,
                                    @interface,
                                    type,
                                    lifetime,
                                    useAssemblyLoad
                                )
                                : StatementGeneration.GenerateServiceFactory(
                                    compilation,
                                    strategyName,
                                    serviceCollectionName,
                                    @interface,
                                    type,
                                    lifetime,
                                    useAssemblyLoad
                                )
                        )
                    );
                    if (!compilation.IsSymbolAccessibleWithin(@interface, compilation.Assembly))
                    {
                        privateAssemblies.Add(type.ContainingAssembly);
                    }

                    emittedTypes.Add(@interface);
                }
            }

            if (asImplementedInterfaces)
            {
                foreach (var @interface in type.AllInterfaces)
                {
                    if (!emittedTypes.Contains(@interface))
                    {
                        innerBlock = innerBlock.AddStatements(
                            ExpressionStatement(
                                typeIsOpenGeneric || !asSelf
                                    ? StatementGeneration.GenerateServiceType(
                                        compilation,
                                        strategyName,
                                        serviceCollectionName,
                                        @interface,
                                        type,
                                        lifetime,
                                        useAssemblyLoad
                                    )
                                    : StatementGeneration.GenerateServiceFactory(
                                        compilation,
                                        strategyName,
                                        serviceCollectionName,
                                        @interface,
                                        type,
                                        lifetime,
                                        useAssemblyLoad
                                    )
                            )
                        );
                        if (!compilation.IsSymbolAccessibleWithin(@interface, compilation.Assembly))
                        {
                            privateAssemblies.Add(type.ContainingAssembly);
                        }

                        emittedTypes.Add(@interface);
                    }
                }
            }

            foreach (var asType in asSpecificTypes)
            {
                if (!emittedTypes.Contains(asType))
                {
                    innerBlock = innerBlock.AddStatements(
                        ExpressionStatement(
                            !asSelf
                                ? StatementGeneration.GenerateServiceType(
                                    compilation,
                                    strategyName,
                                    serviceCollectionName,
                                    asType,
                                    type,
                                    lifetime,
                                    useAssemblyLoad
                                )
                                : StatementGeneration.GenerateServiceFactory(
                                    compilation,
                                    strategyName,
                                    serviceCollectionName,
                                    asType,
                                    type,
                                    lifetime,
                                    useAssemblyLoad
                                )
                        )
                    );
                    emittedTypes.Add(asType);
                }
            }
        }

        return innerBlock;
    }

    private static ImmutableArray<INamedTypeSymbol> NarrowListOfTypes(
        List<IAssemblyDescriptor> assemblies,
        ImmutableArray<INamedTypeSymbol> iNamedTypeSymbols,
        Compilation compilation,
        ClassFilter classFilter,
        List<ITypeFilterDescriptor> typeFilters
    )
    {
        ImmutableArray<INamedTypeSymbol> types;
        if (assemblies.OfType<AllAssemblyDescriptor>().Any())
        {
            types = iNamedTypeSymbols;
        }
        else
        {
            var assemblyReferences = assemblies
                                    .OfType<CompiledAssemblyDependenciesDescriptor>()
                                    .SelectMany(
                                         descriptor => compilation.References
                                                                  .Select(compilation.GetAssemblyOrModuleSymbol)
                                                                  .SelectMany(
                                                                       z => z is IAssemblySymbol assemblySymbol ? assemblySymbol.Modules :
                                                                           z is IModuleSymbol moduleSymbol ? new[] { moduleSymbol } :
                                                                           Array.Empty<IModuleSymbol>()
                                                                   )
                                                                  .Where(
                                                                       module => module.ReferencedAssemblySymbols.Any(
                                                                           reference =>
                                                                               SymbolEqualityComparer.Default.Equals(
                                                                                   descriptor.TypeFromAssembly.ContainingAssembly, reference
                                                                               )
                                                                       )
                                                                   )
                                                                  .Select(z => z.ContainingAssembly)
                                                                  .Distinct(SymbolEqualityComparer.Default)
                                     )
                                    .ToArray();

            var allAssemblyReferences = assemblyReferences
                                       .Concat(assemblies.OfType<CompiledAssemblyDescriptor>().Select(z => z.TypeFromAssembly.ContainingAssembly))
                                       .Distinct(SymbolEqualityComparer.Default);

            types = TypeSymbolVisitor.GetTypes(compilation, allAssemblyReferences);
        }

        if (classFilter == ClassFilter.PublicOnly)
        {
            types = types.RemoveAll(symbol => symbol.DeclaredAccessibility != Accessibility.Public);
        }

        // Ran into a filter we couldn't handle, so we bail out and let the diagnostics handle things.
        if (typeFilters.OfType<CompiledAbortTypeFilterDescriptor>().Any())
        {
            types = types.Clear();
            return types;
        }

        foreach (var filter in typeFilters.OfType<CompiledAssignableToTypeFilterDescriptor>())
        {
            types = types.RemoveAll(toSymbol => StatementGeneration.RemoveImplicitGenericConversion(compilation, filter.Type, toSymbol));
        }

        var anyFilters = typeFilters.OfType<CompiledAssignableToAnyTypeFilterDescriptor>().ToArray();
        if (anyFilters.Length > 0)
        {
            types = types.RemoveAll(
                toSymbol => anyFilters.All(filter => StatementGeneration.RemoveImplicitGenericConversion(compilation, filter.Type, toSymbol))
            );
        }

        foreach (var filter in typeFilters.OfType<CompiledWithAttributeFilterDescriptor>())
        {
            types = types.RemoveAll(toSymbol => !toSymbol.GetAttributes().Any(z => SymbolEqualityComparer.Default.Equals(z.AttributeClass, filter.Attribute)));
        }

        foreach (var filter in typeFilters.OfType<CompiledWithoutAttributeFilterDescriptor>())
        {
            types = types.RemoveAll(toSymbol => toSymbol.GetAttributes().Any(z => SymbolEqualityComparer.Default.Equals(z.AttributeClass, filter.Attribute)));
        }

        foreach (var filter in typeFilters.OfType<NamespaceFilterDescriptor>())
        {
            types = filter.Filter switch
            {
                NamespaceFilter.Exact => types.RemoveAll(toSymbol => filter.Namespaces.All(ns => toSymbol.ContainingNamespace.ToDisplayString() != ns)),
                NamespaceFilter.In => types.RemoveAll(
                    toSymbol => !filter.Namespaces.Any(n => toSymbol.ContainingNamespace.ToDisplayString().StartsWith(n, StringComparison.Ordinal))
                ),
                NamespaceFilter.NotIn => types.RemoveAll(
                    toSymbol => filter.Namespaces.Any(n => toSymbol.ContainingNamespace.ToDisplayString().StartsWith(n, StringComparison.Ordinal))
                ),
                _ => types
            };
        }

        foreach (var filter in typeFilters.OfType<NameFilterDescriptor>())
        {
            types = filter.Filter switch
            {
                TextDirectionFilter.Contains => types.RemoveAll(toSymbol => !filter.Names.Any(name => toSymbol.Name.Contains(name) && toSymbol.Arity == 0)),
                TextDirectionFilter.StartsWith => types.RemoveAll(
                    toSymbol => !filter.Names.Any(name => toSymbol.Name.StartsWith(name, StringComparison.Ordinal) && toSymbol.Arity == 0)
                ),
                TextDirectionFilter.EndsWith => types.RemoveAll(
                    toSymbol => !filter.Names.Any(name => toSymbol.Name.EndsWith(name, StringComparison.Ordinal) && toSymbol.Arity == 0)
                ),
                _ => types
            };
        }

        return types;
    }

    private const string StaticScanSourceTextWithAssemblyLoadContext = @"
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Scrutor;
using Rocket.Surgery.DependencyInjection.Compiled;
#pragma warning disable CS0436
namespace Microsoft.Extensions.DependencyInjection
{
    [CompilerGenerated, ExcludeFromCodeCoverage]
    internal static class CompiledServiceScanningExtensions
    {
        public static IServiceCollection ScanCompiled(
            this IServiceCollection services,
            Action<ICompiledAssemblySelector> action,
	        [CallerFilePathAttribute] string filePath = """",
	        [CallerMemberName] string memberName = """",
	        [CallerLineNumberAttribute] int lineNumber = 0
        )
        {
            return PopulateExtensions.Populate(services, RegistrationStrategy.Append, AssemblyLoadContext.GetLoadContext(typeof(CompiledServiceScanningExtensions).Assembly) ?? AssemblyLoadContext.Default, filePath, memberName, lineNumber);
        }

        public static IServiceCollection ScanCompiled(
            this IServiceCollection services,
            Action<ICompiledAssemblySelector> action,
            RegistrationStrategy strategy,
	        [CallerFilePathAttribute] string filePath = """",
	        [CallerMemberName] string memberName = """",
	        [CallerLineNumberAttribute] int lineNumber = 0
        )
        {
            return PopulateExtensions.Populate(services, strategy, AssemblyLoadContext.GetLoadContext(typeof(CompiledServiceScanningExtensions).Assembly) ?? AssemblyLoadContext.Default, filePath, memberName, lineNumber);
        }

        public static IServiceCollection ScanCompiled(
            this IServiceCollection services,
            Action<ICompiledAssemblySelector> action,
            AssemblyLoadContext context,
	        [CallerFilePathAttribute] string filePath = """",
	        [CallerMemberName] string memberName = """",
	        [CallerLineNumberAttribute] int lineNumber = 0
        )
        {
            return PopulateExtensions.Populate(services, RegistrationStrategy.Append, context, filePath, memberName, lineNumber);
        }

        public static IServiceCollection ScanCompiled(
            this IServiceCollection services,
            Action<ICompiledAssemblySelector> action,
            RegistrationStrategy strategy,
            AssemblyLoadContext context,
	        [CallerFilePathAttribute] string filePath = """",
	        [CallerMemberName] string memberName = """",
	        [CallerLineNumberAttribute] int lineNumber = 0
        )
        {
            return PopulateExtensions.Populate(services, strategy, context, filePath, memberName, lineNumber);
        }
    }
}
#pragma warning restore CS0436
";

    private const string StaticScanSourceText = @"
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Scrutor;
using Rocket.Surgery.DependencyInjection.Compiled;
#pragma warning disable CS0436
namespace Microsoft.Extensions.DependencyInjection
{
    [CompilerGenerated, ExcludeFromCodeCoverage]
    internal static class CompiledServiceScanningExtensions
    {
        public static IServiceCollection ScanCompiled(
            this IServiceCollection services,
            Action<ICompiledAssemblySelector> action,
	        [CallerFilePathAttribute] string filePath = """",
	        [CallerMemberName] string memberName = """",
	        [CallerLineNumberAttribute] int lineNumber = 0
        )
        {
            return PopulateExtensions.Populate(services, RegistrationStrategy.Append, filePath, memberName, lineNumber);
        }

        public static IServiceCollection ScanCompiled(
            this IServiceCollection services,
            Action<ICompiledAssemblySelector> action,
            RegistrationStrategy strategy,
	        [CallerFilePathAttribute] string filePath = """",
	        [CallerMemberName] string memberName = """",
	        [CallerLineNumberAttribute] int lineNumber = 0
        )
        {
            return PopulateExtensions.Populate(services, strategy, filePath, memberName, lineNumber);
        }
    }
}
#pragma warning restore CS0436
";

    private const string PopulateSourceTextWithAssemblyLoadContext = @"
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

#pragma warning disable CS0436
namespace Rocket.Surgery.DependencyInjection.Compiled
{
    [CompilerGenerated, ExcludeFromCodeCoverage]
    internal static class PopulateExtensions
    {
        public static IServiceCollection Populate(IServiceCollection services, RegistrationStrategy strategy, AssemblyLoadContext context, string filePath, string memberName, int lineNumber)
        {
            return services;
        }
    }
}
#pragma warning restore CS0436
";

    private const string PopulateSourceText = @"
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

#pragma warning disable CS0436
namespace Rocket.Surgery.DependencyInjection.Compiled
{
    [CompilerGenerated, ExcludeFromCodeCoverage]
    internal static class PopulateExtensions
    {
        public static IServiceCollection Populate(IServiceCollection services, RegistrationStrategy strategy, string filePath, string memberName, int lineNumber)
        {
            return services;
        }
    }
}
#pragma warning restore CS0436
";
}
