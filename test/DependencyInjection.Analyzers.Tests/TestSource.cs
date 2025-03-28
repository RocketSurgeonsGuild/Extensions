using System.Diagnostics;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Tests;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial record TestSource(string Name, string Source)
{
    public string GetTempDirectory(string? suffix = null) => (
            suffix is { }
                ? Path.Combine(ModuleInitializer.TempDirectory, FileSafeName, suffix)
                : Path.Combine(ModuleInitializer.TempDirectory, FileSafeName)
        )
       .Replace("\\", "/");

    /// <inheritdoc />
    public override string ToString() => AssemblyScanningTestData.GenerateFilenameSafeString(Name);

    public string FileSafeName => AssemblyScanningTestData.GenerateFilenameSafeString(AssemblyScanningTestData.HashFilename(Name, Source));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => Source;
}
