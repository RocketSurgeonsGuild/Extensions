using FastExpressionCompiler;
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9> : InjectableMethodBuilderBase
    {
        internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames) { }


        public InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9, TNext>(Container, MethodNames);
        }

        public InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9>(Container, MethodNames.Add(methodName));
        }

        public Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Compile<TResult>()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo(),
                typeof(T8).GetTypeInfo(),
                typeof(T9).GetTypeInfo());
            var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast<Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(lambda);
        }

        public Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9> Compile()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo(),
                typeof(T8).GetTypeInfo(),
                typeof(T9).GetTypeInfo());
            var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9>>(body, parameters);
            return ExpressionCompiler.CompileFast<Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9>>(lambda);
        }

        public Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, TResult> CompileStatic<TResult>()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo(),
                typeof(T8).GetTypeInfo(),
                typeof(T9).GetTypeInfo());
            var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast<Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>>(lambda);
        }

        public Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9> CompileStatic()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo(),
                typeof(T7).GetTypeInfo(),
                typeof(T8).GetTypeInfo(),
                typeof(T9).GetTypeInfo());
            var lambda = Expression.Lambda<Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9>>(body, parameters);
            return ExpressionCompiler.CompileFast<Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9>>(lambda);
        }
    }
}
