#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.Framework.Common;

/// <summary>
/// Reading and writing tile cache files when tiles are rendered on the fly, safe for concurrent requests of the
/// same tile: clients often request a tile several times at once.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="GetOrCreateAsync"/>: the same tile is created once per process; concurrent requests wait for
/// that result.</item>
/// <item><see cref="TryReadAsync"/>: a file that is locked (being written or replaced, also by another process) is
/// treated as not cached instead of failing the request.</item>
/// <item><see cref="TryWriteAsync"/>: a temp file, then moved without overwriting. If the file exists meanwhile,
/// the other writer has won. A failed write is only logged: the tile is delivered anyway.</item>
/// </list>
/// </remarks>
public static class TileCacheFile
{
    private static readonly ConcurrentDictionary<string, Lazy<Task<byte[]>>> InFlight =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The result of <paramref name="create"/> for <paramref name="key"/> (e.g. the cache path of a tile), created
    /// once while concurrent calls with the same key wait for it. The entry is removed when the task finishes, so a
    /// later call creates again (or finds the cached file).
    /// </summary>
    public static Task<byte[]> GetOrCreateAsync(string key, Func<Task<byte[]>> create)
    {
        Lazy<Task<byte[]>>? lazy = null;
        lazy = new Lazy<Task<byte[]>>(async () =>
        {
            try
            {
                return await create();
            }
            finally
            {
                // only this entry: a newer one for the same key must stay
                ((ICollection<KeyValuePair<string, Lazy<Task<byte[]>>>>)InFlight)
                    .Remove(new KeyValuePair<string, Lazy<Task<byte[]>>>(key, lazy!));
            }
        });

        return InFlight.GetOrAdd(key, lazy).Value;
    }

    /// <summary>
    /// The content of a cached file, or <c>null</c> if it doesn't exist or can't be read right now (locked by a
    /// concurrent writer, also of another process): the caller treats it as not cached.
    /// </summary>
    public static async Task<byte[]?> TryReadAsync(string path)
    {
        try
        {
            // FileShare.Delete: a concurrent replace of the file doesn't fail because of this reader
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                                              FileShare.Read | FileShare.Delete, 4096, useAsync: true);
            var data = new byte[stream.Length];
            await stream.ReadExactlyAsync(data, 0, data.Length);
            return data;
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    /// <summary>
    /// Writes a cache file: a temp file in the same folder, then moved to <paramref name="path"/> without overwriting.
    /// Returns <c>true</c> if the file exists afterwards (written here or by a concurrent writer). Failures are
    /// passed to <paramref name="logWarning"/>, never thrown: caching must not stop delivering the tile.
    /// </summary>
    public static async Task<bool> TryWriteAsync(string path, byte[] data, Action<string>? logWarning = null)
    {
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            if (File.Exists(path))
            {
                return true;   // a concurrent writer (or another process) was faster
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(temp, data);

            try
            {
                File.Move(temp, path, overwrite: false);
            }
            catch (IOException) when (File.Exists(path))
            {
                // written meanwhile by another writer: theirs is the same tile
            }
            catch (UnauthorizedAccessException) when (File.Exists(path))
            {
            }

            return true;
        }
        catch (Exception ex)
        {
            logWarning?.Invoke($"Tile cache file {path} not written: {ex.Message}");
            return File.Exists(path);
        }
        finally
        {
            if (File.Exists(temp))
            {
                try { File.Delete(temp); } catch { }
            }
        }
    }
}
