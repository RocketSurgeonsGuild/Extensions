using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Configuration;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Extensions.Configuration;

/// <summary>
///     YamlConfigOptionsExtensions
/// </summary>
public static class YamlConfigOptionsExtensions
{
    /// <summary>
    ///     Configures the options to inject yaml files into the correct locations in app settings
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ConfigOptions UseYml(this ConfigOptions options)
    {
        options.AddApplicationConfiguration(
            b => b.AddYamlFile("appsettings.yml", true, true)
        );
        options.AddEnvironmentConfiguration(
            (b, environmentName) => b.AddYamlFile(
                $"appsettings.{environmentName}.yml",
                true,
                true
            )
        );
        return options;
    }

    /// <summary>
    ///     Configures the options to inject yaml files into the correct locations in app settings
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ConfigOptions UseYaml(this ConfigOptions options)
    {
        options.AddApplicationConfiguration(
            b => b.AddYamlFile("appsettings.yaml", true, true)
        );
        options.AddEnvironmentConfiguration(
            (b, environmentName) => b.AddYamlFile(
                $"appsettings.{environmentName}.yaml",
                true,
                true
            )
        );
        return options;
    }
}
