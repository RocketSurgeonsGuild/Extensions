using Microsoft.Extensions.Configuration;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Extensions.Configuration
{
    /// <summary>
    /// YamlConfigOptionsExtensions
    /// </summary>
    public static class YamlConfigOptionsExtensions
    {
        /// <summary>
        /// Configures the options to inject yaml files into the correct locations in app settings
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ConfigOptions UseYml(this ConfigOptions options)
        {
            options.AddApplicationConfiguration(
                b => b.AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
            );
            options.AddEnvironmentConfiguration(
                (b, environmentName) => b.AddYamlFile(
                    $"appsettings.{environmentName}.yml",
                    optional: true,
                    reloadOnChange: true
                )
            );
            return options;
        }

        /// <summary>
        /// Configures the options to inject yaml files into the correct locations in app settings
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ConfigOptions UseYaml(this ConfigOptions options)
        {
            options.AddApplicationConfiguration(
                b => b.AddYamlFile("appsettings.yaml", optional: true, reloadOnChange: true)
            );
            options.AddEnvironmentConfiguration(
                (b, environmentName) => b.AddYamlFile(
                    $"appsettings.{environmentName}.yaml",
                    optional: true,
                    reloadOnChange: true
                )
            );
            return options;
        }
    }
}