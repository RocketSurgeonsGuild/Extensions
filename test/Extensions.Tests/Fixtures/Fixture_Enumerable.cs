#nullable disable
namespace Rocket.Surgery.Extensions.Tests.Fixtures;

internal class Fixture_Enumerable
{
    public IEnumerable<string> Values;
    public IEnumerable<Fixture_Thing> Things { get; set; }
}
