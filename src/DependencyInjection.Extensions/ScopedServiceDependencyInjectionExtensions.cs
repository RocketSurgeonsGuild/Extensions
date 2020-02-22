using System;
using Rocket.Surgery.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows injection of IExecuteScoped services
    /// </summary>
    public static class ScopedServiceDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds scoped execution services
        /// </summary>
        public static IServiceCollection AddExecuteScopedServices(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddTransient(typeof(IExecuteScoped<>), typeof(ExecuteScoped<>))
                .AddTransient(typeof(IExecuteScoped<,>), typeof(ExecuteScoped<,>))
                .AddTransient(typeof(IExecuteScoped<,,>), typeof(ExecuteScoped<,,>))
                .AddTransient(typeof(IExecuteScoped<,,,>), typeof(ExecuteScoped<,,,>))
                .AddTransient(typeof(IExecuteScoped<,,,,>), typeof(ExecuteScoped<,,,,>))
                .AddTransient(typeof(IExecuteScoped<,,,,,>), typeof(ExecuteScoped<,,,,,>));
        }
    }
}