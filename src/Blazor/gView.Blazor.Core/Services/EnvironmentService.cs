using Microsoft.Extensions.Options;
using System.IO;

namespace gView.Blazor.Core.Services;

public class EnvironmentService
{
    private readonly EnvironmentServiceOptions _options;

    public EnvironmentService(IOptions<EnvironmentServiceOptions> options)
    {
        _options = options.Value;

        RepositoryPath = TryCreateIfNotExists(_options.RepositoryPath);
        MyApplicationConfigPath = TryCreateIfNotExists(Path.Combine(RepositoryPath, "_user_config"));
    }

    public string RepositoryPath { get; }
    public string MyApplicationConfigPath { get; }

    #region Helper

    public string TryCreateIfNotExists(string path)
    {
        try
        {
            var di = new DirectoryInfo(path);
            if(!di.Exists) { di.Create(); }
        }
        catch { }

        return path;
    }

    #endregion
}

public class EnvironmentServiceOptions
{
    public string RepositoryPath { get; set; } = "";
}
