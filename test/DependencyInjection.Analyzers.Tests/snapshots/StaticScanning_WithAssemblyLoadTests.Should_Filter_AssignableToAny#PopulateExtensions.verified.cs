﻿//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/PopulateExtensions.cs
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

#pragma warning disable CS0436
namespace Rocket.Surgery.DependencyInjection.Compiled
{
    [CompilerGenerated, ExcludeFromCodeCoverage]
    internal static class PopulateExtensions
    {
        public static IServiceCollection Populate(IServiceCollection services, RegistrationStrategy strategy, string filePath, string memberName, int lineNumber)
        {
            switch (lineNumber)
            {
                case 16:
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(Service), typeof(Service), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(IService), _ => _.GetRequiredService<Service>(), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(IServiceB), _ => _.GetRequiredService<Service>(), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(ServiceA), typeof(ServiceA), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(IService), _ => _.GetRequiredService<ServiceA>(), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(ServiceB), typeof(ServiceB), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(IService), _ => _.GetRequiredService<ServiceB>(), ServiceLifetime.Scoped));
                    break;
            }

            return services;
        }
    }
}
#pragma warning restore CS0436
