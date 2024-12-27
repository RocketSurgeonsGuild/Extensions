using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public record GeneratorTestResultsWithServices(GeneratorTestResults Results, IEnumerable<ServiceDescriptor> Services);
public record GeneratorTestResultsWithCacheFiles(GeneratorTestResults Results, string TempDirectory);
