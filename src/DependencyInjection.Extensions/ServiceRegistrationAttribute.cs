using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;
using System.Collections.Generic;

namespace Rocket.Surgery.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServiceRegistrationAttribute : Attribute
    {
        public ServiceRegistrationAttribute() : this(null) { }

        public ServiceRegistrationAttribute(ServiceLifetime lifetime) : this(null, lifetime) { }

        public ServiceRegistrationAttribute(Type? serviceType) : this(serviceType, ServiceLifetime.Transient) { }

        public ServiceRegistrationAttribute(Type? serviceType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
        }

        public Type? ServiceType { get; }

        public ServiceLifetime Lifetime { get; }

        public IEnumerable<Type> GetServiceTypes(Type fallbackType) => new ServiceDescriptorAttribute(ServiceType, Lifetime).GetServiceTypes(fallbackType);
    }
}