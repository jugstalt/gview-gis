using gView.Deploy.Models;
using System.IO.Compression;

namespace gView.Deploy.Services;

internal enum AppName
{
    Server,
    WebApps
}

internal class DeployVersionService
{
    private readonly Dictionary<AppName, string> zipPrefix;

    private readonly string _versionsDirectory;
    private readonly DeployRepositoryService _deployRepositoryService;
    private readonly IOService _ioService;

    public DeployVersionService(DeployRepositoryService repositoryService,
                                IOService ioService)
    {
        zipPrefix = new Dictionary<AppName, string>();
        if (Platform.IsLinux)
        {
            zipPrefix.Add(AppName.Server, "gview-server-linux64-");
            zipPrefix.Add(AppName.WebApps, "gview-webapps-linux64-");
        }
        else if (Platform.IsWindows)
        {
            zipPrefix.Add(AppName.Server, "gview-server-win64-");
            zipPrefix.Add(AppName.WebApps, "gview-webapps-win64-");
        }
        else
        {
            throw new Exception("Unsupported platform! Use Linux or Windows with 64 Bit");
        }

        _deployRepositoryService = repositoryService;
        _ioService = ioService;

        _versionsDirectory =
            Path.Combine(_deployRepositoryService.RepositoryRootDirectoryInfo().Parent!.FullName, "download");
    }

    public IEnumerable<string> GetVersions(AppName appName)
    {
        var di = new DirectoryInfo(_versionsDirectory);

        return di
            .GetFiles($"{zipPrefix[appName]}*.zip")
            .Select(f => f.Name.Substring(zipPrefix[appName].Length, f.Name.Length - zipPrefix[appName].Length - f.Extension.Length))
            .Where(n => Version.TryParse(n, out var version))
            .OrderByDescending(n => Version.Parse(n));
    }

    public void CopyFolderRecursive(string version, string relativeSourcePath, string targetPath)
    {
        if (Directory.Exists(targetPath))
        {
            throw new Exception("Target alreay exists");
        }

        string sourcePath = Path.Combine(_versionsDirectory, version, relativeSourcePath);

        _ioService.CopyFolderRecursive(sourcePath, targetPath);
    }

    public void CopyFiles(string version, string relativeSourcePath, string targetPath, string filter = "*.*")
    {
        string sourcePath = Path.Combine(_versionsDirectory, version, relativeSourcePath);

        _ioService.CopyFiles(sourcePath, targetPath, filter);
    }

    public void ExtractZipFolderRecursive(AppName appName, string version, string relativeSourcePath, string targetPath)
    {
        using (var fileStream = new FileStream(Path.Combine(_versionsDirectory, $"{ZipFile(appName, version)}"), FileMode.Open))
        using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
        {
            _ioService.ExtractZipFolderRecursive(zipArchive, $"{version}/{relativeSourcePath}", targetPath);
        }
    }

    public void ExtractFiles(AppName appName, string version, string relativeSourcePath, string targetPath)
    {
        using (var fileStream = new FileStream(Path.Combine(_versionsDirectory, $"{ZipFile(appName, version)}"), FileMode.Open))
        using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
        {
            _ioService.ExtractFiles(zipArchive, $"{version}/{relativeSourcePath}", targetPath);
        }
    }

    #region Overrides

    public void InitOverrides(string profile, string version)
    {
        using (var fileStream = new FileStream(Path.Combine(_versionsDirectory, $"{ZipFile(AppName.Server, version)}"), FileMode.Open))
        using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
        {
            _ioService.CopyIfNotExists(
                zipArchive,
                $"{version}/server/_config/_mapserver.json",
                Path.Combine(_deployRepositoryService.ProfileDirectory(profile), "server", "override", "_config", "mapserver.json"));
        }

        using (var fileStream = new FileStream(Path.Combine(_versionsDirectory, $"{ZipFile(AppName.WebApps, version)}"), FileMode.Open))
        using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
        {
            _ioService.CopyIfNotExists(
                zipArchive,
                 $"{version}/webapps/_config/_gview-webapps.config",
                Path.Combine(_deployRepositoryService.ProfileDirectory(profile), "webapps", "override", "_config", "gview-webapps.config"));
        }
    }

    public void CopyOverrides(string profile, string appFolder, string targetPath, DeployVersionModel versionModel)
    {
        string sourcePath = Path.Combine(_deployRepositoryService.ProfileDirectory(profile), appFolder, "override");
        targetPath = Path.Combine(targetPath, appFolder);

        _ioService.OverrideFolderRecursive(sourcePath, targetPath, versionModel);
    }

    #endregion

    #region Helper

    private string ZipFile(AppName appName, string version) => $"{zipPrefix[appName]}{version}.zip";

    #endregion
}
