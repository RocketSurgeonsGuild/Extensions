using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Tests.Logging;

public class LoggingExtensionTests
{
    [Test]
    public void TimeInformationShouldNotBeNull()
    {
        A.Fake<ILogger>().TimeInformation("message").Should().NotBeNull();
    }

    [Test]
    public void TimeInformationShouldDispose()
    {
        var a = () =>
                {
                    using (A.Fake<ILogger>().TimeInformation("message")) { }
                };
        a.Should().NotThrow();
    }

    [Test]
    public void TimeDebugShouldNotBeNull()
    {
        A.Fake<ILogger>().TimeDebug("message").Should().NotBeNull();
    }

    [Test]
    public void TimeDebugShouldDispose()
    {
        var a = () =>
                {
                    using (A.Fake<ILogger>().TimeDebug("message")) { }
                };
        a.Should().NotThrow();
    }

    [Test]
    public void TimeTraceShouldNotBeNull()
    {
        A.Fake<ILogger>().TimeTrace("message").Should().NotBeNull();
    }

    [Test]
    public void TimeTraceShouldDispose()
    {
        var a = () =>
                {
                    using (A.Fake<ILogger>().TimeTrace("message")) { }
                };
        a.Should().NotThrow();
    }
}
