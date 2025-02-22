using FakeItEasy;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Tests.Logging;

public class LoggingExtensionTests
{
    [Test]
    public void TimeInformationShouldNotBeNull()
    {
        A.Fake<ILogger>().TimeInformation("message").ShouldNotBeNull();
    }

    [Test]
    public void TimeInformationShouldDispose()
    {
        var a = () =>
                {
                    using (A.Fake<ILogger>().TimeInformation("message")) { }
                };
        a.ShouldNotThrow();
    }

    [Test]
    public void TimeDebugShouldNotBeNull()
    {
        A.Fake<ILogger>().TimeDebug("message").ShouldNotBeNull();
    }

    [Test]
    public void TimeDebugShouldDispose()
    {
        var a = () =>
                {
                    using (A.Fake<ILogger>().TimeDebug("message")) { }
                };
        a.ShouldNotThrow();
    }

    [Test]
    public void TimeTraceShouldNotBeNull()
    {
        A.Fake<ILogger>().TimeTrace("message").ShouldNotBeNull();
    }

    [Test]
    public void TimeTraceShouldDispose()
    {
        var a = () =>
                {
                    using (A.Fake<ILogger>().TimeTrace("message")) { }
                };
        a.ShouldNotThrow();
    }
}
