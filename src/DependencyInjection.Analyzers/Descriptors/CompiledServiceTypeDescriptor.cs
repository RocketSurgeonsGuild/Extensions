using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

internal record struct CompiledServiceTypeDescriptor(INamedTypeSymbol Type) : IServiceTypeDescriptor;
