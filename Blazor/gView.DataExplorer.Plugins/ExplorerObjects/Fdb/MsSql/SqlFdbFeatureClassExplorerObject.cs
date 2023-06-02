﻿using gView.Blazor.Core.Exceptions;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataSources.Fdb;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.MsSql;

[RegisterPlugIn("FE6E1EA7-1300-400c-8674-68465859E991")]
public class SqlFdbFeatureClassExplorerObject : ExplorerObjectCls<SqlFdbDatasetExplorerObject, _IMClass>, 
                                                IExplorerSimpleObject, 
                                                IExplorerObjectDeletable, 
                                                ISerializableExplorerObject,
                                                //, IExplorerObjectContextMenu,
                                                IExplorerObjectRenamable, 
                                                IExplorerObjectCreatable, 
                                                IExporerOjectSchema
{
    private string _dsname = "", _fcname = "", _type = "", _icon = "";
    private IFeatureClass? _fc = null;
    private IRasterClass? _rc = null;
    private bool _isNetwork = false;

    public SqlFdbFeatureClassExplorerObject() : base() { }
    public SqlFdbFeatureClassExplorerObject(SqlFdbDatasetExplorerObject parent, string dsname, IDatasetElement element)
        : base(parent, 1)
    {
        if (element == null)
        {
            return;
        }

        _dsname = dsname;
        _fcname = element.Title;

        string typePrefix = String.Empty;
        bool isLinked = false;
        if (element.Class is LinkedFeatureClass)
        {
            typePrefix = "Linked ";
            isLinked = true;
        }

        if (element.Class is IRasterCatalogClass)
        {
            _icon = "webgis:tiles";
            _type = typePrefix + "Raster Catalog Layer";
            _rc = (IRasterClass)element.Class;
        }
        else if (element.Class is IRasterClass)
        {
            _icon = "webgis:tiles";
            _type = typePrefix + "Raster Layer";
            _rc = (IRasterClass)element.Class;
        }
        else if (element.Class is IFeatureClass)
        {
            _fc = (IFeatureClass)element.Class;
            switch (_fc.GeometryType)
            {
                case GeometryType.Envelope:
                case GeometryType.Polygon:
                    if (isLinked)
                    {
                        _icon = "basic:explode";
                    }
                    else
                    {
                        _icon = "webgis:shape-polygon";
                    }

                    _type = typePrefix + "Polygon Featureclass";
                    break;
                case GeometryType.Multipoint:
                case GeometryType.Point:
                    if (isLinked)
                    {
                        _icon = "basic:explode";
                    }
                    else
                    {
                        _icon = "basic:dot-filled";
                    }

                    _type = typePrefix + "Point Featureclass";
                    break;
                case GeometryType.Polyline:
                    if (isLinked)
                    {
                        _icon = "basic:explode";
                    }
                    else
                    {
                        _icon = "webgis:shape-polyline";
                    }

                    _type = typePrefix + "Polyline Featureclass";
                    break;
                case GeometryType.Network:
                    _icon = "webgis:construct-edge-intersect";
                    _type = "Networkclass";
                    _isNetwork = true;
                    break;
            }

        }
    }

    internal string ConnectionString
    {
        get
        {
            if (TypedParent == null)
            {
                return "";
            }

            return TypedParent.ConnectionString;
        }
    }

    #region IExplorerObject Members

    public string Name=> _fcname; 

    public string FullName
    {
        get
        {
            if (Parent == null)
            {
                return "";
            }

            return $@"{Parent.FullName}\{this.Name}";
        }
    }
    public string Type
    {
        get { return _type != "" ? _type : "SqlFDB Featureclass"; }
    }

    public string Icon => _icon;

    public void Dispose()
    {
        if (_fc != null)
        {
            _fc = null;
        }
        if (_rc != null)
        {
            _rc = null;
        }
    }
    public Task<object?> GetInstanceAsync()
    {
        if (_fc != null)
        {
            return Task.FromResult<object?>(_fc);
        }

        if (_rc != null)
        {
            return Task.FromResult<object?>(_rc);
        }

        return Task.FromResult<object?>(null);
    }

    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache?.Contains(FullName) == true)
        {
            return cache[FullName];
        }

        FullName = FullName.Replace("/", @"\");
        int lastIndex = FullName.LastIndexOf(@"/");
        if (lastIndex == -1)
        {
            return null;
        }

        string dsName = FullName.Substring(0, lastIndex);
        string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

        SqlFdbDatasetExplorerObject? dsObject = new SqlFdbDatasetExplorerObject();
        dsObject = (SqlFdbDatasetExplorerObject?)await dsObject.CreateInstanceByFullName(dsName, cache);
        if (dsObject == null || await dsObject.ChildObjects() == null)
        {
            return null;
        }

        foreach (IExplorerObject exObject in await dsObject.ChildObjects())
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

    //async void ShrinkSpatialIndex_Click(object sender, EventArgs e)
    //{
    //    if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
    //    {
    //        MessageBox.Show("Can't shrink index...\nUncorrect feature class !!!");
    //        return;
    //    }

    //    List<IClass> classes = new List<IClass>();
    //    classes.Add(_fc);

    //    SpatialIndexShrinker rebuilder = new SpatialIndexShrinker();
    //    rebuilder.RebuildIndices(classes);
    //    await _fc.Dataset.RefreshClasses();
    //}

    //async void SpatialIndexDef_Click(object sender, EventArgs e)
    //{
    //    if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
    //    {
    //        MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
    //        return;
    //    }

    //    FormRebuildSpatialIndexDef dlg = await FormRebuildSpatialIndexDef.Create((AccessFDB)_fc.Dataset.Database, _fc);
    //    if (dlg.ShowDialog() == DialogResult.OK)
    //    {
    //    }
    //}

    //void RepairSpatialIndex_Click(object sender, EventArgs e)
    //{
    //    if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
    //    {
    //        MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
    //        return;
    //    }

    //    FormRepairSpatialIndexProgress dlg = new FormRepairSpatialIndexProgress((AccessFDB)_fc.Dataset.Database, _fc);
    //    if (dlg.ShowDialog() == DialogResult.OK)
    //    {
    //    }
    //}

    //void Truncate_Click(object sender, EventArgs e)
    //{
    //    if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
    //    {
    //        MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
    //        return;
    //    }

    //    ((AccessFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
    //    _fc.Dataset.RefreshClasses();
    //}

    //void Altertable_Click(object sender, EventArgs e)
    //{
    //    //if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
    //    //{
    //    //    MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
    //    //    return;
    //    //}

    //    //if (_fc == null) return;
    //    //FormNewFeatureclass dlg = new FormNewFeatureclass(_fc);
    //    //if(dlg.ShowDialog()!=DialogResult.OK) return;

    //    //((AccessFDB)_fc.Dataset.Database).AlterTable(_fc.Name, dlg.Fields);
    //}

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted;

    async public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        if (TypedParent == null)
        {
            return false;
        }

        if (await TypedParent.DeleteFeatureClass(_fcname))
        {
            if (ExplorerObjectDeleted != null)
            {
                ExplorerObjectDeleted(this);
            }

            return true;
        }
        return false;
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed;

    async public Task<bool> RenameExplorerObject(string newName)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
        {
            throw new GeneralException("Can't rename featureclass...\nUncorrect feature class !!!");
        }

        if (!await ((AccessFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
        {
            throw new GeneralException("Can't rename featureclass...\n" + ((AccessFDB)_fc.Dataset.Database).LastErrorMessage);
        }

        _fcname = newName;

        if (ExplorerObjectRenamed != null)
        {
            ExplorerObjectRenamed(this);
        }

        return true;
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        if (parentExObject is SqlFdbDatasetExplorerObject &&
            !((SqlFdbDatasetExplorerObject)parentExObject).IsImageDataset)
        {
            return true;
        }

        return false;
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope appScope, IExplorerObject parentExObject)
    {
        if (!CanCreate(parentExObject))
        {
            return null;
        }

        var instance = await parentExObject.GetInstanceAsync();
        if (!(instance is IFeatureDataset) || !(((IDataset)instance).Database is SqlFDB))
        {
            return null;
        }
        SqlFDB? fdb = ((IDataset)instance).Database as SqlFDB;

        return null;

        //FormNewFeatureclass dlg = await FormNewFeatureclass.Create(instance as IFeatureDataset);
        //if (dlg.ShowDialog() != DialogResult.OK)
        //{
        //    return null;
        //}

        //IGeometryDef gDef = dlg.GeometryDef;

        //int FCID = await fdb.CreateFeatureClass(
        //    parentExObject.Name,
        //    dlg.FeatureclassName,
        //    gDef,
        //    dlg.Fields);

        //if (FCID < 0)
        //{
        //    MessageBox.Show("ERROR: " + fdb.LastErrorMessage);
        //    return null;
        //}

        //ISpatialIndexDef sIndexDef = await fdb.SpatialIndexDef(parentExObject.Name);
        //if (fdb is SqlFDB &&
        //    (sIndexDef.GeometryType == GeometryFieldType.MsGeometry ||
        //     sIndexDef.GeometryType == GeometryFieldType.MsGeography))
        //{
        //    MSSpatialIndex index = dlg.MSSpatialIndexDef;
        //    fdb.SetMSSpatialIndex(index, dlg.FeatureclassName);
        //}
        //else
        //{
        //    await fdb.SetSpatialIndexBounds(dlg.FeatureclassName, "BinaryTree2", dlg.SpatialIndexExtents, 0.55, 200, dlg.SpatialIndexLevels);
        //}

        //IDatasetElement element = await ((IFeatureDataset)instance).Element(dlg.FeatureclassName);
        //return new SqlFDBFeatureClassExplorerObject(
        //    parentExObject as SqlFDBDatasetExplorerObject,
        //    parentExObject.Name,
        //    element);
    }

    #endregion

    #region IExporerOjectSchema Member

    public string Schema
    {
        get
        {
            if (_fc is SqlFDBFeatureClass)
            {
                return ((SqlFDBFeatureClass)_fc).DbSchema;
            }

            return String.Empty;
        }
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