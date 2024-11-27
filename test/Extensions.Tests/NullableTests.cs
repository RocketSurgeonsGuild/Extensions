using FluentAssertions;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CS0649
#pragma warning disable CA1822
#pragma warning disable CS8618
namespace Rocket.Surgery.Extensions.Tests;

public class NullableTests(ITestOutputHelper outputHelper) : LoggerTest<XUnitTestContext>(XUnitTestContext.Create(outputHelper))
{
    [Fact]
    public void Checks_Nullable_Property()
    {
        typeof(NullableTest)
           .GetProperty(nameof(NullableTest.Property))
           .GetNullability()
           .Should()
           .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_Nullable_Field()
    {
        typeof(NullableTest)
           .GetField(nameof(NullableTest._field))
           .GetNullability()
           .Should()
           .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_Nullable_Method()
    {
        typeof(NullableTest)
           .GetMethod(nameof(NullableTest.Method))
           .GetNullability()
           .Should()
           .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_Nullable_Parameter()
    {
        typeof(NullableTest).GetMethod(nameof(NullableTest.Method))!.GetParameters()[0]
                            .GetNullability()
                            .Should()
                            .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_NonNullable_Property()
    {
        typeof(NonNullableTest)
           .GetProperty(nameof(NonNullableTest.Property))
           .GetNullability()
           .Should()
           .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_NonNullable_Field()
    {
        typeof(NonNullableTest)
           .GetField(nameof(NonNullableTest._field))
           .GetNullability()
           .Should()
           .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_NonNullable_Method()
    {
        typeof(NonNullableTest)
           .GetMethod(nameof(NonNullableTest.Method))
           .GetNullability()
           .Should()
           .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_NonNullable_Parameter()
    {
        typeof(NonNullableTest).GetMethod(nameof(NonNullableTest.Method))!.GetParameters()[0]
                               .GetNullability()
                               .Should()
                               .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_DisabledNullable_Property()
    {
        typeof(DisabledNullableTest)
           .GetProperty(nameof(DisabledNullableTest.Property))
           .GetNullability()
           .Should()
           .Be(Nullability.NotDefined);
    }

    [Fact]
    public void Checks_DisabledNullable_Field()
    {
        typeof(DisabledNullableTest)
           .GetField(nameof(DisabledNullableTest._field))
           .GetNullability()
           .Should()
           .Be(Nullability.NotDefined);
    }

    [Fact]
    public void Checks_DisabledNullable_Method()
    {
        typeof(DisabledNullableTest)
           .GetMethod(nameof(DisabledNullableTest.Method))
           .GetNullability()
           .Should()
           .Be(Nullability.NotDefined);
    }

    [Fact]
    public void Checks_DisabledNullable_Parameter()
    {
        typeof(DisabledNullableTest).GetMethod(nameof(DisabledNullableTest.Method))!.GetParameters()[0]
                                    .GetNullability()
                                    .Should()
                                    .Be(Nullability.NotDefined);
    }

    [Fact]
    public void Checks_ValueNullable_Property()
    {
        typeof(ValueNullableTest)
           .GetProperty(nameof(ValueNullableTest.Property))
           .GetNullability()
           .Should()
           .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_ValueNullable_Field()
    {
        typeof(ValueNullableTest)
           .GetField(nameof(ValueNullableTest._field))
           .GetNullability()
           .Should()
           .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_ValueNullable_Method()
    {
        typeof(ValueNullableTest)
           .GetMethod(nameof(ValueNullableTest.Method))
           .GetNullability()
           .Should()
           .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_ValueNullable_Parameter()
    {
        typeof(ValueNullableTest).GetMethod(nameof(ValueNullableTest.Method))!.GetParameters()[0]
                                 .GetNullability()
                                 .Should()
                                 .Be(Nullability.Nullable);
    }

    [Fact]
    public void Checks_ValueNonNullable_Property()
    {
        typeof(ValueNonNullableTest)
           .GetProperty(nameof(ValueNonNullableTest.Property))
           .GetNullability()
           .Should()
           .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_ValueNonNullable_Field()
    {
        typeof(ValueNonNullableTest)
           .GetField(nameof(ValueNonNullableTest._field))
           .GetNullability()
           .Should()
           .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_ValueNonNullable_Method()
    {
        typeof(ValueNonNullableTest)
           .GetMethod(nameof(ValueNonNullableTest.Method))
           .GetNullability()
           .Should()
           .Be(Nullability.NonNullable);
    }

    [Fact]
    public void Checks_ValueNonNullable_Parameter()
    {
        typeof(ValueNonNullableTest).GetMethod(nameof(ValueNonNullableTest.Method))!.GetParameters()[0]
                                    .GetNullability()
                                    .Should()
                                    .Be(Nullability.NonNullable);
    }

    private class NullableTest
    {
        public MyObject? _field;
        public MyObject? Property { get; set; }

        public MyObject? Method(MyObject? value) => value;
    }

    private class NonNullableTest
    {
        public MyObject _field;
        public MyObject Property { get; set; }

        public MyObject Method(MyObject value) => value;
    }

    private class ValueNullableTest
    {
        public int? _field;
        public int? Property { get; set; }

        public int? Method(int? value) => value;
    }

    private class ValueNonNullableTest
    {
        public int _field;
        public int Property { get; set; }

        public int Method(int value) => value;
    }
    #nullable disable
    private class DisabledNullableTest
    {
        public MyObject _field;
        public MyObject Property { get; set; }
        public MyObject Method(MyObject value) => value;
    }

    private class MyObject { }
}
