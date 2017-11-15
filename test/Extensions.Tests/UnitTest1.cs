using System;
using System.Collections.Generic;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using XUnitTestProject1;

namespace Rocket.Surgery.Extensions.Tests
{
    public class UnitTest1
    {
        public class InjectableMethodBuilderTests
        {
            class Instance
            {
                private string InjectableMethod(string a, int b, long c)
                {
                    return a;
                }
            }

            [Fact]
            public void CreatesAMethod()
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

                A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappened(Repeated.Exactly.Once);
                A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappened(Repeated.Exactly.Once);
            }

            [Fact]
            public void CreatesAMethod2()
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

                A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappened(Repeated.Exactly.Once);
                A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappened(Repeated.Exactly.Once);
            }

            [Fact]
            public void CreatesAMethod3()
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


                A.CallTo(() => serviceProvider.GetService(typeof(IInjected1))).MustHaveHappened(Repeated.Exactly.Once);
                A.CallTo(() => serviceProvider.GetService(typeof(IInjected2))).MustHaveHappened(Repeated.Exactly.Once);
                A.CallTo(() => serviceProvider.GetService(typeof(IInjected3))).MustHaveHappened(Repeated.Exactly.Once);
            }

            // #######

            [Fact]
            public void MapsActionAndExecutes()
            {
                var methodFuncTestMock = A.Fake<MethodFuncTest>();

                var action = InjectableMethodBuilder
                    .Create<MethodFuncTest>(nameof(MethodFuncTest.Execute))
                    .Compile();

                action(methodFuncTestMock, A.Fake<IServiceProvider>());

                A.CallTo(() => methodFuncTestMock.Execute()).MustHaveHappened(Repeated.Exactly.Once);
            }

            [Fact]
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
                A.CallTo(() => methodFuncTestMock.Execute1(A<IConfigured1>.Ignored, A<IInjected1>.Ignored, A<IInjected2>.Ignored, A<IInjected3>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            }

            //[Fact]
            //public void MapsInstanceParameterToAction()
            //{
            //    var serviceProviderMock = A.Fake<IServiceProvider>();
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            //    var serviceProvider = serviceProviderMock;
            //    var methodFuncTestMock = A.Fake<MethodFuncTest>();

            //    var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
            //        .Where(x => x.Name == nameof(MethodFuncTest.Execute))
            //        .Single(x =>
            //            x.GetParameters().Count() == 1 && x.GetParameters().Single().ParameterType == typeof(IInstance));

            //    var method = new InjectableMethod(
            //        methodInfo,
            //        new InstanceParameter(typeof(IInstance),
            //            instance => parameterInfo => parameterInfo.ParameterType.GetTypeInfo()
            //                .IsAssignableFrom(instance.GetTypeInfo()), false),
            //        new ConfiguredParameter[] { },
            //        new Dictionary<TypeInfo, ConfiguredParameter>()
            //    );

            //    var action = method.CreateAction<IInstance>();
            //    action(methodFuncTestMock, serviceProvider, A.Fake<IInstance>());
            //    A.CallTo(() => methodFuncTestMock.Execute(A<IInstance>._)).MustHaveHappened(Repeated.Exactly.Once);
            //}

            //[Fact]
            //public void InjectsAllServices()
            //{
            //    var serviceProviderMock = A.Fake<IServiceProvider>();
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            //    var serviceProvider = serviceProviderMock;
            //    var methodFuncTestMock = A.Fake<MethodFuncTest>();

            //    var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
            //        .Where(x => x.Name == nameof(MethodFuncTest.Execute))
            //        .Single(x => x.GetParameters().Count() == 3);

            //    var method = new InjectableMethod(
            //        methodInfo,
            //        new InstanceParameter(typeof(IInstance),
            //            instance => parameterInfo => parameterInfo.ParameterType.GetTypeInfo()
            //                .IsAssignableFrom(instance.GetTypeInfo()), true),
            //        new ConfiguredParameter[] { },
            //        new Dictionary<TypeInfo, ConfiguredParameter>()
            //    );

            //    var action = method.CreateAction();
            //    action(methodFuncTestMock, serviceProvider);
            //    A.CallTo(() => methodFuncTestMock.Execute(A<IInjected1>._, A<IInjected2>._, A<IInjected3>._))
            //        .MustHaveHappened(Repeated.Exactly.Once);
            //}

            //[Fact]
            //public void InjectsAllServicesSafely()
            //{
            //    var serviceProviderMock = A.Fake<IServiceProvider>();
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(null);

            //    var serviceProvider = serviceProviderMock;
            //    var methodFuncTestMock = A.Fake<MethodFuncTest>();

            //    var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
            //        .Where(x => x.Name == nameof(MethodFuncTest.Execute))
            //        .Single(x => x.GetParameters().Count() == 3);

            //    var method = new InjectableMethod(
            //        methodInfo,
            //        new InstanceParameter(typeof(IInstance),
            //            instance => parameterInfo => parameterInfo.ParameterType.GetTypeInfo()
            //                .IsAssignableFrom(instance.GetTypeInfo()), true),
            //        new ConfiguredParameter[] { },
            //        new Dictionary<TypeInfo, ConfiguredParameter>()
            //    );

            //    var action = method.CreateAction();
            //    action(methodFuncTestMock, serviceProvider);
            //    A.CallTo(() => methodFuncTestMock.Execute(A<IInjected1>._, A<IInjected2>._, null))
            //        .MustHaveHappened(Repeated.Exactly.Once);
            //}

            //[Fact]
            //public void Configures1Service()
            //{
            //    var serviceProviderMock = A.Fake<IServiceProvider>();
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            //    A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            //    var serviceProvider = serviceProviderMock;
            //    var methodFuncTestMock = A.Fake<MethodFuncTest>();

            //    var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
            //        .Where(x => x.Name == nameof(MethodFuncTest.Execute))
            //        .Single(x => x.GetParameters().Count() == 5);

            //    var method = new InjectableMethod(
            //        methodInfo,
            //        new InstanceParameter(methodInfo
            //            .GetParameters()
            //            .Single(x => x.ParameterType == typeof(IInstance)), false),
            //        new[]
            //        {
            //            new ConfiguredParameter(methodInfo
            //                .GetParameters()
            //                .Single(x => x.ParameterType == typeof(IConfigured1)), false)
            //        },
            //        new Dictionary<TypeInfo, ConfiguredParameter>()
            //    );

            //    var sub = A.Fake<IConfigured1>();
            //    var action = method.CreateAction<IConfigured1, IInstance>();
            //    action(methodFuncTestMock, serviceProvider, sub, A.Fake<IInstance>());
            //    A.CallTo(() =>
            //            methodFuncTestMock.Execute(sub, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._))
            //        .MustHaveHappened(Repeated.Exactly.Once);
            //}




        }
    }
}
