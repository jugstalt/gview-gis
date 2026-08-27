using gView.Framework.Common;
using gView.GraphicsEngine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace gView.Server.AppCode;

/// <summary>
/// Reads the <c>fonts</c> section of <c>_config/mapserver.json</c> and makes the
/// configured font directories available to the graphics engines. See
/// <c>src/docs/design/font-provisioning.md</c>.
/// </summary>
static internal class ServerFonts
{
    /// <summary>
    /// Registers every configured font directory with all graphics engines (in-process,
    /// no OS install) and - when <c>fonts:install-to-system</c> is <c>true</c> - additionally
    /// performs a best-effort copy into the per-user OS font store.
    /// </summary>
    public static void Register(IConfiguration configuration, string contentRootPath)
    {
        var directories = ResolveDirectories(configuration, contentRootPath).ToArray();
        if (directories.Length == 0)
        {
            return;
        }

        SystemInfo.RegisterFontDirectories(directories);

        var installToSystem =
            configuration["fonts:installtosystem"] ?? configuration["fonts:install-to-system"];

        if ("true".Equals(installToSystem, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var directory in directories)
            {
                TryInstallToOperatingSystem(directory);
            }
        }
    }

    private static IEnumerable<string> ResolveDirectories(IConfiguration configuration, string contentRootPath)
    {
        var configured = configuration.GetSection("fonts:directories").Get<string[]>() ?? Array.Empty<string>();

        foreach (var entry in configured)
        {
            if (String.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            var path = Path.IsPathRooted(entry)
                ? entry
                : Path.Combine(contentRootPath, entry);

            if (Directory.Exists(path))
            {
                yield return path;
            }
            else
            {
                Console.WriteLine($"[fonts] configured font directory does not exist: '{path}'");
            }
        }
    }

    #region Best-effort OS install

    private static void TryInstallToOperatingSystem(string directory)
    {
        try
        {
            var files = FontProvisioning.EnumerateFontFiles(directory).ToArray();
            if (files.Length == 0)
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                InstallLinux(files);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                InstallWindows(files);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[fonts] best-effort OS font install failed for '{directory}': {ex.Message}");
        }
    }

    private static void InstallLinux(IReadOnlyCollection<string> files)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (String.IsNullOrEmpty(home))
        {
            return;
        }

        var targetDir = Path.Combine(home, ".local", "share", "fonts", "gview");
        Directory.CreateDirectory(targetDir);

        foreach (var file in files)
        {
            var target = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, target, overwrite: true);
        }

        RunProcess("fc-cache", "-f");

        Console.WriteLine($"[fonts] copied {files.Count} font file(s) to '{targetDir}'");
    }

    private static void InstallWindows(IReadOnlyCollection<string> files)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (String.IsNullOrEmpty(localAppData))
        {
            return;
        }

        var targetDir = Path.Combine(localAppData, "Microsoft", "Windows", "Fonts");
        Directory.CreateDirectory(targetDir);

        // The app targets net10.0 (not net10.0-windows), so the registry API is not
        // available without an extra package - use reg.exe, which is always present.
        // Per-user font entries live under HKCU and store the absolute path as value.
        const string fontsKey = @"HKCU\Software\Microsoft\Windows NT\CurrentVersion\Fonts";

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var target = Path.Combine(targetDir, fileName);
            File.Copy(file, target, overwrite: true);

            RunProcess("reg", $"add \"{fontsKey}\" /v \"{Path.GetFileNameWithoutExtension(fileName)}\" " +
                              $"/t REG_SZ /d \"{target}\" /f");
        }

        Console.WriteLine($"[fonts] copied {files.Count} font file(s) to '{targetDir}' and registered them for the current user");
    }

    private static void RunProcess(string fileName, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            process?.WaitForExit(30_000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[fonts] '{fileName} {arguments}' could not be run: {ex.Message}");
        }
    }

    #endregion
}
