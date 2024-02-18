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
                case 14:
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IService), context.LoadFromAssemblyName(Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Service0"), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IService), context.LoadFromAssemblyName(Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Service1"), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IService), context.LoadFromAssemblyName(Dependency2ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Service2"), ServiceLifetime.Singleton));
                    break;
            }

            return services;
        }

        private static AssemblyName _Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull => _Dependency0ProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("Dependency0Project, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static AssemblyName _Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull => _Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("Dependency1Project, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static AssemblyName _Dependency2ProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName Dependency2ProjectVersion0000CultureneutralPublicKeyTokennull => _Dependency2ProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("Dependency2Project, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    }
}
#pragma warning restore CS0436
