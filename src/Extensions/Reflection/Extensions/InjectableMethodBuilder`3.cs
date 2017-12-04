using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<T, T2> : InjectableMethodBuilderBase
    {
        internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames) { }

        public InjectableMethodBuilder<T, T2, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<T, T2, TNext>(Container, MethodNames);
        }

        public InjectableMethodBuilder<T, T2> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<T, T2>(Container, MethodNames.Add(methodName));
        }

        public Func<object, IServiceProvider, T, T2, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<object, IServiceProvider, T, T2> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2>>(body, parameters);
            return lambda.Compile();
        }
    }
}
