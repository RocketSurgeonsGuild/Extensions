using Rocket.Surgery.Extensions.Testing;
using Serilog;
using Serilog.Sinks.Spectre;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

#pragma warning disable CS0436
public static class Defaults
{
    public static void LoggerTest<T>(T context, LoggerConfiguration configuration) where T : ILoggingTestContext
    {
        configuration.WriteTo.Spectre(RocketSurgeonsTestingDefaults.LogFormat);
    }
}
