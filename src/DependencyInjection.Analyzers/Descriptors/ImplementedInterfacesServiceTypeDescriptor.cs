using Rocket.Surgery.DependencyInjection.Analyzers.AssemblyProviders;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

internal record ImplementedInterfacesServiceTypeDescriptor(CompiledTypeFilter? InterfaceFilter) : IServiceTypeDescriptor;
