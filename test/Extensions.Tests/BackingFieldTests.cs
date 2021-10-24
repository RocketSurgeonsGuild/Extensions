#nullable disable
using FluentAssertions;
using Rocket.Surgery.Reflection;
using Xunit;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0025 // Use expression body for properties
#pragma warning disable CS0649
namespace Rocket.Surgery.Extensions.Tests;

public class BackingFieldTests
{
    private interface IBackingField
    {
        string Value { get; }
    }

    private class BackingField1 : IBackingField
    {
        public string Value { get; }
    }

    private class BackingField2 : IBackingField
    {
        public string Value { get; private set; }
    }

    private class BackingField3 : IBackingField
    {
        public string Value { get; internal set; }
    }

    private class BackingField5 : IBackingField
    {
        public string Value { get; protected set; }
    }

    private class BackingField6 : IBackingField
    {
        public string Value { get; set; }
    }

    private class BackingField4 : IBackingField
    {
        public string Value
        {
            get;
            // ReSharper disable once UnusedMember.Local
            set;
        }
    }

    private class ExplictField1 : IBackingField
    {
        string IBackingField.Value { get; }
    }

    private class ExplictField2 : IBackingField
    {
#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 649
        private string _value;
#pragma warning restore 649
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore RCS1169 // Make field read-only.
        // ReSharper disable once ConvertToAutoProperty
        string IBackingField.Value
        {
#pragma warning disable IDE0025 // Use expression body for properties
            get => _value;
#pragma warning restore IDE0025 // Use expression body for properties
        }
    }

    [Theory]
    [InlineData(typeof(BackingField1))]
    [InlineData(typeof(BackingField2))]
    [InlineData(typeof(BackingField3))]
    [InlineData(typeof(BackingField5))]
    [InlineData(typeof(BackingField6))]
    [InlineData(typeof(ExplictField1))]
    public void ShouldWorkForCompilerGeneratedProperties(Type backingField)
    {
        var instance = (IBackingField)Activator.CreateInstance(backingField);
        new BackingFieldHelper().SetBackingField(instance, x => x.Value, "abcd");
        instance!.Value.Should().Be("abcd");
    }

    [Theory]
    [InlineData(typeof(BackingField4))]
    [InlineData(typeof(ExplictField2))]
    public void ShouldNotWorkForCustomProperties(Type backingField)
    {
        var instance = (IBackingField)Activator.CreateInstance(backingField);
        Action a = () => new BackingFieldHelper().SetBackingField(instance, x => x.Value, "abcd");
        instance!.Value.Should().BeNullOrWhiteSpace();
        a.Should().Throw<NotSupportedException>();
    }
}
