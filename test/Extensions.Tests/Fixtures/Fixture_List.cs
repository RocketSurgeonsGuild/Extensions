#nullable disable
namespace Rocket.Surgery.Extensions.Tests.Fixtures;

internal class Fixture_List
{
    public List<string> Values;
    public IList<Fixture_Thing> Things { get; set; }
}
