using System.Collections.ObjectModel;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using Rocket.Surgery.Reflection;

namespace Rocket.Surgery.Extensions.Tests;

public class PropertyGetterTests() : AutoFakeTest(Defaults.LoggerTest)
{
    public static IEnumerable<Func<(object fixture, string path, object? value)>> ListData_ForNormal()
    {
        foreach (var item in GetListFixtures())
        {
            yield return () => (item, "Things[0].A", "123");
            yield return () => (item, "Things[0].B", 1);
            yield return () => (item, "Things[0].C", null);
            yield return () => (item, "Things[1].A", "456");
            yield return () => (item, "Things[1].B", 2);
            yield return () => (item, "Things[1].C", 1L);
            yield return () => (item, "Things[2].A", "789");
            yield return () => (item, "Things[2].B", 3);
            yield return () => (item, "Things[2].C", null);
            yield return () => (item, "Values[0]", "1");
            yield return () => (item, "Values[1]", "2");
            yield return () => (item, "Values[2]", "3"
                             );
        }
    }

    public static IEnumerable<Func<(object fixture, string path, object? value)>> ListData_ForSeperator()
    {
        foreach (var item in GetListFixtures())
        {
            yield return () => (item, "Things__0__A", "123");
            yield return () => (item, "Things__0__B", 1);
            yield return () => (item, "Things__0__C", null);
            yield return () => (item, "Things__1__A", "456");
            yield return () => (item, "Things__1__B", 2);
            yield return () => (item, "Things__1__C", 1L);
            yield return () => (item, "Things__2__A", "789");
            yield return () => (item, "Things__2__B", 3);
            yield return () => (item, "Things__2__C", null);
            yield return () => (item, "Values__0", "1");
            yield return () => (item, "Values__1", "2");
            yield return () => (item, "Values__2", "3");
        }
    }

    public static IEnumerable<Func<(object fixture, string path)>> ListData_ForFailureCases() =>
        GetListFixtures().Select(item => (Func<(object fixture, string path)>)(() => (item, "Things[0].D")));

    public static IEnumerable<Func<(object fixture, string path, object? value)>> DictionaryData()
    {
        foreach (var item in GetDictionaryFixtures())
        {
            yield return () => (item, "Things[A].A", "123");
            yield return () => (item, "Things[A].B", 1);
            yield return () => (item, "Things[A].C", null);
            yield return () => (item, "Things[B].A", "456");
            yield return () => (item, "Things[B].B", 2);
            yield return () => (item, "Things[B].C", 1L);
            yield return () => (item, "Things[C].A", "789");
            yield return () => (item, "Things[C].B", 3);
            yield return () => (item, "Things[C].C", null);
            yield return () => (item, "Values[A]", "1");
            yield return () => (item, "Values[B]", "2");
            yield return () => (item, "Values[C]", "3");
        }
    }

    private static IEnumerable<object> GetListFixtures()
    {
        yield return new Fixture_Enumerable
        {
            Things =
            [
                new() { A = "123", B = 1, C = null },
                new() { A = "456", B = 2, C = 1L },
                new() { A = "789", B = 3, C = null },
            ],
            Values = ["1", "2", "3"],
        };
        yield return new Fixture_Array
        {
            Things =
            [
                new() { A = "123", B = 1, C = null },
                new() { A = "456", B = 2, C = 1L },
                new() { A = "789", B = 3, C = null },
            ],
            Values = ["1", "2", "3"],
        };
        yield return new Fixture_Collection
        {
            Things = new Collection<Fixture_Thing>
            {
                new() { A = "123", B = 1, C = null },
                new() { A = "456", B = 2, C = 1L },
                new() { A = "789", B = 3, C = null },
            },
            Values = ["1", "2", "3"],
        };
        yield return new Fixture_ReadOnlyCollection
        {
            Things = new(
                new List<Fixture_Thing>(
                    [
                        new() { A = "123", B = 1, C = null },
                        new() { A = "456", B = 2, C = 1L },
                        new() { A = "789", B = 3, C = null },
                    ]
                )
            ),
            Values = new ReadOnlyCollection<string>(new Collection<string> { "1", "2", "3" }),
        };
        yield return new Fixture_List
        {
            Things = new List<Fixture_Thing>
            {
                new() { A = "123", B = 1, C = null },
                new() { A = "456", B = 2, C = 1L },
                new() { A = "789", B = 3, C = null },
            },
            Values = ["1", "2", "3"],
        };
        yield return new Fixture_ReadOnlyList
        {
            Things = new ReadOnlyCollection<Fixture_Thing>(
                new List<Fixture_Thing>(
                    [
                        new() { A = "123", B = 1, C = null },
                        new() { A = "456", B = 2, C = 1L },
                        new() { A = "789", B = 3, C = null },
                    ]
                )
            ),
            Values = new ReadOnlyCollection<string>(new Collection<string> { "1", "2", "3" }),
        };
    }

    private static IEnumerable<object> GetDictionaryFixtures()
    {
        yield return new Fixture_Dictionary
        {
            Things = new()
            {
                { "A", new() { A = "123", B = 1, C = null } },
                { "B", new() { A = "456", B = 2, C = 1L } },
                { "C", new() { A = "789", B = 3, C = null } },
            },
            Values = new Dictionary<string, string> { { "A", "1" }, { "B", "2" }, { "C", "3" } },
        };
        yield return new Fixture_ReadOnlyDictionary
        {
            Things = new(
                new Dictionary<string, Fixture_Thing>
                {
                    { "A", new() { A = "123", B = 1, C = null } },
                    { "B", new() { A = "456", B = 2, C = 1L } },
                    { "C", new() { A = "789", B = 3, C = null } },
                }
            ),
            Values = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { "A", "1" }, { "B", "2" }, { "C", "3" } }),
        };
    }

    [Test]
    public void Fixture_Thing()
    {
        var getter = new PropertyGetter();


        var fixture = new Fixture_Thing
        {
            A = "123",
            B = 1,
            C = null,
        };

        var value = getter.Get(fixture, "A");
        value.ShouldBe("123");

        value = getter.Get(fixture, "B");
        value.ShouldBe(1);

        value = getter.Get(fixture, "C");
        value.ShouldBeNull();
    }

    [Test]
    public void Fixture_Thing2()
    {
        var getter = new PropertyGetter();


        var fixture = new Fixture_Thing2
        {
            Thing1 = new()
            {
                A = "123",
                B = 1,
                C = null,
            },
            D = 123,
            E = 1234L,
        };

        var value = getter.Get(fixture, "Thing1.A");
        value.ShouldBe("123");

        value = getter.Get(fixture, "Thing1.B");
        value.ShouldBe(1);

        value = getter.Get(fixture, "Thing1.C");
        value.ShouldBeNull();

        value = getter.Get(fixture, "D");
        value.ShouldBe(123);

        value = getter.Get(fixture, "E");
        value.ShouldBe(1234L);
    }

    [Test]
    public void Should_Have_StronglyTypedExpression()
    {
        var getter = new PropertyGetter(comparison: StringComparison.OrdinalIgnoreCase);


        var fixture = new Fixture_Thing2
        {
            Thing1 = new()
            {
                A = "123",
                B = 1,
                C = null,
            },
            D = 123,
            E = 1234L,
        };


        getter.TryGetPropertyDelegate(fixture, "thing1.a", out var propertyDelegate);
        propertyDelegate.ShouldNotBeNull();
    }

    [Test]
    public void Should_Fail_If_StringComparsion_IsSetWrong()
    {
        var getter = new PropertyGetter(comparison: StringComparison.Ordinal);


        var fixture = new Fixture_Thing2
        {
            Thing1 = new()
            {
                A = "123",
                B = 1,
                C = null,
            },
            D = 123,
            E = 1234L,
        };

        getter.TryGetPropertyDelegate(fixture, "thing1.a", out _).ShouldBeFalse();
        getter.TryGetPropertyDelegate(fixture, "Thing1.A", out _).ShouldBeTrue();
    }

    [Test]
    [MethodDataSource(nameof(ListData_ForNormal))]
    public void Fixture_Enumerable(object fixture, string path, object expected)
    {
        var getter = new PropertyGetter();

        var result = getter.Get(fixture, path);
        result.ShouldBe(expected);
    }

    [Test]
    [MethodDataSource(nameof(DictionaryData))]
    public void Fixture_Dictionary(object fixture, string path, object expected)
    {
        var getter = new PropertyGetter();

        var result = getter.Get(fixture, path);
        result.ShouldBe(expected);
    }

    [Test]
    [MethodDataSource(nameof(ListData_ForSeperator))]
    public void Fixture_Enumerable_WithSeperator(object fixture, string path, object expected)
    {
        var getter = new PropertyGetter("__");

        var result = getter.Get(fixture, path);
        result.ShouldBe(expected);
    }

    [Test]
    [MethodDataSource(nameof(ListData_ForFailureCases))]
    public void Fixture_IndexInvalid(object fixture, string path)
    {
        var getter = new PropertyGetter();

        Action a = () => getter.Get(fixture, path);
        a.ShouldThrow<ArgumentException>();
    }
}
