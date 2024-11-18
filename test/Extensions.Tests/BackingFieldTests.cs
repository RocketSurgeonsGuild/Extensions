#nullable disable
using FluentAssertions;
using Rocket.Surgery.Reflection;

// ReSharper disable NullableWarningSuppressionIsUsed

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0025 // Use expression body for properties
#pragma warning disable CS0649
namespace Rocket.Surgery.Extensions.Tests;

public class BackingFieldTests
{
    [Test]
    [Arguments(typeof(BackingField1))]
    [Arguments(typeof(BackingField2))]
    [Arguments(typeof(BackingField3))]
    [Arguments(typeof(BackingField5))]
    [Arguments(typeof(BackingField6))]
    [Arguments(typeof(ExplictField1))]
    public void ShouldWorkForCompilerGeneratedProperties(Type backingField)
    {
        var instance = (IBackingField)Activator.CreateInstance(backingField);
        new BackingFieldHelper().SetBackingField(instance, x => x.Value, "abcd");
        instance!.Value.Should().Be("abcd");
    }

    [Test]
    [Skip("Not supported")]
    [Arguments(typeof(BackingField4))]
    [Arguments(typeof(ExplictField2))]
    public void ShouldNotWorkForCustomProperties(Type backingField)
    {
        var instance = (IBackingField)Activator.CreateInstance(backingField);
        var a = () => new BackingFieldHelper().SetBackingField(instance, x => x.Value, "abcd");
        instance!.Value.Should().BeNullOrWhiteSpace();
        a.Should().Throw<NotSupportedException>();
    }

    internal interface IBackingField
    {
        string Value { get; }
    }

    internal class BackingField1 : IBackingField
    {
        public string Value { get; }
    }

    internal class BackingField2 : IBackingField
    {
        public string Value { get; private set; }
    }

    internal class BackingField3 : IBackingField
    {
        public string Value { get; internal set; }
    }

    internal class BackingField5 : IBackingField
    {
        public string Value { get; protected set; }
    }

    internal class BackingField6 : IBackingField
    {
        public string Value { get; set; }
    }

    internal class BackingField4 : IBackingField
    {
        public string Value { get; set; }
    }

    internal class ExplictField1 : IBackingField
    {
        string IBackingField.Value { get; }
    }

    internal class ExplictField2 : IBackingField
    {
        private string _value;

        // ReSharper disable once ConvertToAutoProperty
        string IBackingField.Value => _value;
    }
}
