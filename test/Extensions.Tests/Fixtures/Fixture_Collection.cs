#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_Collection
    {
        public Collection<string> Values;
        public ICollection<Fixture_Thing> Things { get; set; }
    }
}
