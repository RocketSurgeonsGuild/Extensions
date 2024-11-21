using System.Collections.Immutable;
using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct WithAnyAttributeStringFilterDescriptor(ImmutableHashSet<string> AttributeClassNames) : ITypeFilterDescriptor;
