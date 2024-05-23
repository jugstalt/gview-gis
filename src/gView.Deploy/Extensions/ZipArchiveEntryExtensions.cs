using System.IO.Compression;

namespace gView.Deploy.Extensions;

static internal class ZipArchiveEntryExtensions
{
    static public bool IsDirectory(this ZipArchiveEntry entry)
    {
        // https://stackoverflow.com/questions/40223451/how-to-tell-if-a-ziparchiveentry-is-directory

        return entry.Length == 0 && String.IsNullOrEmpty(entry.Name) &&
            (entry.FullName.EndsWith("/") || entry.FullName.EndsWith(@"\"));
    }
}
