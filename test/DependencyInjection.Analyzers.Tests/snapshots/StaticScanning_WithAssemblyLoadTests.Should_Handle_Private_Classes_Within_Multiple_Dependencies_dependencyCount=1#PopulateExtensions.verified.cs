//HintName: Rocket.Surgery.DependencyInjection.Analyzers/Rocket.Surgery.DependencyInjection.Analyzers.CompiledServiceScanningGenerator/PopulateExtensions.cs
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
                case 14:
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IService), Assembly.Load(Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Service0"), ServiceLifetime.Singleton));
                    break;
            }

            return services;
        }

        private static AssemblyName _Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull => _Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("Dependency0Project, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    }
}
#pragma warning restore CS0436
