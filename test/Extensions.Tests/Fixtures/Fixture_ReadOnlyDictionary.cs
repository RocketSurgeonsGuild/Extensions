#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_ReadOnlyDictionary
    {
        public IReadOnlyDictionary<string, string> Values { get; set; }
        public ReadOnlyDictionary<string, Fixture_Thing> Things;

    }
}
