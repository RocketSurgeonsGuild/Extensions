using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.Extensions.Configuration
{
    /// <summary>
    /// Delegate for defining application configuration
    /// </summary>
    /// <param name="builder"></param>
    public delegate IConfigurationBuilder ConfigOptionApplicationDelegate(IConfigurationBuilder builder);
}