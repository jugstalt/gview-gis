using RasterNet;
using System;
using System.Globalization;
using System.Numerics;
using System.Threading;

namespace gView.DataSources.RasterNet
{
    /// <summary>
    /// Process-wide read settings of the RasterNet data source: one block cache shared by all rasters and requests,
    /// and the number of threads one request may use for decoding. Hosts configure it once at startup from their
    /// configuration (section "rasternet"); without configuration the defaults apply.
    /// </summary>
    /// <remarks>
    /// The block cache keeps decoded JPEG2000 code-blocks, so overlapping and neighboring map requests reuse them;
    /// for 512 x 512 windows of orthophotos this roughly doubles the throughput. Configuration keys:
    /// <list type="bullet">
    /// <item><c>blockcache-mb</c>: budget of the cache in MB (default 256); 0 disables the cache.</item>
    /// <item><c>blockcache-maxidle-minutes</c>: blocks not used for this long are released (default 10); 0 keeps them until the budget is needed.</item>
    /// <item><c>max-threads</c>: threads per request (default and 0: half of the cores, at most 8).</item>
    /// </list>
    /// </remarks>
    public static class RasterNetSettings
    {
        public const long DefaultBlockCacheMegabytes = 256;

        public static readonly TimeSpan DefaultBlockCacheMaxIdle = TimeSpan.FromMinutes(10);

        public static readonly int DefaultMaxThreads = Math.Clamp(Environment.ProcessorCount / 2, 1, 8);

        private static ReadOptions s_readOptions = Create(DefaultBlockCacheMegabytes, DefaultBlockCacheMaxIdle, DefaultMaxThreads);

        /// <summary>The options for all reads of the data source.</summary>
        public static ReadOptions ReadOptions => Volatile.Read(ref s_readOptions);

        /// <summary>The shared block cache, or null if it is disabled.</summary>
        public static BlockCache BlockCache => ReadOptions.BlockCache;

        /// <summary>Replaces the settings; the blocks of the previous cache are released.</summary>
        /// <param name="blockCacheMegabytes">Budget of the block cache in MB; 0 disables it.</param>
        /// <param name="blockCacheMaxIdle">Blocks not used for this long are released; null keeps them until the budget is needed.</param>
        /// <param name="maxThreads">Threads one request may use for decoding.</param>
        public static void Configure(long blockCacheMegabytes, TimeSpan? blockCacheMaxIdle, int maxThreads)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(blockCacheMegabytes);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxThreads, 1);

            var previous = Interlocked.Exchange(ref s_readOptions, Create(blockCacheMegabytes, blockCacheMaxIdle, maxThreads));
            previous.BlockCache?.Dispose();
        }

        /// <summary>
        /// Configures from key/value settings, e.g. <c>key => configuration[$"rasternet:{key}"]</c>; missing keys keep
        /// their defaults. Returns a one-line description of the result for the startup log.
        /// </summary>
        public static string Configure(Func<string, string> setting)
        {
            ArgumentNullException.ThrowIfNull(setting);

            long megabytes = Parse(setting("blockcache-mb"), "blockcache-mb", DefaultBlockCacheMegabytes);
            double idleMinutes = Parse(setting("blockcache-maxidle-minutes"), "blockcache-maxidle-minutes", DefaultBlockCacheMaxIdle.TotalMinutes);
            int threads = Parse(setting("max-threads"), "max-threads", DefaultMaxThreads) is > 0 and var t ? t : DefaultMaxThreads;

            Configure(megabytes, idleMinutes > 0 ? TimeSpan.FromMinutes(idleMinutes) : null, threads);
            return Describe();
        }

        /// <summary>The current settings in one line.</summary>
        public static string Describe()
        {
            var options = ReadOptions;
            string cache = options.BlockCache is { } c
                ? $"block cache {c.MaxBytes / (1024 * 1024)} MB, " + (c.MaxIdle is { } idle ? $"released after {idle.TotalMinutes:0.#} min idle" : "kept until the budget is needed")
                : "block cache disabled";
            return $"{cache}, {options.MaxDegreeOfParallelism} thread(s) per request";
        }

        private static ReadOptions Create(long megabytes, TimeSpan? maxIdle, int maxThreads) => new()
        {
            MaxDegreeOfParallelism = maxThreads,
            BlockCache = megabytes > 0
                ? new BlockCache(new BlockCacheOptions { MaxBytes = megabytes * 1024 * 1024, MaxIdle = maxIdle })
                : null,
        };

        private static T Parse<T>(string text, string key, T fallback) where T : INumber<T>
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return fallback;
            }
            if (!T.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value < T.Zero)
            {
                throw new FormatException($"rasternet:{key} must be a non-negative number, not '{text}'.");
            }
            return value;
        }
    }
}
