using System.Runtime.CompilerServices;
using DiffEngine;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyGeneratorTextContext.Initialize(false, true, DiagnosticSeverity.Error);

        VerifierSettings.AddExtraSettings(
            settings => { settings.Converters.Add(new ServiceDescriptorConverter()); }
        );

        VerifierSettings.RegisterFileConverter<GeneratorTestResultsWithServices>(Convert);

        DiffRunner.Disabled = true;
        DerivePathInfo(
            (sourceFile, projectDirectory, type, method) =>
            {
                static string GetTypeName(Type type)
                {
                    return type.IsNested ? $"{type.ReflectedType!.Name}.{type.Name}" : type.Name;
                }

                var typeName = GetTypeName(type);

                var path = Path.Combine(Path.GetDirectoryName(sourceFile)!, "snapshots");
                return new(path, typeName, method.Name);
            }
        );
    }

    internal static IEnumerable<Diagnostic> OrderDiagnosticResults(this IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity)
    {
        return diagnostics
              .Where(s => s.Severity >= severity)
              .OrderBy(static z => z.Location.GetMappedLineSpan().ToString())
              .ThenBy(static z => z.Severity)
              .ThenBy(static z => z.Id);
    }


    private static ConversionResult Convert(GeneratorTestResultsWithServices result, IReadOnlyDictionary<string, object> context)
    {
        var target = result.Results;
        var services = result.Services;
        var targets = new List<Target>();
        foreach (var item in target.Results)
        {
            targets.AddRange(item.Value.SyntaxTrees.Select(Selector));
        }

        var data = new Dictionary<string, object>();
        // start here
        data["ParseOptions"] = new
        {
            target.ParseOptions.LanguageVersion,
            target.ParseOptions.DocumentationMode,
            target.ParseOptions.Kind,
            target.ParseOptions.Features,
            target.ParseOptions.PreprocessorSymbolNames,
        };

        data["GlobalOptions"] = target.GlobalOptions;
        data["FileOptions"] = target.FileOptions;
        data["References"] = target
                            .FinalCompilation
                            .References
                            .Select(x => x.Display ?? "")
                            .Select(Path.GetFileName)
                            .OrderBy(z => z);

        data["FinalDiagnostics"] = target.FinalDiagnostics.OrderDiagnosticResults(DiagnosticSeverity.Error);
        data["GeneratorDiagnostics"] = target.Results.ToDictionary(
            z => z.Key.FullName!,
            z => z.Value.Diagnostics.OrderDiagnosticResults(DiagnosticSeverity.Error)
        );
        data["Services"] = services;

        return new(data, targets);
    }

    private static Target Selector(SyntaxTree source)
    {
        var hintPath = source.FilePath;
        var data = $@"//HintName: {hintPath.Replace("\\", "/")}
{source.GetText()}";
        return new("cs", data.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase), Path.GetFileNameWithoutExtension(hintPath));
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
}

public record GeneratorTestResultsWithServices(GeneratorTestResults Results, IEnumerable<ServiceDescriptor> Services);