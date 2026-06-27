#nullable disable
using System.Collections.ObjectModel;

namespace Rocket.Surgery.Extensions.Tests.Fixtures;

internal class Fixture_ReadOnlyCollection
{
    public IReadOnlyCollection<string> Values { get; set; }
    public ReadOnlyCollection<Fixture_Thing> Things;
}
