#nullable disable
using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Tests.Fixtures
{
    class Fixture_List
    {
        public List<string> Values;
        public IList<Fixture_Thing> Things { get; set; }
    }
}
