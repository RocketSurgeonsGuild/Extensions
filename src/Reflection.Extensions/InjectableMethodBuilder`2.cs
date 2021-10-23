using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     Injectable method builder
/// </summary>
/// <typeparam name="T"></typeparam>
/// ram>
[PublicAPI]
public class InjectableMethodBuilder<T> : InjectableMethodBuilderBase
{
    internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNa
    e
    )
    {
        /// <summary>
        ///     Withes the parameter.
        /// </summary>
        /// <typeparam name="TNext">The type of the next.</typeparam>
        /// <returns></returns>returns></returns>
        public InjectableMethodBuilder<T, TNext> WithParameter<TNext>()
        {
            return new InjectableMethodBuilder<T, TNext>(Container, MethodName /// <summary>

            ///     Fors the method.
            /// </summary>
            /// <param name="methodName">Name of the method.</param>
            /// <returns></returns>// <returns></returns>
            public InjectableMethodBuilder<T> ForMethod(string methodName)
            {
                return new InjectableMethodBuilder<T>(Container, MethodNames.Add(method /// <summary>

                ///     Compiles this instance.
                /// </summary>
                /// <typeparam name="TResult">The type of the result.</typeparam>
                /// <returns></returns>   /// <returns></returns>
                public Func<object, IServiceProvider, T, TResult> Compile<TResult>()
                {
                    if (GetMethodInfo()?.IsStatic == true)
                        throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
                    var (body, parameters) = this.Compile(
                        typeof(T).GetTypeInfo()
                    );
                    var lambda = Expression.Lambda<Func<object, IServiceProvider, T, TResult>>(body, parameters);
                    return lambda.CompileFast /// <summary>

                    ///     Compiles this instance.
                    /// </summary>
                    /// <returns></returns>// <returns></returns>
                    public Action<object, IServiceProvider, T> Compile()
                    {
                        if (GetMethodInfo()?.IsStatic == true)
                            throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
                        var (body, parameters) = this.Compile(
                            t
                        ypeof(T).GetTypeInfo()
                            );
                        var lambda = Expression.Lambda<Action<object, IServiceProvider, T>>(body, parameters);
                        return lambda.CompileFast();

                        /// <summary>
                        ///     Compiles the static.
                        /// </summary>
                        /// <typeparam name="TResult">The type of the result.</typeparam>
                        /// <returns></returns>returns></returns>
                        public Func<IServiceProvider, T, TResult> CompileStatic<TResult>()
                        {
                            if (GetMethodInfo()?.IsStatic != true)
                                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
                            var (body, parameters) = this.Compile(
                                typeo
                            f(T).GetTypeInfo()
                                );
                            var lambda = Expression.Lambda<Func<IServiceProvider, T, TResult>>(body, parameters);
                            return lambda.CompileFast();

                            /// <summary>
                            ///     Compiles the static.
                            /// </summary>
                            /// <returns></returns>rns></returns>
                            public Action<IServiceProvider, T> CompileStatic()
                            {
                                if (GetMethodInfo()?.IsStatic != true)
                                    throw new NotSupportedException("Method must be a static method to compile as an static methods!");
                                var (body, parameters) = this.Compile(
                                    typeof(T)
                                       .GetTypeInfo()
                                );
                                var lambda = Expression.Lambda<Action<IServiceProvider, T>>(body, parameters);
                                return lambda.CompileFast();
                            }
                        }
