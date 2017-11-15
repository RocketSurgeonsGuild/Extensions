using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XUnitTestProject1
{
    public class InjectableMethodBuilder<TContainer, T> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, T2> WithParameter<T2>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2>(MethodNames);
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
