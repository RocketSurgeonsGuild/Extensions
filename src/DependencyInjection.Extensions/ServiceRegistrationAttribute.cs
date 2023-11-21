using Microsoft.Extensions.DependencyInjection;
using Scrutor;

// ReSharper disable MemberCanBePrivate.Global
namespace Rocket.Surgery.DependencyInjection;

/// <summary>
///     Attribute used to define the service registration of a given type
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ServiceRegistrationAttribute : Attribute
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    public ServiceRegistrationAttribute() : this(null)
    {
    }

    /// <summary>
    ///     Constructor to specify the lifetime
    /// </summary>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime) : this(null, lifetime)
    {
    }

    /// <summary>
    ///     Constructor to specify the service type including optional runtime
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(Type? serviceType, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    /// <summary>
    ///     The service type
    /// </summary>
    public Type? ServiceType { get; }

    /// <summary>
    ///     The lifetime
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    ///     All the defined services types for the given type.
    /// </summary>
    /// <param name="fallbackType"></param>
    /// <returns></returns>
    public IEnumerable<Type> GetServiceTypes(Type fallbackType)
    {
        return new ServiceDescriptorAttribute(ServiceType, Lifetime).GetServiceTypes(fallbackType);
    }
}
