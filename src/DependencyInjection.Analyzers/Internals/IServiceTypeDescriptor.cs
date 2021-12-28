using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Internals;

internal interface IServiceTypeDescriptor
{
}

internal struct SelfServiceTypeDescriptor : IServiceTypeDescriptor
{
}

internal struct ImplementedInterfacesServiceTypeDescriptor : IServiceTypeDescriptor
{
}

internal struct MatchingInterfaceServiceTypeDescriptor : IServiceTypeDescriptor
{
}

internal struct UsingAttributeServiceTypeDescriptor : IServiceTypeDescriptor
{
}

internal struct CompiledServiceTypeDescriptor : IServiceTypeDescriptor
{
    public INamedTypeSymbol Type { get; }

    public CompiledServiceTypeDescriptor(INamedTypeSymbol type)
    {
        Type = type;
    }
}
