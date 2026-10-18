using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

// Content of a release ZIP file: entry path => source file.
// Files are collected first and written directly into the archive (no staging copy).
//
// Layout is the same as the former 7z archives: every directory has its own
// entry ("{version}/server/") written before its content. gView.Deploy looks up
// these directory entries (ZipArchive.GetEntry).
class ZipContent
{
    // already compressed => deflating again only costs time
    static readonly HashSet<string> StoredExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".br", ".gz", ".zip", ".nupkg", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".woff", ".woff2"
    };

    readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    public void AddDirectory(string sourceDirectory, string entryPrefix, bool overwrite)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        AddDirectoryEntry(entryPrefix);

        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            AddDirectoryEntry($"{entryPrefix}{ToEntryPath(Path.GetRelativePath(sourceDirectory, directory))}/");
        }
        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            AddFile(file, $"{entryPrefix}{ToEntryPath(Path.GetRelativePath(sourceDirectory, file))}", overwrite);
        }
    }

    public void AddFile(string sourceFile, string entryName, bool overwrite)
    {
        AddDirectoryEntry(ParentOf(entryName));

        if (overwrite || !_files.ContainsKey(entryName))
        {
            _files[entryName] = sourceFile;
        }
    }

    public void WriteZip(string zipFile, string rootDirectoryEntry)
    {
        var filesByDirectory = _files.Keys.ToLookup(ParentOf, StringComparer.OrdinalIgnoreCase);
        var directoriesByParent = _directories.Where(d => d != rootDirectoryEntry).ToLookup(ParentOf, StringComparer.OrdinalIgnoreCase);

        using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);

        void Write(string directoryEntry)
        {
            archive.CreateEntry(directoryEntry).ExternalAttributes = (int)FileAttributes.Directory;

            foreach (var entryName in filesByDirectory[directoryEntry].Order(StringComparer.OrdinalIgnoreCase))
            {
                var compressionLevel = StoredExtensions.Contains(Path.GetExtension(entryName))
                    ? CompressionLevel.NoCompression
                    : CompressionLevel.Optimal;

                archive.CreateEntryFromFile(_files[entryName], entryName, compressionLevel)
                    .ExternalAttributes = (int)FileAttributes.Archive;
            }
            foreach (var subDirectoryEntry in directoriesByParent[directoryEntry].OrderBy(d => d.TrimEnd('/'), StringComparer.OrdinalIgnoreCase))
            {
                Write(subDirectoryEntry);
            }
        }

        Write(rootDirectoryEntry);
    }

    void AddDirectoryEntry(string directoryEntry)
    {
        // "a/b/" => also "a/"
        while (!String.IsNullOrEmpty(directoryEntry) && _directories.Add(directoryEntry))
        {
            directoryEntry = ParentOf(directoryEntry);
        }
    }

    static string ParentOf(string entryName)
    {
        var index = entryName.TrimEnd('/').LastIndexOf('/');
        return index < 0 ? "" : entryName.Substring(0, index + 1);
    }

    static string ToEntryPath(string relativePath) => relativePath.Replace(Path.DirectorySeparatorChar, '/');
}
