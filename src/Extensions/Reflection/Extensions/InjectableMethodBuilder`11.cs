using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8, T9, T10> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNext>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8, T9, T10> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo(),
                typeof(T8).GetTypeInfo(),
                typeof(T9).GetTypeInfo(),
                typeof(T10).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo(),
                typeof(T8).GetTypeInfo(),
                typeof(T9).GetTypeInfo(),
                typeof(T10).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(body, parameters);
            return lambda.Compile();
        }
    }
}
