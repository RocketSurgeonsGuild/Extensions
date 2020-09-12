using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.Extensions.Configuration
{
    /// <summary>
    /// Delegate for defining application configuration
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="environmentName"></param>
    public delegate IConfigurationBuilder ConfigOptionEnvironmentDelegate(IConfigurationBuilder builder, string environmentName);
}