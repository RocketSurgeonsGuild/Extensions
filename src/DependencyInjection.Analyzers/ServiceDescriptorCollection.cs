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
              .CreateSyntaxProvider((node, _) => IsValidMethod(node), (syntaxContext, _) => GetServiceDescriptorMethod(syntaxContext))
              .Combine(hasAssemblyLoadContext)
              .Where(z => z is { Right: true, Left: { method: { }, selector: { } } })
              .Select((tuple, _) => tuple.Left)
              .Collect();
    }

    public static ResolvedSourceLocation? ResolveSource(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        Item item,
        IAssemblySymbol targetAssembly,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        try
        {
            var pa = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
            var reducedTypes = new TypeSymbolVisitor(compilation, item.AssemblyFilter, item.TypeFilter)
                              .GetReferencedTypes(targetAssembly)
                              .GetTypes();
            if (reducedTypes.Count == 0) return null;
            var localBlock = GenerateDescriptors(compilation, diagnostics, reducedTypes, item.ServicesTypeFilter, pa)
                            .NormalizeWhitespace()
                            .ToFullString();

            privateAssemblies.UnionWith(pa);
            return new(item.Location, localBlock, pa.Select(z => z.MetadataName).ToImmutableHashSet());
        }
        catch (Exception e)
        {
            diagnostics.Add(
                Diagnostic.Create(
                    Diagnostics.UnhandledException,
                    null,
                    e.Message,
                    e.StackTrace.Replace("\r", "").Replace("\n", ""),
                    e.GetType().Name,
                    e.ToString()
                )
            );
            return null;
        }
    }

    public static ImmutableList<ResolvedSourceLocation> ResolveSources(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        IReadOnlyList<Item> items,
        IAssemblySymbol targetAssembly,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        if (!items.Any()) return [];
        var results = new List<ResolvedSourceLocation>();
        foreach (var item in items)
        {
            if (ResolveSource(compilation, diagnostics, item, targetAssembly, privateAssemblies) is not { } location) continue;
            results.Add(location);
        }

        return results.ToImmutableList();
    }

    internal static ImmutableList<Item> GetServiceDescriptorItems(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        IReadOnlyList<(InvocationExpressionSyntax expression, ExpressionSyntax selector, SemanticModel semanticModel)> results,
        CancellationToken cancellationToken
    )
    {
        var items = ImmutableList.CreateBuilder<Item>();
        foreach (var tuple in results)
        {
            try
            {
                ( var methodCallSyntax, var selector, _ ) = tuple;

                List<IAssemblyDescriptor> assemblies = new();
                List<ITypeFilterDescriptor> typeFilters =
                [
                    new TypeKindFilterDescriptor(true, ImmutableHashSet.Create(TypeKind.Class)),
                    new TypeInfoFilterDescriptor(false, ImmutableHashSet.Create(TypeInfoFilter.Abstract, TypeInfoFilter.Static)),
                ];
                List<IServiceTypeDescriptor> serviceDescriptors = new();
                var classFilter = ClassFilter.All;
                var lifetime = 2;

                DataHelpers.HandleInvocationExpressionSyntax(
                    diagnostics,
                    compilation.GetSemanticModel(tuple.expression.SyntaxTree),
                    selector,
                    assemblies,
                    typeFilters,
                    serviceDescriptors,
                    ref lifetime,
                    ref classFilter,
                    cancellationToken
                );

                var source = Helpers.CreateSourceLocation(methodCallSyntax, cancellationToken);
                var assemblyFilter = new CompiledAssemblyFilter(assemblies.ToImmutableList(), source);
                var typeFilter = new CompiledTypeFilter(classFilter, typeFilters.ToImmutableList(), source);
                var serviceDescriptorFilter = new CompiledServiceTypeDescriptors(serviceDescriptors.ToImmutableArray(), lifetime);


                var i = new Item(source, assemblyFilter, typeFilter, serviceDescriptorFilter, lifetime);
                items.Add(i);
            }
            catch (MustBeAnExpressionException e)
            {
                diagnostics.Add(Diagnostic.Create(Diagnostics.MustBeAnExpression, e.Location));
            }
            catch (Exception e)
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.UnhandledException,
                        null,
                        e.Message,
                        e.StackTrace.Replace("\r", "").Replace("\n", ""),
                        e.GetType().Name,
                        e.ToString()
                    )
                );
            }
        }

        return items.ToImmutable();
    }

    private static (InvocationExpressionSyntax method, ExpressionSyntax selector, SemanticModel semanticModel ) GetServiceDescriptorMethod(GeneratorSyntaxContext context)
    {
        var baseData = GetMethod(context.Node);
        if (baseData.method is null
         || baseData.selector is null
         || context.SemanticModel.GetTypeInfo(baseData.selector).ConvertedType is not INamedTypeSymbol
            {
                TypeArguments: [{ Name: IServiceDescriptorAssemblySelector }, ..],
            })
            return default;

        return ( baseData.method, baseData.selector, semanticModel: context.SemanticModel );
    }

    private static bool IsValidMethod(SyntaxNode node) => GetMethod(node) is { method: { }, selector: { } };

    private static (InvocationExpressionSyntax method, ExpressionSyntax selector ) GetMethod(SyntaxNode node) =>
        node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: "Scan",
            },
            ArgumentList.Arguments: [.., { Expression: { } expression }],
        } invocationExpressionSyntax
            ? ( invocationExpressionSyntax, expression )
            : default;

    private static BlockSyntax GenerateDescriptors(
        Compilation compilation,
        HashSet<Diagnostic> diagnostics,
        IEnumerable<INamedTypeSymbol> types,
        CompiledServiceTypeDescriptors serviceTypes,
        HashSet<IAssemblySymbol> privateAssemblies
    )
    {
        var asSelf = serviceTypes.ServiceTypeDescriptors.OfType<SelfServiceTypeDescriptor>().Any() || !serviceTypes.ServiceTypeDescriptors.Any();
        var asImplementedInterfaces = serviceTypes.ServiceTypeDescriptors.OfType<ImplementedInterfacesServiceTypeDescriptor>().ToArray();
        var asMatchingInterface = serviceTypes.ServiceTypeDescriptors.OfType<MatchingInterfaceServiceTypeDescriptor>().Any();
        var asSpecificTypes = serviceTypes.ServiceTypeDescriptors.OfType<CompiledServiceTypeDescriptor>().Select(z => z.Type).Where(z => z is { }).ToArray();
        var registrationLifetimeAttribute = compilation.GetTypeByMetadataName("Rocket.Surgery.DependencyInjection.RegistrationLifetimeAttribute")!;

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

            var discoveredLifetime =
                type
                   .GetAttributes()
                   .SingleOrDefault(
                        attribute => attribute.AttributeClass != null
                         && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, registrationLifetimeAttribute)
                    );

            var serviceRegistrationAttributes =
                type
                   .GetAttributes()
                   .Where(attribute => attribute.AttributeClass is { Name: "ServiceRegistrationAttribute" or "ServiceRegistration" })
                   .ToArray();

            // attribute priority for discovered services
            // DiscoveredLifetimeAttribute -> ServiceRegistrationAttribute(lifetime?) -> ServiceRegistrationAttribute()

            // attribute priority for service registrations
            // ServiceRegistrationAttribute(lifetime?, types) -> DiscoveredLifetimeAttribute -> ServiceRegistrationAttribute(lifetime?) -> ServiceRegistrationAttribute()

            // group all attributes and find any common services to create a lifetime map
            var lifetimeValue = serviceTypes.GetLifetime();
            lifetimeValue = serviceRegistrationAttributes
                           .Where(z => z.ConstructorArguments.Length == 1)
                           .Aggregate(lifetimeValue, (acc, attribute) => GetLifetimeValue(attribute) ?? acc);
            lifetimeValue = GetLifetimeValue(discoveredLifetime) ?? lifetimeValue;

            var lifetimeRegistrations = serviceRegistrationAttributes
                                       .SelectMany(
                                            attributeData =>
                                            {
                                                return ( attributeData switch
                                                         {
                                                             { ConstructorArguments: [_, { Kind: TypedConstantKind.Array, Values: { Length: > 0 } values }] } =>
                                                                 values
                                                                    .Where(z => z is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol })
                                                                    .Select(z => z.Value)
                                                                    .OfType<INamedTypeSymbol>(),
                                                             { AttributeClass.TypeArguments: { Length: > 0 } typeArgs } => typeArgs.OfType<INamedTypeSymbol>(),
                                                             _                                                          => type.AllInterfaces.OfType<INamedTypeSymbol>(),
                                                         } )
                                                   .Prepend(type);
                                            },
                                            (data, serviceType) =>
                                            {
                                                var serviceAttribute = serviceType
                                                                      .GetAttributes()
                                                                      .FirstOrDefault(
                                                                           z => z.AttributeClass != null
                                                                            && SymbolEqualityComparer.Default.Equals(
                                                                                   z.AttributeClass,
                                                                                   registrationLifetimeAttribute
                                                                               )
                                                                       );
                                                return ( Lifetime: GetLifetimeValue(serviceAttribute) ?? GetLifetimeValue(data) ?? lifetimeValue,
                                                         Type: serviceType );
                                            }
                                        )
                                       .GroupBy(z => z.Type.ToDisplayString())
                                       .ToArray();

            var abort = false;
            foreach (var registration in lifetimeRegistrations)
            {
                if (registration.Count() <= 1) continue;
                foreach (var item in registration)
                {
                    if (!item.Type.DeclaringSyntaxReferences.Any()) continue;
                    diagnostics.Add(Diagnostic.Create(Diagnostics.DuplicateServiceDescriptorAttribute, item.Type.Locations.FirstOrDefault()));
                    abort = true;
                }
            }

            if (abort) return Block();

            foreach (( var lifetime, var serviceType ) in lifetimeRegistrations.Select(z => z.First()))
            {
                // todo: start here
                // need diagnostics for double registration
                // need to get services from the generic arguments of the class

                if (emittedTypes.Contains(serviceType)) continue;
                services.Add(
                    !SymbolEqualityComparer.Default.Equals(serviceType, type)
                        ? StatementGeneration.GenerateServiceFactory(
                            compilation,
                            serviceType,
                            type,
                            lifetime
                        )
                        : StatementGeneration.GenerateServiceType(
                            compilation,
                            serviceType,
                            type,
                            lifetime
                        )
                );
                emittedTypes.Add(serviceType);
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

            if (asImplementedInterfaces is { Length: > 0 })
            {
                // filter here
                var interfaces = new List<INamedTypeSymbol>();
                // todo: filter interfaces by type filter or implemented interfaces filter expression (a reuse of the type filter)
                // It should support open generic types as well (think abstract validator from fluent validation)
                interfaces.AddRange(
                    asImplementedInterfaces is [{ InterfaceFilter: { } filter }]
                        ? type.AllInterfaces.Where(item => filter.IsMatch(compilation, item))
                        : type.AllInterfaces
                );


                foreach (var @interface in interfaces.OrderBy(z => z.ToDisplayString()))
                {
                    if (emittedTypes.Contains(@interface)) continue;
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

            foreach (var asType in asSpecificTypes.OrderBy(z => z.ToDisplayString()))
            {
                if (emittedTypes.Contains(asType)) continue;
                if (!Helpers.HasImplicitGenericConversion(compilation, asType, type)) continue;
                services.Add(
                    !asSelf
                        ? StatementGeneration.GenerateServiceType(
                            compilation,
                            Helpers.GetClosedGenericConversion(compilation, asType, type),
                            type,
                            serviceTypes.GetLifetime()
                        )
                        : StatementGeneration.GenerateServiceFactory(
                            compilation,
                            Helpers.GetClosedGenericConversion(compilation, asType, type),
                            type,
                            serviceTypes.GetLifetime()
                        )
                );
                emittedTypes.Add(asType);
            }

            if (!emittedTypes.Any() && ( lifetimeRegistrations.Any() || discoveredLifetime is { } ) && !asMatchingInterface)
            {
                foreach (var @interface in type.AllInterfaces.OrderBy(z => z.ToDisplayString()))
                {
                    if (emittedTypes.Contains(@interface)) continue;
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

    private const string IServiceDescriptorAssemblySelector = nameof(IServiceDescriptorAssemblySelector);

    private static string? GetLifetimeValue(AttributeData? attribute)
    {
        if (attribute is
            {
                ConstructorArguments:
                [
                    {
                        Kind: TypedConstantKind.Enum,
                        Type: { } discoveredType,
                        Value: int discoveredValue,
                    },
                    ..,
                ],
            })
        {
            var value = discoveredType
                       .GetMembers()
                       .OfType<IFieldSymbol>()
                       .First(z => z.ConstantValue is int i && i == discoveredValue);
            return value.Name;
        }

        return null;
    }

    public record Item
    (
        SourceLocation Location,
        CompiledAssemblyFilter AssemblyFilter,
        CompiledTypeFilter TypeFilter,
        CompiledServiceTypeDescriptors ServicesTypeFilter,
        int Lifetime);
}
