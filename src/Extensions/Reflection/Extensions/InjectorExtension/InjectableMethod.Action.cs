using System;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    partial class InjectableMethod
    {
        public Action<object, IServiceProvider> CreateAction()
        {
            return (instance, serviceProvider) =>
            {
                var parameters = new object[_numParams];
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1> CreateAction<T1>()
        {
            return (instance, serviceProvider, t1) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2> CreateAction<T1, T2>()
        {
            return (instance, serviceProvider, t1, t2) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3> CreateAction<T1, T2, T3>()
        {
            return (instance, serviceProvider, t1, t2, t3) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4> CreateAction<T1, T2, T3, T4>()
        {
            return (instance, serviceProvider, t1, t2, t3, t4) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5> CreateAction<T1, T2, T3, T4, T5>()
        {
            return (instance, serviceProvider, t1, t2, t3, t4, t5) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6> CreateAction<T1, T2, T3, T4, T5, T6>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7> CreateAction<T1, T2, T3, T4, T5, T6, T7>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return (instance, serviceProvider, t1, t2, t3, t4, t5, t6, t7, t8) =>
            {
                var parameters = new object[_numParams];
                SetConfiguredParameter(parameters, t1);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t2);
                SetConfiguredParameter(parameters, t3);
                SetConfiguredParameter(parameters, t4);
                SetConfiguredParameter(parameters, t5);
                SetConfiguredParameter(parameters, t6);
                SetConfiguredParameter(parameters, t7);
                SetConfiguredParameter(parameters, t8);
                InjectParameters(serviceProvider, parameters);
                ExecuteResult(instance, parameters);
            };
        }


        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }

        public Action<object, IServiceProvider, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> CreateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
        {
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
                ExecuteResult(instance, parameters);
            };
        }
    }

}
