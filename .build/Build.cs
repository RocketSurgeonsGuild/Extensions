using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke.DotNetCore;

[PublicAPI]
[UnsetVisualStudioEnvironmentVariables]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
[EnsureGitHooks(GitHook.PreCommit)]
[DotNetVerbosityMapping]
[MSBuildVerbosityMapping]
[NuGetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
[LocalBuildConventions]
internal partial class Pipeline : NukeBuild,
    ICanRestoreWithDotNetCore,
    ICanBuildWithDotNetCore,
    ICanTestWithDotNetCore,
    ICanPackWithDotNetCore,
    ICanClean,
    IHaveCommonLintTargets,
//                                IHavePublicApis,
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
    public static int Main() => Execute<Pipeline>(x => x.Default);

    [NonEntryTarget]
    private Target Default => _ => _
                                  .DependsOn(Restore)
                                  .DependsOn(Build)
                                  .DependsOn(Test)
                                  .DependsOn(Pack);

    [Solution(GenerateProjects = true)]
    private Solution Solution { get; } = null!;

    public Target Build => _ => _;
    public Target Pack => _ => _;
    public Target Clean => _ => _;
    public Target Restore => _ => _;
    Nuke.Common.ProjectModel.Solution IHaveSolution.Solution => Solution;

    [GitVersion(NoFetch = true, NoCache = false)]
    public GitVersion GitVersion { get; } = null!;

    public Target Test => _ => _;

    [NonEntryTarget]
    public Target DotnetCoreTest => d => d
                                        .Description("Executes all the unit tests.")
                                        .Unlisted()
                                        .After(Build)
                                        .TryDependentFor<IHaveTestTarget>(a => a.Test)
                                        .TryAfter<IHaveRestoreTarget>(a => a.Restore)
                                        .WhenSkipped(DependencyBehavior.Execute)
                                        .Net9MsBuildFix()
                                        .Executes(
                                             () => DotNetTasks.DotNetBuild(
                                                 s => s
                                                     .SetProcessWorkingDirectory(RootDirectory)
                                                     .SetProjectFile(Solution)
                                                     .SetDefaultLoggers(this.As<ICanTestWithDotNetCore>().LogsDirectory / "test.build.log")
                                                     .SetGitVersionEnvironment(GitVersion)
                                                     .SetConfiguration(this.As<ICanTestWithDotNetCore>().TestBuildConfiguration)
                                                     .EnableNoRestore()
                                             )
                                         )
                                        .CreateOrCleanDirectory(this.As<ICanTestWithDotNetCore>().TestResultsDirectory)
                                        .EnsureRunSettingsExists(this)
                                        .Net9MsBuildFix()
                                        .Executes(
                                             () => DotNetTasks.DotNetRun(
                                                 settings =>
                                                     settings
                                                        .CombineWith(
                                                             Solution.GetTestProjects(),
                                                             (runSettings, project) =>
                                                                 runSettings
                                                                    .SetProcessWorkingDirectory(RootDirectory)
                                                                    //                                                                    .SetDefaultLoggers(this.As<ICanTestWithDotNetCore>().LogsDirectory / $"{project.Name}.log")
                                                                    .SetGitVersionEnvironment(GitVersion)
                                                                    .SetConfiguration(this.As<ICanTestWithDotNetCore>().TestBuildConfiguration)
                                                                    .EnableNoRestore()
                                                                    .EnableNoBuild()
                                                                    .SetProjectFile(project.FilePath)
                                                                    .AddApplicationArguments(
                                                                         "--coverage-settings",
                                                                         this.As<ICanTestWithDotNetCore>().RunSettings,
                                                                         "--coverage-output-format",
                                                                         "cobertura",
                                                                         "--coverage",
                                                                         "--report-trx",
                                                                         "--results-directory",
                                                                         this.As<ICanTestWithDotNetCore>().TestResultsDirectory
                                                                     )
                                                                    .CombineWith(
                                                                         project.TargetFrameworks,
                                                                         (s, framework) =>
                                                                             s
                                                                                .SetFramework(framework)
                                                                                .AddApplicationArguments(
                                                                                     "--coverage-output",
                                                                                     this.As<ICanTestWithDotNetCore>().TestResultsDirectory
                                                                                   / $"{project.Name}.{framework}.cobertura.xml",
                                                                                     "--report-trx-filename",
                                                                                     $"{project.Name}.{framework}.trx"
                                                                                 )
                                                                     )
                                                         )
                                             )
                                         )
    //                                        .Executes(
    //                                             () => DotNetTool.GetTool("dotnet-coverage")(
    //                                                 $"{new Arguments()
    //                                                   .Add("collect")
    //                                                   .Add("--settings {value}", this.As<ICanTestWithDotNetCore>().RunSettings)
    //                                                   .Add("--output {value}", this.As<ICanTestWithDotNetCore>().TestResultsDirectory / "test.cobertura.xml")
    //                                                   .Add("--output-format {value}", "cobertura")
    //                                                   .Add("--")
    //                                                   .Add("dotnet")
    //                                                   .Concatenate(
    //                                                        this.As<ICanTestWithDotNetCore>().CustomizeDotNetTestSettings(
    //                                                                 new DotNetTestSettings()
    //                                                                    .SetProcessWorkingDirectory(RootDirectory)
    //                                                                    .SetProjectFile(Solution)
    //                                                                    .SetDefaultLoggers(this.As<ICanTestWithDotNetCore>().LogsDirectory / "test.log")
    //                                                                    .SetGitVersionEnvironment(GitVersion)
    //                                                                    .SetConfiguration(this.As<ICanTestWithDotNetCore>().TestBuildConfiguration)
    //                                                                    .EnableNoRestore()
    //                                                                    .EnableNoBuild()
    //                                                                    .SetLoggers("trx")
    //                                                                    .SetResultsDirectory(this.As<ICanTestWithDotNetCore>().TestResultsDirectory)
    //                                                             )
    //                                                            .GetProcessArguments()
    //                                                    ).RenderForExecution()}",
    //                                                 RootDirectory
    //                                             )
    //                                         )
    ;

    public Target Lint => _ => _;

    /// <summary>
    ///     Only run the JetBrains cleanup code when running on the server
    /// </summary>
    public Target JetBrainsCleanupCode => _ => _
                                              .Inherit<ICanDotNetFormat>(x => x.JetBrainsCleanupCode)
                                              .OnlyWhenStatic(() => IsServerBuild);

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    [Parameter("Configuration to build")]
    public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;
}
