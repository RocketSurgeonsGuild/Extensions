using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class ServiceDescriptorCollection
{
    public static IncrementalValueProvider<ImmutableArray<(InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel)>> Create(
        SyntaxValueProvider valueProvider,
        IncrementalValueProvider<bool> hasAssemblyLoadContext
    )
    {
        return valueProvider
              .CreateSyntaxProvider((node, _) => IsValidMethod(node), (syntaxContext, _) => GetTypesMethod(syntaxContext))
              .Combine(hasAssemblyLoadContext)
              .Where(z => z is { Right: true, Left: { method: { }, selector: { }, }, })
              .Select((tuple, _) => tuple.Left)
              .Collect();
    }

    public static MethodDeclarationSyntax Execute(Request request)
    {
        if (!request.Items.Any()) return ScanMethod;

        var results = new List<(SourceLocation location, BlockSyntax block)>();
        foreach (var item in request.Items)
        {
            var reducedTypes = AssemblyProviders.TypeSymbolVisitor.GetTypes(request.Compilation, item.AssemblyFilter, item.TypeFilter);
            if (reducedTypes.Length == 0) continue;
            var localBlock = GenerateDescriptors(request.Context, request.Compilation, reducedTypes, item.ServicesTypeFilter, request.PrivateAssemblies);
            results.Add(( item.Location, localBlock ));
        }

        return results.Count == 0
            ? ScanMethod
            : ScanMethod
               .WithBody(Block(SwitchGenerator.GenerateSwitchStatement(results), ReturnStatement(IdentifierName("services"))));
    }

    private static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel ) GetTypesMethod(GeneratorSyntaxContext context)
    {
        var baseData = GetMethod(context.Node);
        if (baseData.method is null
         || baseData.selector is null
         || context.SemanticModel.GetTypeInfo(baseData.selector).ConvertedType is not INamedTypeSymbol
            {
                TypeArguments: [{ Name: IServiceDescriptorAssemblySelector, }, ..,],
            })
            return default;

        return ( baseData.method, baseData.selector, semanticModel: context.SemanticModel );
    }

    private static bool IsValidMethod(SyntaxNode node)
    {
        return GetMethod(node) is { method: { }, selector: { } };
    }

    private static (InvocationExpressionSyntax method, ExpressionSyntax selector ) GetMethod(SyntaxNode node)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: "Scan",
            },
            ArgumentList.Arguments: [.., { Expression: { } expression, },],
        } invocationExpressionSyntax
            ? ( invocationExpressionSyntax, expression )
            : default;
    }

    internal static ImmutableArray<Item> GetTypeDetails(
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
                ( var methodCallSyntax, var selector, _ ) = tuple;

                List<IAssemblyDescriptor> assemblies = new();
                List<ITypeFilterDescriptor> typeFilters =
                [
                    new TypeKindFilterDescriptor(true, ImmutableHashSet.Create(TypeKind.Class)),
                    new TypeInfoFilterDescriptor(false, ImmutableHashSet.Create(TypeInfoFilter.Abstract))
                ];
                List<IServiceTypeDescriptor> serviceDescriptors = new();
                var classFilter = ClassFilter.All;
                var lifetime = 2;

                DataHelpers.HandleInvocationExpressionSyntax(
                    context,
                    compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                    selector,
                    assemblies,
                    typeFilters,
                    serviceDescriptors,
                    ref lifetime,
                    compilation.ObjectType,
                    ref classFilter,
                    context.CancellationToken
                );

                var assemblyFilter = new CompiledAssemblyFilter(assemblies.ToImmutableArray());
                var typeFilter = new CompiledTypeFilter(classFilter, typeFilters.ToImmutableArray());
                var serviceDescriptorFilter = new CompiledServiceTypeDescriptors(serviceDescriptors.ToImmutableArray(), lifetime);

                var source = Helpers.CreateSourceLocation(methodCallSyntax, context.CancellationToken);

                var i = new Item(source, assemblyFilter, typeFilter, serviceDescriptorFilter, lifetime);
                items.Add(i);
            }
            catch (MustBeAnExpressionException e)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAnExpression, e.Location));
            }
        }

        return items.ToImmutable();
    }

    private static BlockSyntax GenerateDescriptors(
        SourceProductionContext context,
        Compilation compilation,
        IEnumerable<INamedTypeSymbol> types,
        CompiledServiceTypeDescriptors serviceTypes,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        var asSelf = serviceTypes.ServiceTypeDescriptors.OfType<SelfServiceTypeDescriptor>().Any() || !serviceTypes.ServiceTypeDescriptors.Any();
        var asImplementedInterfaces = serviceTypes.ServiceTypeDescriptors.OfType<ImplementedInterfacesServiceTypeDescriptor>().Any();
        var asMatchingInterface = serviceTypes.ServiceTypeDescriptors.OfType<MatchingInterfaceServiceTypeDescriptor>().Any();
        var asSpecificTypes = serviceTypes.ServiceTypeDescriptors.OfType<CompiledServiceTypeDescriptor>().Select(z => z.Type).ToArray();
        var usingAttributes = serviceTypes.ServiceTypeDescriptors.OfType<UsingAttributeServiceTypeDescriptor>().Any();
        var serviceRegistrationAttribute = compilation.GetTypeByMetadataName("Rocket.Surgery.DependencyInjection.ServiceRegistrationAttribute")!;

        var services = new List<InvocationExpressionSyntax>();

        foreach (var type in types.OrderBy(z => z.ToDisplayString()))
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
                var attributeDataElements = type
                                           .GetAttributes()
                                           .Where(
                                                attribute => attribute.AttributeClass != null
                                                 && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serviceRegistrationAttribute)
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
                    if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serviceRegistrationAttribute))
                        continue;

                    INamedTypeSymbol? attributeServiceType = null;
                    var lifetimeValue = serviceTypes.GetLifetime();

                    if (attribute.ConstructorArguments is [{ Kind: TypedConstantKind.Enum } _])
                    {
                        var members = attribute.ConstructorArguments[0].Type!
                                               .GetMembers()
                                               .OfType<IFieldSymbol>();
                        if (attribute.ConstructorArguments[0].Value is int v)
                        {
                            var value = members
                               .First(z => z.ConstantValue is int i && i == v);
                            lifetimeValue = value.Name;
                        }
                    }
                    else if (attribute.ConstructorArguments is [_, { Kind: TypedConstantKind.Enum } _])
                    {
                        var members = attribute.ConstructorArguments[1].Type!
                                               .GetMembers()
                                               .OfType<IFieldSymbol>();
                        if (attribute.ConstructorArguments[1].Value is int v)
                        {
                            var value = members
                               .First(z => z.ConstantValue is int i && i == v);
                            lifetimeValue = value.Name;
                        }
                    }

                    if (attribute.ConstructorArguments.Length == 0
                     || attribute.ConstructorArguments is [{ Kind: TypedConstantKind.Enum }])
                    {
                        if (!emittedTypes.Contains(type))
                        {
                            services.Add(StatementGeneration.GenerateServiceType(compilation, type, type, lifetimeValue));

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
                                services.Add(
                                    StatementGeneration.GenerateServiceFactory(
                                        compilation,
                                        @interface,
                                        type,
                                        lifetimeValue
                                    )
                                );
                                emittedTypes.Add(@interface);
                            }
                        }

                        foreach (var baseType in Helpers.GetBaseTypes(compilation, type))
                        {
                            if (!emittedTypes.Contains(baseType))
                            {
                                services.Add(
                                    StatementGeneration.GenerateServiceFactory(
                                        compilation,
                                        baseType,
                                        type,
                                        lifetimeValue
                                    )
                                );
                                emittedTypes.Add(baseType);
                            }
                        }
                    }

                    if (attribute.ConstructorArguments is [{ Kind: TypedConstantKind.Type } _])
                    {
                        attributeServiceType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
                    }

                    if (attribute.ConstructorArguments.Length == 2)
                    {
                        attributeServiceType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
                    }

                    if (attributeServiceType != null)
                    {
                        services.Add(
                            asSelf
                                ? StatementGeneration.GenerateServiceFactory(
                                    compilation,
                                    attributeServiceType,
                                    type,
                                    lifetimeValue
                                )
                                : StatementGeneration.GenerateServiceType(
                                    compilation,
                                    attributeServiceType,
                                    type,
                                    lifetimeValue
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
                services.Add(
                    StatementGeneration.GenerateServiceType(
                        compilation,
                        type,
                        type,
                        serviceTypes.GetLifetime()
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
                if (@interface is { } && !emittedTypes.Contains(@interface))
                {
                    services.Add(
                        typeIsOpenGeneric || !asSelf
                            ? StatementGeneration.GenerateServiceType(
                                compilation,
                                @interface,
                                type,
                                serviceTypes.GetLifetime()
                            )
                            : StatementGeneration.GenerateServiceFactory(
                                compilation,
                                @interface,
                                type,
                                serviceTypes.GetLifetime()
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
                        services.Add(
                            typeIsOpenGeneric || !asSelf
                                ? StatementGeneration.GenerateServiceType(
                                    compilation,
                                    @interface,
                                    type,
                                    serviceTypes.GetLifetime()
                                )
                                : StatementGeneration.GenerateServiceFactory(
                                    compilation,
                                    @interface,
                                    type,
                                    serviceTypes.GetLifetime()
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
                    services.Add(
                        !asSelf
                            ? StatementGeneration.GenerateServiceType(
                                compilation,
                                asType,
                                type,
                                serviceTypes.GetLifetime()
                            )
                            : StatementGeneration.GenerateServiceFactory(
                                compilation,
                                asType,
                                type,
                                serviceTypes.GetLifetime()
                            )
                    );
                    emittedTypes.Add(asType);
                }
            }
        }

        var block = Block();
        foreach (var service in services)
        {
            block = block.AddStatements(
                ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("services"), IdentifierName("Add"))
                        )
                       .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(service))))
                )
            );
        }

        return block;
    }

    private static readonly MethodDeclarationSyntax ScanMethod =
        MethodDeclaration(ParseName("Microsoft.Extensions.DependencyInjection.IServiceCollection"), Identifier("Scan"))
           .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName("ICompiledTypeProvider")))
           .AddParameterListParameters(
                Parameter(Identifier("services")).WithType(ParseName("Microsoft.Extensions.DependencyInjection.IServiceCollection")),
                Parameter(Identifier("selector"))
                   .WithType(GenericName(Identifier("Action")).AddTypeArgumentListArguments(IdentifierName(IServiceDescriptorAssemblySelector))),
                Parameter(Identifier("lineNumber")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
                Parameter(Identifier("filePath")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                Parameter(Identifier("argumentExpression")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            )
           .WithBody(Block(ReturnStatement(IdentifierName("services"))));

    public record Request(SourceProductionContext Context, Compilation Compilation, ImmutableArray<Item> Items, HashSet<IAssemblySymbol> PrivateAssemblies);

    public record Item
    (
        SourceLocation Location,
        CompiledAssemblyFilter AssemblyFilter,
        CompiledTypeFilter TypeFilter,
        CompiledServiceTypeDescriptors ServicesTypeFilter,
        int Lifetime);

    private const string IServiceDescriptorAssemblySelector = nameof(IServiceDescriptorAssemblySelector);
}
