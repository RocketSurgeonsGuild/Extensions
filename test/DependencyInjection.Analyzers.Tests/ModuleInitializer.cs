using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DiffEngine;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

internal static partial class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyGeneratorTextContext.Initialize(DiagnosticSeverity.Warning, Customizers.Default, Customizers.ExcludeParseOptions);

        VerifierSettings.AddExtraSettings(
            settings => settings.Converters.Add(new ServiceDescriptorConverter())
        );

        VerifierSettings.RegisterFileConverter<GeneratorTestResultsWithServices>(Convert);

        DiffRunner.Disabled = true;
        DerivePathInfo(
            (sourceFile, projectDirectory, type, method) =>
            {
                static string GetTypeName(Type type) => ( type.IsNested ) ? $"{type.ReflectedType!.Name}.{type.Name}" : type.Name;

                var typeName = GetTypeName(type);

                var path = Path.Combine(Path.GetDirectoryName(sourceFile)!, "snapshots");
                return new(path, typeName, method.Name);
            }
        );
        VerifierSettings.ScrubLinesWithReplace(
            s => ( s.Contains("AssemblyMetadata(\"AssemblyProvider.", StringComparison.OrdinalIgnoreCase) )
                ? s[..( s.IndexOf('"', s.IndexOf('"') + 1) + 2 )] + "\"{scrubbed}\")]"
                : s
        );
        VerifierSettings.ScrubLinesWithReplace(
            s => ( s.Contains("<Compiled_AssemblyProvider_g>", StringComparison.OrdinalIgnoreCase) )
                ? s[..( s.IndexOf('"') + 1 )] + "{CompiledTypeProvider}" + s[s.LastIndexOf('"')..]
                : s
        );
        VerifierSettings.AddScrubber(
            (builder, counter) =>
            {
                if (typeof(CompiledServiceScanningGenerator).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>() is
                    { Version: { Length: > 0 } version })
                {
                    builder.Replace(version, "version");
                }

                if (typeof(CompiledServiceScanningGenerator).Assembly.GetCustomAttribute<AssemblyVersionAttribute>() is { Version: { Length: > 0 } version2 })
                {
                    builder.Replace(version2, "version");
                }

                // regex to replace the version number in this string Version=12.0.0.0,
                var regex = MyRegex();
                var result = regex.Replace(builder.ToString(), "Version=version,");
                builder.Clear();
                builder.Append(result);
            }
        );
    }

    internal static IEnumerable<Diagnostic> OrderDiagnosticResults(this IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity) => diagnostics
       .Where(s => s.Severity >= severity)
       .OrderBy(static z => z.Location.GetMappedLineSpan().ToString())
       .ThenBy(static z => z.Severity)
       .ThenBy(static z => z.Id);


    private static ConversionResult Convert(GeneratorTestResultsWithServices result, IReadOnlyDictionary<string, object> context)
    {
        var target = result.Results;
        var services = result.Services;
        var targets = new List<Target>();
        foreach (var item in target.Results)
        {
            targets.AddRange(item.Value.SyntaxTrees.Select(Selector));
        }

        var data = new Dictionary<string, object>
        {
            ["GlobalOptions"] = target.GlobalOptions,
            ["FileOptions"] = target.FileOptions,
            ["References"] = target
                            .FinalCompilation
                            .References
                            .Select(x => x.Display ?? "")
                            .Select(Path.GetFileName)
                            .Distinct()
                            .Order(),

            ["FinalDiagnostics"] = target.FinalDiagnostics.OrderDiagnosticResults(DiagnosticSeverity.Error),
            ["GeneratorDiagnostics"] = target.Results.ToDictionary(
                z => z.Key.FullName!,
                z => z.Value.Diagnostics.OrderDiagnosticResults(DiagnosticSeverity.Error)
            ),
            ["Services"] = services
        };

        return new(data, targets);
    }

    private static Target Selector(SyntaxTree source)
    {
        var hintPath = source.FilePath;
        var data = $@"//HintName: {hintPath.Replace("\\", "/")}
{source.GetText()}";
        return new("cs", data.Replace("\r", "", StringComparison.OrdinalIgnoreCase), Path.GetFileNameWithoutExtension(hintPath));
    }

    private class ServiceDescriptorConverter : WriteOnlyJsonConverter<ServiceDescriptor>
    {
        public override void Write(VerifyJsonWriter writer, ServiceDescriptor value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(ServiceDescriptor.Lifetime));
            writer.WriteValue(value.Lifetime.ToString());
            writer.WritePropertyName(nameof(ServiceDescriptor.ServiceType));
            writer.WriteValue(value.ServiceType.FullName);
            if (value.IsKeyedService)
            {
                writer.WritePropertyName(nameof(ServiceDescriptor.ServiceKey));
                writer.WriteValue(value.ServiceKey);
                if (value.KeyedImplementationType?.FullName is { } implementationType)
                {
                    writer.WritePropertyName(nameof(ServiceDescriptor.KeyedImplementationType));
                    writer.WriteValue(implementationType);
                }
                else if (value.KeyedImplementationInstance is { } implementationInstance)
                {
                    writer.WritePropertyName(nameof(ServiceDescriptor.KeyedImplementationInstance));
                    writer.WriteValue(implementationInstance.ToString());
                }
                else if (value.KeyedImplementationFactory is { } implementationFactory)
                {
                    writer.WritePropertyName(nameof(ServiceDescriptor.KeyedImplementationFactory));
                    writer.WriteValue(implementationFactory.ToString());
                }
            }
            else
            {
                if (value.ImplementationType?.FullName is { } implementationType)
                {
                    writer.WritePropertyName(nameof(ServiceDescriptor.ImplementationType));
                    writer.WriteValue(implementationType);
                }
                else if (value.ImplementationInstance is { } implementationInstance)
                {
                    writer.WritePropertyName(nameof(ServiceDescriptor.ImplementationInstance));
                    writer.WriteValue(implementationInstance.ToString());
                }
                else if (value.ImplementationFactory is { } implementationFactory)
                {
                    writer.WritePropertyName(nameof(ServiceDescriptor.ImplementationFactory));
                    writer.WriteValue(implementationFactory.ToString());
                }
            }

            writer.WriteEndObject();
        }
    }

    [GeneratedRegex("Version=(.*?),", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
