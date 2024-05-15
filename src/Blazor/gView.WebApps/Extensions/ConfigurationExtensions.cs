using System.Reflection;

namespace gView.WebApps.Extensions;

static public class ConfigurationExtensions
{
    static public string RepositoryPath(this IConfiguration configuration)
    {
        string? path = configuration["RepositoryPath"];

        if (string.IsNullOrEmpty(path))
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ConfigurationExtensions))!.Location);
            path = Path.Combine(new DirectoryInfo(currentPath!).Parent!.FullName, "gview-web-repository");
        }

        return path;
    }
}
