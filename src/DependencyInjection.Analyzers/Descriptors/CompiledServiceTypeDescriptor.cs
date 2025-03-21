using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

internal record CompiledServiceTypeDescriptor(INamedTypeSymbol Type) : IServiceTypeDescriptor;

internal record UnknownCompiledServiceTypeDescriptor(AnyTypeData Data) : IServiceTypeDescriptor;
