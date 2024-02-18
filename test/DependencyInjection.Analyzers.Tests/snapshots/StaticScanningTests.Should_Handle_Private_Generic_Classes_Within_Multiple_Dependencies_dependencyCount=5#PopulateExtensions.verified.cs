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
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IRequestHandler<Dependency1Project.Request0, Dependency1Project.Response0>), typeof(Dependency1Project.RequestHandler0), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IRequestHandler<, >).MakeGenericType(context.LoadFromAssemblyName(Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Request1"), context.LoadFromAssemblyName(Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Response1")), context.LoadFromAssemblyName(Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.RequestHandler1"), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IRequestHandler<Dependency1Project.Request2, Dependency1Project.Response2>), typeof(Dependency1Project.RequestHandler2), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IRequestHandler<, >).MakeGenericType(context.LoadFromAssemblyName(Dependency3ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Request3"), context.LoadFromAssemblyName(Dependency3ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.Response3")), context.LoadFromAssemblyName(Dependency3ProjectVersion0000CultureneutralPublicKeyTokennull).GetType("Dependency1Project.RequestHandler3"), ServiceLifetime.Singleton));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IRequestHandler<Dependency1Project.Request4, Dependency1Project.Response4>), typeof(Dependency1Project.RequestHandler4), ServiceLifetime.Singleton));
                    break;
            }

            return services;
        }

        private static AssemblyName _Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull => _Dependency1ProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("Dependency1Project, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static AssemblyName _Dependency3ProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName Dependency3ProjectVersion0000CultureneutralPublicKeyTokennull => _Dependency3ProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("Dependency3Project, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    }
}
#pragma warning restore CS0436
