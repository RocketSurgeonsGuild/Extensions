using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using Rocket.Surgery.Extensions.Tests.Fixtures;
using Rocket.Surgery.Reflection.Extensions.InjectorExtension;
using Xunit;

namespace Rocket.Surgery.Extensions.Tests
{
    public class InjectableMethodFuncTests
    {
        [Fact]
        public void MapsConfiguredParameterToFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(null);

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 1 && x.GetParameters().Single().ParameterType == typeof(IConfigured1));

            var method = new InjectableMethod(
                methodInfo,
                null,
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .SingleOrDefault(x => x.ParameterType == typeof(IConfigured1)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void MapsInstanceParameterToFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 1 && x.GetParameters().Single().ParameterType == typeof(IInstance));

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(typeof(IInstance), instance => parameterInfo => parameterInfo.ParameterType.GetTypeInfo().IsAssignableFrom(instance.GetTypeInfo()), false),
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IInstance>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void InjectsAllServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 3);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(typeof(IInstance), instance => parameterInfo => parameterInfo.ParameterType.GetTypeInfo().IsAssignableFrom(instance.GetTypeInfo()), true),
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IReturn>();
            action(methodFuncTestMock, serviceProvider);
            A.CallTo(() => methodFuncTestMock.Execute(A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void InjectsAllServicesSafelyFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(null);

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 3);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(typeof(IInstance), instance => parameterInfo => parameterInfo.ParameterType.GetTypeInfo().IsAssignableFrom(instance.GetTypeInfo()), true),
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IReturn>();
            action(methodFuncTestMock, serviceProvider);
            A.CallTo(() => methodFuncTestMock.Execute(A<IInjected1>._, A<IInjected2>._, null)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures1ServiceFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 5);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures2ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 6);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures3ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 7);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures4ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 8);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures5ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 9);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures6ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 10);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures7ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 11);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures8ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 12);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured8)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IConfigured8, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IConfigured8>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IConfigured8>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures9ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 13);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured8)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured9)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IConfigured8, IConfigured9, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IConfigured8>(), A.Fake<IConfigured9>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IConfigured8>._, A<IConfigured9>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures10ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 14);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured8)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured9)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured10)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IConfigured8, IConfigured9, IConfigured10, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IConfigured8>(), A.Fake<IConfigured9>(), A.Fake<IConfigured10>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IConfigured8>._, A<IConfigured9>._, A<IConfigured10>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures11ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 15);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured8)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured9)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured10)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured11)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IConfigured8, IConfigured9, IConfigured10, IConfigured11, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IConfigured8>(), A.Fake<IConfigured9>(), A.Fake<IConfigured10>(), A.Fake<IConfigured11>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IConfigured8>._, A<IConfigured9>._, A<IConfigured10>._, A<IConfigured11>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures12ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 16);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured8)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured9)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured10)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured11)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured12)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IConfigured8, IConfigured9, IConfigured10, IConfigured11, IConfigured12, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IConfigured8>(), A.Fake<IConfigured9>(), A.Fake<IConfigured10>(), A.Fake<IConfigured11>(), A.Fake<IConfigured12>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IConfigured8>._, A<IConfigured9>._, A<IConfigured10>._, A<IConfigured11>._, A<IConfigured12>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Configures13ServicesFunc()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 17);

            var method = new InjectableMethod(
                methodInfo,
                new InstanceParameter(methodInfo
                    .GetParameters()
                    .Single(x => x.ParameterType == typeof(IInstance)), false),
                new[] {
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured1)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured2)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured3)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured4)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured5)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured6)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured7)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured8)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured9)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured10)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured11)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured12)), false),
                    new ConfiguredParameter(methodInfo
                        .GetParameters()
                        .Single(x => x.ParameterType == typeof(IConfigured13)), false)
                },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IConfigured1, IConfigured2, IConfigured3, IConfigured4, IConfigured5, IConfigured6, IConfigured7, IConfigured8, IConfigured9, IConfigured10, IConfigured11, IConfigured12, IConfigured13, IInstance, IReturn>();
            action(methodFuncTestMock, serviceProvider, A.Fake<IConfigured1>(), A.Fake<IConfigured2>(), A.Fake<IConfigured3>(), A.Fake<IConfigured4>(), A.Fake<IConfigured5>(), A.Fake<IConfigured6>(), A.Fake<IConfigured7>(), A.Fake<IConfigured8>(), A.Fake<IConfigured9>(), A.Fake<IConfigured10>(), A.Fake<IConfigured11>(), A.Fake<IConfigured12>(), A.Fake<IConfigured13>(), A.Fake<IInstance>());
            A.CallTo(() => methodFuncTestMock.Execute(A<IConfigured1>._, A<IConfigured2>._, A<IConfigured3>._, A<IConfigured4>._, A<IConfigured5>._, A<IConfigured6>._, A<IConfigured7>._, A<IConfigured8>._, A<IConfigured9>._, A<IConfigured10>._, A<IConfigured11>._, A<IConfigured12>._, A<IConfigured13>._, A<IInstance>._, A<IInjected1>._, A<IInjected2>._, A<IInjected3>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void ReturnsExpectedTypesWhenInCompatibleTypeIsPresented()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();
            A.CallTo(() => methodFuncTestMock.ExecuteReturn()).Returns(true);

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.ExecuteEnumerable))
                .Single(x => x.GetParameters().Count() == 0);

            var method = new InjectableMethod(
                methodInfo,
                null,
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<bool>();
            var result = action(methodFuncTestMock, serviceProvider);

            Assert.False(result);

            methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 0);

            method = new InjectableMethod(
                methodInfo,
                null,
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            action = method.CreateFunc<bool>();
            result = action(methodFuncTestMock, serviceProvider);

            Assert.False(result);

            methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.ExecuteReturn))
                .Single(x => x.GetParameters().Count() == 0);

            method = new InjectableMethod(
                methodInfo,
                null,
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            action = method.CreateFunc<bool>();
            result = action(methodFuncTestMock, serviceProvider);

            Assert.True(result);
        }

        [Fact]
        public void ReturnsAnEmptyEnumerableWhenReturnTypeIsEnumerable()
        {
            var serviceProviderMock = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected1))).Returns(A.Fake<IInjected1>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected2))).Returns(A.Fake<IInjected2>());
            A.CallTo(() => serviceProviderMock.GetService(typeof(IInjected3))).Returns(A.Fake<IInjected3>());

            var serviceProvider = serviceProviderMock;
            var methodFuncTestMock = A.Fake<MethodFuncTest>();
            A.CallTo(() => methodFuncTestMock.ExecuteEnumerable()).Returns(new[] { A.Fake<IReturn>(), A.Fake<IReturn>(), A.Fake<IReturn>() });

            var methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.ExecuteEnumerable))
                .Single(x => x.GetParameters().Count() == 0);

            var method = new InjectableMethod(
                methodInfo,
                null,
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            var action = method.CreateFunc<IEnumerable<IReturn>>();
            var result = action(methodFuncTestMock, serviceProvider);

            Assert.NotNull(result);
            Assert.True(result.Any());

            methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.Execute))
                .Single(x => x.GetParameters().Count() == 0);

            method = new InjectableMethod(
                methodInfo,
                null,
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            action = method.CreateFunc<IEnumerable<IReturn>>();
            result = action(methodFuncTestMock, serviceProvider);

            Assert.NotNull(result);
            Assert.False(result.Any());

            methodInfo = typeof(MethodFuncTest).GetTypeInfo().DeclaredMethods
                .Where(x => x.Name == nameof(MethodFuncTest.ExecuteReturn))
                .Single(x => x.GetParameters().Count() == 0);

            method = new InjectableMethod(
                methodInfo,
                null,
                new ConfiguredParameter[] { },
                new Dictionary<TypeInfo, ConfiguredParameter>()
            );

            action = method.CreateFunc<IEnumerable<IReturn>>();
            result = action(methodFuncTestMock, serviceProvider);

            Assert.NotNull(result);
            Assert.False(result.Any());
        }
    }
}
