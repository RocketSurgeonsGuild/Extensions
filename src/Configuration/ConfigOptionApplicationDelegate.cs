using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.Extensions.Configuration;

/// <summary>
///     Delegate for defining application configuration
/// </summary>
/// <param name="builder"></param>
#pragma warning disable CA1711
public delegate IConfigurationBuilder ConfigOptionApplicationDelegate(IConfigurationBuilder builder);
#pragma warning restore CA1711
