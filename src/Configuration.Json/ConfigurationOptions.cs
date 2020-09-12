using Microsoft.Extensions.Configuration;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Extensions.Configuration
{
    /// <summary>
    /// JsonConfigOptionsExtensions
    /// </summary>
    public static class JsonConfigOptionsExtensions
    {
        /// <summary>
        /// Configures the options to inject json files into the correct locations in app settings
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ConfigOptions UseJson(this ConfigOptions options)
        {
            options.AddApplicationConfiguration(b => b.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true));
            options.AddEnvironmentConfiguration((b, environmentName) => b.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true));
            return options;
        }
    }
}