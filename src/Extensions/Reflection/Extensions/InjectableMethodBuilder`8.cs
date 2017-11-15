using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XUnitTestProject1
{

    public class InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8> WithParameter<T8>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7>>(body, parameters);
            return lambda.Compile();
        }
    }
}
