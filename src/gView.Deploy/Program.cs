
using gView.Deploy;
using gView.Deploy.Extensions;
using gView.Deploy.Services;
#if DEBUG
string workDirectory = @"C:\deploy\gview-gis";
#else
string workDirectory = Environment.CurrentDirectory;
#endif

Console.WriteLine($"******************************************");
Console.WriteLine($"*                                        *");
Console.WriteLine($"*      gView.Deploy Tool {DeployVersionService.DeployToolVersion}       *");
Console.WriteLine($"*                                        *");
Console.WriteLine($"******************************************");

Console.WriteLine($"Work-Directory: {workDirectory}");
Console.WriteLine();

if (args != null && args.Any(a => a is "-h" or "--help" or "-?"))
{
    new ConsoleService().WriteUsageMessage();
    return;
}

string profile = String.Empty,
       version = String.Empty,
       productArg = String.Empty;

bool yesFlag = false;
bool portableFlag = false;
bool? downloadAnswer = null;
bool? continueAnswer = null;

var modelArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

var consoleService = new ConsoleService();

try
{
    if (args != null)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-p":
                case "--profile":
                    profile = args.ElementAtOrDefault(++i) ?? String.Empty;
                    break;
                case "-v":
                case "--version":
                    version = args.ElementAtOrDefault(++i) ?? String.Empty;
                    break;
                case "--product":
                    productArg = args.ElementAtOrDefault(++i) ?? String.Empty;
                    break;
                case "-y":
                case "--yes":
                    // accept the default answer for every Y/N confirmation and, if
                    // --product isn't given either, fall back to "Everything"
                    yesFlag = true;
                    break;
                case "--download":
                    downloadAnswer = true;
                    break;
                case "--skip-download":
                case "--no-download":
                    downloadAnswer = false;
                    break;
                case "--confirm":
                    continueAnswer = true;
                    break;
                case "--no-confirm":
                    continueAnswer = false;
                    break;
                case "--portable":
                    // use/download the "portable" zip files
                    // (gview-server-portable-win64-..., gview-webapps-portable-linux64-...)
                    portableFlag = true;
                    break;
                default:
                    // any other "--xxx value" pair is kept around and matched later
                    // against the [ModelProperty] flags of the deploy model
                    // (eg. --repository-path, --admin-username, --admin-password, ...)
                    if (args[i].StartsWith("--") && i + 1 < args.Length)
                    {
                        modelArgs[args[i]] = args[++i];
                    }
                    break;
            }
        }
    }

    if (portableFlag)
    {
        Console.WriteLine("Using portable zip files (gview-server-portable-..., gview-webapps-portable-...).");

        // portable zips aren't hosted on GitHub (yet) - skip the download question by
        // default; --download still forces it if that ever changes
        downloadAnswer ??= false;
    }

    if (yesFlag)
    {
        downloadAnswer ??= true;
        continueAnswer ??= true;
    }

    var ioService = new IOService();
    var repoService = new DeployRepositoryService(ioService, workDirectory);
    var versionService = new DeployVersionService(repoService, ioService, portableFlag);

    if (String.IsNullOrEmpty(profile))
    {
        profile = consoleService.ChooseFrom(repoService.Profiles(), "profile", allowNewValues: true, examples: "production, staging, test").Trim();
    }

    repoService.CreateProfile(profile);

    if (consoleService.DoYouWant("to download latetest version from GitHub", downloadAnswer))
    {
        try
        {
            var githubReleaseService = new GitHubReleaseService("jugstalt", "gview-gis", portableFlag);

            var lastServerInstalledVersion = versionService.GetVersions(AppName.Server).FirstOrDefault() switch
            {
                string str when !String.IsNullOrEmpty(str) => new Version(str),
                _ => new Version()
            };
            var lastWebAppsInstalledVersion = versionService.GetVersions(AppName.WebApps).FirstOrDefault() switch
            {
                string str when !String.IsNullOrEmpty(str) => new Version(str),
                _ => new Version()
            };

            foreach (var url in await githubReleaseService.GetLatestReleaseDownloadUrlsAsync())
            {
                var fileName = url.Split('/').Last();

                Console.WriteLine($"Found newest version of {fileName}");

                if (!versionService.Exits(fileName))
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            Console.Write("downloading ...");
                            var fileByptes = await client.GetByteArrayAsync(url);
                            Console.Write("write to disk ...");
                            await versionService.AppendAsync(fileName, fileByptes);
                            Console.WriteLine("done!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception on downloading file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("... already exists in your download folder");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }

        Console.WriteLine();
    }

    if (String.IsNullOrEmpty(version))
    {
        version = consoleService.ChooseFrom(versionService.GetVersions(AppName.Server).Take(5), "version");
    }
    else if (version.Equals("latest", StringComparison.OrdinalIgnoreCase))
    {
        var latestVersion = versionService.GetVersions(AppName.Server).FirstOrDefault();
        if (String.IsNullOrEmpty(latestVersion))
        {
            throw new Exception("Can't resolve --version latest: no local version found. Download a version first (eg. --download).");
        }

        Console.WriteLine($"Resolved --version latest to {latestVersion}");
        version = latestVersion;
    }

    if (String.IsNullOrEmpty(profile) ||
        String.IsNullOrEmpty(version))
    {
        consoleService.WriteUsageMessage();
        return;
    }

    Product product;
    if (!String.IsNullOrEmpty(productArg))
    {
        product = Enum.Parse<Product>(productArg.Split('.').Last(), true);
    }
    else if (yesFlag)
    {
        product = Product.Everything;
        Console.WriteLine("No --product given, using default 'Everything' (non-interactive mode).");
    }
    else
    {
        product = Enum.Parse<Product>(
                consoleService
                    .ChooseFrom(["Everything", "gView.Server", "gView.WebApps"], "product")
                    .Split('.')
                    .Last(),
                    true
                );
    }

    Console.WriteLine($"Deploy '{product}' from version {version} to profile {profile}");
    if (!consoleService.DoYouWant("to continue", continueAnswer))
    {
        return;
    }

    var deployVersionModel = repoService.GetDeployModel(profile);

    var modelPropertiesChanged = consoleService.ApplyModelPropertiesFromArgs(deployVersionModel, modelArgs);
    modelPropertiesChanged |= consoleService.InputRequiredModelProperties(deployVersionModel);

    if (modelPropertiesChanged)
    {
        repoService.SetDeployVersionModel(profile, deployVersionModel);
    }

    var gViewRepoPath = deployVersionModel.RepositoryPath.EndsWith("!") 
            ? deployVersionModel.RepositoryPath.Substring(0, deployVersionModel.RepositoryPath.Length - 1) 
            : deployVersionModel.RepositoryPath;

    if (!String.IsNullOrEmpty(gViewRepoPath))
    {
        var gViewRepoDirectory = new DirectoryInfo(gViewRepoPath);

        if (!gViewRepoDirectory.Exists)
        {
            consoleService.WriteBlock($"Create a new gview repositiry {gViewRepoDirectory.FullName}");

            Directory.CreateDirectory(gViewRepoDirectory.FullName);
        }
    }

    var webAppsTargetExists = Directory.Exists(
            Path.Combine(deployVersionModel.ProfileTargetInstallationPath(profile, version), "webapps")
        );
    var serverTargetExists = Directory.Exists(
            Path.Combine(deployVersionModel.ProfileTargetInstallationPath(profile, version), "server")
        );

    #region Create config overrides

    Console.WriteLine("Init overrides.");
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