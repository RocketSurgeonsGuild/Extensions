using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    public partial class InjectableMethodBuilder
    {
        public Action<object, IServiceProvider> CreateAction()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction();
        }

        public Action<object, IServiceProvider, T1> CreateAction<T1>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1>();
        }

        public Action<object, IServiceProvider, T1, T2> CreateAction<T1, T2>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2>();
        }

        public Action<object, IServiceProvider, T1, T2, T3> CreateAction<T1, T2, T3>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4> CreateAction<T1, T2, T3, T4>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5> CreateAction<T1, T2, T3, T4, T5>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6> CreateAction<T1, T2, T3, T4, T5, T6>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7> CreateAction<T1, T2, T3, T4, T5, T6, T7>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        }
    }
}
