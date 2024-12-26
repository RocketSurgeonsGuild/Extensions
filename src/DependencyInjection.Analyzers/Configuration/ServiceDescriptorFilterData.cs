using System.Collections.Immutable;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record ServiceDescriptorFilterData
(
    ImmutableArray<ServiceTypeData> ServiceTypeDescriptors,
    int Lifetime
);
