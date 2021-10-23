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
/// <typeparam name="T4">The type of the 4.</typeparam>
[PublicAPI]
public class InjectableMethodBuilder<T, T2, T3, T4> : InjectableMethodBuilderBase
{
    internal InjectableMethodBuilder(TypeInfo containerType, ImmutableArray<string> methodNames) : base(containerType.AsType(), methodNames)
    {
    }

    /// <summary>
    ///     Withes the parameter.
    /// </summary>
    /// <typeparam name="TNext">The type of the next.</typeparam>
    /// <returns></returns>
    public InjectableMethodBuilder<T, T2, T3, T4, TNext> WithParameter<TNext>()
    {
        return new InjectableMethodBuilder<T, T2, T3, T4, TNext>(Container, MethodNames);
    }

    /// <summary>
    ///     Fors the method.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <returns></returns>
    public InjectableMethodBuilder<T, T2, T3, T4> ForMethod(string methodName)
    {
        return new InjectableMethodBuilder<T, T2, T3, T4>(Container, MethodNames.Add(methodName));
    }

    /// <summary>
    ///     Compiles this instance.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns></returns>
    public Func<object, IServiceProvider, T, T2, T3, T4, TResult> Compile<TResult>()
    {
        if (GetMethodInfo()?.IsStatic == true)
            throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
        var (body, parameters) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo(),
            typeof(T4).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Func<object, IServiceProvider, T, T2, T3, T4, TResult>>(body, parameters);
        return lambda.CompileFast();
    }

    /// <summary>
    ///     Compiles this instance.
    /// </summary>
    /// <returns></returns>
    public Action<object, IServiceProvider, T, T2, T3, T4> Compile()
    {
        if (GetMethodInfo()?.IsStatic == true)
            throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
        var (body, parameters) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo(),
            typeof(T4).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Action<object, IServiceProvider, T, T2, T3, T4>>(body, parameters);
        return lambda.CompileFast();
    }

    /// <summary>
    ///     Compiles the static.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns></returns>
    public Func<IServiceProvider, T, T2, T3, T4, TResult> CompileStatic<TResult>()
    {
        if (GetMethodInfo()?.IsStatic != true)
            throw new NotSupportedException("Method must be a static method to compile as an static methods!");
        var (body, parameters) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo(),
            typeof(T4).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Func<IServiceProvider, T, T2, T3, T4, TResult>>(body, parameters);
        return lambda.CompileFast();
    }

    /// <summary>
    ///     Compiles the static.
    /// </summary>
    /// <returns></returns>
    public Action<IServiceProvider, T, T2, T3, T4> CompileStatic()
    {
        if (GetMethodInfo()?.IsStatic != true)
            throw new NotSupportedException("Method must be a static method to compile as an static methods!");
        var (body, parameters) = base.Compile(
            typeof(T).GetTypeInfo(),
            typeof(T2).GetTypeInfo(),
            typeof(T3).GetTypeInfo(),
            typeof(T4).GetTypeInfo()
        );
        var lambda = Expression.Lambda<Action<IServiceProvider, T, T2, T3, T4>>(body, parameters);
        return lambda.CompileFast();
    }
}
