using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<TContainer, T, T2, T3, T4> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4, TNext>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, T2, T3, T4, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, T2, T3, T4, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T, T2, T3, T4> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T, T2, T3, T4>>(body, parameters);
            return lambda.Compile();
        }
    }
}
