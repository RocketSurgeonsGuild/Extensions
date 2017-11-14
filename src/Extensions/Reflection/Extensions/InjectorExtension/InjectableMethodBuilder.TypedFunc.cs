using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    public partial class InjectableMethodBuilder
    {
        public Func<TInstance, IServiceProvider, TReturn> CreateTypedFunc<TInstance, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, TReturn> CreateTypedFunc<TInstance, T1, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, TReturn> CreateTypedFunc<TInstance, T1, T2, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>();
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>();
        }
    }
}
