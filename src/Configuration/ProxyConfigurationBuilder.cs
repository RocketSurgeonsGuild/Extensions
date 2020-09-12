using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using IMsftConfigurationBuilder = Microsoft.Extensions.Configuration.IConfigurationBuilder;

namespace Rocket.Surgery.Extensions.Configuration
{
    internal class ProxyConfigurationBuilder : IMsftConfigurationBuilder
    {
        private readonly IMsftConfigurationBuilder _builder;
        private readonly IList<IConfigurationSource> _sources = new List<IConfigurationSource>();

        public ProxyConfigurationBuilder(IMsftConfigurationBuilder builder) => _builder = builder;

        public IEnumerable<IConfigurationSource> GetAdditionalSources() => _sources;

        public IDictionary<string, object> Properties => _builder.Properties;

        public IList<IConfigurationSource> Sources => _builder.Sources;

        public IMsftConfigurationBuilder Add(IConfigurationSource source)
        {
            _sources.Add(source);
            return this;
        }

        public IConfigurationRoot Build() => _builder.Build();
    }
}