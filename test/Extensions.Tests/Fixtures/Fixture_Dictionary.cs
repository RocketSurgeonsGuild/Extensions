using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_Dictionary
    {
        public IDictionary<string, string> Values;
        public Dictionary<string, Fixture_Thing> Things { get; set; }

    }
}
