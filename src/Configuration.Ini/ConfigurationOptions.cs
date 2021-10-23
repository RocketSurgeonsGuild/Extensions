using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Configuration;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Extensions.Configuration;

/// <summary>
///     IniConfigOptionsExtensions
/// </summary>
public static class IniConfigOptionsExtensions
{
    /// <summary>
    ///     Configures the options to inject ini files into the correct locations in app settings
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ConfigOptions UseIni(this ConfigOptions options)
    {
        options.AddApplicationConfiguration(
            b => b.AddIniFile("appsettings.ini", true, true)
        );
        options.AddEnvironmentConfiguration(
            (b, environmentName) => b.AddIniFile(
                $"appsettings.{environmentName}.ini",
                true,
                true
            )
        );
        return options;
    }
}
