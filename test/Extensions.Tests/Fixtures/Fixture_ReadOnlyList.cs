using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_ReadOnlyList
    {
        public IReadOnlyList<string> Values { get; set; }
        public IReadOnlyList<Fixture_Thing> Things;
    }
}
