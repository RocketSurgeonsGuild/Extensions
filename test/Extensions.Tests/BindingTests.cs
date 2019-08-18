#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Binding;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Tests
{
    public class JsonBinderTests : AutoTestBase
    {
        public JsonBinderTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        class AutoProperty
        {
            public string Value { get; set; }
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_AutoProperty(BindDelegate @delegate)
        {
            var binder = new JsonBinder();

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("value", "123")
            };
            var result = (AutoProperty)@delegate(binder, typeof(AutoProperty), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Value.Should().Be("123");

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        class ReadonlyAutoProperty
        {
            public string Value { get; }
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ReadonlyAutoProperty(BindDelegate @delegate)
        {
            var binder = new JsonBinder();

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("value", "123")
            };
            var result = (ReadonlyAutoProperty)@delegate(binder, typeof(ReadonlyAutoProperty), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Value.Should().Be("123");

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        class PrivateSetterProperty
        {
            public string Value { get; }
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_PrivateSetterProperty(BindDelegate @delegate)
        {
            var binder = new JsonBinder();

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("value", "123")
            };
            var result = (PrivateSetterProperty)@delegate(binder, typeof(PrivateSetterProperty), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Value.Should().Be("123");

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        class ComplexProperty
        {
            public AutoProperty AutoProperty { get; set; }
            public string Value { get; set; }
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ComplexProperties(BindDelegate @delegate)
        {
            var binder = new JsonBinder();

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("value", "123"),
                new KeyValuePair<string, string>("AutoProperty:value", "456")
            };
            var result = (ComplexProperty)@delegate(binder, typeof(ComplexProperty), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Value.Should().Be("123");
            result.AutoProperty.Value.Should().Be("456");

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ComplexProperties_CustomSep(BindDelegate @delegate)
        {
            var binder = new JsonBinder("__");

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("value", "123"),
                new KeyValuePair<string, string>("AutoProperty__value", "456")
            };
            var result = (ComplexProperty)@delegate(binder, typeof(ComplexProperty), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Value.Should().Be("123");
            result.AutoProperty.Value.Should().Be("456");

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ComplexProperties_Null(BindDelegate @delegate)
        {
            var binder = new JsonBinder();

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("value", "123")
            };
            var result = (ComplexProperty)@delegate(binder, typeof(ComplexProperty), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Value.Should().Be("123");
            result.AutoProperty.Should().BeNull();

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().Contain(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        class ArrayProperties
        {
            public AutoProperty[] Values { get; set; }
        }

        class SimpleArrayProperties
        {
            public string[] Values { get; set; }
        }

        class EnumerableProperties
        {
            public IEnumerable<AutoProperty> Values { get; set; }
        }

        class IListProperties
        {
            public IList<AutoProperty> Values { get; set; }
        }

        class ListProperties
        {
            public List<AutoProperty> Values { get; set; }
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ArrayProperties(BindDelegate @delegate)
        {
            var binder = new JsonBinder();
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            };
            var result = (ArrayProperties)@delegate(binder, typeof(ArrayProperties), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Values.Should().BeEquivalentTo(new[]
            {
                new AutoProperty {Value = "123"},
                new AutoProperty {Value = "456"},
                new AutoProperty {Value = "789"}
            });

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                    .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_SimpleArrayProperties(BindDelegate @delegate)
        {
            var binder = new JsonBinder();
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("Values:0", "123"),
                new KeyValuePair<string, string>("Values:1", "456"),
                new KeyValuePair<string, string>("Values:2", "789")
            };
            var result = (SimpleArrayProperties)@delegate(binder, typeof(SimpleArrayProperties), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Values.Should().BeEquivalentTo("123", "456", "789");

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_EnumerableProperties(BindDelegate @delegate)
        {
            var binder = new JsonBinder();
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            };
            var result = (EnumerableProperties)@delegate(binder, typeof(EnumerableProperties), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Values.Should().BeEquivalentTo(new[]
            {
                new AutoProperty {Value = "123"},
                new AutoProperty {Value = "456"},
                new AutoProperty {Value = "789"}
            });

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_IListProperties(BindDelegate @delegate)
        {
            var binder = new JsonBinder();
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            };
            var result = (IListProperties)@delegate(binder, typeof(IListProperties), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Values.Should().BeEquivalentTo(new[]
            {
                new AutoProperty {Value = "123"},
                new AutoProperty {Value = "456"},
                new AutoProperty {Value = "789"}
            });

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ListProperties(BindDelegate @delegate)
        {
            var binder = new JsonBinder();
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("Values:0:value", "123"),
                new KeyValuePair<string, string>("Values:1:value", "456"),
                new KeyValuePair<string, string>("Values:2:value", "789")
            };
            var result = (ListProperties)@delegate(binder, typeof(ListProperties), JsonBinder.DefaultSerializer, keyValuePairs);

            result.Values.Should().BeEquivalentTo(new[]
            {
                new AutoProperty {Value = "123"},
                new AutoProperty {Value = "456"},
                new AutoProperty {Value = "789"}
            });

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        [Fact]
        public void GetKey_Default()
        {
            var binder = new JsonBinder();

            var values = binder.GetValues(new { a = new { b = new { value = "ABC" } } });

            values.Should().Contain(x => x.Key == "a:b:value");
        }

        [Fact]
        public void GetKey_CustomSep()
        {
            var binder = new JsonBinder("__");

            var values = binder.GetValues(new { a = new { b = new { value = "ABC" } } });

            values.Should().Contain(x => x.Key == "a__b__value");
        }

        class DerivedComplexProperty : ComplexProperty
        {
            [JsonExtensionData] public IDictionary<string, JToken> CustomFields { get; set; }
        }

        class ExtraProperties
        {
            public DerivedComplexProperty ComplexProperty { get; }
            [JsonExtensionData] public IDictionary<string, JToken> CustomFields { get; set; }
        }

        [Theory, MemberData(nameof(BindValues))]
        public void BindsTo_ExtensionData(BindDelegate @delegate)
        {
            var binder = new JsonBinder();

            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>("ComplexProperty:value", "123"),
                new KeyValuePair<string, string>("ComplexProperty:AutoProperty:value", "456"),
                new KeyValuePair<string, string>("something", "1123"),
                new KeyValuePair<string, string>("somethingelse:value", "1456"),
                new KeyValuePair<string, string>("ComplexProperty:something", "2123"),
                new KeyValuePair<string, string>("ComplexProperty:somethingelse:value", "2456"),
            };
            var result = (ExtraProperties)@delegate(binder, typeof(ExtraProperties), JsonBinder.DefaultSerializer, keyValuePairs);

            result.ComplexProperty.Value.Should().Be("123");
            result.ComplexProperty.AutoProperty.Value.Should().Be("456");
            result.ComplexProperty.CustomFields["something"].ToString().Should().Be("2123");
            result.ComplexProperty.CustomFields["somethingelse"]["value"].ToString().Should().Be("2456");

            result.CustomFields.Should().NotBeEmpty();
            result.CustomFields["something"].ToString().Should().Be("1123");
            result.CustomFields["somethingelse"]["value"].ToString().Should().Be("1456");

            Logger.LogInformation(JsonConvert.SerializeObject(result.CustomFields));

            binder.From(result)
                .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                .Should().BeEquivalentTo(
                    keyValuePairs
                        .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value))
                );
        }

        class PopulatesFixture
        {
            public string A { get; set; }
            public int B { get; set; }
            public DerivedComplexProperty ComplexProperty { get; }
            [JsonExtensionData] public IDictionary<string, JToken> CustomFields { get; set; }
        }

        [Theory, MemberData(nameof(PopulateValues))]
        public void Populates_Value(PopulateDelegate @delegate)
        {
            var binder = new JsonBinder();

            var result = (PopulatesFixture)@delegate(
                binder,
                new PopulatesFixture()
                {
                    A = "123",
                    B = 789,
                },
                JsonBinder.DefaultSerializer,
                new[]
                {
                    new KeyValuePair<string, string>("B", "123"),
                    new KeyValuePair<string, string>("ComplexProperty:value", "123"),
                    new KeyValuePair<string, string>("ComplexProperty:AutoProperty:value", "456"),
                    new KeyValuePair<string, string>("something", "1123"),
                    new KeyValuePair<string, string>("somethingelse:value", "1456"),
                    new KeyValuePair<string, string>("ComplexProperty:something", "2123"),
                    new KeyValuePair<string, string>("ComplexProperty:somethingelse:value", "2456"),
                });

            result.A.Should().Be("123");
            result.B.Should().Be(123);

            result.ComplexProperty.Value.Should().Be("123");
            result.ComplexProperty.CustomFields.Should().NotBeEmpty();
            result.CustomFields.Should().NotBeEmpty();

            Logger.LogInformation(JsonConvert.SerializeObject(result.CustomFields));
        }


        public delegate object BindDelegate(
            JsonBinder binder,
            Type objectType,
            JsonSerializer serializer,
            IEnumerable<KeyValuePair<string, string>> values);

        public static IEnumerable<object[]> BindValues()
        {
            yield return new object[]
            {
                (BindDelegate)Bind
            };
            yield return new object[]
            {
                (BindDelegate)Bind2
            };

            yield return new object[]
            {
                (BindDelegate)BindJsonSerializer
            };
            yield return new object[]
            {
                (BindDelegate)Bind2JsonSerializer
            };

            yield return new object[]
            {
                (BindDelegate)BindConfiguration
            };
            yield return new object[]
            {
                (BindDelegate)Bind2Configuration
            };

            yield return new object[]
            {
                (BindDelegate)BindConfigurationJsonSerializer
            };
            yield return new object[]
            {
                (BindDelegate)Bind2ConfigurationJsonSerializer
            };
        }

        public delegate object PopulateDelegate(
            JsonBinder binder,
            object value,
            JsonSerializer serializer,
            IEnumerable<KeyValuePair<string, string>> values);

        public static IEnumerable<object[]> PopulateValues()
        {
            yield return new object[]
            {
                (PopulateDelegate)Populate
            };
            yield return new object[]
            {
                (PopulateDelegate)PopulateJsonSerializer
            };
            yield return new object[]
            {
                (PopulateDelegate)PopulateConfiguration
            };
            yield return new object[]
            {
                (PopulateDelegate)PopulateConfigurationJsonSerializer
            };
        }

        static object Populate(JsonBinder binder, object value, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            return binder.Populate(value, values);
        }

        static object PopulateJsonSerializer(JsonBinder binder, object value, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            return binder.Populate(value, values, serializer);
        }

        static object Bind(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            return binder.Bind(objectType, values);
        }

        static object BindJsonSerializer(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            return binder.Bind(objectType, values, serializer);
        }

        static object Bind2(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var method = typeof(JsonBinderTests).GetTypeInfo()
                .GetMethod(nameof(BindGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            return method
                .MakeGenericMethod(objectType)
                .Invoke(null, new object[]
                {
                    binder, values
                });
        }

        static object Bind2JsonSerializer(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var method = typeof(JsonBinderTests).GetTypeInfo()
                .GetMethod(nameof(BindGenericJsonSerializer), BindingFlags.Static | BindingFlags.NonPublic);
            return method
                .MakeGenericMethod(objectType)
                .Invoke(null, new object[]
                {
                    binder, serializer, values
                });
        }

        static T BindGeneric<T>(JsonBinder binder, IEnumerable<KeyValuePair<string, string>> values)
            where T : class, new()
        {
            return binder.Bind<T>(values);
        }

        static T BindGenericJsonSerializer<T>(JsonBinder binder, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
            where T : class, new()
        {
            return binder.Bind<T>(values, serializer);
        }

        static object PopulateConfiguration(JsonBinder binder, object value, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var config = new ConfigurationBuilder();
            config.AddInMemoryCollection(values);
            return binder.Populate(value, config.Build());
        }

        static object PopulateConfigurationJsonSerializer(JsonBinder binder, object value, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var config = new ConfigurationBuilder();
            config.AddInMemoryCollection(values);
            return binder.Populate(value, config.Build(), serializer);
        }

        static object BindConfiguration(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var config = new ConfigurationBuilder();
            config.AddInMemoryCollection(values);
            return binder.Bind(objectType, config.Build());
        }

        static object BindConfigurationJsonSerializer(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var config = new ConfigurationBuilder();
            config.AddInMemoryCollection(values);
            return binder.Bind(objectType, config.Build(), serializer);
        }

        static object Bind2Configuration(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var method = typeof(JsonBinderTests).GetTypeInfo()
                .GetMethod(nameof(BindConfigurationGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            return method
                .MakeGenericMethod(objectType)
                .Invoke(null, new object[]
                {
                    binder, values
                });
        }

        static object Bind2ConfigurationJsonSerializer(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        {
            var method = typeof(JsonBinderTests).GetTypeInfo()
                .GetMethod(nameof(BindConfigurationGenericJsonSerializer), BindingFlags.Static | BindingFlags.NonPublic);
            return method
                .MakeGenericMethod(objectType)
                .Invoke(null, new object[]
                {
                    binder, serializer, values
                });
        }

        static T BindConfigurationGeneric<T>(JsonBinder binder, IEnumerable<KeyValuePair<string, string>> values)
            where T : class, new()
        {
            var config = new ConfigurationBuilder();
            config.AddInMemoryCollection(values);
            return binder.Bind<T>(config.Build());
        }

        static T BindConfigurationGenericJsonSerializer<T>(JsonBinder binder, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
            where T : class, new()
        {
            var config = new ConfigurationBuilder();
            config.AddInMemoryCollection(values);
            return binder.Bind<T>(config.Build(), serializer);
        }
    }
}
