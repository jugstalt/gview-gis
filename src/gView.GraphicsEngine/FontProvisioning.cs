using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.GraphicsEngine
{
    /// <summary>
    /// Shared helpers for the "font provisioning" feature: a graphics engine can be
    /// pointed at a directory of font files and will then resolve those font families
    /// for map rendering without the fonts being installed in the operating system.
    /// See <c>src/docs/design/font-provisioning.md</c>.
    /// </summary>
    static public class FontProvisioning
    {
        static private readonly string[] _fontExtensions = { ".ttf", ".otf", ".ttc" };

        /// <summary>
        /// Enumerates all font files (*.ttf, *.otf, *.ttc) below <paramref name="directory"/>
        /// (recursively). Returns an empty sequence when the directory is null/empty or
        /// does not exist.
        /// </summary>
        static public IEnumerable<string> EnumerateFontFiles(string directory)
        {
            if (String.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return Enumerable.Empty<string>();
            }

            return Directory
                .EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(f => _fontExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        }
    }
}
