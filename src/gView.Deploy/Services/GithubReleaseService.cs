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
