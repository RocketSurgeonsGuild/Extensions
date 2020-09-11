using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Logging;
using Xunit;

namespace Rocket.Surgery.Conventions.Tests.Logging
{
    public class LoggingExtensionTests
    {
        [Fact]
        public void TimeInformationShouldNotBeNull()
            => A.Fake<ILogger>().TimeInformation("message").Should().NotBeNull();

        [Fact]
        public void TimeInformationShouldDispose()
        {
            Action a = () =>
            {
                using (A.Fake<ILogger>().TimeInformation("message")) { }
            };
            a.Should().NotThrow();
        }

        [Fact]
        public void TimeDebugShouldNotBeNull() => A.Fake<ILogger>().TimeDebug("message").Should().NotBeNull();

        [Fact]
        public void TimeDebugShouldDispose()
        {
            Action a = () =>
            {
                using (A.Fake<ILogger>().TimeDebug("message")) { }
            };
            a.Should().NotThrow();
        }

        [Fact]
        public void TimeTraceShouldNotBeNull() => A.Fake<ILogger>().TimeTrace("message").Should().NotBeNull();

        [Fact]
        public void TimeTraceShouldDispose()
        {
            Action a = () =>
            {
                using (A.Fake<ILogger>().TimeTrace("message")) { }
            };
            a.Should().NotThrow();
        }
    }
}