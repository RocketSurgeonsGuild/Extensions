using FluentAssertions;
using Rocket.Surgery.Extensions.Testing;

namespace Rocket.Surgery.Extensions.Tests;

public class DeconstructorExtensionsTests() : AutoFakeTest(Defaults.LoggerTest)
{
    [Test]
    public void Deconstructs_TheTarget_KeyValuePair()
    {
        var kvp = new KeyValuePair<string, object>("abc", 123);

        ( var key, var value ) = kvp;

        key.Should().Be("abc");
        value.Should().Be(123);
    }
}
