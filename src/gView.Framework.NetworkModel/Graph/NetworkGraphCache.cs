using gView.Framework.NetworkModel.Store;
using System.Collections.Concurrent;

namespace gView.Framework.NetworkModel.Graph;

/// <summary>
/// One shared <see cref="FileNetworkGraph"/> per network directory. Each call checks network.json and opens the network
/// again when a new build replaced it; searches still running keep the graph they got.
/// </summary>
public sealed class NetworkGraphCache
{
    public static NetworkGraphCache Shared { get; } = new();

    private sealed record Entry(FileNetworkGraph Graph, DateTime Stamp);

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _openLock = new();

    /// <summary>The shard limit of graphs opened from now on; 0 for no limit.</summary>
    public int MaxLoadedShards { get; set; } = 512;

    public FileNetworkGraph Get(string directory)
    {
        var key = Path.GetFullPath(directory);
        var stamp = Stamp(key);
        if (_entries.TryGetValue(key, out var entry) && (entry.Stamp == stamp || stamp == DateTime.MinValue))
        {
            // MinValue: the directory is being swapped right now, keep the old network for this call
            return entry.Graph;
        }

        lock (_openLock)
        {
            if (_entries.TryGetValue(key, out entry) && entry.Stamp == stamp)
            {
                return entry.Graph;
            }
            var graph = new FileNetworkGraph(key) { MaxLoadedShards = MaxLoadedShards };
            _entries[key] = new Entry(graph, stamp);
            return graph;
        }
    }

    /// <summary>Forgets a network (or all with null); it is opened again on the next call.</summary>
    public void Invalidate(string? directory = null)
    {
        if (directory is null)
        {
            _entries.Clear();
        }
        else
        {
            _entries.TryRemove(Path.GetFullPath(directory), out _);
        }
    }

    private static DateTime Stamp(string directory)
    {
        var path = Path.Combine(directory, FileNetworkStore.MetadataFile);
        return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
    }
}
