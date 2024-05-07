using gView.Deploy.Models;

namespace gView.Deploy.Services;

internal class DeployRepositoryService
{
    private readonly string _workingDirectory,
                            _repoDirectory;

    public DeployRepositoryService(IOService ioService, string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        _repoDirectory = Path.Combine(_workingDirectory, "_deploy_repository");

        var repoDirectoryInfo = this.RepositoryRootDirectoryInfo();
        if (!repoDirectoryInfo.Exists)
        {
            repoDirectoryInfo.Create();
        }
    }

    public DirectoryInfo RepositoryRootDirectoryInfo() =>
        new DirectoryInfo(_repoDirectory);

    public string RepositoryRootDirectory => _repoDirectory;

    public string ProfileDirectory(string profile) => Path.Combine(_repoDirectory, "profiles", profile);

    #region Profiles

    public IEnumerable<string> Profiles()
    {
        DirectoryInfo di = new DirectoryInfo(Path.Combine(_repoDirectory, "profiles"));

        if (!di.Exists)
        {
            di.Create();
        }

        return di
            .GetDirectories()
            .OrderBy(d => d.CreationTime)
            .Select(d => d.Name);
    }

    public void CreateProfile(string profile)
    {
        if (string.IsNullOrEmpty(profile))
        {
            throw new Exception("Invalid profile name");
        }

        var di = new DirectoryInfo(Path.Combine(_repoDirectory, "profiles", profile));
        di.Create();
    }

    #endregion

    public DeployVersionModel GetDeployModel(string profile)
    {
        var di = new DirectoryInfo(Path.Combine(_repoDirectory, "profiles", profile));

        var deployVersionModelFile = new FileInfo(Path.Combine(di.FullName, "deploy-model.json"));
        if (!deployVersionModelFile.Exists)
        {
            var deployVersionModel = new DeployVersionModel()
            {
                ProfileName = profile
            };

            File.WriteAllText(deployVersionModelFile.FullName, System.Text.Json.JsonSerializer.Serialize(deployVersionModel));

            return deployVersionModel;
        }

        return System.Text.Json.JsonSerializer.Deserialize<DeployVersionModel>(File.ReadAllText(deployVersionModelFile.FullName))!;
    }

    public void SetDeployVersionModel(string profile, DeployVersionModel deployVersionModel)
    {
        var deployVersionModelFile = new FileInfo(Path.Combine(_repoDirectory, "profiles", profile, "deploy-model.json"));

        File.WriteAllText(deployVersionModelFile.FullName,
            System.Text.Json.JsonSerializer.Serialize(deployVersionModel, new System.Text.Json.JsonSerializerOptions()
            {
                WriteIndented = true
            }));
    }
}


