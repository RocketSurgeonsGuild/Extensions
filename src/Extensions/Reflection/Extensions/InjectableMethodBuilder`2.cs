using FastExpressionCompiler;
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<T> : InjectableMethodBuilderBase
    {
        internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames) { }

        public InjectableMethodBuilder<T, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<T, TNext>(Container, MethodNames);
        }

        public InjectableMethodBuilder<T> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<T>(Container, MethodNames.Add(methodName));
        }

        public Func<object, IServiceProvider, T, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo());
            var lambda = Expression.Lambda<Func<object, IServiceProvider, T, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<object, IServiceProvider, T> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo());
            var lambda = Expression.Lambda<Action<object, IServiceProvider, T>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }
    }
}
