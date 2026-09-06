using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("gView.DataSources.SpatiaLite.Tests")]
[assembly: InternalsVisibleTo("gView.DataSources.Fdb.SQLite")]

namespace gView.DataSources.SpatiaLite
{
    /// <summary>
    /// SpatiaLite / GeoPackage storage layout of the opened SQLite file.
    /// </summary>
    internal enum SpatiaLiteFlavor
    {
        SpatiaLite,
        GeoPackage
    }

    /// <summary>
    /// Locates the bundled <c>mod_spatialite</c> native extension and loads it into
    /// <see cref="SQLiteConnection"/>s. All spatial SQL used by
    /// <see cref="SpatiaLiteDataset"/> (GeomFromWKB, ST_AsBinary, ST_Intersects,
    /// CreateSpatialIndex, EnableGpkgAmphibiousMode, …) comes from this extension.
    /// </summary>
    internal static class SpatiaLiteNative
    {
        private static readonly object _gate = new object();
        private static bool _probed;
        private static bool _available;
        private static string _error = String.Empty;
        private static string _modSpatialitePath = "mod_spatialite";

        /// <summary>Resolved path (or bare name) passed to <c>LoadExtension</c>.</summary>
        public static string ModSpatialitePath => _modSpatialitePath;

        /// <summary>
        /// One-time probe: resolve the library, put its folder on the native search
        /// path and verify it loads (<c>spatialite_version()</c>). Result is cached.
        /// </summary>
        public static bool EnsureAvailable(out string error)
        {
            lock (_gate)
            {
                if (_probed)
                {
                    error = _error;
                    return _available;
                }

                _probed = true;
                try
                {
                    _modSpatialitePath = ResolvePath();
                    PrepareNativeSearchPath(_modSpatialitePath);

                    using (var connection = new SQLiteConnection("Data Source=:memory:"))
                    {
                        connection.Open();
                        connection.EnableExtensions(true);
                        connection.LoadExtension(_modSpatialitePath);

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT spatialite_version()";
                            var version = command.ExecuteScalar()?.ToString();

                            _available = !String.IsNullOrEmpty(version);
                            _error = _available
                                ? String.Empty
                                : "spatialite_version() returned no value";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _available = false;
                    _error = $"could not load '{_modSpatialitePath}': {ex.Message}";
                }

                error = _error;
                return _available;
            }
        }

        /// <summary>
        /// Enables and loads <c>mod_spatialite</c> on an already-open connection.
        /// Call once per freshly opened connection (extensions do not survive a
        /// connection being returned to the pool and re-handed out as a new handle).
        /// </summary>
        public static void LoadInto(SQLiteConnection connection)
        {
            if (!_probed)
            {
                EnsureAvailable(out _);
            }

            connection.EnableExtensions(true);
            connection.LoadExtension(_modSpatialitePath);
        }

        private static string FileName =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "mod_spatialite.dll" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "mod_spatialite.dylib" :
            "mod_spatialite.so";

        private static string ResolvePath()
        {
            // 1) explicit override
            var overridePath = Environment.GetEnvironmentVariable("GVIEW_MOD_SPATIALITE");
            if (!String.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
            {
                return overridePath;
            }

            // 2) binaries bundled next to the application
            foreach (var dir in NativeProbeDirectories())
            {
                var candidate = Path.Combine(dir, FileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            // 3) a local GIS install (QGIS / OSGeo4W on Windows) or a system package
            foreach (var candidate in SystemProbeCandidates())
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            // 4) last resort: hand the bare name to SQLite and let the OS loader find it
            //    (PATH / LD_LIBRARY_PATH)
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "mod_spatialite" : FileName;
        }

        private static string[] NativeProbeDirectories()
        {
            var baseDir = AppContext.BaseDirectory;

            return new[]
            {
                Path.Combine(baseDir, "runtimes", RuntimeInformation.RuntimeIdentifier, "native"),
                Path.Combine(baseDir, "runtimes", GenericRid(), "native"),
                baseDir
            };
        }

        /// <summary>
        /// Well-known locations of a system-wide <c>mod_spatialite</c>: a QGIS or OSGeo4W
        /// install on Windows, the distro package on Linux, Homebrew on macOS, plus every
        /// directory already on <c>PATH</c>.
        /// </summary>
        private static IEnumerable<string> SystemProbeCandidates()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var roots = new[]
                {
                    Environment.GetEnvironmentVariable("ProgramFiles"),
                    Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                    Environment.GetEnvironmentVariable("ProgramW6432"),
                    @"C:\OSGeo4W",
                    @"C:\OSGeo4W64",
                };

                foreach (var root in roots)
                {
                    if (String.IsNullOrEmpty(root) || !SafeDirExists(root))
                    {
                        continue;
                    }

                    if (root.IndexOf("OSGeo4W", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        yield return Path.Combine(root, "bin", FileName);
                        continue;
                    }

                    yield return Path.Combine(root, "OSGeo4W", "bin", FileName);

                    foreach (var qgisDir in SafeEnumerateDirectories(root, "QGIS*"))
                    {
                        yield return Path.Combine(qgisDir, "bin", FileName);

                        foreach (var appDir in SafeEnumerateDirectories(Path.Combine(qgisDir, "apps"), "qgis*"))
                        {
                            yield return Path.Combine(appDir, "bin", FileName);
                        }
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                yield return "/opt/homebrew/lib/mod_spatialite.dylib";
                yield return "/usr/local/lib/mod_spatialite.dylib";
                yield return "/opt/homebrew/lib/mod_spatialite.8.dylib";
            }
            else
            {
                yield return "/usr/lib/x86_64-linux-gnu/mod_spatialite.so";
                yield return "/usr/lib/aarch64-linux-gnu/mod_spatialite.so";
                yield return "/usr/lib64/mod_spatialite.so";
                yield return "/usr/lib/mod_spatialite.so";
                yield return "/usr/local/lib/mod_spatialite.so";
            }

            // anything already on PATH
            var pathVar = Environment.GetEnvironmentVariable("PATH") ?? String.Empty;
            foreach (var pathDir in pathVar.Split(Path.PathSeparator))
            {
                if (!String.IsNullOrWhiteSpace(pathDir))
                {
                    yield return Path.Combine(pathDir.Trim(), FileName);
                }
            }
        }

        private static bool SafeDirExists(string path)
        {
            try { return Directory.Exists(path); }
            catch { return false; }
        }

        private static IEnumerable<string> SafeEnumerateDirectories(string root, string pattern)
        {
            try
            {
                return Directory.EnumerateDirectories(root, pattern)
                                .OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static string GenericRid()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.Is64BitProcess ? "win-x64" : "win-x86";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
            }

            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
        }

        /// <summary>
        /// On Windows, make sure <c>mod_spatialite.dll</c>'s dependency DLLs (geos_c, proj,
        /// libxml2, iconv, freexl, …) resolve from its own folder: prepend that folder to
        /// the process <c>PATH</c>, register it with the loader, and pre-load the library by
        /// full path with <c>LOAD_WITH_ALTERED_SEARCH_PATH</c> so SQLite's later
        /// <c>LoadExtension</c> just picks up the already-resident module.
        /// </summary>
        private static void PrepareNativeSearchPath(string modSpatialitePath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            try
            {
                if (!File.Exists(modSpatialitePath))
                {
                    return; // bare-name fallback - nothing to prepare
                }

                var fullPath = Path.GetFullPath(modSpatialitePath);
                var dir = Path.GetDirectoryName(fullPath);
                if (String.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                {
                    return;
                }

                var path = Environment.GetEnvironmentVariable("PATH") ?? String.Empty;
                if (path.IndexOf(dir, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    Environment.SetEnvironmentVariable("PATH", dir + Path.PathSeparator + path);
                }

                SetDllDirectory(dir);
                LoadLibraryEx(fullPath, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
            }
            catch
            {
                // best effort - LoadExtension will still try and surface a clear error
            }
        }

        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);
    }
}
