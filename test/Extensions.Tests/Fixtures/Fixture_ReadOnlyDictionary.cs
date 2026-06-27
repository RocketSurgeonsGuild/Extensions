#nullable disable
using System.Collections.ObjectModel;

namespace Rocket.Surgery.Extensions.Tests.Fixtures;

internal class Fixture_ReadOnlyDictionary
{
    public IReadOnlyDictionary<string, string> Values { get; set; }
    public ReadOnlyDictionary<string, Fixture_Thing> Things;
}
