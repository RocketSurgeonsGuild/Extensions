#nullable disable
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Binding;
using Rocket.Surgery.Extensions.Testing;

#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CS0436 // Type conflicts with imported type

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable MemberHidesStaticFromOuterClass
#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0025 // Use expression body for properties
#pragma warning disable CS0649

namespace Rocket.Surgery.Extensions.Tests;

public class JsonBinderTests() : AutoFakeTest(Defaults.LoggerTest)
{
    public delegate object BindDelegate(
        JsonBinder binder,
        Type objectType,
        JsonSerializer serializer,
        IEnumerable<KeyValuePair<string, string>> values
    );

    public delegate object PopulateDelegate(
        JsonBinder binder,
        object value,
        JsonSerializer serializer,
        IEnumerable<KeyValuePair<string, string>> values
    );

    public static IEnumerable<Func<BindDelegate>> BindValues()
    {
        yield return () => Bind;
        yield return () => Bind2;

        yield return () => BindJsonSerializer;
        yield return () => Bind2JsonSerializer;

        yield return () => BindConfiguration;
        yield return () => Bind2Configuration;

        yield return () => BindConfigurationJsonSerializer;
        yield return () => Bind2ConfigurationJsonSerializer;
    }

    public static IEnumerable<Func<PopulateDelegate>> PopulateValues()
    {
        yield return () => Populate;
        yield return () => PopulateJsonSerializer;
        yield return () => PopulateConfiguration;
        yield return () => PopulateConfigurationJsonSerializer;
    }

    private static object Populate(JsonBinder binder, object value, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values) =>
        binder.Populate(value, values);

    private static object PopulateJsonSerializer(
        JsonBinder binder,
        object value,
        JsonSerializer serializer,
        IEnumerable<KeyValuePair<string, string>> values
    ) => binder.Populate(value, values, serializer);

    private static object Bind(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values) =>
        binder.Bind(objectType, values);

    private static object BindJsonSerializer(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values) =>
        binder.Bind(objectType, values, serializer);

    [CanBeNull]
    private static object Bind2(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
    {
        var method = typeof(JsonBinderTests)
                    .GetTypeInfo()
                    .GetMethod(nameof(BindGeneric), BindingFlags.Static | BindingFlags.NonPublic)!;
        return method
              .MakeGenericMethod(objectType)
              .Invoke(
                   null,
                   [
                       binder, values,
                   ]
               );
    }

    [CanBeNull]
    private static object Bind2JsonSerializer(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
    {
        var method = typeof(JsonBinderTests)
                    .GetTypeInfo()
                    .GetMethod(nameof(BindGenericJsonSerializer), BindingFlags.Static | BindingFlags.NonPublic)!;
        return method
              .MakeGenericMethod(objectType)
              .Invoke(
                   null,
                   [
                       binder, serializer, values,
                   ]
               );
    }

    private static T BindGeneric<T>(JsonBinder binder, IEnumerable<KeyValuePair<string, string>> values)
        where T : class, new() =>
        binder.Bind<T>(values);

    private static T BindGenericJsonSerializer<T>(JsonBinder binder, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        where T : class, new() =>
        binder.Bind<T>(values, serializer);

    private static object PopulateConfiguration(JsonBinder binder, object value, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
    {
        var config = new ConfigurationBuilder();
        config.AddInMemoryCollection(values);
        return binder.Populate(value, config.Build());
    }

    private static object PopulateConfigurationJsonSerializer(
        JsonBinder binder,
        object value,
        JsonSerializer serializer,
        IEnumerable<KeyValuePair<string, string>> values
    )
    {
        var config = new ConfigurationBuilder();
        config.AddInMemoryCollection(values);
        return binder.Populate(value, config.Build(), serializer);
    }

    private static object BindConfiguration(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
    {
        var config = new ConfigurationBuilder();
        config.AddInMemoryCollection(values);
        return binder.Bind(objectType, config.Build());
    }

    private static object BindConfigurationJsonSerializer(
        JsonBinder binder,
        Type objectType,
        JsonSerializer serializer,
        IEnumerable<KeyValuePair<string, string>> values
    )
    {
        var config = new ConfigurationBuilder();
        config.AddInMemoryCollection(values);
        return binder.Bind(objectType, config.Build(), serializer);
    }

    [CanBeNull]
    private static object Bind2Configuration(JsonBinder binder, Type objectType, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
    {
        var method = typeof(JsonBinderTests)
                    .GetTypeInfo()
                    .GetMethod(nameof(BindConfigurationGeneric), BindingFlags.Static | BindingFlags.NonPublic)!;
        return method
              .MakeGenericMethod(objectType)
              .Invoke(
                   null,
                   [
                       binder, values,
                   ]
               );
    }

    [CanBeNull]
    private static object Bind2ConfigurationJsonSerializer(
        JsonBinder binder,
        Type objectType,
        JsonSerializer serializer,
        IEnumerable<KeyValuePair<string, string>> values
    )
    {
        var method = typeof(JsonBinderTests)
                    .GetTypeInfo()
                    .GetMethod(nameof(BindConfigurationGenericJsonSerializer), BindingFlags.Static | BindingFlags.NonPublic)!;
        return method
              .MakeGenericMethod(objectType)
              .Invoke(
                   null,
                   [
                       binder, serializer, values,
                   ]
               );
    }

    private static T BindConfigurationGeneric<T>(JsonBinder binder, IEnumerable<KeyValuePair<string, string>> values)
        where T : class, new()
    {
        var config = new ConfigurationBuilder();
        config.AddInMemoryCollection(values);
        return binder.Bind<T>(config.Build());
    }

    private static T BindConfigurationGenericJsonSerializer<T>(JsonBinder binder, JsonSerializer serializer, IEnumerable<KeyValuePair<string, string>> values)
        where T : class, new()
    {
        var config = new ConfigurationBuilder();
        config.AddInMemoryCollection(values);
        return binder.Bind<T>(config.Build(), serializer);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_AutoProperty(BindDelegate @delegate)
    {
        var binder = new JsonBinder();

        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("value", "123"),
        };
        var result = (AutoProperty)@delegate(binder, typeof(AutoProperty), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ReadonlyAutoProperty(BindDelegate @delegate)
    {
        var binder = new JsonBinder();

        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("value", "123"),
        };
        var result = (ReadonlyAutoProperty)@delegate(binder, typeof(ReadonlyAutoProperty), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_PrivateSetterProperty(BindDelegate @delegate)
    {
        var binder = new JsonBinder();

        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("value", "123"),
        };
        var result = (PrivateSetterProperty)@delegate(binder, typeof(PrivateSetterProperty), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ComplexProperties(BindDelegate @delegate)
    {
        var binder = new JsonBinder();

        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("value", "123"),
            new KeyValuePair<string, string>("AutoProperty:value", "456"),
        };
        var result = (ComplexProperty)@delegate(binder, typeof(ComplexProperty), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ComplexProperties_CustomSep(BindDelegate @delegate)
    {
        var binder = new JsonBinder("__");

        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("value", "123"),
            new KeyValuePair<string, string>("AutoProperty__value", "456"),
        };
        var result = (ComplexProperty)@delegate(binder, typeof(ComplexProperty), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ComplexProperties_Null(BindDelegate @delegate)
    {
        var binder = new JsonBinder();

        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("value", "123"),
        };
        var result = (ComplexProperty)@delegate(binder, typeof(ComplexProperty), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ArrayProperties(BindDelegate @delegate)
    {
        var binder = new JsonBinder();
        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("Values:0:value", "123"),
            new KeyValuePair<string, string>("Values:1:value", "456"),
            new KeyValuePair<string, string>("Values:2:value", "789"),
        };
        var result = (ArrayProperties)@delegate(binder, typeof(ArrayProperties), JsonBinder.DefaultSerializer, keyValuePairs);


        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_SimpleArrayProperties(BindDelegate @delegate)
    {
        var binder = new JsonBinder();
        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("Values:0", "123"),
            new KeyValuePair<string, string>("Values:1", "456"),
            new KeyValuePair<string, string>("Values:2", "789"),
        };
        var result = (SimpleArrayProperties)@delegate(binder, typeof(SimpleArrayProperties), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_EnumerableProperties(BindDelegate @delegate)
    {
        var binder = new JsonBinder();
        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("Values:0:value", "123"),
            new KeyValuePair<string, string>("Values:1:value", "456"),
            new KeyValuePair<string, string>("Values:2:value", "789"),
        };
        var result = (EnumerableProperties)@delegate(binder, typeof(EnumerableProperties), JsonBinder.DefaultSerializer, keyValuePairs);

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_IListProperties(BindDelegate @delegate)
    {
        var binder = new JsonBinder();
        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("Values:0:value", "123"),
            new KeyValuePair<string, string>("Values:1:value", "456"),
            new KeyValuePair<string, string>("Values:2:value", "789"),
        };
        var result = (IListProperties)@delegate(binder, typeof(IListProperties), JsonBinder.DefaultSerializer, keyValuePairs);
        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ListProperties(BindDelegate @delegate)
    {
        var binder = new JsonBinder();
        var keyValuePairs = new[]
        {
            new KeyValuePair<string, string>("Values:0:value", "123"),
            new KeyValuePair<string, string>("Values:1:value", "456"),
            new KeyValuePair<string, string>("Values:2:value", "789"),
        };
        var result = (ListProperties)@delegate(binder, typeof(ListProperties), JsonBinder.DefaultSerializer, keyValuePairs);
        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    public void GetKey_Default()
    {
        var binder = new JsonBinder();

        var values = binder.GetValues(new { a = new { b = new { value = "ABC" } } });

        values.ShouldContain(x => x.Key == "a:b:value");
    }

    [Test]
    public void GetKey_CustomSep()
    {
        var binder = new JsonBinder("__");

        var values = binder.GetValues(new { a = new { b = new { value = "ABC" } } });

        values.ShouldContain(x => x.Key == "a__b__value");
    }

    [Test]
    [MethodDataSource(nameof(BindValues))]
    public Task BindsTo_ExtensionData(BindDelegate @delegate)
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

        result.ComplexProperty.Value.ShouldBe("123");
        result.ComplexProperty.AutoProperty.Value.ShouldBe("456");
        result.ComplexProperty.CustomFields["something"].ToString().ShouldBe("2123");
        result.ComplexProperty.CustomFields["somethingelse"]["value"]!.ToString().ShouldBe("2456");

        result.CustomFields.ShouldNotBeEmpty();
        result.CustomFields["something"].ToString().ShouldBe("1123");
        result.CustomFields["somethingelse"]["value"]!.ToString().ShouldBe("1456");

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    [Test]
    [MethodDataSource(nameof(PopulateValues))]
    public Task Populates_Value(PopulateDelegate @delegate)
    {
        var binder = new JsonBinder();

        var result = (PopulatesFixture)@delegate(
            binder,
            new PopulatesFixture
            {
                A = "123",
                B = 789,
            },
            JsonBinder.DefaultSerializer,
            [
                new("B", "123"),
                new("ComplexProperty:value", "123"),
                new("ComplexProperty:AutoProperty:value", "456"),
                new("something", "1123"),
                new("somethingelse:value", "1456"),
                new("ComplexProperty:something", "2123"),
                new("ComplexProperty:somethingelse:value", "2456"),
            ]
        );

        result.A.ShouldBe("123");
        result.B.ShouldBe(123);

        result.ComplexProperty.Value.ShouldBe("123");
        result.ComplexProperty.CustomFields.ShouldNotBeEmpty();
        result.CustomFields.ShouldNotBeEmpty();

        return Verify(new { result, bindingResult = binder.From(result) }).UseParameters(TUnit.Core.TestContext.Current.TestDetails.TestId);
    }

    private class AutoProperty
    {
        public string Value { get; set; }
    }

    private class ReadonlyAutoProperty
    {
        public string Value { get; }
    }

    [UsedImplicitly]
    private class PrivateSetterProperty
    {
        public string Value { get; }
    }

    private class ComplexProperty
    {
        public AutoProperty AutoProperty { get; set; }
        public string Value { get; set; }
    }

    private class ArrayProperties
    {
        public AutoProperty[] Values { get; set; }
    }

    private class SimpleArrayProperties
    {
        public string[] Values { get; set; }
    }

    private class EnumerableProperties
    {
        public IEnumerable<AutoProperty> Values { get; set; }
    }

    private class IListProperties
    {
        public IList<AutoProperty> Values { get; set; }
    }

    private class ListProperties
    {
        public List<AutoProperty> Values { get; set; }
    }

    private class DerivedComplexProperty : ComplexProperty
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> CustomFields { get; set; }
    }

    private class ExtraProperties
    {
        public DerivedComplexProperty ComplexProperty { get; }

        [JsonExtensionData]
        public IDictionary<string, JToken> CustomFields { get; set; }
    }

    private class PopulatesFixture
    {
        public string A { get; set; }
        public int B { get; set; }
        public DerivedComplexProperty ComplexProperty { get; }

        [JsonExtensionData]
        public IDictionary<string, JToken> CustomFields { get; set; }
    }
}
