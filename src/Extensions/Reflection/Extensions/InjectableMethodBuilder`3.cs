using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XUnitTestProject1
{
    public class InjectableMethodBuilder<TContainer, T, T2> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, T2, T3> WithParameter<T3>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T, T2> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T, T2>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, T2, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, T2, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T, T2> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T, T2>>(body, parameters);
            return lambda.Compile();
        }
    }
}
