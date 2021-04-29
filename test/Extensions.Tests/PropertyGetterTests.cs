using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using Rocket.Surgery.Reflection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Tests
{
    public class PropertyGetterTests : AutoFakeTest
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

        [Fact]
        public void Should_Have_StronglyTypedExpression()
        {
            var getter = new PropertyGetter(comparison: StringComparison.OrdinalIgnoreCase);


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


            getter.TryGetPropertyDelegate(fixture, "thing1.a", out var propertyDelegate);
            propertyDelegate.Should().NotBeNull();
            propertyDelegate.StronglyTypedExpression.Type.Should().Be<Func<Fixture_Thing2, string>>();
        }

        [Fact]
        public void Should_Fail_If_StringComparsion_IsSetWrong()
        {
            var getter = new PropertyGetter(comparison: StringComparison.Ordinal);


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

            getter.TryGetPropertyDelegate(fixture, "thing1.a", out _).Should().BeFalse();
            getter.TryGetPropertyDelegate(fixture, "Thing1.A", out _).Should().BeTrue();
        }

        public static IEnumerable<object?[]> ListData_ForNormal()
        {
            foreach (var item in GetListFixtures())
            {
                yield return new object?[]
                {
                    item,
                    "Things[0].A",
                    "123"
                };
                yield return new object?[]
                {
                    item,
                    "Things[0].B",
                    1
                };
                yield return new object?[]
                {
                    item,
                    "Things[0].C",
                    null
                };
                yield return new object?[]
                {
                    item,
                    "Things[1].A",
                    "456"
                };
                yield return new object?[]
                {
                    item,
                    "Things[1].B",
                    2
                };
                yield return new object?[]
                {
                    item,
                    "Things[1].C",
                    1L
                };
                yield return new object?[]
                {
                    item,
                    "Things[2].A",
                    "789"
                };
                yield return new object?[]
                {
                    item,
                    "Things[2].B",
                    3
                };
                yield return new object?[]
                {
                    item,
                    "Things[2].C",
                    null
                };
                yield return new object?[]
                {
                    item,
                    "Values[0]",
                    "1"
                };
                yield return new object?[]
                {
                    item,
                    "Values[1]",
                    "2"
                };
                yield return new object?[]
                {
                    item,
                    "Values[2]",
                    "3"
                };
            }
        }

        public static IEnumerable<object?[]> ListData_ForSeperator()
        {
            foreach (var item in GetListFixtures())
            {
                yield return new object?[]
                {
                    item,
                    "Things__0__A",
                    "123"
                };
                yield return new object?[]
                {
                    item,
                    "Things__0__B",
                    1
                };
                yield return new object?[]
                {
                    item,
                    "Things__0__C",
                    null
                };
                yield return new object?[]
                {
                    item,
                    "Things__1__A",
                    "456"
                };
                yield return new object?[]
                {
                    item,
                    "Things__1__B",
                    2
                };
                yield return new object?[]
                {
                    item,
                    "Things__1__C",
                    1L
                };
                yield return new object?[]
                {
                    item,
                    "Things__2__A",
                    "789"
                };
                yield return new object?[]
                {
                    item,
                    "Things__2__B",
                    3
                };
                yield return new object?[]
                {
                    item,
                    "Things__2__C",
                    null
                };
                yield return new object?[]
                {
                    item,
                    "Values__0",
                    "1"
                };
                yield return new object?[]
                {
                    item,
                    "Values__1",
                    "2"
                };
                yield return new object?[]
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
                yield return new[]
                {
                    item,
                    "Things[hello world].A"
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

        public static IEnumerable<object?[]> DictionaryData()
        {
            foreach (var item in GetDictionaryFixtures())
            {
                yield return new object?[]
                {
                    item,
                    "Things[A].A",
                    "123"
                };
                yield return new object?[]
                {
                    item,
                    "Things[A].B",
                    1
                };
                yield return new object?[]
                {
                    item,
                    "Things[A].C",
                    null
                };
                yield return new object?[]
                {
                    item,
                    "Things[B].A",
                    "456"
                };
                yield return new object?[]
                {
                    item,
                    "Things[B].B",
                    2
                };
                yield return new object?[]
                {
                    item,
                    "Things[B].C",
                    1L
                };
                yield return new object?[]
                {
                    item,
                    "Things[C].A",
                    "789"
                };
                yield return new object?[]
                {
                    item,
                    "Things[C].B",
                    3
                };
                yield return new object?[]
                {
                    item,
                    "Things[C].C",
                    null
                };
                yield return new object?[]
                {
                    item,
                    "Values[A]",
                    "1"
                };
                yield return new object?[]
                {
                    item,
                    "Values[B]",
                    "2"
                };
                yield return new object?[]
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
                Values = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "A", "1" }, { "B", "2" }, { "C", "3" } })
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
        public void Fixture_IndexInvalid(object fixture, string path)
        {
            var getter = new PropertyGetter();

            Action a = () => getter.Get(fixture, path);
            a.Should().Throw<ArgumentException>();
        }
    }
}
