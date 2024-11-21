using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class RegistrationLifetimeAttribute(ServiceLifetime lifetime) : Attribute
{
    public ServiceLifetime Lifetime { get; } = lifetime;
}
