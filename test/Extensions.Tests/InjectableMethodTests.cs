using FakeItEasy;
using FluentAssertions;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using Rocket.Surgery.Reflection;

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA1040 // Avoid empty interfaces
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CS0436 // Type conflicts with imported type

namespace Rocket.Surgery.Extensions.Tests;

public class InjectableMethodTests
{
    public static class MethodFuncTestStatic
    {
        public static void Execute(IConfigured1 configured1)
        {
            ArgumentNullException.ThrowIfNull(configured1);
        }
    }

    [Test]
    public void HandlesInheritedValues()
    {
        var serviceProviderMock = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null!);
        var methodFuncTest = A.Fake<MethodFuncTest>();
        var action = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute))
                    .WithParameter<IConfigured1>()
                    .Compile();

        action(methodFuncTest, serviceProviderMock, new Configured1());

        A
           .CallTo(
                () => methodFuncTest.Execute(
                    A<IConfigured1>.That.Matches(x => x.GetType() == typeof(Configured1))
                )
            )
           .MustHaveHappened();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void HandlesInheritedTypes()
    {
        var serviceProviderMock = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null!);
        A.Fake<MethodFuncTest2>();
        var a = () =>
                {
                    InjectableMethodBuilder
                       .Create<MethodFuncTest2>(nameof(MethodFuncTest2.Execute))
                       .WithParameter<IConfigured1>()
                       .Compile();
                };
        a.Should().Throw<ArgumentException>();
    }

    [Test]
    public void HandlesOutheritedTypes()
    {
        var serviceProviderMock = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null!);
        var methodFuncTest = A.Fake<MethodFuncTest3>();
        var action = InjectableMethodBuilder
                    .Create<MethodFuncTest3>(nameof(MethodFuncTest3.Execute))
                    .WithParameter<IConfigured1Sub>()
                    .Compile();

        action(methodFuncTest, serviceProviderMock, new Configured1Sub());

        A.CallTo(() => serviceProviderMock.GetService(A<Type>.Ignored)).MustNotHaveHappened();
    }


    [Test]
    public void HandlesOutheritedTypes2()
    {
        var serviceProviderMock = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null!);
        var methodFuncTest = A.Fake<MethodFuncTest3>();
        var action = InjectableMethodBuilder
                    .Create<MethodFuncTest3>(nameof(MethodFuncTest3.Execute2))
                    .WithParameter<DerivedA>()
                    .Compile();

        action(methodFuncTest, serviceProviderMock, new());

        A.CallTo(() => serviceProviderMock.GetService(A<Type>.Ignored)).MustNotHaveHappened();
    }

    [Test]
    public void HandlesStaticFunctions()
    {
        var serviceProviderMock = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(A.Fake<IConfigured1>());
        var action = InjectableMethodBuilder
                    .Create(typeof(MethodFuncTestStatic), nameof(MethodFuncTestStatic.Execute))
                    .CompileStatic();

        action(serviceProviderMock);

        A.CallTo(() => serviceProviderMock.GetService(typeof(IConfigured1))).MustHaveHappened();
    }

    public interface IConfigured1Sub : IConfigured1 { }

    private class Configured1 : IConfigured1 { }

    private class Configured1Sub : IConfigured1Sub { }

    public abstract class AbstractBase { }

    public class DerivedA : AbstractBase { }

    public class DerivedB : AbstractBase { }

    [UsedImplicitly]
    public abstract class MethodFuncTest
    {
        public virtual void Execute(IConfigured1 configured1) { }
    }

    [UsedImplicitly]
    public abstract class MethodFuncTest2
    {
        public virtual void Execute(IConfigured1Sub configured1) { }
    }

    [UsedImplicitly]
    public abstract class MethodFuncTest3
    {
        public virtual void Execute(IConfigured1 configured1) { }

        public virtual void Execute2(AbstractBase configured1) { }
    }
}
