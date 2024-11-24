using System.Collections.Immutable;
using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal record TypeInfoFilterDescriptor(bool Include, ImmutableHashSet<TypeInfoFilter> TypeInfos) : ITypeFilterDescriptor;
