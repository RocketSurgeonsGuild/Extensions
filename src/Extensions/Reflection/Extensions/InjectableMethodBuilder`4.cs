using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace XUnitTestProject1
{
    public class InjectableMethodBuilder<TContainer, T, T2, T3> : InjectableMethodBuilderBase
    {
        public InjectableMethodBuilder() : base(typeof(TContainer)) { }
        public InjectableMethodBuilder(ImmutableArray<string> methodNames) : base(typeof(TContainer), methodNames) { }

        public InjectableMethodBuilder<TContainer, T, T2, T3, T4> WithParameter<T4>()
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3, T4>(MethodNames);
        }

        public InjectableMethodBuilder<TContainer, T, T2, T3> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<TContainer, T, T2, T3>(MethodNames.Add(methodName));
        }

        public Func<TContainer, IServiceProvider, T, T2, T3, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo());
            var lambda = Expression.Lambda<Func<TContainer, IServiceProvider, T, T2, T3, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<TContainer, IServiceProvider, T, T2, T3> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo());
            var lambda = Expression.Lambda<Action<TContainer, IServiceProvider, T, T2, T3>>(body, parameters);
            return lambda.Compile();
        }
    }
}
