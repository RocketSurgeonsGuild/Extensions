using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection;

/// <summary>
///     Scoped service extensions
/// </summary>
public static class ScopedServiceExtensions
{
    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <typeparam name="T" />
    public static IExecuteScoped<T> WithScoped<T>(this IServiceScopeFactory serviceScopeFactory)
        where T : notnull
    {
        return new ExecuteScoped<T>(serviceScopeFactory);
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceProvider">The service scope factory</param>
    /// <typeparam name="T" />
    public static IExecuteScoped<T> WithScoped<T>(this IServiceProvider serviceProvider)
        where T : notnull
    {
        return new ExecuteScoped<T>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    public static IExecuteScoped<T1, T2> WithScoped<T1, T2>(this IServiceScopeFactory serviceScopeFactory)
        where T1 : notnull
        where T2 : notnull
    {
        return new ExecuteScoped<T1, T2>(serviceScopeFactory);
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceProvider">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    public static IExecuteScoped<T1, T2> WithScoped<T1, T2>(this IServiceProvider serviceProvider)
        where T1 : notnull
        where T2 : notnull
    {
        return new ExecuteScoped<T1, T2>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    public static IExecuteScoped<T1, T2, T3> WithScoped<T1, T2, T3>(this IServiceScopeFactory serviceScopeFactory)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        return new ExecuteScoped<T1, T2, T3>(serviceScopeFactory);
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceProvider">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    public static IExecuteScoped<T1, T2, T3> WithScoped<T1, T2, T3>(this IServiceProvider serviceProvider)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        return new ExecuteScoped<T1, T2, T3>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    /// <typeparam name="T4" />
    public static IExecuteScoped<T1, T2, T3, T4> WithScoped<T1, T2, T3, T4>(this IServiceScopeFactory serviceScopeFactory)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        return new ExecuteScoped<T1, T2, T3, T4>(serviceScopeFactory);
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceProvider">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    /// <typeparam name="T4" />
    public static IExecuteScoped<T1, T2, T3, T4> WithScoped<T1, T2, T3, T4>(this IServiceProvider serviceProvider)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        return new ExecuteScoped<T1, T2, T3, T4>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    /// <typeparam name="T4" />
    /// <typeparam name="T5" />
    public static IExecuteScoped<T1, T2, T3, T4, T5> WithScoped<T1, T2, T3, T4, T5>(this IServiceScopeFactory serviceScopeFactory)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        return new ExecuteScoped<T1, T2, T3, T4, T5>(serviceScopeFactory);
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceProvider">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    /// <typeparam name="T4" />
    /// <typeparam name="T5" />
    public static IExecuteScoped<T1, T2, T3, T4, T5> WithScoped<T1, T2, T3, T4, T5>(this IServiceProvider serviceProvider)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        return new ExecuteScoped<T1, T2, T3, T4, T5>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    /// <typeparam name="T4" />
    /// <typeparam name="T5" />
    /// <typeparam name="T6" />
    public static IExecuteScoped<T1, T2, T3, T4, T5, T6> WithScoped<T1, T2, T3, T4, T5, T6>(this IServiceScopeFactory serviceScopeFactory)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
    {
        return new ExecuteScoped<T1, T2, T3, T4, T5, T6>(serviceScopeFactory);
    }

    /// <summary>
    ///     Get a service that runs actions with a new scope every time
    /// </summary>
    /// <param name="serviceProvider">The service scope factory</param>
    /// <typeparam name="T1" />
    /// <typeparam name="T2" />
    /// <typeparam name="T3" />
    /// <typeparam name="T4" />
    /// <typeparam name="T5" />
    /// <typeparam name="T6" />
    public static IExecuteScoped<T1, T2, T3, T4, T5, T6> WithScoped<T1, T2, T3, T4, T5, T6>(this IServiceProvider serviceProvider)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
    {
        return new ExecuteScoped<T1, T2, T3, T4, T5, T6>(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }
}
