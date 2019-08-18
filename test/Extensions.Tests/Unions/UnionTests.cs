#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Unions;
using Xunit;

namespace Rocket.Surgery.Extensions.Tests.Unions
{
    enum BaseType
    {
        Thing1,
        Thing2
    }

    [UnionKey(nameof(Type))]
    [JsonConverter(typeof(UnionConverter))]
    abstract class Base
    {
        protected Base(BaseType type)
        {
            Type = type;
        }

        public BaseType Type { get; }
    }

    [Union(BaseType.Thing1)]
    class Thing1 : Base
    {
        public Thing1() : base(BaseType.Thing1)
        {
        }
        public string Thing1Name { get; set; }
    }

    [Union(BaseType.Thing2)]
    class Thing2 : Base
    {
        public Thing2() : base(BaseType.Thing2)
        {
        }

        public string Thing2Name { get; set; }
    }

    class Parent
    {
        public Base Item { get; set; }
    }

    public class UnionTests
    {
        private readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            }
        });

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            }
        };

        public static IEnumerable<object[]> Serialize_Things_Data()
        {
            yield return new object[]
            {
                new Thing1 { Thing1Name = "Thing1" },
                "{\"Thing1Name\":\"Thing1\",\"Type\":\"Thing1\"}"
            };
            yield return new object[]
            {
                new Thing2 { Thing2Name = "Thing2" },
                "{\"Thing2Name\":\"Thing2\",\"Type\":\"Thing2\"}"
            };
        }

        [Theory]
        [MemberData(nameof(Serialize_Things_Data))]
        public void Serialize_Things(object thing, string expected)
        {
            var result = JsonConvert.SerializeObject(thing, Formatting.None, _settings);
            result.Should().Be(expected);
        }

        public static IEnumerable<object[]> Deserialize_Things_Data()
        {
            yield return new object[]
            {
                "{\"Thing1Name\":\"Thing1\",\"Type\":\"Thing1\"}",
                typeof(Thing1)
            };
            yield return new object[]
            {
                "{\"Thing2Name\":\"Thing2\",\"Type\":\"Thing2\"}",
                typeof(Thing2)
            };
        }

        [Theory]
        [MemberData(nameof(Deserialize_Things_Data))]
        public void Deserialize_Things(string thing, Type expected)
        {
            var result = JsonConvert.DeserializeObject<Base>(thing, _settings);
            result.Should().BeOfType(expected);
        }

        public static IEnumerable<object[]> Deserialize_Things_WithVariedNames_Data()
        {
            static IEnumerable<object[]> Assign(Func<string, string> transformName)
            {
                foreach (var item in Deserialize_Things_Data())
                {
                    var obj = JObject.Parse((string)item[0]);
                    foreach (var prop in obj.Properties().ToArray())
                    {
                        obj.Remove(prop.Name);
                        obj[transformName(prop.Name)] = prop.Value;
                    }

                    item[0] = obj.ToString();

                    yield return new object[] { item[0], item[1] };
                }
            }
            foreach (var a in Assign(Inflector.Camelize))
                yield return a;
            foreach (var a in Assign(Inflector.Kebaberize))
                yield return a;
            foreach (var a in Assign(Inflector.Pascalize))
                yield return a;
        }

        [Theory]
        [MemberData(nameof(Deserialize_Things_WithVariedNames_Data))]
        public void Deserialize_Things_WithVariedNames(string thing, Type expected)
        {
            var result = JsonConvert.DeserializeObject<Base>(thing, _settings);
            result.Should().BeOfType(expected);
        }

        [Theory]
        [InlineData(typeof(Base))]
        [InlineData(typeof(Thing1))]
        [InlineData(typeof(Thing2))]
        public void ShouldGetUnionTypeProperly(Type type)
        {
            UnionHelper.GetUnionEnumType(type).Should().Be(typeof(BaseType));
            UnionHelper.GetUnionEnumType(type, nameof(Base.Type)).Should().Be(typeof(BaseType));
        }

        [Theory]
        [MemberData(nameof(GetAllEnumDiscriminatorTypes))]
        public void All_Union_Types_Are_Implemented(TypeInfo rootType, bool allImplemented)
        {
            allImplemented.Should().BeTrue($"All types must be implemented, not just the correct number.  RootType: {rootType.FullName}");
        }

        [Fact]
        public void Should_Handle_Missing_Property_Values()
        {
            var result = JsonConvert.DeserializeObject<Parent>("{}");
            result.Should().NotBeNull();
            result.Item.Should().BeNull();
        }

        [Fact]
        public void Should_Handle_Null_Property_Values()
        {
            var result = JsonConvert.DeserializeObject<Parent>("{ \"Item\": null }");
            result.Should().NotBeNull();
            result.Item.Should().BeNull();
        }

        public static IEnumerable<object[]> GetAllEnumDiscriminatorTypes()
        {
            foreach (var (enumType, rootType, allImplemented) in UnionHelper.GetAll(typeof(UnionTests).GetTypeInfo().Assembly))
            {
                yield return new object[] { rootType, allImplemented };
            }
        }
    }
}
