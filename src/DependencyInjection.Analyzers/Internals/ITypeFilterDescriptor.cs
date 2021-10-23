using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Internals;

internal interface ITypeFilterDescriptor
{
}

internal struct NamespaceFilterDescriptor : ITypeFilterDescriptor
{
    public IEnumerable<string> Namespaces { get; }
    public NamespaceFilter Filter { get; }

    public NamespaceFilterDescriptor(NamespaceFilter filter, IEnumerable<string> namespaces)
    {
        Filter = filter;
        Namespaces = namespaces;
    }
}

internal struct NameFilterDescriptor : ITypeFilterDescriptor
{
    public TextDirectionFilter Filter { get; }
    public IEnumerable<string> Names { get; }

    public NameFilterDescriptor(TextDirectionFilter filter, IEnumerable<string> names)
    {
        Names = names;
        Filter = filter;
    }
}

internal struct CompiledWithAttributeFilterDescriptor : ITypeFilterDescriptor
{
    public INamedTypeSymbol Attribute { get; }

    public CompiledWithAttributeFilterDescriptor(INamedTypeSymbol attribute)
    {
        Attribute = attribute;
    }
}

internal struct CompiledWithoutAttributeFilterDescriptor : ITypeFilterDescriptor
{
    public INamedTypeSymbol Attribute { get; }

    public CompiledWithoutAttributeFilterDescriptor(INamedTypeSymbol attribute)
    {
        Attribute = attribute;
    }
}

internal struct CompiledAssignableToTypeFilterDescriptor : ITypeFilterDescriptor
{
    public INamedTypeSymbol Type { get; }

    public CompiledAssignableToTypeFilterDescriptor(INamedTypeSymbol type)
    {
        Type = type;
    }
}

internal struct CompiledAssignableToAnyTypeFilterDescriptor : ITypeFilterDescriptor
{
    public INamedTypeSymbol Type { get; }

    public CompiledAssignableToAnyTypeFilterDescriptor(INamedTypeSymbol type)
    {
        Type = type;
    }
}

internal struct CompiledAbortTypeFilterDescriptor : ITypeFilterDescriptor
{
    public INamedTypeSymbol Type { get; }

    public CompiledAbortTypeFilterDescriptor(INamedTypeSymbol type)
    {
        Type = type;
    }
}
