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
    [Union(typeof(Base))]
    enum BaseType
    {
        [Union(typeof(Thing1))]
        Thing1,
        [Union(typeof(Thing2))]
        Thing2
    }

    [JsonConverter(typeof(UnionConverter), typeof(Base), nameof(Type))]
    abstract class Base
    {
        protected Base(BaseType type)
        {
            Type = type;
        }

        public BaseType Type { get; }
    }

    class Thing1 : Base
    {
        public Thing1() : base(BaseType.Thing1)
        {
        }
        public string Thing1Name { get; set; }
    }

    class Thing2 : Base
    {
        public Thing2() : base(BaseType.Thing2)
        {
        }

        public string Thing2Name { get; set; }
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
            IEnumerable<object[]> Assign(Func<string, string> transformName)
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

        [Fact]
        public void ShouldGetUnionTypeProperly()
        {
            UnionHelper.GetUnionType(typeof(BaseType)).Should().Be(typeof(BaseType));
            UnionHelper.GetUnionType(typeof(Base), nameof(Base.Type)).Should().Be(typeof(BaseType));
        }

        [Theory]
        [MemberData(nameof(GetAllEnumDiscriminatorTypes))]
        public void All_Union_Types_Are_Implemented(TypeInfo enumType, TypeInfo[] typesFromEnum, TypeInfo[] implementationTypes)
        {
            typesFromEnum.Should().Contain(implementationTypes, $"All types must be implemented, not just the correct number.  EnumType: {enumType.FullName}");
        }

        public static IEnumerable<object[]> GetAllEnumDiscriminatorTypes()
        {
            foreach (var type in UnionHelper.GetAll(typeof(UnionTests).GetTypeInfo().Assembly))
            {
                yield return new object[] { type.enumType, type.typesFromEnum, type.implementationTypes };
            }
        }
    }
}
