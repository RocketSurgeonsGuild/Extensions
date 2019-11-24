using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Rocket.Surgery.Reflection
{
    /// <summary>
    /// Injectable method builder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2">The type of the 2.</typeparam>
    /// <typeparam name="T3">The type of the 3.</typeparam>
    /// <typeparam name="T4">The type of the 4.</typeparam>
    /// <typeparam name="T5">The type of the 5.</typeparam>
    /// <typeparam name="T6">The type of the 6.</typeparam>
    /// <typeparam name="T7">The type of the 7.</typeparam>
    /// <typeparam name="T8">The type of the 8.</typeparam>
    [PublicAPI]
    public class InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8> : InjectableMethodBuilderBase
    {
        internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames) { }

        /// <summary>
        /// Withes the parameter.
        /// </summary>
        /// <typeparam name="TNext">The type of the next.</typeparam>
        /// <returns></returns>
        public InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, TNext>(Container, MethodNames);
        }


        /// <summary>
        /// Fors the method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8>(Container, MethodNames.Add(methodName));
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns></returns>
        public Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, TResult> Compile<TResult>()
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
                typeof(T8).GetTypeInfo());
            var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast<Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, TResult>>(lambda);
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns></returns>
        public Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8> Compile()
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
                typeof(T8).GetTypeInfo());
            var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8>>(body, parameters);
            return ExpressionCompiler.CompileFast<Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8>>(lambda);
        }

        /// <summary>
        /// Compiles the static.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns></returns>
        public Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, TResult> CompileStatic<TResult>()
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
                typeof(T8).GetTypeInfo());
            var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast<Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, TResult>>(lambda);
        }

        /// <summary>
        /// Compiles the static.
        /// </summary>
        /// <returns></returns>
        public Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8> CompileStatic()
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
                typeof(T8).GetTypeInfo());
            var lambda = Expression.Lambda<Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8>>(body, parameters);
            return ExpressionCompiler.CompileFast<Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8>>(lambda);
        }
    }
}
