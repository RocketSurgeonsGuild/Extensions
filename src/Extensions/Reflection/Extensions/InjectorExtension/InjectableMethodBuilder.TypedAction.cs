using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    public partial class InjectableMethodBuilder
    {
        public Action<TInstance, IServiceProvider> CreateTypedAction<TInstance>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance>();
        }

        public Action<TInstance, IServiceProvider, T1> CreateTypedAction<TInstance, T1>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1>();
        }

        public Action<TInstance, IServiceProvider, T1, T2> CreateTypedAction<TInstance, T1, T2>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3> CreateTypedAction<TInstance, T1, T2, T3>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4> CreateTypedAction<TInstance, T1, T2, T3, T4>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5> CreateTypedAction<TInstance, T1, T2, T3, T4, T5>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        }

        public Action<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        }
    }
}
