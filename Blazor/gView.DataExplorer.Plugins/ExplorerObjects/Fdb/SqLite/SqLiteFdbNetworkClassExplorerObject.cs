using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("AFDE90FF-D063-4224-BD31-1B30C266D55B")]
public class SqLiteFdbNetworkClassExplorerObject : ExplorerObjectCls, 
                                                   IExplorerSimpleObject, 
                                                   IExplorerObjectCreatable
{
    public SqLiteFdbNetworkClassExplorerObject() : base(null, typeof(SQLiteFDBNetworkFeatureClass), 1) { }

    #region IExplorerObject Member

    public string Name
    {
        get { return String.Empty; }
    }

    public string FullName
    {
        get { return String.Empty; }
    }

    public string Type
    {
        get { return "Network Class"; }
    }

    public string Icon => "webgis:construct-edge-intersect";

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    #endregion

    #region IDisposable Member

    public void Dispose()
    {

    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        if (parentExObject is SqLiteFdbDatasetExplorerObject)
        {
            return true;
        }

        return false;
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope scope, IExplorerObject? parentExObject)
    {
        if (!(parentExObject is SqLiteFdbDatasetExplorerObject))
        {
            return null;
        }

        SqLiteFdbDatasetExplorerObject parent = (SqLiteFdbDatasetExplorerObject)parentExObject;

        throw new NotImplementedException();

        //IFeatureDataset dataset = await ((SqLiteFdbDatasetExplorerObject)parentExObject).GetInstanceAsync() as IFeatureDataset;
        //if (dataset == null || !(dataset.Database is SQLiteFDB))
        //{
        //    return null;
        //}

        //FormNewNetworkclass dlg = new FormNewNetworkclass(dataset, typeof(CreateFDBNetworkFeatureclass));
        //if (dlg.ShowDialog() != DialogResult.OK)
        //{
        //    return null;
        //}

        //CreateFDBNetworkFeatureclass creator = new CreateFDBNetworkFeatureclass(
        //    dataset, dlg.NetworkName,
        //    dlg.EdgeFeatureclasses,
        //    dlg.NodeFeatureclasses);
        //creator.SnapTolerance = dlg.SnapTolerance;
        //creator.ComplexEdgeFcIds = await dlg.ComplexEdgeFcIds();
        //creator.GraphWeights = dlg.GraphWeights;
        //creator.SwitchNodeFcIdAndFieldnames = dlg.SwitchNodeFcIds;
        //creator.NodeTypeFcIds = dlg.NetworkNodeTypeFcIds;

        //FormTaskProgress progress = new FormTaskProgress();
        //progress.ShowProgressDialog(creator, creator.Run());

        //IDatasetElement element = await dataset.Element(dlg.NetworkName);
        //return new SqLiteFdbFeatureClassExplorerObject(
        //                        parent,
        //                        parent.FileName,
        //                        parent.Name,
        //                        element);
    }

    #endregion
}
