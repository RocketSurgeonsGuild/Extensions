#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_ReadOnlyCollection
    {
        public IReadOnlyCollection<string> Values { get; set; }
        public ReadOnlyCollection<Fixture_Thing> Things;
    }
}
