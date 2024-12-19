using System.Collections.Immutable;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record ServiceDescriptorFilterData
(
    ImmutableArray<ServiceTypeData> ServiceTypeDescriptors,
    int Lifetime
);