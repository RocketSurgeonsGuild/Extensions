using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke.DotNetCore;
using Serilog;

[PublicAPI]
[UnsetVisualStudioEnvironmentVariables]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
[EnsureGitHooks(GitHook.PreCommit)]
[EnsureReadmeIsUpdated("Readme.md")]
[DotNetVerbosityMapping]
[MSBuildVerbosityMapping]
[NuGetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
public partial class Pipeline : NukeBuild,
                                ICanRestoreWithDotNetCore,
                                ICanBuildWithDotNetCore,
                                ICanTestWithDotNetCore,
                                ICanPackWithDotNetCore,
                                IHaveDataCollector,
                                ICanClean,
                                ICanUpdateReadme,
                                IGenerateCodeCoverageReport,
                                IGenerateCodeCoverageSummary,
                                IGenerateCodeCoverageBadges,
                                IHaveConfiguration<Configuration>
{
    /// <summary>
    ///     Support plugins are available for:
    ///     - JetBrains ReSharper        https://nuke.build/resharper
    ///     - JetBrains Rider            https://nuke.build/rider
    ///     - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///     - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main()
    {
        return Execute<Pipeline>(x => x.Default);
    }

    private Target Default => _ => _
                                  .DependsOn(Restore)
                                  .DependsOn(Build)
                                  .DependsOn(Test)
                                  .DependsOn(Pack);

    public Target Build => _ => _.Inherit<ICanBuildWithDotNetCore>(x => x.CoreBuild);

    public Target Pack => _ => _.Inherit<ICanPackWithDotNetCore>(x => x.CorePack)
                                .DependsOn(Clean);

    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<ICanRestoreWithDotNetCore>(x => x.CoreRestore);
    public Target Test => _ => _.Inherit<ICanTestWithDotNetCore>(x => x.CoreTest);

    public Target BuildVersion => _ => _.Inherit<IHaveBuildVersion>(x => x.BuildVersion)
                                        .Before(Default)
                                        .Before(Clean);

    public Target ShipApis => _ => _.Executes(
        async () =>
        {
            foreach (var project in Solution.AllProjects.Where(z => z.HasPackageReference("Microsoft.CodeAnalysis.PublicApiAnalyzers")))
            {
                Log.Logger.Information(project.Name);
                var dotnetFormat = DotnetTool.GetTool("dotnet-format");
                DotNetTasks.DotNet($"format analyzers --diagnostics=RS0016", project.Directory);
                await MarkShipped(project.Directory);
            }

            static async Task MarkShipped(AbsolutePath directory)
            {
                var shippedFilePath = directory / "PublicAPI.Shipped.txt";
                var shipped = ( File.Exists(shippedFilePath) ? await File.ReadAllLinesAsync(shippedFilePath) : Array.Empty<string>() )
                             .Where(z => z != "#nullable enable")
                             .ToList();
                var unshippedFilePath = directory / "PublicAPI.Unshipped.txt";
                var unshipped = ( File.Exists(unshippedFilePath) ? await File.ReadAllLinesAsync(unshippedFilePath) : Array.Empty<string>() )
                               .Where(z => z != "#nullable enable")
                               .ToList();
                Log.Logger.Information("Processing {Directory}", directory);

                foreach (var item in unshipped)
                {
                    if (item is not { Length: > 0 }) continue;
                    shipped.Add(item);
                }

                shipped.Sort();
                shipped.Insert(0, "#nullable enable");
                await File.WriteAllLinesAsync(shippedFilePath, shipped);
                await File.WriteAllTextAsync(unshippedFilePath, "#nullable enable");
            }
        }
    );

    [Solution(GenerateProjects = true)] private Solution Solution { get; } = null!;
    Nuke.Common.ProjectModel.Solution IHaveSolution.Solution => Solution;

    [OptionalGitRepository] public GitRepository? GitRepository { get; }
    [ComputedGitVersion] public GitVersion GitVersion { get; } = null!;
    [Parameter("Configuration to build")] public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;
}
