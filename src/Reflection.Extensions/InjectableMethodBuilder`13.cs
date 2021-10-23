using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     Injectable method builder
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="T2">The type of the 2.</typeparam>
/// <typeparam name="T3">The type of the 3.</typeparam>
/// <typeparam name="T4">The type of the 4.</typeparam>
/// <typeparam name="T5">The type of the 5.</typeparam>
/// <typeparam name="T6">The type of the 6.</typeparam>
/// <typeparam name="T7">The type of the 7.</typeparam>
/// <typeparam name="T8">The type of the 8.</typeparam>
/// <typeparam name="T9">The type of the 9.</typeparam>
/// <typeparam name="T10">The type of the 10.</typeparam>
/// <typeparam name="T11">The type of the 11.</typeparam>
/// <typeparam name="T12">The type of the 12.</typeparam>
/// am>
[PublicAPI]
public class InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : InjectableMethodBuilderBase
{
    internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNam

    s

    {
        /// <summary>
        ///     Fors the method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>eturns></returns>
        public InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Container, MethodNames.Add(methodName) /// <summary>

            ///     Compiles this instance.
            /// </summary>
            /// <typeparam name="TResult">The type of the result.</typeparam>
            /// <returns></returns>/ <returns></returns>
            public Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Compile<TResult>()
            {
                if (GetMethodInfo()?.IsStatic == true)
                    throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
                var (body, parameters) = this.Compile(
                    typeof(T).GetTypeInfo(),
                    typeof(T2).GetTypeInfo(),
                    typeof(T3).GetTypeInfo(),
                    typeof(T4).GetTypeInfo(),
                    typeof(T5).GetTypeInfo(),
                    typeof(T6).GetTypeInfo(),
                    typeof(T7).GetTypeInfo(),
                    typeof(T8).GetTypeInfo(),
                    typeof(T9).GetTypeInfo(),
                    typeof(T10).GetTypeInfo(),
                    typeof(T11).GetTypeInfo(),
                    type
                of(T12).GetTypeInfo()
                    );
                var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>(body, parameters);
                return lambda.CompileFast<Func<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>();

                /// <summary>
                ///     Compiles this instance.
                /// </summary>
                /// <returns></returns>eturns></returns>
                public Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Compile()
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
                        typeof(T9).GetTypeInfo(),
                        typeof(T10).GetTypeInfo(),
                        typeof(T11).GetTypeInfo(),
                        typeof(T
                    12).GetTypeInfo()
                        );
                    var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(body, parameters);
                    return lambda.CompileFast<Action<object, IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>();

                    /// <summary>
                    ///     Compiles the static.
                    /// </summary>
                    /// <typeparam name="TResult">The type of the result.</typeparam>
                    /// <returns></returns>ns></returns>
                    public Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> CompileStatic<TResult>()
                    {
                        if (GetMethodInfo()?.IsStatic != true)
                            throw new NotSupportedException("Method must be a static method to compile as an static methods!");
                        var (body, parameters) = this.Compile(
                            typeof(T).GetTypeInfo(),
                            typeof(T2).GetTypeInfo(),
                            typeof(T3).GetTypeInfo(),
                            typeof(T4).GetTypeInfo(),
                            typeof(T5).GetTypeInfo(),
                            typeof(T6).GetTypeInfo(),
                            typeof(T7).GetTypeInfo(),
                            typeof(T8).GetTypeInfo(),
                            typeof(T9).GetTypeInfo(),
                            typeof(T10).GetTypeInfo(),
                            typeof(T11).GetTypeInfo(),
                            typeof(T12).GetTypeInfo()
                        );
                        var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>(body, parameters);
                        return lambda.CompileFast<Func<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>>();
                    }

                    /// <summary>
                    ///     Compiles the static.
                    /// </summary>
                    /// <returns></returns>/returns>
                    public Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> CompileStatic()
                    {
                        if (GetMethodInfo()?.IsStatic != true)
                            throw new NotSupportedException("Method must be a static method to compile as an static methods!");
                        var (body, parameters) = this.Compile(
                            typeof(T).GetTypeInfo(),
                            typeof(T2).GetTypeInfo(),
                            typeof(T3).GetTypeInfo(),
                            typeof(T4).GetTypeInfo(),
                            typeof(T5).GetTypeInfo(),
                            typeof(T6).GetTypeInfo(),
                            typeof(T7).GetTypeInfo(),
                            typeof(T8).GetTypeInfo(),
                            typeof(T9).GetTypeInfo(),
                            typeof(T10).GetTypeInfo(),
                            typeof(T11).GetTypeInfo(),
                            typeof(T12).GetT
                        ypeInfo()
                            );
                        var lambda = Expression.Lambda<Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(body, parameters);
                        return lambda.CompileFast<Action<IServiceProvider, T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>();
                    }
                }
