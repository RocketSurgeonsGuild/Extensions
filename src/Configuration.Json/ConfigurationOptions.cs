using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.Extensions.Configuration;

/// <summary>
///     JsonConfigOptionsExtensions
/// </summary>
public static class JsonConfigOptionsExtensions
{
    /// <summary>
    ///     Configures the options to inject json files into the correct locations in app settings
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ConfigOptions UseJson(this ConfigOptions options)
    {
        options.AddApplicationConfiguration(b => b.AddJsonFile("appsettings.json", true, true));
        options.AddEnvironmentConfiguration((b, environmentName) => b.AddJsonFile($"appsettings.{environmentName}.json", true, true));
        return options;
    }
}
