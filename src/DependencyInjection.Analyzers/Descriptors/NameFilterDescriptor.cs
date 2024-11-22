using System.Collections.Immutable;
using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct NameFilterDescriptor(bool Include, TextDirectionFilter Filter, ImmutableHashSet<string> Names) : ITypeFilterDescriptor;
