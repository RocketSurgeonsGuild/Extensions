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
                case 11:
                    strategy.Apply(services, ServiceDescriptor.Describe(context.LoadFromAssemblyName(RootDependencyProjectVersion0000CultureneutralPublicKeyTokennull).GetType("RootDependencyProject.Service`1"), context.LoadFromAssemblyName(RootDependencyProjectVersion0000CultureneutralPublicKeyTokennull).GetType("RootDependencyProject.Service`1"), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(RootDependencyProject.IService<>), context.LoadFromAssemblyName(RootDependencyProjectVersion0000CultureneutralPublicKeyTokennull).GetType("RootDependencyProject.Service`1"), ServiceLifetime.Scoped));
                    break;
            }

            return services;
        }

        private static AssemblyName _RootDependencyProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName RootDependencyProjectVersion0000CultureneutralPublicKeyTokennull => _RootDependencyProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("RootDependencyProject, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    }
}
#pragma warning restore CS0436
