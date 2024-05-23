using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Deploy.Services;
public class GitHubReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;

    public GitHubReleaseService(string repositoryOwner, string repositoryName)
    {
        _httpClient = new HttpClient();
        _repositoryOwner = repositoryOwner;
        _repositoryName = repositoryName;
    }

    public async Task<List<string>> GetReleaseDownloadUrlsAsync()
    {
        var releaseUrls = new List<string>();
        var apiUrl = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/releases";

        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(content);

            foreach (var release in releases ?? [])
            {
                foreach (var asset in release.Assets)
                {
                    if (asset.Name.EndsWith(".zip"))
                    {
                        releaseUrls.Add(asset.BrowserDownloadUrl);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error on downloading releases: {ex.Message}");
        }

        return releaseUrls;
    }

    public async Task<List<string>> GetLatestReleaseDownloadUrlsAsync()
    {
        var releaseUrls = new List<string>();
        var apiUrl = $"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/releases";
        var latestVersions = new Dictionary<string, GitHubAsset>();

        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(content);

            foreach (var release in releases ?? [])
            {
                foreach (var asset in release.Assets)
                {
                    if (asset.Name.EndsWith(".zip") && 
                        (asset.Name.Contains($"gview-webapps-{PlatformName}-") 
                        || asset.Name.Contains($"gview-server-{PlatformName}-")))
                    {
                        var key = asset.Name.StartsWith("gview-webapps") 
                            ? "gview-webapps" 
                            : "gview-server";

                        if (!latestVersions.ContainsKey(key) 
                            || CompareVersions(asset.Name, latestVersions[key].Name) > 0)
                        {
                            latestVersions[key] = asset;
                        }
                    }
                }
            }

            releaseUrls = latestVersions.Values.Select(a => a.BrowserDownloadUrl).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen von Releases: {ex.Message}");
        }

        return releaseUrls;
    }

    private int CompareVersions(string fileName1, string fileName2)
    {
        var version1 = ExtractVersion(fileName1);
        var version2 = ExtractVersion(fileName2);
        return version1.CompareTo(version2);
    }

    private Version ExtractVersion(string fileName)
    {
        var versionString = fileName.Split('-').Last().Replace(".zip", "");
        return Version.Parse(versionString);
    }

    private string PlatformName =>
        Platform.IsWindows
        ? "win64"
        : Platform.IsLinux
           ? "linux64"
           : "unknown";
}

public class GitHubRelease
{
    [JsonPropertyName("assets")]
    public List<GitHubAsset> Assets { get; set; } = [];
}

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = "";
}
