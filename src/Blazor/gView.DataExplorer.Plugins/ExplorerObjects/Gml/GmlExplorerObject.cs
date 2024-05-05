using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Interoperability.OGC.Dataset.GML;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Gml;

[RegisterPlugIn("5767BD6D-1F74-4A60-900C-646463593F92")]
public class GmlExplorerObject : ExplorerObjectCls<IExplorerObject, IFeatureClass>,
                                 IExplorerFileObject,
                                 ISerializableExplorerObject,
                                 IExplorerObjectDeletable
{
    private string _filename = "";
    private Dataset? _gmlDataset = null;
    private IDatasetElement? _gml = null;

    public GmlExplorerObject() : base() { }
    private GmlExplorerObject(IExplorerObject parent, string filename)
        : base(parent, 2)
    {
        _filename = filename;
        OpenGml().Wait();
    }

    static private async Task<GmlExplorerObject> Create(IExplorerObject parent, string filename)
    {
        var exObject = new GmlExplorerObject(parent, filename);
        await exObject.OpenGml();

        return exObject;
    }

    #region IExplorerFileObject Member

    public string Filter
    {
        get
        {
            return "*.gml";
        }
    }

    public async Task<IExplorerFileObject?> CreateInstance(IExplorerObject parent, string filename)
    {
        try
        {
            if (!(new FileInfo(filename)).Exists)
            {
                return null;
            }

            return await GmlExplorerObject.Create(parent, filename);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region IExplorerObject Member

    public string Name
    {
        get
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);
                return fi.Name;
            }
            catch { return "???"; }
        }
    }

    public string FullName
    {
        get { return _filename; }
    }

    public string Type => "GML File";

    public string Icon
    {
        get
        {
            switch ((_gml?.Class as IFeatureClass)?.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return "webgis:shape-circle";
                case GeometryType.Polyline:
                    return "webgis:shape-polyline";
                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return "webgis:shape-polygon";
                default:
                    return "basic:code-markup";
            }
        }
    }

    public void Dispose()
    {
        if (_gmlDataset != null)
        {
            _gmlDataset.Dispose();
            _gmlDataset = null;
        }
    }

    async public Task<object?> GetInstanceAsync()
    {
        if (_gmlDataset == null)
        {
            await OpenGml();
        }

        return _gml?.Class;
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        IExplorerObject? obj = (cache != null && cache.Contains(FullName)) ?
            cache[FullName] :
            await CreateInstance(Parent, FullName);

        if (obj != null)
        {
            cache?.Append(obj);
        }

        return obj;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    async public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        if (_gmlDataset == null)
        {
            await OpenGml();
        }

        if (_gmlDataset != null)
        {
            FileInfo fi = new FileInfo(_filename);
            string name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

            if (_gmlDataset.Delete())
            {
                if (ExplorerObjectDeleted != null)
                {
                    ExplorerObjectDeleted(this);
                }
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    #endregion

    async private Task OpenGml()
    {
        FileInfo fi = new FileInfo(_filename);

        if (!fi.Exists)
        {
            throw new System.Exception($"{_filename} does not exits");
        }

        _gmlDataset = new Dataset();
        await _gmlDataset.SetConnectionString(fi.FullName);

        //
        // ToDo: not only the first 
        // GML can contain multiple featureclasses
        //
        if (!await _gmlDataset.Open()
            || (_gml = (await _gmlDataset.Elements()).FirstOrDefault()) == null)
        {
            _gmlDataset.Dispose();
            _gmlDataset = null;
        }
    }
}
