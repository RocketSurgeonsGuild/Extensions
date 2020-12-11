using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Rocket.Surgery.DependencyInjection
{
    /// <summary>
    /// Invoke the specified action with the scoped dependency
    /// </summary>
    public interface IExecuteScopedOptional<T>
        where T : notnull
    {
        /// <summary>
        /// Invoke the specified action with the scoped dependency
        /// </summary>
        void Invoke(Action<T?> action);

        /// <summary>
        /// Invoke the specified action returning the given result with the scoped dependency
        /// </summary>
        TResult Invoke<TResult>(Func<T?, TResult> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T?, Task> action);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependency
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T?, Task<TResult>> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T?, CancellationToken, Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependency
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Execution of a set of dependencies from a scope that is automatically disposed after execution
    /// </summary>
    public interface IExecuteScopedOptional<T1, T2>
        where T1 : notnull
        where T2 : notnull
    {
        /// <summary>
        /// Invoke the specified action with the scoped dependencies
        /// </summary>
        void Invoke(Action<T1?, T2?> action);

        /// <summary>
        /// Invoke the specified action returning the given result with the scoped dependencies
        /// </summary>
        TResult Invoke<TResult>(Func<T1?, T2?, TResult> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, Task> action);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, Task<TResult>> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, CancellationToken, Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Execution of a set of dependencies from a scope that is automatically disposed after execution
    /// </summary>
    public interface IExecuteScopedOptional<T1, T2, T3>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        /// <summary>
        /// Invoke the specified action with the scoped dependencies
        /// </summary>
        void Invoke(Action<T1?, T2?, T3?> action);

        /// <summary>
        /// Invoke the specified action returning the given result with the scoped dependencies
        /// </summary>
        TResult Invoke<TResult>(Func<T1?, T2?, T3?, TResult> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, Task> action);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, Task<TResult>> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, CancellationToken, Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Execution of a set of dependencies from a scope that is automatically disposed after execution
    /// </summary>
    public interface IExecuteScopedOptional<T1, T2, T3, T4>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        /// <summary>
        /// Invoke the specified action with the scoped dependencies
        /// </summary>
        void Invoke(Action<T1?, T2?, T3?, T4?> action);

        /// <summary>
        /// Invoke the specified action returning the given result with the scoped dependencies
        /// </summary>
        TResult Invoke<TResult>(Func<T1?, T2?, T3?, T4?, TResult> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, T4?, Task> action);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, Task<TResult>> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, T4?, CancellationToken, Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Execution of a set of dependencies from a scope that is automatically disposed after execution
    /// </summary>
    public interface IExecuteScopedOptional<T1, T2, T3, T4, T5>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        /// <summary>
        /// Invoke the specified action with the scoped dependencies
        /// </summary>
        void Invoke(Action<T1?, T2?, T3?, T4?, T5?> action);

        /// <summary>
        /// Invoke the specified action returning the given result with the scoped dependencies
        /// </summary>
        TResult Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, TResult> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, Task> action);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, Task<TResult>> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, CancellationToken, Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Execution of a set of dependencies from a scope that is automatically disposed after execution
    /// </summary>
    public interface IExecuteScopedOptional<T1, T2, T3, T4, T5, T6>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
    {
        /// <summary>
        /// Invoke the specified action with the scoped dependencies
        /// </summary>
        void Invoke(Action<T1?, T2?, T3?, T4?, T5?, T6?> action);

        /// <summary>
        /// Invoke the specified action returning the given result with the scoped dependencies
        /// </summary>
        TResult Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, T6?, TResult> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, T6?, Task> action);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, T6?, Task<TResult>> action);

        /// <summary>
        /// Invoke the specified async action with the scoped dependency
        /// </summary>
        Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, T6?, CancellationToken, Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoke the specified async action returning the given result with with the scoped dependencies
        /// </summary>
        Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, T6?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }
}