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
                case 15:
                    strategy.Apply(services, ServiceDescriptor.Describe(Assembly.Load(DependencyProjectVersion0000CultureneutralPublicKeyTokennull).GetType("DependencyProject.Service"), Assembly.Load(DependencyProjectVersion0000CultureneutralPublicKeyTokennull).GetType("DependencyProject.Service"), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(DependencyProject.IService), _ => _.GetRequiredService(Assembly.Load(DependencyProjectVersion0000CultureneutralPublicKeyTokennull).GetType("DependencyProject.Service")) as DependencyProject.IService, ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(TestProject.Service), typeof(TestProject.Service), ServiceLifetime.Scoped));
                    strategy.Apply(services, ServiceDescriptor.Describe(typeof(DependencyProject.IService), _ => _.GetRequiredService<TestProject.Service>(), ServiceLifetime.Scoped));
                    break;
            }

            return services;
        }

        private static AssemblyName _DependencyProjectVersion0000CultureneutralPublicKeyTokennull;
        private static AssemblyName DependencyProjectVersion0000CultureneutralPublicKeyTokennull => _DependencyProjectVersion0000CultureneutralPublicKeyTokennull ??= new AssemblyName("DependencyProject, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    }
}
#pragma warning restore CS0436
