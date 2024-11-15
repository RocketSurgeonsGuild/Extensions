using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct TypeKindFilterDescriptor(bool Include, ImmutableHashSet<TypeKind> TypeKinds) : ITypeFilterDescriptor;
