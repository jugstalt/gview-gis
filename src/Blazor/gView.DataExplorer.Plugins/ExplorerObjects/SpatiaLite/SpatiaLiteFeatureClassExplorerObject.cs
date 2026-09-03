using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Extensions;
using gView.DataSources.SpatiaLite;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.SpatiaLite;

[RegisterPlugIn("52b80768-20ce-4baa-9b5a-e0df586a13f5")]
public class SpatiaLiteFeatureClassExplorerObject :
                    ExplorerObjectCls<SpatiaLiteExplorerObject, IFeatureClass>,
                    IExplorerSimpleObject,
                    ISerializableExplorerObject,
                    IExplorerObjectDeletable,
                    IPlugInDependencies
{
    private readonly string _fcName = "";
    private readonly string _type = "";
    private readonly string _icon = "basic:table";
    private IFeatureClass? _fc;

    public SpatiaLiteFeatureClassExplorerObject() : base() { }

    public SpatiaLiteFeatureClassExplorerObject(SpatiaLiteExplorerObject parent, IDatasetElement element)
        : base(parent, 1)
    {
        if (element?.Class is not IFeatureClass featureClass)
        {
            return;
        }

        _fcName = element.Title;
        _fc = featureClass;

        switch (featureClass.GeometryType)
        {
            case GeometryType.Envelope:
            case GeometryType.Polygon:
                _icon = "webgis:shape-polygon";
                _type = "Polygon Featureclass";
                break;
            case GeometryType.Point:
            case GeometryType.Multipoint:
                _icon = "basic:dot-filled";
                _type = "Point Featureclass";
                break;
            case GeometryType.Polyline:
                _icon = "webgis:shape-polyline";
                _type = "Polyline Featureclass";
                break;
            default:
                _icon = "basic:table";
                _type = "Featureclass";
                break;
        }
    }

    #region IExplorerObject Members

    public string Name => _fcName;

    public string FullName => base.Parent.IsNull() ? "" : $@"{base.Parent.FullName}\{Name}";

    public string? Type => _type;

    public string Icon => _icon;

    public void Dispose() => _fc = null;

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(_fc);

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache != null && cache.Contains(FullName))
        {
            return cache[FullName];
        }

        FullName = FullName.Replace("/", @"\");
        int lastIndex = FullName.LastIndexOf(@"\");
        if (lastIndex == -1)
        {
            return null;
        }

        string dsName = FullName.Substring(0, lastIndex);
        string fcName = FullName.Substring(lastIndex + 1);

        var dsObject = await new SpatiaLiteExplorerObject()
                                    .CreateInstanceByFullName(dsName, cache) as SpatiaLiteExplorerObject;
        if (dsObject == null)
        {
            return null;
        }

        foreach (var exObject in await dsObject.ChildObjects())
        {
            if (exObject.Name == fcName)
            {
                cache?.Append(exObject);
                return exObject;
            }
        }

        return null;
    }

    #endregion

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    async public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        if (base.Parent.IsNull())
        {
            return false;
        }

        var instance = await base.Parent.GetInstanceAsync();
        if (instance is IFeatureDatabase featureDatabase)
        {
            if (await featureDatabase.DeleteFeatureClass(Name))
            {
                ExplorerObjectDeleted?.Invoke(this);
                return true;
            }

            throw new GeneralException("ERROR: " + featureDatabase.LastErrorMessage);
        }

        return false;
    }

    #endregion

    #region IPlugInDependencies Member

    public bool HasUnsolvedDependencies() => SpatiaLiteDataset.HasUnsolvedDependenciesStatic;

    #endregion
}
