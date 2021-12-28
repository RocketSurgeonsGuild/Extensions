using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.Configuration;

/// <summary>
///     Delegate for defining application configuration
/// </summary>
/// <param name="builder"></param>
/// <param name="environmentName"></param>
#pragma warning disable CA1711
public delegate IConfigurationBuilder ConfigOptionEnvironmentDelegate(IConfigurationBuilder builder, string environmentName);
#pragma warning restore CA1711
