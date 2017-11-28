using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_Enumerable
    {
        public IEnumerable<string> Values;
        public IEnumerable<Fixture_Thing> Things { get; set; }
    }
}
