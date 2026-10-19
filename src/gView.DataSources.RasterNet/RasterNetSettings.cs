using RasterNet;
using System;
using System.Collections.Generic;
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
    /// Rasters given as <c>http://</c> or <c>https://</c> URL (GeoTIFF/COG only) are read with range requests through a client
    /// shared by the process; these keys configure them:
    /// <list type="bullet">
    /// <item><c>http-timeout-seconds</c>: timeout of one request (default 30).</item>
    /// <item><c>http-retries</c>: retries after network errors, timeouts and HTTP 408/429/5xx (default 3).</item>
    /// <item><c>http-headers</c>: headers for every request, as <c>Name=Value;Name2=Value2</c> (e.g. an API key).</item>
    /// <item><c>http-probe-sidecars</c>: <c>true</c> to look for <c>.ovr</c> and <c>.aux.xml</c> next to the URL (default false).</item>
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

        private static HttpSourceOptions s_httpOptions = HttpSourceOptions.Default;

        /// <summary>How rasters given as URL are read: client, timeout, retries, headers, sidecar files.</summary>
        public static HttpSourceOptions HttpOptions => Volatile.Read(ref s_httpOptions);

        /// <summary>Replaces the HTTP settings for rasters opened from now on (e.g. with a client that has a proxy or signs requests).</summary>
        public static void ConfigureHttp(HttpSourceOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            Volatile.Write(ref s_httpOptions, options);
        }

        /// <summary>True for an absolute <c>http://</c> or <c>https://</c> URL.</summary>
        public static bool IsUrl(string path, out Uri uri)
        {
            uri = null;
            return path != null &&
                   (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
                   Uri.TryCreate(path, UriKind.Absolute, out uri);
        }

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

            var http = HttpSourceOptions.Default;
            ConfigureHttp(http with
            {
                Timeout = TimeSpan.FromSeconds(Parse(setting("http-timeout-seconds"), "http-timeout-seconds", http.Timeout.TotalSeconds) is > 0 and var s
                    ? s
                    : http.Timeout.TotalSeconds),
                MaxRetries = Parse(setting("http-retries"), "http-retries", http.MaxRetries),
                Headers = ParseHeaders(setting("http-headers")),
                ProbeSidecars = ParseBool(setting("http-probe-sidecars"), "http-probe-sidecars", http.ProbeSidecars),
            });
            return Describe();
        }

        /// <summary>The current settings in one line.</summary>
        public static string Describe()
        {
            var options = ReadOptions;
            string cache = options.BlockCache is { } c
                ? $"block cache {c.MaxBytes / (1024 * 1024)} MB, " + (c.MaxIdle is { } idle ? $"released after {idle.TotalMinutes:0.#} min idle" : "kept until the budget is needed")
                : "block cache disabled";
            var http = HttpOptions;
            string headers = http.Headers is { Count: > 0 } h ? $", {h.Count} header(s)" : "";
            string sidecars = http.ProbeSidecars ? ", sidecar files" : "";
            return $"{cache}, {options.MaxDegreeOfParallelism} thread(s) per request; " +
                   $"URLs: timeout {http.Timeout.TotalSeconds:0.#} s, {http.MaxRetries} retries{headers}{sidecars}";
        }

        /// <summary><c>Name=Value;Name2=Value2</c>; null if empty.</summary>
        private static IReadOnlyDictionary<string, string> ParseHeaders(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int equals = part.IndexOf('=');
                if (equals <= 0)
                {
                    throw new FormatException($"rasternet:http-headers must be Name=Value pairs separated by ';', not '{part}'.");
                }
                headers[part[..equals].Trim()] = part[(equals + 1)..].Trim();
            }
            return headers;
        }

        private static bool ParseBool(string text, string key, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return fallback;
            }
            return bool.TryParse(text.Trim(), out bool value)
                ? value
                : throw new FormatException($"rasternet:{key} must be true or false, not '{text}'.");
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
