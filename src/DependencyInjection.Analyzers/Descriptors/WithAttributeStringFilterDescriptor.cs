using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Descriptors;

[DebuggerDisplay("{ToString()}")]
internal record WithAttributeStringFilterDescriptor
(
    [property: JsonPropertyName("a")]
    string AttributeClassName) : ITypeFilterDescriptor;
