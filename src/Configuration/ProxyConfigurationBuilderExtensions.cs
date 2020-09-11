using System.Collections.Generic;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Extensions.Configuration
{
    internal static class ProxyConfigurationBuilderExtensions
    {
        public static ProxyConfigurationBuilder Apply(
            this ProxyConfigurationBuilder builder,
            IEnumerable<ConfigOptionApplicationDelegate> builders
        )
        {
            foreach (var b in builders)
            {
                b(builder);
            }

            return builder;
        }

        public static ProxyConfigurationBuilder Apply(
            this ProxyConfigurationBuilder builder,
            IEnumerable<ConfigOptionEnvironmentDelegate> builders,
            string environmentName
        )
        {
            foreach (var b in builders)
            {
                b(builder, environmentName);
            }

            return builder;
        }
        public static IMsftConfigurationBuilder Apply(
            this IMsftConfigurationBuilder builder,
            IEnumerable<ConfigOptionApplicationDelegate> builders
        )
        {
            foreach (var b in builders)
            {
                b(builder);
            }

            return builder;
        }

        public static IMsftConfigurationBuilder Apply(
            this IMsftConfigurationBuilder builder,
            IEnumerable<ConfigOptionEnvironmentDelegate> builders,
            string environmentName
        )
        {
            foreach (var b in builders)
            {
                b(builder, environmentName);
            }

            return builder;
        }
    }
}