using System;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    partial class InjectableMethod
    {
        public Func<TInstance, IServiceProvider, TReturn> CreateTypedFunc<TInstance, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider) =>
            {
                var parameters = new object[_numParams];
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, TReturn> CreateTypedFunc<TInstance, T1, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, TReturn> CreateTypedFunc<TInstance, T1, T2, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }


        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8, t9) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                SetConfiguredParameter(parameters, t9);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                SetConfiguredParameter(parameters, t9);
                SetConfiguredParameter(parameters, t10);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                SetConfiguredParameter(parameters, t9);
                SetConfiguredParameter(parameters, t10);
                SetConfiguredParameter(parameters, t11);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                SetConfiguredParameter(parameters, t9);
                SetConfiguredParameter(parameters, t10);
                SetConfiguredParameter(parameters, t11);
                SetConfiguredParameter(parameters, t12);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                SetConfiguredParameter(parameters, t9);
                SetConfiguredParameter(parameters, t10);
                SetConfiguredParameter(parameters, t11);
                SetConfiguredParameter(parameters, t12);
                SetConfiguredParameter(parameters, t13);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }

        public Func<TInstance, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> CreateTypedFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>()
        {
            // if return is enumerable then the default should be empty, not null.
            var returnTypeInfo = typeof(TReturn).GetTypeInfo();
            var returnDefault = GetDefaultReturnValue<TReturn>(returnTypeInfo);

            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                SetConfiguredParameter(parameters, t9);
                SetConfiguredParameter(parameters, t10);
                SetConfiguredParameter(parameters, t11);
                SetConfiguredParameter(parameters, t12);
                SetConfiguredParameter(parameters, t13);
                SetConfiguredParameter(parameters, t14);
                InjectParameters(serviceProvider, parameters);
                return GetResult(instance, parameters, returnDefault, returnTypeInfo);
            };
        }
    }

}
