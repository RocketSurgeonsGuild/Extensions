#nullable disable
using System;
using FluentAssertions;
using Rocket.Surgery.Reflection;
using Xunit;

namespace Rocket.Surgery.Extensions.Tests
{
    public class BackingFieldTests
    {
        interface IBackingField
        {
            string Value { get; }
        }
        class BackingField1 : IBackingField
        {
            public string Value { get; }
        }
        class BackingField2 : IBackingField
        {
            public string Value { get; private set; }
        }
        class BackingField3 : IBackingField
        {
            public string Value { get; internal set; }
        }
        class BackingField5 : IBackingField
        {
            public string Value { get; protected set; }
        }
        class BackingField6 : IBackingField
        {
            public string Value { get; set; }
        }
        class BackingField4 : IBackingField
        {
            private string _value;
            public string Value
            {
                get => _value;
                set => _value = value;
            }
        }
        class ExplictField1 : IBackingField
        {
            string IBackingField.Value { get; }
        }
        class ExplictField2 : IBackingField
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
            var instance = Activator.CreateInstance(backingField) as IBackingField;
            new BackingFieldHelper().SetBackingField(instance, x => x.Value, "abcd");
            instance.Value.Should().Be("abcd");
        }

        [Theory]
        [InlineData(typeof(BackingField4))]
        [InlineData(typeof(ExplictField2))]
        public void ShouldNotWorkForCustomProperties(Type backingField)
        {
            var instance = Activator.CreateInstance(backingField) as IBackingField;
            Action a = () => new BackingFieldHelper().SetBackingField(instance, x => x.Value, "abcd");
            a.Should().Throw<NotSupportedException>();
        }
    }
}
