using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal record WithAttributeFilterDescriptor(INamedTypeSymbol Attribute) : ITypeFilterDescriptor;
