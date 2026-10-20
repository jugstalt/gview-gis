using gView.Framework.IO;

namespace gView.DataSources.RasterNet.Tests;

/// <summary>
/// RasterNet as fallback for the GDAL (file list) and MrSID (directory) raster file datasets.
/// No real raster files: an unreadable file stands for a format RasterNet can't read.
/// </summary>
public class DatasetTests : IDisposable
{
    private readonly string _directory;

    public DatasetTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"gview-rasternet-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }

    private string CreateUnreadableFile(string name)
    {
        var filename = Path.Combine(_directory, name);
        File.WriteAllText(filename, "not a raster");
        return filename;
    }

    private static XmlStream Roundtrip(Action<XmlStream> save)
    {
        var stream = new XmlStream("dataset");
        save(stream);

        using var memory = new MemoryStream();
        stream.WriteStream(memory);
        memory.Position = 0;

        var result = new XmlStream("");
        result.ReadStream(memory);
        return result;
    }

    [Fact]
    public async Task LoadAsync_MrSidDirectoryStream_SavesDirectoryAgain()
    {
        var mrSidStream = Roundtrip(s => s.Save("Directory", _directory));
        var dataset = new Dataset();

        await dataset.LoadAsync(mrSidStream);
        var saved = Roundtrip(dataset.Save);

        Assert.Equal(_directory, dataset.ConnectionString);
        Assert.Equal(_directory, saved.Load("Directory", null));
        Assert.Null(saved.Load("filename", null));
    }

    [Fact]
    public async Task Element_SidFileInDirectoryMode_NullWithClearError()
    {
        var dataset = new Dataset();
        await dataset.LoadAsync(Roundtrip(s => s.Save("Directory", _directory)));

        var element = await dataset.Element("image.sid");

        Assert.Null(element);
        Assert.Contains("MrSID", dataset.LastErrorMessage);
    }

    [Fact]
    public async Task Element_ErrorOfOneLayer_IsResetForTheNextLayer()
    {
        var dataset = new Dataset();
        await dataset.LoadAsync(Roundtrip(s => s.Save("Directory", _directory)));
        await dataset.Element("image.sid");

        await dataset.Element("missing.tif");

        Assert.Equal("", dataset.LastErrorMessage);
    }

    [Fact]
    public async Task SetConnectionString_UnreadableFile_KeptWithoutDatasetError()
    {
        var unreadable = CreateUnreadableFile("unreadable.tif");
        var dataset = new Dataset();

        await dataset.SetConnectionString(unreadable);

        Assert.Equal("", dataset.LastErrorMessage);
        Assert.Equal(unreadable, dataset.ConnectionString);
        Assert.Empty(await dataset.Elements());
    }

    [Fact]
    public async Task Element_UnreadableFileOfConnectionString_ReportsItsError()
    {
        var unreadable = CreateUnreadableFile("unreadable.tif");
        var dataset = new Dataset();
        await dataset.SetConnectionString(unreadable);

        var element = await dataset.Element("unreadable.tif");

        Assert.Null(element);
        Assert.StartsWith("unreadable.tif:", dataset.LastErrorMessage);
    }

    [Fact]
    public async Task SetConnectionString_Directory_IsNoDirectoryMode()
    {
        var dataset = new Dataset();

        await dataset.SetConnectionString(_directory);
        var saved = Roundtrip(dataset.Save);

        Assert.Null(saved.Load("Directory", null));
        Assert.Equal(_directory, saved.Load("filename", null));
    }
}
