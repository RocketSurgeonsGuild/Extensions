using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Binding;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Tests
{
    public class BindingTests : AutoTestBase
    {
        public BindingTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        class AutoProperty
        {
            public string Value { get; set; }
        }

        [Fact]
        public void BindsTo_AutoProperty()
        {
            var binder = new JsonBinder();

            var result = binder.Get<AutoProperty>(new[] {
                new KeyValuePair<string, string>("value", "123")
            });

            result.Value.Should().Be("123");
        }

        class ReadonlyAutoProperty
        {
            public string Value { get; }
        }

        [Fact]
        public void BindsTo_ReadonlyAutoProperty()
        {
            var binder = new JsonBinder();

            var result = binder.Get<ReadonlyAutoProperty>(new[] {
                new KeyValuePair<string, string>("value", "123")
            });

            result.Value.Should().Be("123");
        }

        class PrivateSetterProperty
        {
            public string Value { get; }
        }

        [Fact]
        public void BindsTo_PrivateSetterProperty()
        {
            var binder = new JsonBinder();

            var result = binder.Get<PrivateSetterProperty>(new[] {
                new KeyValuePair<string, string>("value", "123")
            });

            result.Value.Should().Be("123");
        }

        class ComplexProperty
        {
            public AutoProperty AutoProperty { get; set; }
            public string Value { get; set; }
        }

        [Fact]
        public void BindsTo_ComplexProperties()
        {
            var binder = new JsonBinder();

            var result = binder.Get<ComplexProperty>(new[] {
                new KeyValuePair<string, string>("value", "123"),
                new KeyValuePair<string, string>("AutoProperty:value", "456")
            });

            result.Value.Should().Be("123");
            result.AutoProperty.Value.Should().Be("456");
        }

        [Fact]
        public void BindsTo_ComplexProperties_CustomSep()
        {
            var binder = new JsonBinder("__");

            var result = binder.Get<ComplexProperty>(new[] {
                new KeyValuePair<string, string>("value", "123"),
                new KeyValuePair<string, string>("AutoProperty__value", "456")
            });

            result.Value.Should().Be("123");
            result.AutoProperty.Value.Should().Be("456");
        }

        [Fact]
        public void BindsTo_ComplexProperties_Null()
        {
            var binder = new JsonBinder();

            var result = binder.Get<ComplexProperty>(new[] {
                new KeyValuePair<string, string>("value", "123")
            });

            result.Value.Should().Be("123");
            result.AutoProperty.Should().BeNull();
        }

        class ArrayProperties { public AutoProperty[] Values { get; set; } }
        class SimpleArrayProperties { public string[] Values { get; set; } }
        class EnumerableProperties { public IEnumerable<AutoProperty> Values { get; set; } }
        class IListProperties { public IList<AutoProperty> Values { get; set; } }
        class ListProperties { public List<AutoProperty> Values { get; set; } }

        [Fact]
        public void BindsTo_ArrayProperties()
        {
            var binder = new JsonBinder();
            var result = binder.Get<ArrayProperties>(new[] {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            });

            result.Values.Should().BeEquivalentTo(new[] {
                new AutoProperty { Value = "123" },
                new AutoProperty { Value = "456" },
                new AutoProperty { Value = "789" }
            });
        }

        [Fact]
        public void BindsTo_SimpleArrayProperties()
        {
            var binder = new JsonBinder();
            var result = binder.Get<SimpleArrayProperties>(new[] {
                new KeyValuePair<string, string>("Values:0", "123"),
                new KeyValuePair<string, string>("Values:1", "456"),
                new KeyValuePair<string, string>("Values:2", "789")
            });

            result.Values.Should().BeEquivalentTo("123", "456", "789");
        }

        [Fact]
        public void BindsTo_EnumerableProperties()
        {
            var binder = new JsonBinder();
            var result = binder.Get<EnumerableProperties>(new[] {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            });

            result.Values.Should().BeEquivalentTo(new[] {
                new AutoProperty { Value = "123" },
                new AutoProperty { Value = "456" },
                new AutoProperty { Value = "789" }
            });
        }

        [Fact]
        public void BindsTo_IListProperties()
        {
            var binder = new JsonBinder();
            var result = binder.Get<IListProperties>(new[] {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            });

            result.Values.Should().BeEquivalentTo(new[] {
                new AutoProperty { Value = "123" },
                new AutoProperty { Value = "456" },
                new AutoProperty { Value = "789" }
            });
        }

        [Fact]
        public void BindsTo_ListProperties()
        {
            var binder = new JsonBinder();
            var result = binder.Get<ListProperties>(new[] {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            });

            result.Values.Should().BeEquivalentTo(new[] {
                new AutoProperty { Value = "123" },
                new AutoProperty { Value = "456" },
                new AutoProperty { Value = "789" }
            });
        }

        [Fact]
        public void GetKey_Default()
        {
            var binder = new JsonBinder();

            var o = new JObject();
            var o2 = new JObject();
            var o3 = new JObject();
            o["A"] = o2;
            o2["B"] = o3;
            o3["Value"] = "ABC";
            binder.GetKey(o["A"]["B"]["Value"]).Should().Equals("A:B:Value");
        }

        [Fact]
        public void GetKey_CustomSep()
        {
            var binder = new JsonBinder("__");

            var o = new JObject();
            var o2 = new JObject();
            var o3 = new JObject();
            o["A"] = o2;
            o2["B"] = o3;
            o3["Value"] = "ABC";
            binder.GetKey(o["A"]["B"]["Value"]).Should().Equals("A__B__Value");
        }
    }
}
