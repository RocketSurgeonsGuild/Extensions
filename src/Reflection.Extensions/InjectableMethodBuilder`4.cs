using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;

namespace Rocket.Surgery.Reflection;

/// <summary>
///     Injectable method builder
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="T2">The type of the 2.</typeparam>
/// <typeparam name="T3">The type of the 3.</typeparam>
[PublicAPI]
public class InjectableMethodBuilder<T, T2, T3> : InjectableMethodBuilderBase
{
    internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames) { }

    /// <summary>
    ///     Withes the parameter.
    /// </summary>
    /// <typeparam name="TNext">The type of the next.</typeparam>
    /// <returns></returns>
    public InjectableMethodBuilder<T, T2, T3, TNext> WithParameter<TNext>() => new(Container, MethodNames);

    /// <summary>
    ///     Fors the method.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <returns></returns>
    public InjectableMethodBuilder<T, T2, T3> ForMethod(string methodName) => new(Container, MethodNames.Add(methodName));

    /// <summary>
    ///     Compiles this instance.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns></returns>
    public Func<object, IServiceProvider, T, T2, T3, TResult> Compile<TResult>()
    {
        if (GetMethodInfo()?.IsStatic == true)
            throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
        ( var body, var parameters ) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, T3, TResult>>(body, parameters);
        return lambda.CompileFast();
    }

    /// <summary>
    ///     Compiles this instance.
    /// </summary>
    /// <returns></returns>
    public Action<object, IServiceProvider, T, T2, T3> Compile()
    {
        if (GetMethodInfo()?.IsStatic == true)
            throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
        ( var body, var parameters ) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2, T3>>(body, parameters);
        return lambda.CompileFast();
    }

    /// <summary>
    ///     Compiles the static.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns></returns>
    public Func<IServiceProvider, T, T2, T3, TResult> CompileStatic<TResult>()
    {
        if (GetMethodInfo()?.IsStatic != true)
            throw new NotSupportedException("Method must be a static method to compile as an static methods!");
        ( var body, var parameters ) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, T3, TResult>>(body, parameters);
        return lambda.CompileFast();
    }

    /// <summary>
    ///     Compiles the static.
    /// </summary>
    /// <returns></returns>
    public Action<IServiceProvider, T, T2, T3> CompileStatic()
    {
        if (GetMethodInfo()?.IsStatic != true)
            throw new NotSupportedException("Method must be a static method to compile as an static methods!");
        ( var body, var parameters ) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Action<IServiceProvider, T, T2, T3>>(body, parameters);
        return lambda.CompileFast();
    }
}
