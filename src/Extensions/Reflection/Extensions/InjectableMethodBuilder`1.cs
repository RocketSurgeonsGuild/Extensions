using FastExpressionCompiler;
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// Injectable method builder
    /// </summary>
    /// <seealso cref="InjectableMethodBuilderBase" />
    public class InjectableMethodBuilder : InjectableMethodBuilderBase
    {
        /// <summary>
        /// Creates the specified method name.
        /// </summary>
        /// <typeparam name="TContainer">The type of the container.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public static InjectableMethodBuilder Create<TContainer>(string methodName)
        {
            return new InjectableMethodBuilder(typeof(TContainer)).ForMethod(methodName);
        }

        /// <summary>
        /// Creates the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public static InjectableMethodBuilder Create(Type container, string methodName)
        {
            return new InjectableMethodBuilder(container).ForMethod(methodName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectableMethodBuilder" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public InjectableMethodBuilder(Type container) : base(container) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectableMethodBuilder" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="methodNames">The method names.</param>
        public InjectableMethodBuilder(Type container, ImmutableArray<string> methodNames) : base(container, methodNames) { }

        /// <summary>
        /// Withes the parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public InjectableMethodBuilder<T> WithParameter<T>()
        {
            return new InjectableMethodBuilder<T>(Container, MethodNames);
        }

        /// <summary>
        /// Fors the method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public InjectableMethodBuilder ForMethod(string methodName)
        {
            return new InjectableMethodBuilder(Container.AsType(), MethodNames.Add(methodName));
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns></returns>
        public Func<object, IServiceProvider, TResult> Compile<TResult>()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<object, IServiceProvider, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns></returns>
        public Action<object, IServiceProvider> Compile()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<object, IServiceProvider>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        /// <summary>
        /// Compiles the static.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns></returns>
        public Func<IServiceProvider, TResult> CompileStatic<TResult>()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<IServiceProvider, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        /// <summary>
        /// Compiles the static.
        /// </summary>
        /// <returns></returns>
        public Action<IServiceProvider> CompileStatic()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<IServiceProvider>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }
    }
}
