using gView.DataExplorer.Core.Models.Content;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Tiles;

[RegisterPlugIn("15A4512C-78CC-43EC-9DBA-DCCEE1D1406E")]
public class TileBundleFileObject : ExplorerObjectCls<IExplorerObject, TileBundleContent>,
                                   IExplorerFileObject
{
    private string _filePath = "";

    public TileBundleFileObject() : base() { }
    public TileBundleFileObject(IExplorerObject parent, string filePath)
        : base(parent, 2)
    {
        _filePath = filePath;
    }

    public string Filter => "*.tilebundle";

    public string Name
    {
        get
        {
            try
            {
                var fi = new FileInfo(_filePath);
                return fi.Name;
            }
            catch { return ""; }
        }
    }

    public string FullName => _filePath;

    public string? Type => "Compact Tile Bundle File";

    public string Icon => "basic:square-medium";

    public Task<IExplorerFileObject?> CreateInstance(IExplorerObject parent, string filePath)
    {
        try
        {
            if (!(new FileInfo(filePath).Exists))
            {
                return Task.FromResult<IExplorerFileObject?>(null);
            }
        }
        catch
        {
            return Task.FromResult<IExplorerFileObject?>(null);
        }

        return Task.FromResult<IExplorerFileObject?>(new TileBundleFileObject(parent, filePath));
    }

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        IExplorerObject? obj = (cache?.Contains(FullName) == true) ? cache[FullName] : await CreateInstance(new NullParentExplorerObject(), FullName);

        if (obj != null)
        {
            cache?.Append(obj);
        }

        return obj;
    }

    public void Dispose()
    {

    }

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(new TileBundleContent(_filePath));
    }
}
