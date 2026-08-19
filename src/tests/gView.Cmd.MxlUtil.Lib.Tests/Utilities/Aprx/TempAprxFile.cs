using System.IO.Compression;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Builds a temporary .aprx file (a ZIP archive of JSON entries) on disk for
/// <c>AprxReader</c> tests, and deletes it again on <see cref="Dispose"/>.
/// </summary>
internal sealed class TempAprxFile : IDisposable
{
    public string Path { get; }

    private TempAprxFile(string path) => Path = path;

    /// <summary>Creates a temp .aprx file containing the given (entryName, jsonContent) pairs.</summary>
    public static TempAprxFile Create(params (string EntryName, string Content)[] entries)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"aprxreadertest_{Guid.NewGuid():N}.aprx");

        using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var stream = entry.Open();
                using var writer = new StreamWriter(stream);
                writer.Write(content);
            }
        }

        return new TempAprxFile(path);
    }

    /// <summary>Creates an empty (zero-entry) temp .aprx file.</summary>
    public static TempAprxFile CreateEmpty()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"aprxreadertest_{Guid.NewGuid():N}.aprx");
        using (ZipFile.Open(path, ZipArchiveMode.Create)) { }
        return new TempAprxFile(path);
    }

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}
