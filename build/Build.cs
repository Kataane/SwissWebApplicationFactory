using Serilog;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;

[GitHubActions("Continuous",
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.MacOsLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    OnPushBranches = new[]
    {
        "main",
        "dev",
    },
    OnPullRequestBranches = new[]
    {
        "dev"
    },
    InvokedTargets = new[]
    {
        nameof(Test)
    }
)]

[GitHubActions("Push to nuget",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[]
    {
        nameof(Publish),
    },
    EnableGitHubToken = true,
    ImportSecrets = new[]
    {
        nameof(NugetApiKey)
    }
)]

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";
    AbsolutePath ArtifactsNugetDirectory => RootDirectory / ".artifacts" / "nuget";

    [Parameter("Nuget Api Key"), Secret] readonly string NugetApiKey;
    const string NugetApiUrl = "https://api.nuget.org/v3/index.json";

    [Parameter("Collect code coverage. Default is 'true'")] readonly bool? Cover = true;
    [Parameter("Coverage threshold. Default is 75%")] readonly int Threshold = 70;

    protected override void OnBuildInitialized()
    {
        Logging.Level = LogLevel.Normal;
        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{NuGetVersionV2}", GitVersion.NuGetVersionV2);
        Log.Information("Configuration:\t{AssemblySemVer}", GitVersion.AssemblySemVer);
        Log.Information("Configuration:\t{InformationalVersion}", GitVersion.InformationalVersion);
    }

    Target Clean => _ => _
        .Before(Restore)
        .Description("Cleaning Project.")
        .Executes(() =>
        {
            Solution.src.SwissWebApplicationFactory.Directory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Description("Restoring Project Dependencies.")
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(r => r.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .Description("Building Project with the version.")
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(b => b
                .SetProjectFile(Solution.src.SwissWebApplicationFactory)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(t => t
                .EnableNoRestore()
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution.tests.SwissWebApplicationFactory_Tests)
                .SetProcessArgumentConfigurator(arguments => arguments
                    .Add("/p:CollectCoverage={0}", Cover)
                    .Add("/p:Threshold={0}", Threshold)
                    .Add("/p:UseSourceLink={0}", "true")
                    .Add("/p:Exclude={0}", "\"[SwissWebApplicationFactory.Stand*]*\"")
                    .Add("/p:CoverletOutputFormat={0}", "cobertura"))
                .SetResultsDirectory(ArtifactsDirectory / "tests")
            );
        });

    Target Pack => _ => _
        .After(Compile, Test)
        .Description("Packing Project with the version.")
        .Produces(ArtifactsDirectory / "nuget")
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(p => p
                    .SetProject(Solution.src.SwissWebApplicationFactory)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(ArtifactsNugetDirectory)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetVersion(GitVersion.NuGetVersionV2)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion)
                    .SetFileVersion(GitVersion.AssemblySemFileVer));
        });

    Target Publish => _ => _
        .Description("Publishing to NuGet with the version.")
        .Requires(() => Configuration.Equals(Configuration.Release))
        .DependsOn(Pack)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
            ArtifactsNugetDirectory.GlobFiles("*.nupkg")
                .ForEach(package =>
                {
                    Log.Information("Publish:\t{Name}", package.Name);
                    DotNetNuGetPush(settings => settings
                        .SetTargetPath(package)
                        .SetSource(NugetApiUrl)
                        .SetApiKey(NugetApiKey));
                });
        });
}