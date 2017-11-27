using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Binding;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Reflection.Extensions;
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

    class Fixture_Thing
    {
        public string A;
        public int B { get; set; }
        public long? C { get; set; }
    }

    class Fixture_Thing2
    {
        public Fixture_Thing Thing1 { get; set; }
        public int D;
        public long? E { get; set; }
    }

    class Fixture_Enumerable
    {
        public IEnumerable<string> Values;
        public IEnumerable<Fixture_Thing> Things { get; set; }
    }

    class Fixture_Array
    {
        public string[] Values { get; set; }
        public Fixture_Thing[] Things;
    }

    class Fixture_Collection
    {
        public Collection<string> Values;
        public ICollection<Fixture_Thing> Things { get; set; }
    }

    class Fixture_ReadOnlyCollection
    {
        public IReadOnlyCollection<string> Values { get; set; }
        public ReadOnlyCollection<Fixture_Thing> Things;
    }

    class Fixture_List
    {
        public List<string> Values;
        public IList<Fixture_Thing> Things { get; set; }
    }

    class Fixture_ReadOnlyList
    {
        public IReadOnlyList<string> Values { get; set; }
        public IReadOnlyList<Fixture_Thing> Things;
    }

    class Fixture_Dictionary
    {
        public IDictionary<string, string> Values;
        public Dictionary<string, Fixture_Thing> Things { get; set; }

    }

    class Fixture_ReadOnlyDictionary
    {
        public IReadOnlyDictionary<string, string> Values { get; set; }
        public ReadOnlyDictionary<string, Fixture_Thing> Things;

    }

    public class PropertyGetterTests : AutoTestBase
    {
        public PropertyGetterTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Fixture_Thing()
        {
            var getter = new PropertyGetter();


            var fixture = new Fixture_Thing()
            {
                A = "123",
                B = 1,
                C = null
            };

            var value = getter.Get(fixture, "A");
            value.Should().Be("123");

            value = getter.Get(fixture, "B");
            value.Should().Be(1);

            value = getter.Get(fixture, "C");
            value.Should().BeNull();
        }

        [Fact]
        public void Fixture_Thing2()
        {
            var getter = new PropertyGetter();


            var fixture = new Fixture_Thing2()
            {
                Thing1 = new Fixture_Thing()
                {
                    A = "123",
                    B = 1,
                    C = null
                },
                D = 123,
                E = 1234L
            };

            var value = getter.Get(fixture, "Thing1.A");
            value.Should().Be("123");

            value = getter.Get(fixture, "Thing1.B");
            value.Should().Be(1);

            value = getter.Get(fixture, "Thing1.C");
            value.Should().BeNull();

            value = getter.Get(fixture, "D");
            value.Should().Be(123);

            value = getter.Get(fixture, "E");
            value.Should().Be(1234L);
        }

        public static IEnumerable<object[]> ListData_ForNormal()
        {
            foreach (var item in GetListFixtures())
            {
                yield return new object[]
                {
                    item,
                    "Things[0].A",
                    "123"
                };
                yield return new object[]
                {
                    item,
                    "Things[0].B",
                    1
                };
                yield return new object[]
                {
                    item,
                    "Things[0].C",
                    null
                };
                yield return new object[]
                {
                    item,
                    "Things[1].A",
                    "456"
                };
                yield return new object[]
                {
                    item,
                    "Things[1].B",
                    2
                };
                yield return new object[]
                {
                    item,
                    "Things[1].C",
                    1L
                };
                yield return new object[]
                {
                    item,
                    "Things[2].A",
                    "789"
                };
                yield return new object[]
                {
                    item,
                    "Things[2].B",
                    3
                };
                yield return new object[]
                {
                    item,
                    "Things[2].C",
                    null
                };
                yield return new object[]
                {
                    item,
                    "Values[0]",
                    "1"
                };
                yield return new object[]
                {
                    item,
                    "Values[1]",
                    "2"
                };
                yield return new object[]
                {
                    item,
                    "Values[2]",
                    "3"
                };
            }
        }

        public static IEnumerable<object[]> ListData_ForSeperator()
        {
            foreach (var item in GetListFixtures())
            {
                yield return new object[]
                {
                    item,
                    "Things__0__A",
                    "123"
                };
                yield return new object[]
                {
                    item,
                    "Things__0__B",
                    1
                };
                yield return new object[]
                {
                    item,
                    "Things__0__C",
                    null
                };
                yield return new object[]
                {
                    item,
                    "Things__1__A",
                    "456"
                };
                yield return new object[]
                {
                    item,
                    "Things__1__B",
                    2
                };
                yield return new object[]
                {
                    item,
                    "Things__1__C",
                    1L
                };
                yield return new object[]
                {
                    item,
                    "Things__2__A",
                    "789"
                };
                yield return new object[]
                {
                    item,
                    "Things__2__B",
                    3
                };
                yield return new object[]
                {
                    item,
                    "Things__2__C",
                    null
                };
                yield return new object[]
                {
                    item,
                    "Values__0",
                    "1"
                };
                yield return new object[]
                {
                    item,
                    "Values__1",
                    "2"
                };
                yield return new object[]
                {
                    item,
                    "Values__2",
                    "3"
                };
            }
        }

        public static IEnumerable<object[]> ListData_ForFailureCases()
        {
            foreach (var item in GetListFixtures())
            {
                yield return new object[]
                {
                    item,
                    "Things[hello world].A",
                    "123"
                };
            }
        }

        private static IEnumerable<object> GetListFixtures()
        {
            yield return new Fixture_Enumerable()
            {
                Things = new[]
                {
                    new Fixture_Thing()
                    {
                        A = "123",
                        B = 1,
                        C = null
                    },
                    new Fixture_Thing()
                    {
                        A = "456",
                        B = 2,
                        C = 1L
                    },
                    new Fixture_Thing()
                    {
                        A = "789",
                        B = 3,
                        C = null
                    }
                }.AsEnumerable(),
                Values = new[] { "1", "2", "3" }.AsEnumerable()
            };
            yield return new Fixture_Array()
            {
                Things = new[]
                {
                    new Fixture_Thing()
                    {
                        A = "123",
                        B = 1,
                        C = null
                    },
                    new Fixture_Thing()
                    {
                        A = "456",
                        B = 2,
                        C = 1L
                    },
                    new Fixture_Thing()
                    {
                        A = "789",
                        B = 3,
                        C = null
                    }
                },
                Values = new[] { "1", "2", "3" }
            };
            yield return new Fixture_Collection()
            {
                Things = new Collection<Fixture_Thing>
                {
                    new Fixture_Thing()
                    {
                        A = "123",
                        B = 1,
                        C = null
                    },
                    new Fixture_Thing()
                    {
                        A = "456",
                        B = 2,
                        C = 1L
                    },
                    new Fixture_Thing()
                    {
                        A = "789",
                        B = 3,
                        C = null
                    }
                },
                Values = new Collection<string> { "1", "2", "3" }
            };
            yield return new Fixture_ReadOnlyCollection()
            {
                Things = new ReadOnlyCollection<Fixture_Thing>(
                    new List<Fixture_Thing>(new[]
                    {
                        new Fixture_Thing()
                        {
                            A = "123",
                            B = 1,
                            C = null
                        },
                        new Fixture_Thing()
                        {
                            A = "456",
                            B = 2,
                            C = 1L
                        },
                        new Fixture_Thing()
                        {
                            A = "789",
                            B = 3,
                            C = null
                        }
                    })
                ),
                Values = new ReadOnlyCollection<string>(new Collection<string> { "1", "2", "3" })
            };
            yield return new Fixture_List()
            {
                Things = new List<Fixture_Thing>
                {
                    new Fixture_Thing()
                    {
                        A = "123",
                        B = 1,
                        C = null
                    },
                    new Fixture_Thing()
                    {
                        A = "456",
                        B = 2,
                        C = 1L
                    },
                    new Fixture_Thing()
                    {
                        A = "789",
                        B = 3,
                        C = null
                    }
                },
                Values = new List<string> { "1", "2", "3" }
            };
            yield return new Fixture_ReadOnlyList()
            {
                Things = new ReadOnlyCollection<Fixture_Thing>(
                    new List<Fixture_Thing>(new[]
                    {
                        new Fixture_Thing()
                        {
                            A = "123",
                            B = 1,
                            C = null
                        },
                        new Fixture_Thing()
                        {
                            A = "456",
                            B = 2,
                            C = 1L
                        },
                        new Fixture_Thing()
                        {
                            A = "789",
                            B = 3,
                            C = null
                        }
                    })
                ),
                Values = new ReadOnlyCollection<string>(new Collection<string> { "1", "2", "3" })
            };
        }

        public static IEnumerable<object[]> DictionaryData()
        {
            foreach (var item in GetDictionaryFixtures())
            {
                yield return new object[]
                {
                    item,
                    "Things[A].A",
                    "123"
                };
                yield return new object[]
                {
                    item,
                    "Things[A].B",
                    1
                };
                yield return new object[]
                {
                    item,
                    "Things[A].C",
                    null
                };
                yield return new object[]
                {
                    item,
                    "Things[B].A",
                    "456"
                };
                yield return new object[]
                {
                    item,
                    "Things[B].B",
                    2
                };
                yield return new object[]
                {
                    item,
                    "Things[B].C",
                    1L
                };
                yield return new object[]
                {
                    item,
                    "Things[C].A",
                    "789"
                };
                yield return new object[]
                {
                    item,
                    "Things[C].B",
                    3
                };
                yield return new object[]
                {
                    item,
                    "Things[C].C",
                    null
                };
                yield return new object[]
                {
                    item,
                    "Values[A]",
                    "1"
                };
                yield return new object[]
                {
                    item,
                    "Values[B]",
                    "2"
                };
                yield return new object[]
                {
                    item,
                    "Values[C]",
                    "3"
                };
            }
        }

        private static IEnumerable<object> GetDictionaryFixtures()
        {
            yield return new Fixture_Dictionary()
            {
                Things = new Dictionary<string, Fixture_Thing> {
                    {
                        "A",
                        new Fixture_Thing()
                        {
                            A = "123",
                            B = 1,
                            C = null
                        }
                    },
                    {
                        "B",
                        new Fixture_Thing()
                        {
                            A = "456",
                            B = 2,
                            C = 1L
                        }
                    },
                    {"C",
                        new Fixture_Thing()
                        {
                            A = "789",
                            B = 3,
                            C = null
                        }
                    }
                },
                Values = new Dictionary<string, string> { { "A", "1" }, { "B", "2" }, { "C", "3" } }
            };
            yield return new Fixture_ReadOnlyDictionary()
            {
                Things = new ReadOnlyDictionary<string, Fixture_Thing>(new Dictionary<string, Fixture_Thing> {
                    {
                        "A",
                        new Fixture_Thing()
                        {
                            A = "123",
                            B = 1,
                            C = null
                        }
                    },
                    {
                        "B",
                        new Fixture_Thing()
                        {
                            A = "456",
                            B = 2,
                            C = 1L
                        }
                    },
                    {
                        "C",
                        new Fixture_Thing()
                        {
                            A = "789",
                            B = 3,
                            C = null
                        }
                    }
                }),
                Values = new ReadOnlyDictionary<string, string> (new Dictionary<string, string> { { "A", "1" }, { "B", "2" }, { "C", "3" } })
            };
        }

        [Theory, MemberData(nameof(ListData_ForNormal))]
        public void Fixture_Enumerable(object fixture, string path, object expected)
        {
            var getter = new PropertyGetter();

            var result = getter.Get(fixture, path);
            result.Should().Be(expected);
        }

        [Theory, MemberData(nameof(DictionaryData))]
        public void Fixture_Dictionary(object fixture, string path, object expected)
        {
            var getter = new PropertyGetter();

            var result = getter.Get(fixture, path);
            result.Should().Be(expected);
        }

        [Theory, MemberData(nameof(ListData_ForSeperator))]
        public void Fixture_Enumerable_WithSeperator(object fixture, string path, object expected)
        {
            var getter = new PropertyGetter("__");

            var result = getter.Get(fixture, path);
            result.Should().Be(expected);
        }

        [Theory, MemberData(nameof(ListData_ForFailureCases))]
        public void Fixture_IndexInvalid(object fixture, string path, object expected)
        {
            var getter = new PropertyGetter();

            Action a = () => getter.Get(fixture, path);
            a.Should().Throw<ArgumentException>();
        }
    }
}
