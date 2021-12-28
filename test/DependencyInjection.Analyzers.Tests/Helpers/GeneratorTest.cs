using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing;
using Scrutor;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests.Helpers;

public static class Helpers
{
    public static Assembly EmitInto(this CSharpCompilation compilation, AssemblyLoadContext context, string? outputName = null)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream, options: new EmitOptions(outputNameOverride: outputName));
        if (!emitResult.Success)
        {
            Assert.Empty(emitResult.Diagnostics);
        }

        var data = stream.ToArray();

        using var assemblyStream = new MemoryStream(data);
        return context.LoadFromStream(assemblyStream);
    }

    public static MetadataReference CreateMetadataReference(this CSharpCompilation compilation)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream, options: new EmitOptions(outputNameOverride: compilation.AssemblyName));
        if (!emitResult.Success)
        {
            Assert.Empty(emitResult.Diagnostics);
        }

        var data = stream.ToArray();

        using var assemblyStream = new MemoryStream(data);
        return MetadataReference.CreateFromStream(assemblyStream, MetadataReferenceProperties.Assembly);
    }
}

public abstract class GeneratorTest : GeneratorTester
{
    protected GeneratorTest(ITestOutputHelper testOutputHelper, LogLevel minLevel) : base(
        GenerationHelpers.TestProjectName,
        new CollectibleTestAssemblyLoadContext(),
        testOutputHelper,
        minLevel
    )
    {
        Disposables.Add(( AssemblyLoadContext as IDisposable )!);
    }
}

public class GeneratorTester : LoggerTest
{
    private readonly string _projectName;
    protected AssemblyLoadContext AssemblyLoadContext { get; }
    private readonly HashSet<MetadataReference> _metadataReferences = new(global::Castle.Core.ReferenceEqualityComparer<MetadataReference>.Instance);
    private readonly HashSet<Type> _generators = new();
    private readonly List<SourceText> _sources = new();
    private GenerationTestResults? _lastResult;
    private readonly HashSet<string> _ignoredFilePaths = new();
    private readonly OptionsProvider _optionsProvider = new OptionsProvider();

    public GeneratorTester(
        string projectName, AssemblyLoadContext assemblyLoadContext, ITestOutputHelper testOutputHelper, LogLevel minLevel = LogLevel.Trace
    ) : base(
        testOutputHelper,
        minLevel
    )
    {
        _projectName = projectName;
        AssemblyLoadContext = assemblyLoadContext;
        AddReferences(
            "mscorlib.dll",
            "netstandard.dll",
            "System.dll",
            "System.Core.dll",
#if NETCOREAPP
            "System.Private.CoreLib.dll",
#endif
            "System.Runtime.dll"
        );

        AddReferences(
            typeof(ActivatorUtilities).Assembly,
            typeof(IServiceProvider).Assembly,
            typeof(IFluentInterface).Assembly,
            typeof(IServiceCollection).Assembly,
            typeof(ServiceCollection).Assembly,
            typeof(ServiceRegistrationAttribute).Assembly
        );
    }

    public GeneratorTester AddCompilationReference(params CSharpCompilation[] additionalCompilations)
    {
        AddReferences(additionalCompilations.Select(z => z.CreateMetadataReference()).ToArray());
        return this;
    }

    public GeneratorTester IgnoreOutputFile(string path)
    {
        _ignoredFilePaths.Add(path);
        return this;
    }

    public GeneratorTester WithGenerator(Type type)
    {
        _generators.Add(type);
        return this;
    }

    public GeneratorTester WithGenerator<T>()
        where T : ISourceGenerator, new()
    {
        _generators.Add(typeof(T));
        return this;
    }

    public GeneratorTester AddReferences(params string[] coreAssemblyNames)
    {
        // this "core assemblies hack" is from https://stackoverflow.com/a/47196516/4418060
        var coreAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        foreach (var name in coreAssemblyNames)
        {
            _metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(coreAssemblyPath, name)));
        }

        return this;
    }

    public GeneratorTester AddReferences(params MetadataReference[] references)
    {
        foreach (var metadataReference in references)
        {
            _metadataReferences.Add(metadataReference);
        }

        return this;
    }

    public GeneratorTester AddReferences(params Type[] references)
    {
        foreach (var type in references)
        {
            _metadataReferences.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
        }

        return this;
    }

    public GeneratorTester AddReferences(params Assembly[] references)
    {
        foreach (var type in references)
        {
            _metadataReferences.Add(MetadataReference.CreateFromFile(type.Location));
        }

        return this;
    }

    public GeneratorTester AddSources(params SourceText[] additionalSources)
    {
        _sources.AddRange(additionalSources);
        return this;
    }

    public GeneratorTester AddSources(params string[] additionalSources)
    {
        _sources.AddRange(additionalSources.Select(s => SourceText.From(s, Encoding.UTF8)));
        return this;
    }

    public GeneratorTester AddGlobalOption(string key, string value)
    {
        _optionsProvider.AddGlobalOption(key, value);
        return this;
    }

    public GeneratorTester AddOption(SyntaxTree tree, string value)
    {
        _optionsProvider.AddOption(tree, value);
        return this;
    }

    public GeneratorTester AddOption(AdditionalText key, string value)
    {
        _optionsProvider.AddOption(key, value);
        return this;
    }

    public CSharpCompilation Compile()
    {
        return GenerateAsync().ConfigureAwait(false).GetAwaiter().GetResult().FinalCompilation;
    }

    private Assembly? Emit(CSharpCompilation compilation, string? outputName = null)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(
            stream,
            options: new EmitOptions(outputNameOverride: outputName)
        );
        if (!emitResult.Success)
        {
            return null;
        }

        var data = stream.ToArray();

        using var assemblyStream = new MemoryStream(data);
        return AssemblyLoadContext.LoadFromStream(assemblyStream);
    }

    public async Task<GenerationTestResults> GenerateAsync()
    {
        Logger.LogInformation("Starting Generation for {SourceCount}", _sources.Count);
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("--- References --- {Count}", _sources.Count);
            foreach (var reference in _metadataReferences)
                Logger.LogTrace("    Reference: {Name}", reference.Display);
        }

        var project = GenerationHelpers.CreateProject(_projectName, _metadataReferences, _sources.ToArray());

        var compilation = (CSharpCompilation?)await project.GetCompilationAsync().ConfigureAwait(false);
        if (compilation is null)
        {
            throw new InvalidOperationException("Could not compile the sources");
        }

        var diagnostics = compilation.GetDiagnostics();
        if (Logger.IsEnabled(LogLevel.Trace) && diagnostics is { Length: > 0 })
        {
            Logger.LogTrace("--- Input Diagnostics --- {Count}", _sources.Count);
            foreach (var d in diagnostics)
                Logger.LogTrace("    Reference: {Name}", d.ToString());
        }

        var results = new GenerationTestResults(
            compilation,
            diagnostics,
            compilation.SyntaxTrees,
            ImmutableDictionary<Type, GenerationTestResult>.Empty,
            null!,
            ImmutableArray<Diagnostic>.Empty,
            null!
        );

        var builder = ImmutableDictionary<Type, GenerationTestResult>.Empty.ToBuilder();

        var inputCompilation = compilation;

        foreach (var generatorType in _generators)
        {
            Logger.LogInformation("--- {Generator} ---", generatorType.FullName);
            var generator = ( Activator.CreateInstance(generatorType) as ISourceGenerator )!;
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, optionsProvider: _optionsProvider);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);

            if (Logger.IsEnabled(LogLevel.Trace) && diagnostics is { Length: > 0 })
            {
                Logger.LogTrace("--- Diagnostics --- {Count}", _sources.Count);
                foreach (var d in diagnostics)
                    Logger.LogTrace("    Reference: {Name}", d.ToString());
            }

            var trees = outputCompilation.SyntaxTrees
                                         .Except(compilation.SyntaxTrees)
                                         .ToImmutableArray();
            if (Logger.IsEnabled(LogLevel.Trace) && trees is { Length: > 0 })
            {
                Logger.LogTrace("--- Syntax Trees --- {Count}", _sources.Count);
                foreach (var t in trees)
                {
                    Logger.LogTrace("    FilePath: {Name}", t.FilePath);
                    Logger.LogTrace("    Source:\n{Name}", ( await t.GetTextAsync().ConfigureAwait(false) ).ToString());
                }
            }

            inputCompilation = inputCompilation.AddSyntaxTrees(trees);

            builder.Add(
                generatorType,
                new GenerationTestResult(
                    ( outputCompilation as CSharpCompilation )!,
                    diagnostics,
                    trees.Where(z => !_ignoredFilePaths.Any(x => z.FilePath.Contains(x))).ToImmutableArray(),
                    Logger
                )
            );
        }

        results = results with
        {
            FinalCompilation = inputCompilation,
            FinalDiagnostics = inputCompilation.GetDiagnostics(),
            Assembly = Emit(inputCompilation)
        };

        return _lastResult = results with { Results = builder.ToImmutable() };
    }

    private class OptionsProvider : AnalyzerConfigOptionsProvider
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly Dictionary<string, string> _options = new();
        private readonly Dictionary<string, string> _globalOptions;

        public OptionsProvider()
        {
            _globalOptions = new();
            GlobalOptions = new Options(_globalOptions);
        }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            throw new NotImplementedException();
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            throw new NotImplementedException();
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public void AddOption(SyntaxTree tree, string value)
        {
            _options.Add(tree.FilePath, value);
        }

        public void AddOption(AdditionalText tree, string value)
        {
            _options.Add(tree.Path, value);
        }

        public void AddGlobalOption(string key, string value)
        {
            _globalOptions.Add(key, value);
        }
    }

    private class Options : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _options;

        public Options(IReadOnlyDictionary<string, string> options)
        {
            _options = options;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _options.TryGetValue(key, out value!);
        }
    }
}
