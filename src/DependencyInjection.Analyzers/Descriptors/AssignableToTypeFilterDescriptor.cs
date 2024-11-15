using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct AssignableToTypeFilterDescriptor(INamedTypeSymbol Type) : ITypeFilterDescriptor;
