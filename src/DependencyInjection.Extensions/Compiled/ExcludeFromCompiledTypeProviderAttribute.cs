namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
/// Exclude the given assembly from compiled type provider resolution.
/// </summary>
/// <remarks>This assembly will still have access to compiled types, but nothing will be resolved internally.</remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Assembly)]
public class ExcludeFromCompiledTypeProviderAttribute() : Attribute;
