#nullable enable

using gView.Framework.Common;
using System.Collections.Concurrent;
using System.IO;

namespace gView.Framework.IO;
static public class FileInfoFactory
{
    static public FileInfo Create(string path)
        => new FileInfo(ReplaceAliases(path));

    static private ConcurrentDictionary<string, string>? _aliases;

    static public bool AddAlias(string path, string alias)
    {
        if(_aliases is null)
        {
            _aliases = new();
        }

        return _aliases.TryAdd(path, alias);   
    }

    static public string ReplaceAliases(string path)
    {
        if (_aliases is not null)
        {
            foreach (var key in _aliases.Keys)
            {
                if (path.StartsWith(key, SystemInfo.IsWindows ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal))
                {
                    return $"{_aliases[key]}{path.Substring(key.Length)}";
                }
            }
        }

        return path;
    }
}
