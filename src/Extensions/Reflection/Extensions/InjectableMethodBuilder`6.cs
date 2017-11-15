using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }


        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5, TNext>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, T5>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, T2, T3, T4, T5, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, T2, T3, T4, T5, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T, T2, T3, T4, T5> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T, T2, T3, T4, T5>>(body, parameters);
            return lambda.Compile();
        }
    }
}
