using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection
{
    /// <summary>
    /// Scoped service extensions
    /// </summary>
    public static class ScopedServiceOptionalExtensions
    {
        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T" />
        public static IExecuteScopedOptional<T> WithScopedOptional<T>(this IServiceScopeFactory serviceScopeFactory)
            where T : notnull => new ExecuteScopedOptional<T>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T" />
        public static IExecuteScopedOptional<T> WithScopedOptional<T>(this IServiceProvider serviceProvider)
            where T : notnull => new ExecuteScopedOptional<T>(serviceProvider.GetRequiredService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        public static IExecuteScopedOptional<T1, T2> WithScopedOptional<T1, T2>(this IServiceScopeFactory serviceScopeFactory)
            where T1 : notnull
            where T2 : notnull => new ExecuteScopedOptional<T1, T2>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        public static IExecuteScopedOptional<T1, T2> WithScopedOptional<T1, T2>(this IServiceProvider serviceProvider)
            where T1 : notnull
            where T2 : notnull => new ExecuteScopedOptional<T1, T2>(serviceProvider.GetRequiredService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        public static IExecuteScopedOptional<T1, T2, T3> WithScopedOptional<T1, T2, T3>(this IServiceScopeFactory serviceScopeFactory)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull => new ExecuteScopedOptional<T1, T2, T3>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        public static IExecuteScopedOptional<T1, T2, T3> WithScopedOptional<T1, T2, T3>(this IServiceProvider serviceProvider)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull => new ExecuteScopedOptional<T1, T2, T3>(serviceProvider.GetRequiredService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        public static IExecuteScopedOptional<T1, T2, T3, T4> WithScopedOptional<T1, T2, T3, T4>(this IServiceScopeFactory serviceScopeFactory)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull => new ExecuteScopedOptional<T1, T2, T3, T4>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        public static IExecuteScopedOptional<T1, T2, T3, T4> WithScopedOptional<T1, T2, T3, T4>(this IServiceProvider serviceProvider)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull => new ExecuteScopedOptional<T1, T2, T3, T4>(serviceProvider.GetRequiredService<IServiceScopeFactory>());

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        public static IExecuteScopedOptional<T1, T2, T3, T4, T5> WithScopedOptional<T1, T2, T3, T4, T5>(this IServiceScopeFactory serviceScopeFactory)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull => new ExecuteScopedOptional<T1, T2, T3, T4, T5>(serviceScopeFactory);

        /// <summary>
        /// Get a service that runs actions with a new scope every time
        /// </summary>
        /// <param name="serviceProvider">The service scope factory</param>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        public static IExecuteScopedOptional<T1, T2, T3, T4, T5> WithScopedOptional<T1, T2, T3, T4, T5>(this IServiceProvider serviceProvider)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull => new ExecuteScopedOptional<T1, T2, T3, T4, T5>(serviceProvider.GetRequiredService<IServiceScopeFactory>());

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
        public static IExecuteScopedOptional<T1, T2, T3, T4, T5, T6> WithScopedOptional<T1, T2, T3, T4, T5, T6>(this IServiceScopeFactory serviceScopeFactory)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
            where T6 : notnull => new ExecuteScopedOptional<T1, T2, T3, T4, T5, T6>(serviceScopeFactory);

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
        public static IExecuteScopedOptional<T1, T2, T3, T4, T5, T6> WithScopedOptional<T1, T2, T3, T4, T5, T6>(this IServiceProvider serviceProvider)
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
            where T6 : notnull => new ExecuteScopedOptional<T1, T2, T3, T4, T5, T6>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }
}