#nullable disable
namespace Rocket.Surgery.Extensions.Tests.Fixtures;

internal class Fixture_ReadOnlyList
{
    public IReadOnlyList<string> Values { get; set; }
    public IReadOnlyList<Fixture_Thing> Things;
}
