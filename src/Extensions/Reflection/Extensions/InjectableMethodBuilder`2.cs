using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<TContainer, T> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<TContainer, T, TNext>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T>>(body, parameters);
            return lambda.Compile();
        }
    }
}
