
using gView.Deploy;
using gView.Deploy.Extensions;
using gView.Deploy.Services;
#if DEBUG
string workDirectory = @"C:\deploy\gview-gis";
#else
string workDirectory = Environment.CurrentDirectory;
#endif

Console.WriteLine($"Work-Directory: {workDirectory}");

string profile = String.Empty,
       version = String.Empty;

var consoleService = new ConsoleService();

try
{
    if (args != null)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "-p":
                case "--profile":
                    profile = args[i + 1];
                    break;
                case "-v":
                case "--version":
                    version = args[i + 1];
                    break;
            }
        }
    }

    var ioService = new IOService();
    var repoService = new DeployRepositoryService(ioService, workDirectory);
    var versionService = new DeployVersionService(repoService, ioService);

    if (String.IsNullOrEmpty(profile))
    {
        profile = consoleService.ChooseFrom(repoService.Profiles(), "profile", allowNewVales: true, examples: "production, staging, test").Trim();

        repoService.CreateProfile(profile);
    }

    if (consoleService.DoYouWant("to download latetest version from GitHub"))
    {
        var githubReleaseService = new GitHubReleaseService("jugstalt", "gview5");

        foreach (var url in await githubReleaseService.GetReleaseDownloadUrlsAsync())
        {
            Console.WriteLine(url);
        }
        Console.WriteLine("Download not implementet! Comming soon. Please download laytest Versions manually...");
        Console.WriteLine();
    }

    if (String.IsNullOrEmpty(version))
    {
        version = consoleService.ChooseFrom(versionService.GetVersions(AppName.Server).Take(5), "version");
    }

    if (String.IsNullOrEmpty(profile) ||
        String.IsNullOrEmpty(version))
    {
        consoleService.WriteUsageMessage();
        return;
    }

    var product = Enum.Parse<Product>(
            consoleService
                .ChooseFrom(["Everything", "gView.Server", "gView.WebApps"], "product")
                .Split('.')
                .Last(),
                true
            );

    Console.WriteLine($"Deploy '{product}' from version {version} to profile {profile}");
    if (!consoleService.DoYouWantToContinue())
    {
        return;
    }

    var deployVersionModel = repoService.GetDeployModel(profile);
    if (consoleService.InputRequiredModelProperties(deployVersionModel))
    {
        repoService.SetDeployVersionModel(profile, deployVersionModel);
    }

    var gViewRepoPath = new DirectoryInfo(deployVersionModel.RepositoryPath);

    if (!gViewRepoPath.Exists)
    {
        consoleService.WriteBlock($"Create a new gview repositiry {gViewRepoPath.FullName}");

        Directory.CreateDirectory(gViewRepoPath.FullName);
    }

    var webAppsTargetExists = Directory.Exists(
            Path.Combine(deployVersionModel.ProfileTargetInstallationPath(profile, version), "webapps")
        );
    var serverTargetExists = Directory.Exists(
            Path.Combine(deployVersionModel.ProfileTargetInstallationPath(profile, version), "server")
        );

    #region Create config overrides

    versionService.InitOverrides(profile, version);

    #endregion

    Console.WriteLine();
    Console.WriteLine($"Deploy '{product}' from version {version}");

    if (product == Product.Everything || product == Product.Server)
    {
        if (!serverTargetExists)
        {
            Console.WriteLine("Deploy gView Server:");
            versionService.ExtractZipFolderRecursive(
                    AppName.Server,
                    version,
                    "",
                    deployVersionModel.ProfileTargetInstallationPath(profile, version)
                 );
        }
        else
        {
            consoleService.WriteBlock("Warning: gview.Server version already deployed");
        }
    }

    if (product == Product.Everything || product == Product.WebApps)
    {
        if (!webAppsTargetExists)
        {
            Console.WriteLine("Deploy gView WebApps:");
            versionService.ExtractZipFolderRecursive(
                    AppName.WebApps,
                    version,
                    "",
                    deployVersionModel.ProfileTargetInstallationPath(profile, version)
                 );
        }
        else
        {
            consoleService.WriteBlock("Warning: gView.WebApps version already deployed");
        }
    }

    Console.WriteLine("Overrides");
    if (product == Product.Everything || product == Product.Server)
    {
        versionService.CopyOverrides(profile, "server", Path.Combine(deployVersionModel.ProfileTargetInstallationPath(profile, version)), deployVersionModel);
    }
    if (product == Product.Everything || product == Product.WebApps)
    {
        versionService.CopyOverrides(profile, "webapps", Path.Combine(deployVersionModel.ProfileTargetInstallationPath(profile, version)), deployVersionModel);
    }
}
catch (Exception ex)
{
    consoleService.WriteBlock($"Error: {ex.Message}", '!');
}

//Console.Write("Press ENTER to quit...");
//Console.ReadLine();