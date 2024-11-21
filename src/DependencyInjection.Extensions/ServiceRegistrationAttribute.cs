using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection.Compiled;

// ReSharper disable MemberCanBePrivate.Global
namespace Rocket.Surgery.DependencyInjection;

/// <summary>
/// Defines the lifetime that should be used service registrations created using the <see cref="ICompiledTypeProvider"/>
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
#pragma warning disable CA1813, CA1019
public class RegistrationLifetimeAttribute : Attribute
{
    /// <summary>
    ///     Constructor to specify the lifetime
    /// </summary>
    /// <param name="lifetime"></param>
    public RegistrationLifetimeAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    /// <summary>
    /// The lifetime
    /// </summary>
    public ServiceLifetime Lifetime { get; }
}

/// <summary>
///     Attribute used to define the service registration of a given type
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#pragma warning disable CA1813, CA1019
public class ServiceRegistrationAttribute : Attribute
{
    /// <summary>
    ///     Constructor to specify the service type including optional runtime
    /// </summary>
    /// <remarks>The default lifetime is <see cref="ServiceLifetime.Singleton"/></remarks>
    /// <param name="serviceTypes"></param>
    public ServiceRegistrationAttribute(params Type[] serviceTypes) : this(ServiceLifetime.Singleton, serviceTypes) { }

    /// <summary>
    ///     Constructor to specify the service type including optional runtime
    /// </summary>
    /// <param name="serviceTypes"></param>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime, params Type[] serviceTypes)
    {
        ServiceTypes = [..serviceTypes];
        Lifetime = lifetime;
    }

    /// <summary>
    ///     The service type
    /// </summary>
    public ImmutableArray<Type> ServiceTypes { get; }

    /// <summary>
    ///     The lifetime
    /// </summary>
    public ServiceLifetime Lifetime { get; }
}

/// <summary>
///    Attribute used to define the service registration of a given type
/// </summary>
/// <typeparam name="TService"></typeparam>
[PublicAPI]
public sealed class ServiceRegistrationAttribute<TService> : ServiceRegistrationAttribute
{
    /// <summary>
    ///     Constructor to specify the lifetime
    /// </summary>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) : base(lifetime, typeof(TService)) { }
}

/// <summary>
///    Attribute used to define the service registration of a given type
/// </summary>
/// <typeparam name="TService1"></typeparam>
/// <typeparam name="TService2"></typeparam>
[PublicAPI]
public sealed class ServiceRegistrationAttribute<TService1, TService2> : ServiceRegistrationAttribute
{
    /// <summary>
    ///     Constructor to specify the lifetime
    /// </summary>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) : base(lifetime, typeof(TService1), typeof(TService2)) { }
}

/// <summary>
///    Attribute used to define the service registration of a given type
/// </summary>
/// <typeparam name="TService1"></typeparam>
/// <typeparam name="TService2"></typeparam>
/// <typeparam name="TService3"></typeparam>
[PublicAPI]
public sealed class ServiceRegistrationAttribute<TService1, TService2, TService3> : ServiceRegistrationAttribute
{
    /// <summary>
    ///     Constructor to specify the lifetime
    /// </summary>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) : base(
        lifetime,
        typeof(TService1),
        typeof(TService2),
        typeof(TService3)
    ) { }
}

/// <summary>
///    Attribute used to define the service registration of a given type
/// </summary>
/// <typeparam name="TService1"></typeparam>
/// <typeparam name="TService2"></typeparam>
/// <typeparam name="TService3"></typeparam>
/// <typeparam name="TService4"></typeparam>
[PublicAPI]
public sealed class ServiceRegistrationAttribute<TService1, TService2, TService3, TService4> : ServiceRegistrationAttribute
{
    /// <summary>
    ///     Constructor to specify the lifetime
    /// </summary>
    /// <param name="lifetime"></param>
    public ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) : base(
        lifetime,
        typeof(TService1),
        typeof(TService2),
        typeof(TService3),
        typeof(TService4)
    ) { }
}
