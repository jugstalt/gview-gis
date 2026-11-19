using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Evaluates the json output of
//   dotnet list <solution|project> package [--vulnerable] --include-transitive --format json
static class PackageAudit
{
    public record Vulnerability(string Project, string Package, string Version, bool Transitive, string Severity, string AdvisoryUrl);

    public record Package(string Project, string Id, string Version);

    // NuGet severities, ascending
    static readonly string[] Severities = { "low", "moderate", "high", "critical" };

    public static int SeverityRank(string severity)
        => Array.IndexOf(Severities, severity?.Trim().ToLowerInvariant());

    public static bool IsKnownSeverity(string severity) => SeverityRank(severity) >= 0;

    public static IEnumerable<Vulnerability> Vulnerabilities(string json)
    {
        foreach (var (project, package, transitive) in Packages(json))
        {
            if (!package.TryGetProperty("vulnerabilities", out var vulnerabilities))
            {
                continue;
            }

            foreach (var vulnerability in vulnerabilities.EnumerateArray())
            {
                yield return new Vulnerability(
                    project,
                    package.GetProperty("id").GetString(),
                    ResolvedVersion(package),
                    transitive,
                    vulnerability.TryGetProperty("severity", out var severity) ? severity.GetString() : "unknown",
                    vulnerability.TryGetProperty("advisoryurl", out var url) ? url.GetString() : "");
            }
        }
    }

    public static IEnumerable<Package> AllPackages(string json)
        => Packages(json)
            .Select(p => new Package(p.project, p.package.GetProperty("id").GetString(), ResolvedVersion(p.package)));

    /// <summary>
    /// Native libraries (shipped as binaries in NuGet packages) can't be checked against the
    /// NuGet advisory database reliably: listed with their versions for a manual check.
    /// </summary>
    public static bool IsNativePackage(string id)
        => id.Contains(".NativeAssets.", StringComparison.OrdinalIgnoreCase)
           || id.StartsWith("SkiaSharp", StringComparison.OrdinalIgnoreCase)
           || id.StartsWith("HarfBuzzSharp", StringComparison.OrdinalIgnoreCase)
           || id.StartsWith("SQLitePCLRaw.lib", StringComparison.OrdinalIgnoreCase)
           || id.StartsWith("Microsoft.Data.SqlClient.SNI", StringComparison.OrdinalIgnoreCase)
           || id.Contains("Gdal", StringComparison.OrdinalIgnoreCase);

    static IEnumerable<(string project, JsonElement package, bool transitive)> Packages(string json)
    {
        // dotnet list writes restore messages before the json in some cases
        int start = json.IndexOf('{');
        if (start < 0)
        {
            throw new InvalidDataException("dotnet list returned no json");
        }

        using var document = JsonDocument.Parse(json.Substring(start));
        var result = new List<(string, JsonElement, bool)>();

        if (!document.RootElement.TryGetProperty("projects", out var projects))
        {
            return result;
        }

        foreach (var project in projects.EnumerateArray())
        {
            string projectName = Path.GetFileNameWithoutExtension(project.GetProperty("path").GetString());

            if (!project.TryGetProperty("frameworks", out var frameworks))
            {
                continue;
            }

            foreach (var framework in frameworks.EnumerateArray())
            {
                foreach (var (property, transitive) in new[] { ("topLevelPackages", false), ("transitivePackages", true) })
                {
                    if (framework.TryGetProperty(property, out var packages))
                    {
                        foreach (var package in packages.EnumerateArray())
                        {
                            result.Add((projectName, package.Clone(), transitive));
                        }
                    }
                }
            }
        }

        return result;
    }

    static string ResolvedVersion(JsonElement package)
        => package.TryGetProperty("resolvedVersion", out var version) ? version.GetString() : "";

    /// <summary>
    /// All projects an app is built from (the app itself and every referenced project),
    /// read from obj/project.assets.json after a restore (conditional references are evaluated).
    /// </summary>
    public static IEnumerable<string> ProjectClosure(string projectFile)
    {
        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFile));
        var assetsFile = Path.Combine(projectDirectory, "obj", "project.assets.json");
        if (!File.Exists(assetsFile))
        {
            throw new FileNotFoundException("Restore the project first", assetsFile);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(assetsFile));

        yield return Path.GetFullPath(projectFile);

        foreach (var library in document.RootElement.GetProperty("libraries").EnumerateObject())
        {
            if (library.Value.TryGetProperty("type", out var type) && type.GetString() == "project"
                && library.Value.TryGetProperty("path", out var path))
            {
                yield return Path.GetFullPath(Path.Combine(projectDirectory, path.GetString()));
            }
        }
    }
}
