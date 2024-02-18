//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/PopulateExtensions.cs
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

#pragma warning disable CS0436
namespace Rocket.Surgery.DependencyInjection.Compiled
{
    [CompilerGenerated, ExcludeFromCodeCoverage]
    internal static class PopulateExtensions
    {
        public static IServiceCollection Populate(IServiceCollection services, RegistrationStrategy strategy, AssemblyLoadContext context, string filePath, string memberName, int lineNumber)
        {
            switch (lineNumber)
            {
                case 17:
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(DependencyProjectB.ServiceB), typeof(DependencyProjectB.ServiceB), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(DependencyProjectB.IServiceB), _ => _.GetRequiredService<DependencyProjectB.ServiceB>(), ServiceLifetime.Singleton));
                    break;
            }

            return services;
        }
    }
}
#pragma warning restore CS0436
