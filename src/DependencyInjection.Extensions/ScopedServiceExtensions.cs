using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection
{
    /// <summary>
    /// Scoped service extensions
    /// </summary>
    public static class ScopedServiceExtensions
    {
        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T" />
        public static IExecuteScoped<T> Scoped<T>(this IServiceScopeFactory serviceScopeFactory) =>
            new ExecuteScoped<T>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T" />
        public static IExecuteScoped<T> Scoped<T>(this IServiceProvider serviceProvider) =>
            new ExecuteScoped<T>(serviceProvider.GetService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        public static IExecuteScoped<T1, T2> Scoped<T1, T2>(this IServiceScopeFactory serviceScopeFactory) =>
            new ExecuteScoped<T1, T2>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        public static IExecuteScoped<T1, T2> Scoped<T1, T2>(this IServiceProvider serviceProvider) =>
            new ExecuteScoped<T1, T2>(serviceProvider.GetService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        public static IExecuteScoped<T1, T2, T3> Scoped<T1, T2, T3>(this IServiceScopeFactory serviceScopeFactory) =>
            new ExecuteScoped<T1, T2, T3>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        public static IExecuteScoped<T1, T2, T3> Scoped<T1, T2, T3>(this IServiceProvider serviceProvider) =>
            new ExecuteScoped<T1, T2, T3>(serviceProvider.GetService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        public static IExecuteScoped<T1, T2, T3, T4> Scoped<T1, T2, T3, T4>(this IServiceScopeFactory serviceScopeFactory) =>
            new ExecuteScoped<T1, T2, T3, T4>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        public static IExecuteScoped<T1, T2, T3, T4> Scoped<T1, T2, T3, T4>(this IServiceProvider serviceProvider) =>
            new ExecuteScoped<T1, T2, T3, T4>(serviceProvider.GetService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        public static IExecuteScoped<T1, T2, T3, T4, T5> Scoped<T1, T2, T3, T4, T5>(this IServiceScopeFactory serviceScopeFactory) =>
            new ExecuteScoped<T1, T2, T3, T4, T5>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        public static IExecuteScoped<T1, T2, T3, T4, T5> Scoped<T1, T2, T3, T4, T5>(this IServiceProvider serviceProvider) =>
            new ExecuteScoped<T1, T2, T3, T4, T5>(serviceProvider.GetService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        /// <typeparam name="T6" />
        public static IExecuteScoped<T1, T2, T3, T4, T5, T6> Scoped<T1, T2, T3, T4, T5, T6>(this IServiceScopeFactory serviceScopeFactory) =>
            new ExecuteScoped<T1, T2, T3, T4, T5, T6>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        /// <typeparam name="T6" />
        public static IExecuteScoped<T1, T2, T3, T4, T5, T6> Scoped<T1, T2, T3, T4, T5, T6>(this IServiceProvider serviceProvider) =>
            new ExecuteScoped<T1, T2, T3, T4, T5, T6>(serviceProvider.GetService<IServiceScopeFactory>());

    }
}