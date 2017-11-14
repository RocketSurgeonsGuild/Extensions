using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Rocket.Surgery.Extensions.Tests
{
    public class DeconstructorExtensionsTests
    {
        [Fact]
        public void Deconstructs_TheTarget_KeyValuePair()
        {
            var kvp = new KeyValuePair<string, object>("abc", 123);

            var (key, value) = kvp;

            key.Should().Be("abc");
            value.Should().Be(123);
        }
    }
}
