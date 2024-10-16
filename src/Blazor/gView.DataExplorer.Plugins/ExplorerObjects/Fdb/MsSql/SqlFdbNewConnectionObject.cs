﻿using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Fdb.MsSql.Extensions;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.MsSql;

[RegisterPlugIn("8368C71F-51CE-42E8-B167-BE9AE414C1C8")]
public class SqlFdbNewConnectionObject : ExplorerObjectCls<SqlFdbExplorerGroupObject>,
                                         IExplorerSimpleObject,
                                         IExplorerObjectDoubleClick,
                                         IExplorerObjectCreatable
{
    public SqlFdbNewConnectionObject()
        : base() { }
    public SqlFdbNewConnectionObject(SqlFdbExplorerGroupObject parent)
        : base(parent, 0) { }

    #region IExplorerSimpleObject Members

    public string Icon => "basic:round-plus";

    #endregion

    #region IExplorerObject Members

    public string Name => "New Connection...";

    public string FullName => "";

    public string Type => "New SqlFDB Connection";

    public void Dispose()
    {

    }

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(null);

    #endregion

    #region IExplorerObjectDoubleClick Members

    async public Task ExplorerObjectDoubleClick(IExplorerApplicationScopeService appScope, ExplorerObjectEventArgs e)
    {
        var model = await appScope.ShowKnownDialog(KnownDialogs.ConnectionString,
                                                                    model: new ConnectionStringModel("mssql", false));

        if (model != null)
        {
            e.NewExplorerObject = model.DbConnectionString.ToSqlFdbExplorerObject(this.TypedParent);
        }
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache?.Contains(FullName) == true)
        {
            return Task.FromResult<IExplorerObject?>(cache[FullName]);
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        return (parentExObject is SqlFdbExplorerGroupObject);
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScopeService appScope, IExplorerObject parentExObject)
    {
        ExplorerObjectEventArgs e = new ExplorerObjectEventArgs();
        await ExplorerObjectDoubleClick(appScope, e);
        return e.NewExplorerObject;
    }

    #endregion
}

//[RegisterPlugIn("97914E6A-3084-4fc0-8B31-4A6D2C990F72")]
//public class SqlFDBNetworkClassExplorerObject : ExplorerObjectCls, 
//                                                IExplorerSimpleObject, 
//                                                IExplorerObjectCreatable
//{
//    public SqlFDBNetworkClassExplorerObject() : base(null, typeof(SqlFDBNetworkFeatureclass), 0) { }

//    #region IExplorerObject Member

//    public string Name
//    {
//        get { return String.Empty; }
//    }

//    public string FullName
//    {
//        get { return String.Empty; }
//    }

//    public string Type
//    {
//        get { return "Network Class"; }
//    }

//    public IExplorerIcon Icon
//    {
//        get { return new AccessFDBNetworkIcon(); }
//    }

//    public Task<object> GetInstanceAsync()
//    {
//        return Task.FromResult<object>(null);
//    }

//    #endregion

//    #region IDisposable Member

//    public void Dispose()
//    {

//    }

//    #endregion

//    #region ISerializableExplorerObject Member

//    public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
//    {
//        return Task.FromResult<IExplorerObject>(null);
//    }

//    #endregion

//    #region IExplorerObjectCreatable Member

//    public bool CanCreate(IExplorerObject parentExObject)
//    {
//        if (parentExObject is SqlFDBDatasetExplorerObject)
//        {
//            return true;
//        }

//        return false;
//    }

//    async public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
//    {
//        if (!(parentExObject is SqlFDBDatasetExplorerObject))
//        {
//            return null;
//        }

//        var instance = await parentExObject.GetInstanceAsync();
//        IFeatureDataset dataset = (SqlFDBDatasetExplorerObject)instance as IFeatureDataset;
//        if (dataset == null || !(dataset.Database is SqlFDB))
//        {
//            return null;
//        }

//        FormNewNetworkclass dlg = new FormNewNetworkclass(dataset, typeof(CreateFDBNetworkFeatureclass));
//        if (dlg.ShowDialog() != DialogResult.OK)
//        {
//            return null;
//        }

//        CreateFDBNetworkFeatureclass creator = new CreateFDBNetworkFeatureclass(
//            dataset, dlg.NetworkName,
//            dlg.EdgeFeatureclasses,
//            dlg.NodeFeatureclasses);
//        creator.SnapTolerance = dlg.SnapTolerance;
//        creator.ComplexEdgeFcIds = await dlg.ComplexEdgeFcIds();
//        creator.GraphWeights = dlg.GraphWeights;
//        creator.SwitchNodeFcIdAndFieldnames = dlg.SwitchNodeFcIds;
//        creator.NodeTypeFcIds = dlg.NetworkNodeTypeFcIds;

//        FormTaskProgress progress = new FormTaskProgress();
//        progress.ShowProgressDialog(creator, creator.Run());

//        IDatasetElement element = await ((IFeatureDataset)instance).Element(dlg.NetworkName);
//        return new SqlFDBFeatureClassExplorerObject(
//            parentExObject as SqlFDBDatasetExplorerObject,
//            parentExObject.Name,
//            element);
//    }

//    #endregion
//}

//[RegisterPlugIn("11D0739F-66FC-4ca7-BA58-887DBB6F088C")]
//public class SqlFDBGeographicViewExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
//{
//    #region IExplorerObjectCreatable Member

//    public bool CanCreate(IExplorerObject parentExObject)
//    {
//        return parentExObject is SqlFDBDatasetExplorerObject;
//    }

//    async public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
//    {
//        SqlFDBDatasetExplorerObject parent = (SqlFDBDatasetExplorerObject)parentExObject;

//        IFeatureDataset dataset = await parent.GetInstanceAsync() as IFeatureDataset;
//        if (dataset == null)
//        {
//            return null;
//        }

//        AccessFDB fdb = dataset.Database as AccessFDB;
//        if (fdb == null)
//        {
//            return null;
//        }

//        FormRegisterGeographicView dlg = await FormRegisterGeographicView.CreateAsync(dataset as IFeatureDataset);
//        if (dlg.ShowDialog() == DialogResult.OK)
//        {
//            int fc_id = await fdb.CreateSpatialView(dataset.DatasetName, dlg.SpatialViewAlias);

//            IDatasetElement element = await dataset.Element(dlg.SpatialViewAlias);
//            return new SqlFDBFeatureClassExplorerObject(
//                parentExObject as SqlFDBDatasetExplorerObject,
//                parentExObject.Name,
//                element);
//        }
//        return null;
//    }

//    #endregion

//    #region IExplorerObject Member

//    public string Name
//    {
//        get { return "Geographic View"; }
//    }

//    public string FullName
//    {
//        get { return "Geographic View"; }
//    }

//    public string Type
//    {
//        get { return String.Empty; }
//    }

//    public IExplorerIcon Icon
//    {
//        get { return new AccessFDBGeographicViewIcon(); }
//    }

//    public IExplorerObject ParentExplorerObject
//    {
//        get { return null; }
//    }

//    public Task<object> GetInstanceAsync()
//    {
//        return Task.FromResult<object>(null);
//    }

//    public Type ObjectType
//    {
//        get { return null; }
//    }

//    public int Priority { get { return 1; } }

//    #endregion

//    #region IDisposable Member

//    public void Dispose()
//    {

//    }

//    #endregion

//    #region ISerializableExplorerObject Member

//    public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
//    {
//        return Task.FromResult<IExplorerObject>(null);
//    }

//    #endregion
//}

//[RegisterPlugIn("19AF8E6C-8324-4290-AF7C-5B19E31A952E")]
//public class SqlFDBLinkedFeatureclassExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
//{
//    #region IExplorerObjectCreatable Member

//    public bool CanCreate(IExplorerObject parentExObject)
//    {
//        return parentExObject is SqlFDBDatasetExplorerObject;
//    }

//    async public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
//    {
//        SqlFDBDatasetExplorerObject parent = (SqlFDBDatasetExplorerObject)parentExObject;

//        IFeatureDataset dataset = await parent.GetInstanceAsync() as IFeatureDataset;
//        if (dataset == null)
//        {
//            return null;
//        }

//        AccessFDB fdb = dataset.Database as AccessFDB;
//        if (fdb == null)
//        {
//            return null;
//        }

//        List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
//        filters.Add(new OpenFeatureclassFilter());
//        ExplorerDialog dlg = new ExplorerDialog("Select Featureclass", filters, true);

//        IExplorerObject ret = null;

//        if (dlg.ShowDialog() == DialogResult.OK &&
//            dlg.ExplorerObjects != null)
//        {
//            foreach (IExplorerObject exObj in dlg.ExplorerObjects)
//            {
//                var exObjectInstance = await exObj?.GetInstanceAsync();
//                if (exObjectInstance is IFeatureClass)
//                {
//                    int fcid = await fdb.CreateLinkedFeatureClass(dataset.DatasetName, (IFeatureClass)exObjectInstance);
//                    if (fcid < 0)
//                    {
//                        MessageBox.Show(fdb.LastErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                        continue;
//                    }
//                    if (ret == null)
//                    {
//                        IDatasetElement element = await dataset.Element(((IFeatureClass)exObjectInstance).Name);
//                        if (element != null)
//                        {
//                            ret = new SqlFDBFeatureClassExplorerObject(
//                                parentExObject as SqlFDBDatasetExplorerObject,
//                                parentExObject.Name,
//                                element);
//                        }
//                    }
//                }
//            }
//        }
//        return ret;
//    }

//    #endregion

//    #region IExplorerObject Member

//    public string Name
//    {
//        get { return "Linked Featureclass"; }
//    }

//    public string FullName
//    {
//        get { return "Linked Featureclass"; }
//    }

//    public string Type
//    {
//        get { return String.Empty; }
//    }

//    public IExplorerIcon Icon
//    {
//        get { return new AccessFDBGeographicViewIcon(); }
//    }

//    public IExplorerObject ParentExplorerObject
//    {
//        get { return null; }
//    }

//    public Task<object> GetInstanceAsync()
//    {
//        return Task.FromResult<object>(null);
//    }

//    public Type ObjectType
//    {
//        get { return null; }
//    }

//    public int Priority { get { return 1; } }

//    #endregion

//    #region IDisposable Member

//    public void Dispose()
//    {

//    }

//    #endregion

//    #region ISerializableExplorerObject Member

//    public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
//    {
//        return Task.FromResult<IExplorerObject>(null);
//    }

//    #endregion
//}

////[RegisterPlugIn("C3D1F9CA-69B5-46e9-B4DB-05534512F8B9")]
//public class SqlFDBTileGridClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectCreatable
//{
//    public SqlFDBTileGridClassExplorerObject() : base(null, null, 1) { }

//    #region IExplorerObject Member

//    public string Name
//    {
//        get { return String.Empty; }
//    }

//    public string FullName
//    {
//        get { return String.Empty; }
//    }

//    public string Type
//    {
//        get { return "Tile Grid Class"; }
//    }

//    public IExplorerIcon Icon
//    {
//        get { return new AccessFDBRasterIcon(); }
//    }

//    public Task<object> GetInstanceAsync()
//    {
//        return Task.FromResult<object>(null);
//    }

//    #endregion

//    #region IDisposable Member

//    public void Dispose()
//    {

//    }

//    #endregion

//    #region ISerializableExplorerObject Member

//    public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
//    {
//        return Task.FromResult<IExplorerObject>(null);
//    }

//    #endregion

//    #region IExplorerObjectCreatable Member

//    public bool CanCreate(IExplorerObject parentExObject)
//    {
//        if (parentExObject is SqlFDBDatasetExplorerObject)
//        {
//            return true;
//        }

//        return false;
//    }

//    async public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
//    {
//        if (!(parentExObject is SqlFDBDatasetExplorerObject))
//        {
//            return null;
//        }

//        var instance = await ((SqlFDBDatasetExplorerObject)parentExObject).GetInstanceAsync();
//        IFeatureDataset dataset = (SqlFDBDatasetExplorerObject)instance as IFeatureDataset;
//        if (dataset == null || !(dataset.Database is SqlFDB))
//        {
//            return null;
//        }

//        FormNewTileGridClass dlg = new FormNewTileGridClass();
//        if (dlg.ShowDialog() != DialogResult.OK)
//        {
//            return null;
//        }

//        CreateTileGridClass creator = new CreateTileGridClass(
//            dlg.GridName,
//            dataset, dlg.SpatialIndexDefinition,
//            dlg.RasterDataset,
//            dlg.TileSizeX, dlg.TileSizeY, dlg.ResolutionX, dlg.ResolutionY, dlg.Levels,
//            dlg.TileCacheDirectory,
//            dlg.TileGridType);
//        creator.CreateTiles = dlg.GenerateTileCache;
//        creator.TileLevelType = dlg.TileLevelType;
//        creator.CreateLevels = dlg.CreateLevels;

//        FormTaskProgress progress = new FormTaskProgress();
//        progress.ShowProgressDialog(creator, creator.RunTask());

//        IDatasetElement element = await ((IFeatureDataset)instance).Element(dlg.GridName);
//        return new SqlFDBFeatureClassExplorerObject(
//            parentExObject as SqlFDBDatasetExplorerObject,
//            parentExObject.Name,
//            element);
//    }

//    #endregion
//}
