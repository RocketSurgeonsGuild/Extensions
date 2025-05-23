using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Rocket.Surgery.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Conventions;

/// <summary>
///     Helper method for working with <see cref="IConfigurationBuilder" />
/// </summary>
public static class ConfigHostBuilderExtensions
{
    /// <summary>
    ///     Configures the application configuration.
    /// </summary>
    /// <param name="configurationBuilder">The host builder.</param>
    /// <param name="options"></param>
    public static IConfigurationBuilder UseLocalConfiguration(this IConfigurationBuilder configurationBuilder, ConfigOptions options)
    {
        return UseLocalConfiguration(configurationBuilder, () => options);
    }

    /// <summary>
    ///     Configures the application configuration.
    /// </summary>
    /// <param name="configurationBuilder">The host builder.</param>
    /// <param name="configOptionsAction"></param>
    public static IConfigurationBuilder UseLocalConfiguration(this IConfigurationBuilder configurationBuilder, Func<ConfigOptions> configOptionsAction)
    {
        return UseLocalConfiguration(configurationBuilder, _ => configOptionsAction());
    }

    /// <summary>
    ///     Configures the application configuration.
    /// </summary>
    /// <param name="configurationBuilder">The host builder.</param>
    /// <param name="configOptionsAction"></param>
    public static IConfigurationBuilder UseLocalConfiguration(this IConfigurationBuilder configurationBuilder, Action<ConfigOptions> configOptionsAction)
    {
        return UseLocalConfiguration(
            configurationBuilder,
            x =>
            {
                configOptionsAction(x);
                return x;
            }
        );
    }

    /// <summary>
    ///     Configures the application configuration.
    /// </summary>
    /// <param name="configurationBuilder">The host builder.</param>
    /// <param name="configOptionsAction"></param>
    public static IConfigurationBuilder UseLocalConfiguration(
        this IConfigurationBuilder configurationBuilder,
        Func<ConfigOptions, ConfigOptions> configOptionsAction
    )
    {
        var options = configOptionsAction(new());
        InsertConfigurationSourceAfter(
            configurationBuilder.Sources,
            sources => Enumerable.FirstOrDefault(
                sources
                   .OfType<FileConfigurationSource>(),
                x => string.Equals(x.Path, $"appsettings.{options.EnvironmentName ?? string.Empty}.json", StringComparison.OrdinalIgnoreCase)
            ),
            [
                new JsonConfigurationSource
                {
                    FileProvider = configurationBuilder.GetFileProvider(),
                    Path = "appsettings.local.json",
                    Optional = true,
                    ReloadOnChange = true,
                },
            ]
        );

        ReplaceConfigurationSourceAt(
            configurationBuilder.Sources,
            sources => Enumerable.FirstOrDefault(
                sources
                   .OfType<FileConfigurationSource>(),
                x => string.Equals(x.Path, "appsettings.json", StringComparison.OrdinalIgnoreCase)
            ),
            new ProxyConfigurationBuilder(configurationBuilder)
               .Apply(options.ApplicationConfiguration)
               .GetAdditionalSources()
        );

        if (!string.IsNullOrEmpty(options.EnvironmentName))
        {
            ReplaceConfigurationSourceAt(
                configurationBuilder.Sources,
                sources => Enumerable.FirstOrDefault(
                    sources
                       .OfType<FileConfigurationSource>(),
                    x => string.Equals(x.Path, $"appsettings.{options.EnvironmentName}.json", StringComparison.OrdinalIgnoreCase)
                ),
                new ProxyConfigurationBuilder(configurationBuilder).Apply(options.EnvironmentConfiguration, options.EnvironmentName!).GetAdditionalSources()
            );
        }

        ReplaceConfigurationSourceAt(
            configurationBuilder.Sources,
            sources => Enumerable.FirstOrDefault(
                sources
                   .OfType<FileConfigurationSource>(),
                x => string.Equals(x.Path, "appsettings.local.json", StringComparison.OrdinalIgnoreCase)
            ),
            new ProxyConfigurationBuilder(configurationBuilder)
               .Apply(options.EnvironmentConfiguration, "local")
               .GetAdditionalSources()
        );

        return configurationBuilder;
    }

    private static void InsertConfigurationSourceAfter<T>(
        IList<IConfigurationSource> sources,
        Func<IList<IConfigurationSource>, T?> getSource,
        IEnumerable<IConfigurationSource> createSourceFrom
    )
        where T : IConfigurationSource
    {
        var source = getSource(sources);
        if (source != null)
        {
            var index = sources.IndexOf(source);
            foreach (var newSource in createSourceFrom)
            {
                sources.Insert(index + 1, newSource);
            }
        }
        else
        {
            foreach (var newSource in createSourceFrom)
            {
                sources.Add(newSource);
            }
        }
    }

    private static void ReplaceConfigurationSourceAt<T>(
        IList<IConfigurationSource> sources,
        Func<IList<IConfigurationSource>, T?> getSource,
        IEnumerable<IConfigurationSource> createSourceFrom
    )
        where T : class, IConfigurationSource
    {
        var iConfigurationSources = createSourceFrom as IConfigurationSource[] ?? createSourceFrom.ToArray();
        if (iConfigurationSources.Length == 0)
            return;
        var source = getSource(sources);
        if (source != null)
        {
            var index = sources.IndexOf(source);
            sources.RemoveAt(index);
            foreach (var newSource in iConfigurationSources)
            {
                sources.Insert(index, newSource);
            }
        }
        else
        {
            foreach (var newSource in iConfigurationSources)
            {
                sources.Add(newSource);
            }
        }
    }
}
