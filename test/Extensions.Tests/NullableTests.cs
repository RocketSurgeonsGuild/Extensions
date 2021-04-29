using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;
using Rocket.Surgery.Extensions;
using FluentAssertions;

namespace Rocket.Surgery.Extensions.Tests
{
    public class NullableTests : LoggerTest
    {
        public NullableTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Checks_Nullable_Property()
        {
            typeof(NullableTest).GetProperty(nameof(NullableTest.Property))
            .GetNullability().Should().Be(Nullability.Nullable);
        }

        [Fact]
        public void Checks_Nullable_Field()
        {
            typeof(NullableTest).GetField(nameof(NullableTest._field))
            .GetNullability().Should().Be(Nullability.Nullable);
        }

        [Fact]
        public void Checks_Nullable_Method()
        {
            typeof(NullableTest).GetMethod(nameof(NullableTest.Method))
            .GetNullability().Should().Be(Nullability.Nullable);
        }

        [Fact]
        public void Checks_Nullable_Parameter()
        {
            typeof(NullableTest).GetMethod(nameof(NullableTest.Method)).GetParameters()[0]
            .GetNullability().Should().Be(Nullability.Nullable);
        }

        [Fact]
        public void Checks_NonNullable_Property()
        {
            typeof(NonNullableTest).GetProperty(nameof(NonNullableTest.Property))
            .GetNullability().Should().Be(Nullability.NonNullable);
        }

        [Fact]
        public void Checks_NonNullable_Field()
        {
            typeof(NonNullableTest).GetField(nameof(NonNullableTest._field))
            .GetNullability().Should().Be(Nullability.NonNullable);
        }

        [Fact]
        public void Checks_NonNullable_Method()
        {
            typeof(NonNullableTest).GetMethod(nameof(NonNullableTest.Method))
            .GetNullability().Should().Be(Nullability.NonNullable);
        }

        [Fact]
        public void Checks_NonNullable_Parameter()
        {
            typeof(NonNullableTest).GetMethod(nameof(NonNullableTest.Method)).GetParameters()[0]
            .GetNullability().Should().Be(Nullability.NonNullable);
        }

        [Fact]
        public void Checks_DisabledNullable_Property()
        {
            typeof(DisabledNullableTest).GetProperty(nameof(DisabledNullableTest.Property))
            .GetNullability().Should().Be(Nullability.NotDefined);
        }

        [Fact]
        public void Checks_DisabledNullable_Field()
        {
            typeof(DisabledNullableTest).GetField(nameof(DisabledNullableTest._field))
            .GetNullability().Should().Be(Nullability.NotDefined);
        }

        [Fact]
        public void Checks_DisabledNullable_Method()
        {
            typeof(DisabledNullableTest).GetMethod(nameof(DisabledNullableTest.Method))
            .GetNullability().Should().Be(Nullability.NotDefined);
        }

        [Fact]
        public void Checks_DisabledNullable_Parameter()
        {
            typeof(DisabledNullableTest).GetMethod(nameof(DisabledNullableTest.Method)).GetParameters()[0]
            .GetNullability().Should().Be(Nullability.NotDefined);
        }

#nullable enable
        class NullableTest
        {
            public MyObject? Property { get; set; }
            public MyObject? Method(MyObject? value) => value;
            public MyObject? _field;
        }

        class NonNullableTest
        {
            public MyObject Property { get; set; }
            public MyObject Method(MyObject value) => value;
            public MyObject _field;
        }
#nullable restore
#nullable disable
        class DisabledNullableTest
        {
            public MyObject Property { get; set; }
            public MyObject Method(MyObject value) => value;
            public MyObject _field;
        }
#nullable restore


        class MyObject { }
    }
}
