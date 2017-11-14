using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    public partial class InjectableMethodBuilder
    {
        public Func<object, IServiceProvider, TReturn> CreateFunc<TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<TReturn>();
        }

        public Func<object, IServiceProvider, T1, TReturn> CreateFunc<T1, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, TReturn> CreateFunc<T1, T2, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, TReturn> CreateFunc<T1, T2, T3, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, TReturn> CreateFunc<T1, T2, T3, T4, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, TReturn> CreateFunc<T1, T2, T3, T4, T5, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>();
        }

        public Func<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>()
        {
            ValidateReturnType();

            var resolvedConfiguredParameter = new Dictionary<TypeInfo, ConfiguredParameter>();

            return new InjectableMethod(_methodInfo, _instanceParameter, _configuredParameters, resolvedConfiguredParameter)
                .CreateFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>();
        }
    }
}
