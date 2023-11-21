using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.DependencyInjection.Analyzers.Internals;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable UnusedParameter.Local
namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class DataHelpers
{
    public static void HandleInvocationExpressionSyntax(
        List<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        ExpressionSyntax rootExpression,
        List<IAssemblyDescriptor> assemblies,
        List<ITypeFilterDescriptor> typeFilters,
        List<IServiceTypeDescriptor> serviceTypes,
        INamedTypeSymbol objectType,
        ref ClassFilter classFilter,
        ref MemberAccessExpressionSyntax lifetimeExpressionSyntax,
        CancellationToken cancellationToken
    )
    {
        if (rootExpression is not InvocationExpressionSyntax expression)
        {
            if (rootExpression is not SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax || simpleLambdaExpressionSyntax is { ExpressionBody: null }
                                                                                                || simpleLambdaExpressionSyntax.ExpressionBody is not
                                                                                                       InvocationExpressionSyntax body)
            {
                diagnostics.Add(Diagnostic.Create(Diagnostics.MustBeAnExpression, rootExpression.GetLocation()));
                return;
            }

            expression = body;
        }

        if (expression.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax
         || !memberAccessExpressionSyntax.IsKind(SyntaxKind.SimpleMemberAccessExpression))
        {
            return;
        }

        if (memberAccessExpressionSyntax.Expression is InvocationExpressionSyntax childExpression)
        {
            if (cancellationToken.IsCancellationRequested) return;
            HandleInvocationExpressionSyntax(
                diagnostics,
                semanticModel,
                childExpression,
                assemblies,
                typeFilters,
                serviceTypes,
                objectType,
                ref classFilter,
                ref lifetimeExpressionSyntax,
                cancellationToken
            );
        }

        var type = semanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression);
        if (type.Type is { })
        {
            var typeName = type.Type.ToDisplayString();
            if (typeName == "Rocket.Surgery.DependencyInjection.Compiled.ICompiledAssemblySelector")
            {
                var selector = HandleCompiledAssemblySelector(
                    semanticModel,
                    expression,
                    memberAccessExpressionSyntax.Name
                );
                if (selector is { })
                {
                    assemblies.Add(selector);
                }
            }

            if (typeName == "Rocket.Surgery.DependencyInjection.Compiled.ICompiledImplementationTypeSelector")
            {
                if (cancellationToken.IsCancellationRequested) return;
                var selector = HandleCompiledAssemblySelector(
                    semanticModel,
                    expression,
                    memberAccessExpressionSyntax.Name
                );
                if (selector is { })
                {
                    assemblies.Add(selector);
                }
                else
                {
                    classFilter = HandleCompiledImplementationTypeSelector(expression, memberAccessExpressionSyntax.Name);
                }
            }

            if (typeName == "Rocket.Surgery.DependencyInjection.Compiled.ICompiledImplementationTypeFilter")
            {
                if (cancellationToken.IsCancellationRequested) return;
                typeFilters.AddRange(
                    HandleCompiledImplementationTypeFilter(diagnostics, semanticModel, expression, memberAccessExpressionSyntax.Name, objectType)
                );
            }

            if (typeName == "Rocket.Surgery.DependencyInjection.Compiled.ICompiledServiceTypeSelector")
            {
                if (cancellationToken.IsCancellationRequested) return;
                serviceTypes.AddRange(HandleCompiledServiceTypeSelector(diagnostics, semanticModel, expression, memberAccessExpressionSyntax.Name));
            }

            if (typeName == "Rocket.Surgery.DependencyInjection.Compiled.ICompiledLifetimeSelector")
            {
                if (cancellationToken.IsCancellationRequested) return;
                serviceTypes.AddRange(HandleCompiledServiceTypeSelector(diagnostics, semanticModel, expression, memberAccessExpressionSyntax.Name));
                if (cancellationToken.IsCancellationRequested) return;
                lifetimeExpressionSyntax = HandleCompiledLifetimeSelector(diagnostics, semanticModel, expression, memberAccessExpressionSyntax.Name) ??
                                           lifetimeExpressionSyntax;
            }
        }

        foreach (var argument in expression.ArgumentList.Arguments.Where(argument => argument.Expression is SimpleLambdaExpressionSyntax))
        {
            if (cancellationToken.IsCancellationRequested) return;
            HandleInvocationExpressionSyntax(
                diagnostics,
                semanticModel,
                argument.Expression,
                assemblies,
                typeFilters,
                serviceTypes,
                objectType,
                ref classFilter,
                ref lifetimeExpressionSyntax,
                cancellationToken
            );
        }
    }

    private static MemberAccessExpressionSyntax? HandleCompiledLifetimeSelector(
        List<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name.ToFullString() == "WithSingletonLifetime")
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("ServiceLifetime"),
                IdentifierName("Singleton")
            );
        }

        if (name.ToFullString() == "WithScopedLifetime")
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("ServiceLifetime"),
                IdentifierName("Scoped")
            );
        }

        if (name.ToFullString() == "WithTransientLifetime")
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("ServiceLifetime"),
                IdentifierName("Transient")
            );
        }

        if (name.ToFullString() == "WithLifetime" && expression.ArgumentList.Arguments.Count == 1)
        {
            if (expression.ArgumentList.Arguments[0].Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                return memberAccessExpressionSyntax;
        }

        return null;
    }

    private static IEnumerable<IServiceTypeDescriptor> HandleCompiledServiceTypeSelector(
        List<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name.ToFullString() == "AsSelf")
        {
            yield return new SelfServiceTypeDescriptor();
            yield break;
        }

        if (name.ToFullString() == "AsImplementedInterfaces")
        {
            yield return new ImplementedInterfacesServiceTypeDescriptor();
            yield break;
        }

        if (name.ToFullString() == "AsMatchingInterface")
        {
            yield return new MatchingInterfaceServiceTypeDescriptor();
            yield break;
        }

        if (name.ToFullString() == "UsingAttributes")
        {
            yield return new UsingAttributeServiceTypeDescriptor();
            yield break;
        }

        if (name.ToFullString() == "AsSelfWithInterfaces")
        {
            yield return new SelfServiceTypeDescriptor();
            yield return new ImplementedInterfacesServiceTypeDescriptor();
            yield break;
        }

        if (name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
        {
            var typeSyntax = Helpers.ExtractSyntaxFromMethod(expression, name);
            if (typeSyntax == null)
            {
                yield break;
            }

            var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type;
            switch (typeInfo)
            {
                case INamedTypeSymbol nts:
                    yield return new CompiledServiceTypeDescriptor(nts);
                    yield break;
                default:
                    diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                    yield break;
            }
        }
    }

    private static IAssemblyDescriptor? HandleCompiledAssemblySelector(
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name.ToFullString() == "FromAssembly")
            return new AssemblyDescriptor(semanticModel.Compilation.Assembly);
        if (name.ToFullString() == "FromAssemblies")
            return new AllAssemblyDescriptor();

        var typeSyntax = Helpers.ExtractSyntaxFromMethod(expression, name);
        if (typeSyntax == null)
            return null;

        var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type;
        if (typeInfo == null)
            return null;
        return typeInfo switch
        {
            INamedTypeSymbol nts when name.ToFullString().StartsWith("FromAssemblyDependenciesOf", StringComparison.Ordinal) =>
                new CompiledAssemblyDependenciesDescriptor(nts),
            INamedTypeSymbol nts when name.ToFullString().StartsWith("FromAssemblyOf", StringComparison.Ordinal) =>
                new CompiledAssemblyDescriptor(nts),
            _ => null
        };
    }

    private static ClassFilter HandleCompiledImplementationTypeSelector(
        InvocationExpressionSyntax expression,
        NameSyntax name
    )
    {
        if (name.ToFullString() == "AddClasses" && expression.ArgumentList.Arguments.Count is >= 0 and <= 2)
        {
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.IsKind(SyntaxKind.TrueKeyword))
                {
                    return ClassFilter.PublicOnly;
                }
            }
        }

        return ClassFilter.All;
    }

    private static IEnumerable<ITypeFilterDescriptor> HandleCompiledImplementationTypeFilter(
        List<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        InvocationExpressionSyntax expression,
        NameSyntax name,
        INamedTypeSymbol objectType
    )
    {
        if (name.ToFullString() == "AssignableToAny")
        {
            foreach (var argument in expression.ArgumentList.Arguments)
            {
                if (argument.Expression is TypeOfExpressionSyntax typeOfExpressionSyntax)
                {
                    var typeInfo = semanticModel.GetTypeInfo(typeOfExpressionSyntax.Type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledAssignableToAnyTypeFilterDescriptor(nts);
                            continue;

                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                diagnostics.Add(Diagnostic.Create(Diagnostics.MustBeTypeOf, argument.Expression.GetLocation()));
                yield return new CompiledAbortTypeFilterDescriptor(objectType);
                yield break;
            }

            yield break;
        }

        if (name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
        {
            if (genericNameSyntax.Identifier.ToFullString() == "AssignableTo")
            {
                var type = Helpers.ExtractSyntaxFromMethod(expression, name);
                if (type is { })
                {
                    var typeInfo = semanticModel.GetTypeInfo(type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledAssignableToTypeFilterDescriptor(nts);
                            yield break;

                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                yield break;
            }

            if (genericNameSyntax.Identifier.ToFullString() == "WithAttribute")
            {
                var type = Helpers.ExtractSyntaxFromMethod(expression, name);
                if (type is { })
                {
                    var typeInfo = semanticModel.GetTypeInfo(type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledWithAttributeFilterDescriptor(nts);
                            yield break;

                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                yield break;
            }

            if (genericNameSyntax.Identifier.ToFullString() == "WithoutAttribute")
            {
                var type = Helpers.ExtractSyntaxFromMethod(expression, name);
                if (type is { })
                {
                    var typeInfo = semanticModel.GetTypeInfo(type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledWithoutAttributeFilterDescriptor(nts);
                            yield break;

                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                yield break;
            }

            NamespaceFilter? filter = null;
            if (genericNameSyntax.Identifier.ToFullString() == "InExactNamespaceOf")
            {
                filter = NamespaceFilter.Exact;
            }

            if (genericNameSyntax.Identifier.ToFullString() == "InNamespaceOf")
            {
                filter = NamespaceFilter.In;
            }

            if (genericNameSyntax.Identifier.ToFullString() == "NotInNamespaceOf")
            {
                filter = NamespaceFilter.NotIn;
            }

            if (filter is { })
            {
                var symbol = semanticModel.GetTypeInfo(genericNameSyntax.TypeArgumentList.Arguments[0]).Type!;
                yield return new NamespaceFilterDescriptor(filter.Value, new[] { symbol.ContainingNamespace.ToDisplayString() });
            }

            yield break;
        }

        if (name is SimpleNameSyntax simpleNameSyntax)
        {
            if (simpleNameSyntax.ToFullString() == "AssignableTo")
            {
                var type = Helpers.ExtractSyntaxFromMethod(expression, name);
                if (type is { })
                {
                    var typeInfo = semanticModel.GetTypeInfo(type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledAssignableToTypeFilterDescriptor(nts);
                            yield break;
                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                diagnostics.Add(Diagnostic.Create(Diagnostics.MustBeTypeOf, simpleNameSyntax.Identifier.GetLocation()));
                yield return new CompiledAbortTypeFilterDescriptor(objectType);
                yield break;
            }

            if (simpleNameSyntax.ToFullString() == "WithAttribute")
            {
                var type = Helpers.ExtractSyntaxFromMethod(expression, name);
                if (type is { })
                {
                    var typeInfo = semanticModel.GetTypeInfo(type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledWithAttributeFilterDescriptor(nts);
                            yield break;

                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                yield break;
            }

            if (simpleNameSyntax.ToFullString() == "WithoutAttribute")
            {
                var type = Helpers.ExtractSyntaxFromMethod(expression, name);
                if (type is { })
                {
                    var typeInfo = semanticModel.GetTypeInfo(type).Type;
                    switch (typeInfo)
                    {
                        case INamedTypeSymbol nts:
                            yield return new CompiledWithoutAttributeFilterDescriptor(nts);
                            yield break;

                        default:
                            diagnostics.Add(Diagnostic.Create(Diagnostics.UnhandledSymbol, name.GetLocation()));
                            yield break;
                    }
                }

                yield break;
            }

            {
                NamespaceFilter? filter = null;
                if (simpleNameSyntax.ToFullString() == "InExactNamespaces" ||
                    simpleNameSyntax.Identifier.ToFullString() == "InExactNamespaceOf")
                {
                    filter = NamespaceFilter.Exact;
                }

                if (simpleNameSyntax.ToFullString() == "InNamespaces" ||
                    simpleNameSyntax.Identifier.ToFullString() == "InNamespaceOf")
                {
                    filter = NamespaceFilter.In;
                }

                if (simpleNameSyntax.ToFullString() == "NotInNamespaces" ||
                    simpleNameSyntax.Identifier.ToFullString() == "NotInNamespaceOf")
                {
                    filter = NamespaceFilter.NotIn;
                }

                if (filter is { })
                {
                    var namespaces = expression.ArgumentList.Arguments
                                               .Select(
                                                    argument =>
                                                    {
                                                        switch (argument.Expression)
                                                        {
                                                            case LiteralExpressionSyntax literalExpressionSyntax
                                                                when literalExpressionSyntax.Token.IsKind(SyntaxKind.StringLiteralToken):
                                                                return literalExpressionSyntax.Token.ValueText;
                                                            case InvocationExpressionSyntax
                                                            {
                                                                Expression: IdentifierNameSyntax { Identifier: { Text: "nameof" } }
                                                            } invocationExpressionSyntax
                                                                when invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax
                                                                    identifierNameSyntax:
                                                                return identifierNameSyntax.Identifier.Text;
                                                            case TypeOfExpressionSyntax typeOfExpressionSyntax:
                                                                {
                                                                    var symbol = semanticModel.GetTypeInfo(typeOfExpressionSyntax.Type).Type!;
                                                                    return symbol.ContainingNamespace.ToDisplayString();
                                                                }
                                                            default:
                                                                diagnostics.Add(
                                                                    Diagnostic.Create(Diagnostics.NamespaceMustBeAString, argument.GetLocation())
                                                                );
                                                                return null!;
                                                        }
                                                    }
                                                )
                                               .Where(z => !string.IsNullOrWhiteSpace(z))
                                               .ToArray();

                    yield return new NamespaceFilterDescriptor(filter.Value, namespaces);
                }
            }


            {
                TextDirectionFilter? filter = null;
                if (simpleNameSyntax.ToString() is "Suffix" or "Postfix" or "EndsWith")
                {
                    filter = TextDirectionFilter.EndsWith;
                }

                if (simpleNameSyntax.ToFullString() is "Affix" or "Prefix" or "StartsWith")
                {
                    filter = TextDirectionFilter.StartsWith;
                }

                if (simpleNameSyntax.ToFullString() is "Contains" or "Includes")
                {
                    filter = TextDirectionFilter.Contains;
                }

                if (filter is { })
                {
                    var stringValues = expression.ArgumentList.Arguments
                                                 .Select(
                                                      argument =>
                                                      {
                                                          switch (argument.Expression)
                                                          {
                                                              case LiteralExpressionSyntax literalExpressionSyntax
                                                                  when literalExpressionSyntax.Token.IsKind(SyntaxKind.StringLiteralToken):
                                                                  return literalExpressionSyntax.Token.ValueText;
                                                              case InvocationExpressionSyntax
                                                              {
                                                                  Expression: IdentifierNameSyntax { Identifier: { Text: "nameof" } }
                                                              } invocationExpressionSyntax
                                                                  when invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax
                                                                      identifierNameSyntax:
                                                                  return identifierNameSyntax.Identifier.Text;
                                                              default:
                                                                  diagnostics.Add(
                                                                      Diagnostic.Create(Diagnostics.NamespaceMustBeAString, argument.GetLocation())
                                                                  );
                                                                  return null!;
                                                          }
                                                      }
                                                  )
                                                 .Where(z => !string.IsNullOrWhiteSpace(z))
                                                 .ToArray();

                    yield return new NameFilterDescriptor(filter.Value, stringValues);
                }
            }
        }
    }
}
