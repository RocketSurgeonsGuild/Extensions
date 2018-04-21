using FastExpressionCompiler;
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
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<object, IServiceProvider, T, T2> Compile()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Func<IServiceProvider, T, T2, TResult> CompileStatic<TResult>()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<IServiceProvider, T, T2> CompileStatic()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Action<IServiceProvider, T, T2>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }
    }
}
