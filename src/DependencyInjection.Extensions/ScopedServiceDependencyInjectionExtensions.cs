using Rocket.Surgery.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Allows injection of IExecuteScoped services
/// </summary>
public static class ScopedServiceDependencyInjectionExtensions
{
    /// <summary>
    ///     Adds scoped execution services
    /// </summary>
    public static IServiceCollection AddExecuteScopedServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
              .AddTransient(typeof(IExecuteScoped<>), typeof(ExecuteScoped<>))
              .AddTransient(typeof(IExecuteScoped<,>), typeof(ExecuteScoped<,>))
              .AddTransient(typeof(IExecuteScoped<,,>), typeof(ExecuteScoped<,,>))
              .AddTransient(typeof(IExecuteScoped<,,,>), typeof(ExecuteScoped<,,,>))
              .AddTransient(typeof(IExecuteScoped<,,,,>), typeof(ExecuteScoped<,,,,>))
              .AddTransient(typeof(IExecuteScoped<,,,,,>), typeof(ExecuteScoped<,,,,,>))
              .AddTransient(typeof(IExecuteScopedOptional<>), typeof(ExecuteScopedOptional<>))
              .AddTransient(typeof(IExecuteScopedOptional<,>), typeof(ExecuteScopedOptional<,>))
              .AddTransient(typeof(IExecuteScopedOptional<,,>), typeof(ExecuteScopedOptional<,,>))
              .AddTransient(typeof(IExecuteScopedOptional<,,,>), typeof(ExecuteScopedOptional<,,,>))
              .AddTransient(typeof(IExecuteScopedOptional<,,,,>), typeof(ExecuteScopedOptional<,,,,>))
              .AddTransient(typeof(IExecuteScopedOptional<,,,,,>), typeof(ExecuteScopedOptional<,,,,,>));
    }
}
