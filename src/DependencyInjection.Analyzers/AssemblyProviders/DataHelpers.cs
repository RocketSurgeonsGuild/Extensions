using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

namespace Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

internal static class DataHelpers
{
    public static void HandleInvocationExpressionSyntax(
        SourceProductionContext context,
        SemanticModel semanticModel,
        ExpressionSyntax selectorExpression,
        List<IAssemblyDescriptor> assemblies,
        List<ITypeFilterDescriptor> typeFilters,
        List<IServiceTypeDescriptor> serviceTypeFilters,
        ref int lifetime,
        ref ClassFilter classFilter,
        CancellationToken cancellationToken
    )
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (selectorExpression is not InvocationExpressionSyntax expression)
        {
            if (selectorExpression is not SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax
             || simpleLambdaExpressionSyntax is { ExpressionBody: null }
             || simpleLambdaExpressionSyntax.ExpressionBody is not InvocationExpressionSyntax body)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAnExpression, selectorExpression.GetLocation()));
                return;
            }

            expression = body;
        }

        if (expression.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax
         || !memberAccessExpressionSyntax.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            return;

        if (memberAccessExpressionSyntax.Expression is InvocationExpressionSyntax childExpression)
        {
            if (cancellationToken.IsCancellationRequested) return;
            HandleInvocationExpressionSyntax(
                context,
                semanticModel,
                childExpression,
                assemblies,
                typeFilters,
                serviceTypeFilters,
                ref lifetime,
                ref classFilter,
                cancellationToken
            );
        }

        var type = ModelExtensions.GetTypeInfo(semanticModel, memberAccessExpressionSyntax.Expression);
        if (type.Type is null) return;
        var typeName = type.Type.ToDisplayString();
        if (typeName is "Rocket.Surgery.DependencyInjection.Compiled.IReflectionAssemblySelector"
                     or "Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorAssemblySelector")
        {
            if (cancellationToken.IsCancellationRequested) return;
            var selector = HandleCompiledAssemblySelector(semanticModel, expression, memberAccessExpressionSyntax.Name);
            if (selector is { }) assemblies.Add(selector);
        }

        if (typeName is "Rocket.Surgery.DependencyInjection.Compiled.IReflectionTypeSelector"
                     or "Rocket.Surgery.DependencyInjection.Compiled.IServiceDescriptorTypeSelector")
        {
            if (cancellationToken.IsCancellationRequested) return;
            var selector = HandleCompiledAssemblySelector(semanticModel, expression, memberAccessExpressionSyntax.Name);
            if (selector is { })
                assemblies.Add(selector);
            else
                classFilter = HandleCompiledImplementationTypeSelector(expression, memberAccessExpressionSyntax.Name);
        }

        if (typeName == "Rocket.Surgery.DependencyInjection.Compiled.ITypeFilter")
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (HandleCompiledTypeFilter(context, semanticModel, expression, memberAccessExpressionSyntax.Name) is { } filter)
                typeFilters.Add(filter);
        }

        if (typeName is "Rocket.Surgery.DependencyInjection.Compiled.IServiceTypeSelector"
                     or "Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector")
        {
            if (cancellationToken.IsCancellationRequested) return;
            serviceTypeFilters.AddRange(HandleCompiledServiceTypeFilter(context, semanticModel, expression, memberAccessExpressionSyntax.Name));
        }

        if (typeName is "Rocket.Surgery.DependencyInjection.Compiled.IServiceLifetimeSelector")
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (HandleCompiledServiceLifetimeFilter(expression, memberAccessExpressionSyntax.Name) is { } filter)
                lifetime = filter;
            return;
        }

        foreach (var argument in expression.ArgumentList.Arguments.Where(argument => argument.Expression is SimpleLambdaExpressionSyntax))
        {
            if (cancellationToken.IsCancellationRequested) return;
            HandleInvocationExpressionSyntax(
                context,
                semanticModel,
                argument.Expression,
                assemblies,
                typeFilters,
                serviceTypeFilters,
                ref lifetime,
                ref classFilter,
                cancellationToken
            );
        }
    }

    public static ExpressionSyntax? ExtractSyntaxFromMethod(
        ExpressionSyntax expression,
        NameSyntax name
    )
    {
        return ( name, expression ) switch
               {
                   (GenericNameSyntax { TypeArgumentList.Arguments: [var syntax] }, _) => syntax,
                   (SimpleNameSyntax, InvocationExpressionSyntax
                   {
                       ArgumentList.Arguments: [{ Expression: TypeOfExpressionSyntax typeOfExpression }, ..],
                   }) => typeOfExpression.Type,
                   // lambda's are methods to be handled.
                   (_, TypeOfExpressionSyntax typeOfExpression)                                                                   => typeOfExpression.Type,
                   (_, InvocationExpressionSyntax { ArgumentList.Arguments: [{ Expression: LambdaExpressionSyntax }, ..] })       => null,
                   (_, InvocationExpressionSyntax { ArgumentList.Arguments: [{ Expression: LiteralExpressionSyntax }, ..] })      => null,
                   (_, InvocationExpressionSyntax { ArgumentList.Arguments: [{ Expression: MemberAccessExpressionSyntax }, ..] }) => null,
                   (_, InvocationExpressionSyntax
                   {
                       ArgumentList.Arguments:
                       [
                           {
                               Expression: InvocationExpressionSyntax
                               {
                                   Expression: IdentifierNameSyntax { Identifier.Value: "nameof" },
                               } nameofName,
                           },
                           ..,
                       ],
                   }) => nameofName,
                   _ => throw new MustBeAnExpressionException(expression.GetLocation(), string.Join(", ", expression.ToFullString())),
               };
    }

    private static IAssemblyDescriptor? HandleCompiledAssemblySelector(
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        SimpleNameSyntax name
    )
    {
        if (name.ToFullString() == "FromAssembly")
            return new AssemblyDescriptor(semanticModel.Compilation.Assembly);
        if (name.ToFullString() == "FromAssemblies")
            return new AllAssemblyDescriptor();
        if (name.ToFullString() == "IncludeSystemAssemblies")
            return new IncludeSystemAssembliesDescriptor();

        var typeSyntax = ExtractSyntaxFromMethod(expression, name);
        if (typeSyntax == null)
            return null;

        var typeInfo = ModelExtensions.GetTypeInfo(semanticModel, typeSyntax).Type;
        if (typeInfo is null)
            return null;
        return typeInfo switch
               {
                   INamedTypeSymbol nts when name is { Identifier.Text: "FromAssemblyDependenciesOf" } =>
                       new AssemblyDependenciesDescriptor(nts.ContainingAssembly),
                   INamedTypeSymbol namedType when name is { Identifier.Text: "FromAssemblyOf" or "NotFromAssemblyOf" } =>
                       name.Identifier.Text.StartsWith("Not")
                           ? new NotAssemblyDescriptor(namedType.ContainingAssembly)
                           : new AssemblyDescriptor(namedType.ContainingAssembly),
                   _ => null,
               };
    }

    private static ClassFilter HandleCompiledImplementationTypeSelector(
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name.ToFullString() is "GetTypes" && expression.ArgumentList.Arguments.Count is >= 2 and <= 2)
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.IsKind(SyntaxKind.TrueKeyword))
                    return ClassFilter.PublicOnly;
            }

        return ClassFilter.All;
    }

    private static ITypeFilterDescriptor? HandleCompiledTypeFilter(
        SourceProductionContext context,
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        SimpleNameSyntax name
    )
    {
        return ( name, GetSyntaxTypeInfo(semanticModel, expression, name) ) switch
               {
                   ({ Identifier.Text: "AssignableToAny" or "NotAssignableToAny" }, _) =>
                       createAssignableToAnyTypeFilterDescriptor(name, expression, semanticModel),
                   ({ Identifier.Text: "AssignableTo" or "NotAssignableTo" }, { } namedType) =>
                       createAssignableToTypeFilterDescriptor(name, namedType),
                   ({ Identifier.Text: "WithAttribute" or "WithoutAttribute" }, { } namedType) =>
                       createWithAttributeFilterDescriptor(name, namedType),
                   ({ Identifier.Text: "WithAttribute" or "WithoutAttribute" }, _) =>
                       createWithAttributeStringFilterDescriptor(context, name, expression, semanticModel),
                   ({ Identifier.Text: "WithAnyAttribute" }, { } namedType) =>
                       createWithAnyAttributeFilterDescriptor(name, expression, semanticModel),
                   ({ Identifier.Text: "WithAnyAttribute" }, _) =>
                       createWithAnyAttributeStringFilterDescriptor(name, expression, semanticModel),
                   ({ Identifier.Text: "InExactNamespaceOf" or "InNamespaceOf" or "NotInNamespaceOf" }, _) =>
                       createNamespaceTypeFilterDescriptor(context, name, expression, semanticModel),
                   ({ Identifier.Text: "InExactNamespaces" or "InNamespaces" or "NotInNamespaces" }, _) =>
                       createNamespaceStringFilterDescriptor(context, name, expression, semanticModel),
                   ({ Identifier.Text: "EndsWith" or "StartsWith" or "Contains" or "NotEndsWith" or "NotStartsWith" or "NotContains" }, _) =>
                       createNameFilterDescriptor(context, name, expression),
                   ({ Identifier.Text: "KindOf" or "NotKindOf" }, _) =>
                       createTypeKindFilterDescriptor(context, name, expression),
                   ({ Identifier.Text: "InfoOf" or "NotInfoOf" }, _) =>
                       createTypeInfoFilterDescriptor(context, name, expression),
                   _ => null,
//                   _ => throw new NotSupportedException($"Not supported type filter. Method: {name.ToFullString()}  {expression.ToFullString()} method."),
               };

        static ITypeFilterDescriptor createAssignableToTypeFilterDescriptor(SimpleNameSyntax name, INamedTypeSymbol namedType)
        {
            return name.Identifier.Text.StartsWith("Not")
                ? new NotAssignableToTypeFilterDescriptor(namedType)
                : new AssignableToTypeFilterDescriptor(namedType);
        }

        static ITypeFilterDescriptor createWithAttributeFilterDescriptor(SimpleNameSyntax name, INamedTypeSymbol namedType)
        {
            return name.Identifier.Text.StartsWith("Without") ? new WithoutAttributeFilterDescriptor(namedType) : new WithAttributeFilterDescriptor(namedType);
        }

        static ITypeFilterDescriptor createWithAnyAttributeFilterDescriptor(
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression,
            SemanticModel semanticModel
        )
        {
            var arguments = expression
                           .ArgumentList.Arguments.Select(z => GetSyntaxTypeInfo(semanticModel, z.Expression, name))
                           .OfType<INamedTypeSymbol>()
                           .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            return new WithAnyAttributeFilterDescriptor(arguments);
        }

        static ITypeFilterDescriptor createAssignableToAnyTypeFilterDescriptor(
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression,
            SemanticModel semanticModel
        )
        {
            var arguments = expression
                           .ArgumentList.Arguments.Select(z => GetSyntaxTypeInfo(semanticModel, z.Expression, name))
                           .OfType<INamedTypeSymbol>()
                           .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            return name.Identifier.Text.StartsWith("Not")
                ? new NotAssignableToAnyTypeFilterDescriptor(arguments)
                : new AssignableToAnyTypeFilterDescriptor(arguments);
        }

        static ITypeFilterDescriptor createNameFilterDescriptor(
            SourceProductionContext context,
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression
        )
        {
            var filter = name.Identifier.Text switch
                         {
                             "EndsWith" or "NotEndsWith"     => TextDirectionFilter.EndsWith,
                             "StartsWith" or "NotStartsWith" => TextDirectionFilter.StartsWith,
                             "Contains" or "NotContains"     => TextDirectionFilter.Contains,
                             _ => throw new NotSupportedException(
                                 $"Not supported name filter. Method: {name.ToFullString()}  {expression.ToFullString()} method."
                             ),
                         };
            var stringValues = ImmutableHashSet.CreateBuilder<string>();
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (getStringValue(argument) is not { Length: > 0 } item)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAString, argument.GetLocation()));
                    continue;
                }

                stringValues.Add(item);
            }

            return new NameFilterDescriptor(!name.Identifier.Text.StartsWith("Not"), filter, stringValues.ToImmutable());
        }

        static NamespaceFilterDescriptor createNamespaceTypeFilterDescriptor(
            SourceProductionContext context,
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression,
            SemanticModel semanticModel
        )
        {
            var filter = name.Identifier.Text switch
                         {
                             "InExactNamespaceOf" => NamespaceFilter.Exact,
                             "InNamespaceOf"      => NamespaceFilter.In,
                             "NotInNamespaceOf"   => NamespaceFilter.NotIn,
                             _ => throw new NotSupportedException(
                                 $"Not supported namespace filter. Method: {name.ToFullString()}  {expression.ToFullString()} method."
                             ),
                         };

            var namespaces = ImmutableHashSet.CreateBuilder<string>();
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (argument.Expression is not TypeOfExpressionSyntax typeOfExpressionSyntax
                 || ModelExtensions.GetTypeInfo(semanticModel, typeOfExpressionSyntax.Type).Type is not { } type)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAnExpression, argument.GetLocation()));
                    continue;
                }

                namespaces.Add(type.ContainingNamespace.ToDisplayString());
            }

            if (expression.Expression is MemberAccessExpressionSyntax { Name: GenericNameSyntax { TypeArgumentList.Arguments: [var arg] } }
             && ModelExtensions.GetTypeInfo(semanticModel, arg).Type is { } type2)
                namespaces.Add(type2.ContainingNamespace.ToDisplayString());

            return new(filter, namespaces.ToImmutable());
        }

        static NamespaceFilterDescriptor createNamespaceStringFilterDescriptor(
            SourceProductionContext context,
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression,
            SemanticModel semanticModel
        )
        {
            var filter = name.Identifier.Text switch
                         {
                             "InExactNamespaces" => NamespaceFilter.Exact,
                             "InNamespaces"      => NamespaceFilter.In,
                             "NotInNamespaces"   => NamespaceFilter.NotIn,
                             _ => throw new NotSupportedException(
                                 $"Not supported namespace filter. Method: {name.ToFullString()}  {expression.ToFullString()} method."
                             ),
                         };

            var namespaces = ImmutableHashSet.CreateBuilder<string>();
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (argument.Expression is MemberAccessExpressionSyntax
                    {
                        Name.Identifier.Text: "Namespace", Expression: TypeOfExpressionSyntax typeOfExpressionSyntax,
                    }
                 && GetSyntaxTypeInfo(semanticModel, typeOfExpressionSyntax, name) is { } type)
                {
                    namespaces.Add(type.ContainingNamespace.ToDisplayString());
                    continue;
                }

                if (getStringValue(argument) is { Length: > 0 } item)
                {
                    namespaces.Add(item);
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAString, argument.GetLocation()));
            }

            return new(filter, namespaces.ToImmutable());
        }

        static ITypeFilterDescriptor? createWithAttributeStringFilterDescriptor(
            SourceProductionContext context,
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression,
            SemanticModel semanticModel
        )
        {
            if (expression is not { ArgumentList.Arguments: [var argument] }) return null;
            if (argument.Expression is MemberAccessExpressionSyntax
                {
                    Name.Identifier.Text: "FullName", Expression: TypeOfExpressionSyntax typeOfExpressionSyntax,
                }
             && GetSyntaxTypeInfo(semanticModel, typeOfExpressionSyntax, name) is { } type)
                return name.Identifier.Text.StartsWith("Without")
                    ? new WithoutAttributeStringFilterDescriptor(Helpers.GetFullMetadataName(type))
                    : new WithAttributeStringFilterDescriptor(Helpers.GetFullMetadataName(type));

            if (getStringValue(argument) is { Length: > 0 } item)
                return name.Identifier.Text.StartsWith("Without")
                    ? new WithoutAttributeStringFilterDescriptor(item)
                    : new WithAttributeStringFilterDescriptor(item);

            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAString, argument.GetLocation()));
            return null;
        }

        static ITypeFilterDescriptor createWithAnyAttributeStringFilterDescriptor(
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression,
            SemanticModel semanticModel
        )
        {
            var results = ImmutableHashSet.CreateBuilder<string>();
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (argument.Expression is MemberAccessExpressionSyntax
                    {
                        Name.Identifier.Text: "FullName", Expression: TypeOfExpressionSyntax typeOfExpressionSyntax,
                    }
                 && GetSyntaxTypeInfo(semanticModel, typeOfExpressionSyntax, name) is { } type)
                {
                    results.Add(Helpers.GetFullMetadataName(type));
                }

                if (getStringValue(argument) is { Length: > 0 } item)
                {
                    results.Add(item);
                }
            }

            return new WithAnyAttributeStringFilterDescriptor(results.ToImmutable());
        }

        static TypeKindFilterDescriptor createTypeKindFilterDescriptor(
            SourceProductionContext context,
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression
        )
        {
            var include = name.Identifier.Text switch { "KindOf" => true, _ => false };

            var namespaces = ImmutableHashSet.CreateBuilder<TypeKind>();
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (getStringValue(argument) is not { Length: > 0 } item)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAString, argument.GetLocation()));
                    continue;
                }

                namespaces.Add(Enum.TryParse<TypeKind>(item, out var result) ? result : TypeKind.Unknown);
            }

            return new(include, namespaces.ToImmutable());
        }

        static TypeInfoFilterDescriptor createTypeInfoFilterDescriptor(
            SourceProductionContext context,
            SimpleNameSyntax name,
            InvocationExpressionSyntax expression
        )
        {
            var include = name.Identifier.Text switch { "InfoOf" => true, _ => false };

            var namespaces = ImmutableHashSet.CreateBuilder<TypeInfoFilter>();
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (getStringValue(argument) is not { Length: > 0 } item)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBeAString, argument.GetLocation()));
                    continue;
                }

                namespaces.Add(Enum.TryParse<TypeInfoFilter>(item, out var result) ? result : TypeInfoFilter.Unknown);
            }

            return new(include, namespaces.ToImmutable());
        }

        static string? getStringValue(ArgumentSyntax argument)
        {
            return argument.Expression switch
                   {
                       LiteralExpressionSyntax { Token.RawKind: (int)SyntaxKind.StringLiteralToken, Token.ValueText: { Length: > 0 } result } => result,
                       MemberAccessExpressionSyntax { Name.Identifier.Text: var text }                                                        => text,
                       InvocationExpressionSyntax
                       {
                           Expression: IdentifierNameSyntax { Identifier: { Text: "nameof" } },
                           ArgumentList.Arguments: [{ Expression: IdentifierNameSyntax { Identifier.Text: { Length: > 0 } text } }],
                       } => text,
                       _ => null,
                   };
        }
    }

    private static IEnumerable<IServiceTypeDescriptor> HandleCompiledServiceTypeFilter(
        SourceProductionContext context,
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        SimpleNameSyntax name
    )
    {
        if (name.ToFullString() == "AsSelf")
        {
            yield return new SelfServiceTypeDescriptor();
            yield break;
        }

        if (name.ToFullString() == "AsSelfWithInterfaces")
        {
            yield return new SelfServiceTypeDescriptor();
        }

        if (name.ToFullString() is "AsImplementedInterfaces" or "AsSelfWithInterfaces")
        {
            CompiledTypeFilter? interfaceFilter = null;
            if (expression is
                {
                    ArgumentList.Arguments: [{ Expression: SimpleLambdaExpressionSyntax { ExpressionBody: InvocationExpressionSyntax expressionBody } }],
                })
            {
                var interfaceFilters = new List<ITypeFilterDescriptor>();
                var classFilter = ClassFilter.All;
                var lifetime = 0;
                HandleInvocationExpressionSyntax(
                    context,
                    semanticModel,
                    expressionBody,
                    [],
                    interfaceFilters,
                    [],
                    ref lifetime,
                    ref classFilter,
                    context.CancellationToken
                );
                // ReSharper disable once UseCollectionExpression
                interfaceFilter = new(classFilter, interfaceFilters.ToImmutableArray());
            }

            yield return new ImplementedInterfacesServiceTypeDescriptor(interfaceFilter);
            yield break;
        }

        if (name.ToFullString() == "AsMatchingInterface")
        {
            yield return new MatchingInterfaceServiceTypeDescriptor();
            yield break;
        }

        if (name is { Identifier.Value: "As" })
        {
            var typeSyntax = Helpers.ExtractSyntaxFromMethod(expression, name);
            if (typeSyntax == null) yield break;

            var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type;
            switch (typeInfo)
            {
                case INamedTypeSymbol nts:
                    yield return new CompiledServiceTypeDescriptor(nts);
                    yield break;
                default:
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                    yield break;
            }
        }

        if (name is GenericNameSyntax { Identifier.Value: "As", TypeArgumentList.Arguments.Count: 1 })
        {
            var typeSyntax = Helpers.ExtractSyntaxFromMethod(expression, name);
            if (typeSyntax == null) yield break;

            var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type;
            switch (typeInfo)
            {
                case INamedTypeSymbol nts:
                    yield return new CompiledServiceTypeDescriptor(nts);
                    yield break;
                default:
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                    yield break;
            }
        }
    }

    private static int? HandleCompiledServiceLifetimeFilter(
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name.ToFullString() == "WithLifetime"
         && expression is { ArgumentList.Arguments: [{ Expression: MemberAccessExpressionSyntax { Name: { } lifetimeName } }] })
        {
            return lifetimeName.Identifier.Text switch { "Scoped" => 1, "Transient" => 2, "Singleton" => 0, _ => throw new ArgumentOutOfRangeException() };
        }

        return name.ToFullString() switch
               {
                   "WithSingletonLifetime" => 0, // Singleton
                   "WithScopedLifetime"    => 1, // Scoped
                   "WithTransientLifetime" => 2, // Transient
                   _                       => null,
               };
    }

    private static INamedTypeSymbol? GetSyntaxTypeInfo(
        SemanticModel semanticModel,
        ExpressionSyntax expression,
        NameSyntax name
    ) =>
        ExtractSyntaxFromMethod(expression, name) is not { } type
     || ModelExtensions.GetTypeInfo(semanticModel, type).Type is not INamedTypeSymbol { Kind: not SymbolKind.ErrorType } nts
            ? null
            : nts;
}

internal class MustBeAnExpressionException(Location location, string expression) : Exception($"The expression {expression} must be a constant expression.")
{
    public Location Location { get; } = location;
}
