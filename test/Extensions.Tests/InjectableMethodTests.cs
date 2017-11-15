using System;
using FakeItEasy;
using FluentAssertions;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using Rocket.Surgery.Reflection.Extensions;
using Xunit;

namespace Rocket.Surgery.Extensions.Tests
{
    public class InjectableMethodTests
    {
        public interface IConfigured1Sub : IConfigured1 { }

        class Configured1 : IConfigured1 { }
        class Configured1Sub : IConfigured1Sub { }

        public abstract class AbstractBase{ }

        public class DerivedA : AbstractBase { }

        public class DerivedB : AbstractBase { }

        public class MethodFuncTest
        {
            public virtual void Execute(IConfigured1 configured1)
            {

            }
        }

        [Fact]
        public void HandlesInheritedValues()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null);
            var methodFuncTest= A.Fake <MethodFuncTest>();
            var action = InjectableMethodBuilder
                .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute))
                .WithParameter<IConfigured1>()
                .Compile();

            action(methodFuncTest, serviceProviderMock, new Configured1());

            A.CallTo(() => methodFuncTest.Execute(
                A<IConfigured1>.That.Matches(x => x.GetType() == typeof(Configured1)))
            ).MustHaveHappened();
            A.CallTo(() => serviceProviderMock.GetService(A<Type>.Ignored)).MustNotHaveHappened();
        }

        public class MethodFuncTest2
        {
            public virtual void Execute(IConfigured1Sub configured1)
            {

            }
        }

        [Fact]
        public void HandlesInheritedTypes()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null);
            var methodFuncTest = A.Fake<MethodFuncTest2>();
            Action a = () =>
            {
                InjectableMethodBuilder
                    .Create<MethodFuncTest2>(nameof(MethodFuncTest2.Execute))
                    .WithParameter<IConfigured1>()
                    .Compile();
            };
            a.Should().Throw<ArgumentException>();
        }

        public class MethodFuncTest3
        {
            public virtual void Execute(IConfigured1 configured1)
            {

            }
            public virtual void Execute2(AbstractBase configured1)
            {

            }
        }

        [Fact]
        public void HandlesOutheritedTypes()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null);
            var methodFuncTest = A.Fake<MethodFuncTest3>();
            var action = InjectableMethodBuilder
                .Create<MethodFuncTest3>(nameof(MethodFuncTest3.Execute))
                .WithParameter<IConfigured1Sub>()
                .Compile();

            action(methodFuncTest, serviceProviderMock, new Configured1Sub());

            A.CallTo(() => serviceProviderMock.GetService(A<Type>.Ignored)).MustNotHaveHappened();
        }


        [Fact]
        public void HandlesOutheritedTypes2()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(A<Type>._)).Returns(null);
            var methodFuncTest = A.Fake<MethodFuncTest3>();
            var action = InjectableMethodBuilder
                .Create<MethodFuncTest3>(nameof(MethodFuncTest3.Execute2))
                .WithParameter<DerivedA>()
                .Compile();

            action(methodFuncTest, serviceProviderMock, new DerivedA());

            A.CallTo(() => serviceProviderMock.GetService(A<Type>.Ignored)).MustNotHaveHappened();
        }
    }
}