using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder : InjectableMethodBuilderBase
    {
        public static InjectableMethodBuilder Create<TContainer>(string methodName)
        {
            return new InjectableMethodBuilder(typeof(TContainer)).ForMethod(methodName);
        }

        public static InjectableMethodBuilder Create(Type container, string methodName)
        {
            return new InjectableMethodBuilder(container).ForMethod(methodName);
        }

        public InjectableMethodBuilder(Type container) : base(container) { }
        public InjectableMethodBuilder(Type container, ImmutableArray<string> methodNames) : base(container, methodNames) { }

        public InjectableMethodBuilder<T> WithParameter<T>()
        {
            return new InjectableMethodBuilder<T>(Container, MethodNames);
        }

        public InjectableMethodBuilder ForMethod(string methodName)
        {
            return new InjectableMethodBuilder(Container.AsType(), MethodNames.Add(methodName));
        }

        public Func<object, IServiceProvider, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<object, IServiceProvider, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<object, IServiceProvider> Compile()
        {
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<object, IServiceProvider>>(body, parameters);
            return lambda.Compile();
        }
    }
}
