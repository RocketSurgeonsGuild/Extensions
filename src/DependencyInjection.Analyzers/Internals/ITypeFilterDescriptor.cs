using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Internals
{
    interface ITypeFilterDescriptor { }

    struct NamespaceFilterDescriptor : ITypeFilterDescriptor
    {
        public IEnumerable<string> Namespaces { get; }
        public NamespaceFilter Filter { get; }

        public NamespaceFilterDescriptor(NamespaceFilter filter, IEnumerable<string> namespaces)
        {
            Filter = filter;
            Namespaces = namespaces;
        }
    }

    struct NameFilterDescriptor : ITypeFilterDescriptor
    {
        public TextDirectionFilter Filter { get; }
        public IEnumerable<string> Names { get; }

        public NameFilterDescriptor(TextDirectionFilter filter, IEnumerable<string> names)
        {
            Names = names;
            Filter = filter;
        }
    }

    struct CompiledWithAttributeFilterDescriptor : ITypeFilterDescriptor
    {
        public INamedTypeSymbol Attribute { get; }

        public CompiledWithAttributeFilterDescriptor(INamedTypeSymbol attribute) => Attribute = attribute;
    }

    struct CompiledWithoutAttributeFilterDescriptor : ITypeFilterDescriptor
    {
        public INamedTypeSymbol Attribute { get; }

        public CompiledWithoutAttributeFilterDescriptor(INamedTypeSymbol attribute) => Attribute = attribute;
    }

    struct CompiledAssignableToTypeFilterDescriptor : ITypeFilterDescriptor
    {
        public INamedTypeSymbol Type { get; }

        public CompiledAssignableToTypeFilterDescriptor(INamedTypeSymbol type) => Type = type;
    }

    struct CompiledAssignableToAnyTypeFilterDescriptor : ITypeFilterDescriptor
    {
        public INamedTypeSymbol Type { get; }

        public CompiledAssignableToAnyTypeFilterDescriptor(INamedTypeSymbol type) => Type = type;
    }

    struct CompiledAbortTypeFilterDescriptor : ITypeFilterDescriptor
    {
        public INamedTypeSymbol Type { get; }

        public CompiledAbortTypeFilterDescriptor(INamedTypeSymbol type) => Type = type;
    }
}