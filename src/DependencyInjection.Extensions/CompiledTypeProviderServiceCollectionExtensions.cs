using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extension methods for the service collection using the compiled type provider
/// </summary>
public static class CompiledTypeProviderServiceCollectionExtensions
{
    /// <summary>
    ///     Adds all the services with the <see cref="ServiceRegistrationAttribute" /> to the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static IServiceCollection AddCompiledServiceRegistrations(this IServiceCollection services, ICompiledTypeProvider provider)
    {
        return provider.Scan(
            services,
            z => z
                .FromAssemblies()
                .AddClasses(
                     f => f
                        .WithAnyAttribute(
                             typeof(ServiceRegistrationAttribute),
                             typeof(ServiceRegistrationAttribute<,>),
                             typeof(ServiceRegistrationAttribute<,,>),
                             typeof(ServiceRegistrationAttribute<,,>),
                             typeof(ServiceRegistrationAttribute<,,,>)
                         )
                 )
                .AsSelf()
                .WithSingletonLifetime()
        );
    }
}
