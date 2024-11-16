using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal readonly record struct WithoutAttributeStringFilterDescriptor
(
    [property: JsonPropertyName("a")]
    string AttributeClassName) : ITypeFilterDescriptor;
