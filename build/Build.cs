using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

using Serilog;

// Deploy build for gView GIS
//
//   .\build.cmd                    => Test, Compile, CleanArtifacts, Zip, Docker
//   .\build.cmd Zip                => only the ZIP files (no docker)
//   .\build.cmd Deploy --skip Test
//   .\build.cmd Zip --edition portable
//   .\build.cmd DockerPush         => also push images to ghcr.io
//
// Output (DeployRoot, default c:\deploy\gview-gis):
//   download\gview-{server|webapps}[-edition]-{win64|linux64}-{version}.zip
//   docker\{server|webapps}\       docker build context
//
// The ZIP structure ({version}/{server|webapps}/... + start script) is consumed
// by gView.Deploy and must not change.
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Deploy);

    [Parameter("Configuration to build - Default is 'Release'")]
    readonly string Configuration = "Release";

    [Parameter("Version - Default is the version from SystemInfo.cs")]
    string Version;

    [Parameter("Edition - '' (default) or 'portable'")]
    readonly string Edition = "";

    [Parameter("Platform to build - win64, linux64 (default: both)")]
    readonly string[] Platform = { "win64", "linux64" };

    [Parameter("Deploy root - Default is c:\\deploy\\gview-gis")]
    readonly AbsolutePath DeployRoot = (AbsolutePath)@"c:\deploy\gview-gis";

    [Parameter("Docker exe file name")]
    readonly string DockerExe = "docker";

    [Parameter("Docker registry for DockerPush")]
    readonly string DockerRegistry = "ghcr.io/jugstalt";

    record GViewApp(string Name, RelativePath Project, string ZipName, string DockerImage, string DerivedDockerImage, bool IncludeCmd);

    static readonly GViewApp Server = new("server", (RelativePath)"src/gView.Server/gView.Server.csproj", "gview-server", "gview-server-base", "gview-server", false);
    static readonly GViewApp WebApps = new("webapps", (RelativePath)"src/Blazor/gView.WebApps/gView.WebApps.csproj", "gview-webapps", "gview-webapps", null, true);
    static readonly GViewApp Cmd = new("cmd", (RelativePath)"src/gView.Cmd/gView.Cmd.csproj", null, null, null, false);

    static readonly GViewApp[] DeployApps = { Server, WebApps };

    AbsolutePath AssetsDirectory => RootDirectory / "publish";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath DownloadDirectory => DeployRoot / "download";
    AbsolutePath DockerDirectory => DeployRoot / "docker";

    bool IsPortable => Edition.Equals("portable", StringComparison.OrdinalIgnoreCase);
    string EditionSuffix => String.IsNullOrEmpty(Edition) ? "" : $"-{Edition.ToLowerInvariant()}";

    AbsolutePath Artifacts(GViewApp app, string platform) => ArtifactsDirectory / app.Name / platform;

    protected override void OnBuildInitialized()
    {
        if (String.IsNullOrEmpty(Version))
        {
            var systemInfo = (RootDirectory / "src" / "gView.Framework" / "Common" / "SystemInfo.cs").ReadAllText();
            var match = Regex.Match(systemInfo, @"Version\s+Version\s*=\s*new\s+Version\((\d+),\s*(\d+),\s*(\d+)\)");
            Assert.True(match.Success, "Can't read version from SystemInfo.cs");
            Version = $"{match.Groups[1].Value}.{match.Groups[2].Value}.{match.Groups[3].Value}";
        }

        foreach (var platform in Platform)
        {
            Assert.True(platform is "win64" or "linux64", $"Unknown platform '{platform}'");
        }
        Assert.True(String.IsNullOrEmpty(Edition) || IsPortable, $"Unknown edition '{Edition}'");

        Log.Information("gView GIS {Version}{Edition} for {Platform}", Version, EditionSuffix, String.Join(", ", Platform));
    }

    Target Test => _ => _
        .Before(Compile)
        .Executes(() =>
        {
            foreach (var testProject in (RootDirectory / "src" / "tests").GlobFiles("*/*.csproj"))
            {
                DotNetTasks.DotNetTest(s => s
                    .SetProjectFile(testProject)
                    .SetConfiguration(Configuration)
                    .SetVerbosity(DotNetVerbosity.minimal));
            }
        });

    Target Compile => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            foreach (var platform in Platform)
            {
                var publishProfile = IsPortable ? $"{platform}-portable" : platform;

                foreach (var app in DeployApps)
                {
                    Log.Information("Compile {App} {Platform}", app.Name, publishProfile);

                    Artifacts(app, platform).CreateOrCleanDirectory();
                    // web projects: build + DeployOnBuild (same as the former publish_*.bat)
                    DotNetTasks.DotNetBuild(s => s
                        .SetProjectFile(RootDirectory / app.Project)
                        .SetConfiguration(Configuration)
                        .SetProperty("DeployOnBuild", "true")
                        .SetProperty("PublishProfile", publishProfile)
                        .SetProperty("PublishUrl", Artifacts(app, platform))
                        .SetProperty("SkiaVariant", "Skia3"));
                }

                Log.Information("Compile {App} {Platform}", Cmd.Name, publishProfile);

                Artifacts(Cmd, platform).CreateOrCleanDirectory();
                DotNetTasks.DotNetPublish(s => s
                    .SetProject(RootDirectory / Cmd.Project)
                    .SetConfiguration(Configuration)
                    .SetProperty("PublishProfile", publishProfile)
                    .SetProperty("PublishDir", Artifacts(Cmd, platform))
                    .SetProperty("SkiaVariant", "Skia3"));
            }
        });

    Target CleanArtifacts => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var platform in Platform)
            {
                foreach (var app in DeployApps.Append(Cmd))
                {
                    var artifacts = Artifacts(app, platform);

                    // never deploy (developer) configurations
                    var configDirectory = artifacts / "_config";
                    if (configDirectory.DirectoryExists())
                    {
                        configDirectory.CreateOrCleanDirectory();
                    }

                    // third party pdb files (eg. libSkiaSharp.pdb is > 80 MB) and doc xml files
                    foreach (var file in artifacts.GlobFiles("*.pdb", "*.xml"))
                    {
                        if (!file.Name.StartsWith("gView.", StringComparison.OrdinalIgnoreCase))
                        {
                            file.DeleteFile();
                        }
                    }
                }
            }
        });

    Target Zip => _ => _
        .DependsOn(CleanArtifacts)
        .Executes(() =>
        {
            DownloadDirectory.CreateDirectory();

            var packages = Platform.SelectMany(platform => DeployApps.Select(app => (platform, app)));

            // the ZIP files are independent => create them in parallel
            Parallel.ForEach(packages, package =>
            {
                var (platform, app) = package;
                var zipFile = DownloadDirectory / $"{app.ZipName}{EditionSuffix}-{platform}-{Version}.zip";

                // ZIP content as entry path => source file (no staging copy)
                var content = new ZipContent();
                var appPrefix = $"{Version}/{app.Name}/";

                content.AddDirectory(Artifacts(app, platform), appPrefix, overwrite: true);
                if (app.IncludeCmd)
                {
                    // gView.Cmd tools: only add files, never replace the app's own files
                    content.AddDirectory(Artifacts(Cmd, platform), appPrefix, overwrite: false);
                }
                content.AddDirectory(AssetsDirectory / app.Name / "override", appPrefix, overwrite: true);
                content.AddDirectory(AssetsDirectory / app.Name / platform / "override", appPrefix, overwrite: true);

                // start scripts
                foreach (var script in (AssetsDirectory / app.Name / platform).GlobFiles("*.bat", "*.sh"))
                {
                    content.AddFile(script, $"{Version}/{script.Name}", overwrite: true);
                }

                var stopwatch = Stopwatch.StartNew();
                zipFile.DeleteFile();
                content.WriteZip(zipFile, $"{Version}/");

                Log.Information("Zip {ZipFile} ({Seconds:0.0}s)", zipFile, stopwatch.Elapsed.TotalSeconds);
            });
        });

    Target Docker => _ => _
        .DependsOn(CleanArtifacts)
        .After(Zip)
        .OnlyWhenStatic(() => !IsPortable && Platform.Contains("linux64"))
        .Executes(() =>
        {
            foreach (var app in DeployApps)
            {
                var contextDirectory = DockerDirectory / app.Name;

                contextDirectory.CreateOrCleanDirectory();

                CopyDirectory(Artifacts(app, "linux64"), contextDirectory / "app", overwrite: true);
                CopyDirectory(AssetsDirectory / app.Name / "override", contextDirectory / "app", overwrite: true);
                CopyDirectory(AssetsDirectory / app.Name / "docker" / "override", contextDirectory / "app", overwrite: true);
                (AssetsDirectory / app.Name / "docker" / "Dockerfile").Copy(contextDirectory / "Dockerfile", ExistsPolicy.FileOverwrite);

                BuildDockerImage(app.DockerImage, contextDirectory);

                if (app.DerivedDockerImage is not null)
                {
                    // image based on the app image (eg. gview-server-base => gview-server)
                    var derivedContextDirectory = DockerDirectory / app.DerivedDockerImage;

                    derivedContextDirectory.CreateOrCleanDirectory();
                    (AssetsDirectory / app.Name / "docker" / app.DerivedDockerImage / "Dockerfile").Copy(derivedContextDirectory / "Dockerfile", ExistsPolicy.FileOverwrite);

                    BuildDockerImage(app.DerivedDockerImage, derivedContextDirectory, $"--build-arg BASE_IMAGE={app.DockerImage}:{Version}");
                }
            }
        });

    Target DockerPush => _ => _
        .DependsOn(Docker)
        .Executes(() =>
        {
            foreach (var image in DeployApps.SelectMany(app => new[] { app.DockerImage, app.DerivedDockerImage }).Where(image => image is not null))
            {
                RunDocker($"push {DockerRegistry}/{image}:{Version}");
                RunDocker($"push {DockerRegistry}/{image}:latest");
            }
        });

    Target Deploy => _ => _
        .DependsOn(Zip, Docker);

    void BuildDockerImage(string image, AbsolutePath contextDirectory, string buildArguments = "")
    {
        Log.Information("Build docker image {Image}:{Version}", image, Version);

        RunDocker($"build -t {image}:{Version} {buildArguments} -f Dockerfile .", contextDirectory);
        RunDocker($"tag {image}:{Version} {image}:latest");
        RunDocker($"tag {image}:{Version} {DockerRegistry}/{image}:{Version}");
        RunDocker($"tag {image}:{Version} {DockerRegistry}/{image}:latest");
    }

    void RunDocker(string arguments, AbsolutePath workingDirectory = null)
    {
        ProcessTasks.StartProcess(DockerExe, arguments,
                workingDirectory: workingDirectory,
                logger: (_, text) => Log.Information(text))
            .AssertZeroExitCode();
    }

    static void CopyDirectory(AbsolutePath source, AbsolutePath target, bool overwrite)
    {
        if (!source.DirectoryExists())
        {
            return;
        }

        target.CreateDirectory();

        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var targetFile = Path.Combine(target, Path.GetRelativePath(source, file));
            if (overwrite || !File.Exists(targetFile))
            {
                File.Copy(file, targetFile, overwrite);
            }
        }
    }
}
