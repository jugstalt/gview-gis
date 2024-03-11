using System.Collections.Generic;
using System.IO;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Extensions;

static internal class StringExtensions
{
    static public IDictionary<string, object?> GetFileProperties(this string path)
    {
        var properties = new Dictionary<string, object?>();

        try
        {
            FileInfo fi = new FileInfo(path);

            properties.Add("Size", fi.Length.ToSizeString());
        }
        catch { }

        return properties;
    }

    private static string ToSizeString(this long bytes)
    {
        const long scale = 1024;

        if (bytes < scale) return $"{bytes} B";

        long kb = bytes / scale;
        if (kb < scale) return $"{kb} KB";

        long mb = kb / scale;
        if (mb < scale) return $"{mb} MB";

        long gb = mb / scale;
        return $"{gb} GB";
    }
}
