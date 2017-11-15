using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<TContainer> : InjectableMethodBuilderBase
    {

        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(Type container) : base(container) { }


        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames)
        {
        }

        public InjectableMethodBuilder<TContainer, T> WithParameter<T>()
        {
            return new InjectableMethodBuilder<TContainer, T>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider> Compile()
        {
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider>>(body, parameters);
            return lambda.Compile();
        }
    }
}
