using FakeItEasy;
using FluentAssertions;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using Rocket.Surgery.Reflection;

namespace Rocket.Surgery.Extensions.Tests;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class InjectableMethodBuilderTests
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    [Test]
    public void CreatesAMethod_WithZeroParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.ExecuteReturn0))
                    .Compile<bool>();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(new MethodFuncTest(), serviceProvider);

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithZeroParameters_UsingTypeForMethod()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.ExecuteReturn0))
                    .Compile<bool>();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(new MethodFuncTest(), serviceProvider);

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithOneParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute1))
                    .WithParameter<IConfigured1>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(new MethodFuncTest(), serviceProvider, configured1);

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithOneParameters_Enumerable()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.ExecuteEnumerable1))
                    .WithParameter<IConfigured1>()
                    .Compile<IEnumerable<IReturn>>();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        var injected3 = A.Fake<IInjected3>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected3))).Returns(injected3);
        method(new MethodFuncTest(), serviceProvider, configured1);


        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected3))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithTwoParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute2))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithThreeParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute3))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithFourParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute4))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithFiveParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute5))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithSixParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute6))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithSevenParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute7))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .WithParameter<IConfigured7>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var configured7 = A.Fake<IConfigured7>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6,
            configured7
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_WithEightParameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute8))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .WithParameter<IConfigured7>()
                    .WithParameter<IConfigured8>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var configured7 = A.Fake<IConfigured7>();
        var configured8 = A.Fake<IConfigured8>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6,
            configured7,
            configured8
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_With9Parameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute9))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .WithParameter<IConfigured7>()
                    .WithParameter<IConfigured8>()
                    .WithParameter<IConfigured9>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var configured7 = A.Fake<IConfigured7>();
        var configured8 = A.Fake<IConfigured8>();
        var configured9 = A.Fake<IConfigured9>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6,
            configured7,
            configured8,
            configured9
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_With10Parameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute10))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .WithParameter<IConfigured7>()
                    .WithParameter<IConfigured8>()
                    .WithParameter<IConfigured9>()
                    .WithParameter<IConfigured10>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var configured7 = A.Fake<IConfigured7>();
        var configured8 = A.Fake<IConfigured8>();
        var configured9 = A.Fake<IConfigured9>();
        var configured10 = A.Fake<IConfigured10>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6,
            configured7,
            configured8,
            configured9,
            configured10
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_With11Parameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute11))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .WithParameter<IConfigured7>()
                    .WithParameter<IConfigured8>()
                    .WithParameter<IConfigured9>()
                    .WithParameter<IConfigured10>()
                    .WithParameter<IConfigured11>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var configured7 = A.Fake<IConfigured7>();
        var configured8 = A.Fake<IConfigured8>();
        var configured9 = A.Fake<IConfigured9>();
        var configured10 = A.Fake<IConfigured10>();
        var configured11 = A.Fake<IConfigured11>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6,
            configured7,
            configured8,
            configured9,
            configured10,
            configured11
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void CreatesAMethod_With12Parameters()
    {
        var method = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute12))
                    .WithParameter<IConfigured1>()
                    .WithParameter<IConfigured2>()
                    .WithParameter<IConfigured3>()
                    .WithParameter<IConfigured4>()
                    .WithParameter<IConfigured5>()
                    .WithParameter<IConfigured6>()
                    .WithParameter<IConfigured7>()
                    .WithParameter<IConfigured8>()
                    .WithParameter<IConfigured9>()
                    .WithParameter<IConfigured10>()
                    .WithParameter<IConfigured11>()
                    .WithParameter<IConfigured12>()
                    .Compile();
        method.Should().NotBeNull();
        var serviceProvider = A.Fake<IServiceProvider>();
        var configured1 = A.Fake<IConfigured1>();
        var configured2 = A.Fake<IConfigured2>();
        var configured3 = A.Fake<IConfigured3>();
        var configured4 = A.Fake<IConfigured4>();
        var configured5 = A.Fake<IConfigured5>();
        var configured6 = A.Fake<IConfigured6>();
        var configured7 = A.Fake<IConfigured7>();
        var configured8 = A.Fake<IConfigured8>();
        var configured9 = A.Fake<IConfigured9>();
        var configured10 = A.Fake<IConfigured10>();
        var configured11 = A.Fake<IConfigured11>();
        var configured12 = A.Fake<IConfigured12>();
        var injected1 = A.Fake<IInjected1>();
        var injected2 = A.Fake<IInjected2>();
        A.CallTo(() => serviceProvider.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).Returns(injected1);
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).Returns(injected2);
        method(
            new MethodFuncTest(),
            serviceProvider,
            configured1,
            configured2,
            configured3,
            configured4,
            configured5,
            configured6,
            configured7,
            configured8,
            configured9,
            configured10,
            configured11,
            configured12
        );

        A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappenedOnceExactly();
        A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappenedOnceExactly();
    }

    // #######

    [Test]
    public void MapsActionAndExecutes()
    {
        var methodFuncTestMock = A.Fake<MethodFuncTest>();

        var action = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute))
                    .Compile();

        action(methodFuncTestMock, A.Fake<IServiceProvider>());

        A.CallTo(() => methodFuncTestMock.Execute()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MapsConfiguredParameterToAction()
    {
        var serviceProviderMock = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null);
        A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
        A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
        A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(null);

        var serviceProvider = serviceProviderMock;
        var methodFuncTestMock = A.Fake<MethodFuncTest>();

        var action = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute1))
                    .WithParameter<IConfigured1>()
                    .Compile();

        action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>());
        A
           .CallTo(() => methodFuncTestMock.Execute1(A<IConfigured1>.Ignored, A<IInjected1>.Ignored, A<IInjected2>.Ignored, A<IInjected3>.Ignored))
           .MustHaveHappenedOnceExactly();
    }
}
