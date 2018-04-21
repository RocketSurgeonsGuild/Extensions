using FastExpressionCompiler;
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<T, T2, T3, T4> : InjectableMethodBuilderBase
    {
        internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames) { }

        public InjectableMethodBuilder<T, T2, T3, T4, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<T, T2, T3, T4, TNext>(Container, MethodNames);
        }

        public InjectableMethodBuilder<T, T2, T3, T4> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<T, T2, T3, T4>(Container, MethodNames.Add(methodName));
        }

        public Func<object, IServiceProvider, T, T2, T3, T4, TResult> Compile<TResult>()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, T3, T4, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<object, IServiceProvider, T, T2, T3, T4> Compile()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2, T3, T4>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Func<IServiceProvider, T, T2, T3, T4, TResult> CompileStatic<TResult>()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, T3, T4, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<IServiceProvider, T, T2, T3, T4> CompileStatic()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Action<IServiceProvider, T, T2, T3, T4>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }
    }
}
