using gView.Framework.Common;

namespace gView.Framework.Tests.Common;

public class TileCacheFileTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gview-tilecachefile-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_folder, recursive: true); } catch { }
    }

    private string TilePath() => Path.Combine(_folder, "ul", "31256", "2500", "14042", "16173.pbf");

    [Fact]
    public async Task GetOrCreateAsync_ConcurrentRequestsOfOneTile_CreatesOnceAndSharesTheResult()
    {
        string path = TilePath();
        int created = 0;
        var release = new TaskCompletionSource();

        async Task<byte[]> Create()
        {
            Interlocked.Increment(ref created);
            await release.Task;   // all requests arrive while the first is still rendering
            return [1, 2, 3];
        }

        var requests = Enumerable.Range(0, 20).Select(_ => Task.Run(() => TileCacheFile.GetOrCreateAsync(path, Create))).ToArray();
        await Task.Delay(100);
        release.SetResult();
        var results = await Task.WhenAll(requests);

        Assert.Equal(1, created);
        Assert.All(results, r => Assert.Equal(new byte[] { 1, 2, 3 }, r));
    }

    [Fact]
    public async Task GetOrCreateAsync_AfterTheFirstFinished_CreatesAgain()
    {
        string path = TilePath();
        int created = 0;

        await TileCacheFile.GetOrCreateAsync(path, () => { created++; return Task.FromResult(new byte[] { 1 }); });
        await TileCacheFile.GetOrCreateAsync(path, () => { created++; return Task.FromResult(new byte[] { 1 }); });

        Assert.Equal(2, created);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreateFails_AllWaitingRequestsGetTheErrorAndTheNextCallCreatesAgain()
    {
        string path = TilePath();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TileCacheFile.GetOrCreateAsync(path, () => throw new InvalidOperationException("render failed")));

        var data = await TileCacheFile.GetOrCreateAsync(path, () => Task.FromResult(new byte[] { 7 }));
        Assert.Equal(new byte[] { 7 }, data);
    }

    [Fact]
    public async Task TryWriteAsync_ConcurrentWritersAndReaders_NoExceptionsAndOneCompleteFile()
    {
        string path = TilePath();
        var content = Enumerable.Range(0, 64 * 1024).Select(i => (byte)i).ToArray();

        var writers = Enumerable.Range(0, 20).Select(_ => Task.Run(() => TileCacheFile.TryWriteAsync(path, content)));
        var readers = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            var read = await TileCacheFile.TryReadAsync(path);
            // either not cached yet or complete: never a partial file
            Assert.True(read == null || read.Length == content.Length);
        }));

        var written = await Task.WhenAll(writers);
        await Task.WhenAll(readers);

        Assert.All(written, Assert.True);
        Assert.Equal(content, await File.ReadAllBytesAsync(path));
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(path)!, "*.tmp"));
    }

    [Fact]
    public async Task TryReadAsync_FileLockedByAWriter_ReturnsNullInsteadOfThrowing()
    {
        string path = TilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using (new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            Assert.Null(await TileCacheFile.TryReadAsync(path));
        }
    }

    [Fact]
    public async Task TryReadAsync_Missing_ReturnsNull()
    {
        Assert.Null(await TileCacheFile.TryReadAsync(TilePath()));
    }

    [Fact]
    public async Task TryWriteAsync_TargetFolderNotWritable_ReportsAWarningInsteadOfThrowing()
    {
        // a path below an existing file can't be created
        Directory.CreateDirectory(_folder);
        string blocker = Path.Combine(_folder, "file");
        await File.WriteAllBytesAsync(blocker, [0]);
        string warning = null;

        bool written = await TileCacheFile.TryWriteAsync(Path.Combine(blocker, "tile.pbf"), [1], w => warning = w);

        Assert.False(written);
        Assert.NotNull(warning);
    }
}
