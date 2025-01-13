using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(AssemblyCollectionData))]
[JsonSerializable(typeof(ReflectionCollectionData))]
[JsonSerializable(typeof(ServiceDescriptorCollectionData))]
[JsonSerializable(typeof(AssemblyFilterData))]
[JsonSerializable(typeof(SourceLocation))]
[JsonSerializable(typeof(SavedSourceLocation))]
[JsonSerializable(typeof(TypeFilterData))]
[JsonSerializable(typeof(NamespaceFilterData))]
[JsonSerializable(typeof(NameFilterData))]
[JsonSerializable(typeof(TypeKindFilterData))]
[JsonSerializable(typeof(TypeInfoFilterData))]
[JsonSerializable(typeof(WithAttributeData))]
[JsonSerializable(typeof(WithAttributeStringData))]
[JsonSerializable(typeof(AssignableToTypeData))]
[JsonSerializable(typeof(AssignableToAnyTypeData))]
[JsonSerializable(typeof(CompiledAssemblyProviderData))]
[JsonSerializable(typeof(GetAssemblyConfiguration))]
[JsonSerializable(typeof(GetReflectionCollectionData))]
[JsonSerializable(typeof(GetServiceDescriptorCollectionData))]
[JsonSerializable(typeof(GeneratedAssemblyProviderData))]
[JsonSerializable(typeof(GeneratedLocationAssemblyResolvedSourceCollection))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();
}
