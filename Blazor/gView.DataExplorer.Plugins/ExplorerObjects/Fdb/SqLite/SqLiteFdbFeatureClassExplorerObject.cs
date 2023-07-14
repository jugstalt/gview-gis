using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Models;
using gView.Cmd.Fdb.Lib;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.ExplorerObjects.Extensions;
using gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.Fdb;
using gView.DataSources.Fdb.SQLite;
using gView.DataSources.MSSqlSpatial;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.UI;
using Microsoft.Azure.Documents.SystemFunctions;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("16DB07EC-5C30-4C2E-85AC-B49A44188B1A")]
public class SqLiteFdbFeatureClassExplorerObject : ExplorerObjectCls<SqLiteFdbDatasetExplorerObject, FeatureClass>,
                                                   IExplorerSimpleObject,
                                                   ISerializableExplorerObject,
                                                   IExplorerObjectDeletable,
                                                   IExplorerObjectContextTools,
                                                   IExplorerObjectRenamable,
                                                   IExplorerObjectCreatable
{
    private string _filename = "", _dsname = "", _fcname = "", _type = "";
    private string _icon = "basic:table";
    private IFeatureClass? _fc = null;
    private IRasterClass? _rc = null;
    private bool _isNetwork = false;
    private IEnumerable<IExplorerObjectContextTool>? _contextTools = null;

    public SqLiteFdbFeatureClassExplorerObject() :
        base()
    {
    }
    public SqLiteFdbFeatureClassExplorerObject(SqLiteFdbDatasetExplorerObject parent, string filename, string dsname, IDatasetElement element)
        : base(parent, 1)
    {
        if (element == null)
        {
            return;
        }

        _filename = filename;
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
                        _icon = "basic:open-in-window";
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
                        _icon = "basic:open-in-window";
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
                        _icon = "basic:open-in-window";
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

        if (!_isNetwork)
        {
            _contextTools = new IExplorerObjectContextTool[]
            {
                new ShrinkSpatialIndices(),
                new RepairSpatialIndex(),
                new SpatialIndexDefinition(),
                new TruncateFeatureClass()
            };
        }
    }

    #region IExplorerObject Members

    public string Name
    {
        get { return _fcname; }
    }

    public string FullName
    {
        get
        {
            return _filename + ((_filename != "") ? @"\" : "") + _dsname + ((_dsname != "") ? @"\" : "") + _fcname;
        }
    }
    public string Type
    {
        get { return String.IsNullOrEmpty(_type) ? "Feature Class" : _type; }
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
        string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

        SqLiteFdbDatasetExplorerObject? dsObject = new SqLiteFdbDatasetExplorerObject();
        dsObject = await dsObject.CreateInstanceByFullName(dsName, cache) as SqLiteFdbDatasetExplorerObject;
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

    void Truncate_Click(object sender, EventArgs e)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't rebuild index...\nUncorrect feature class !!!");
        }

        ((SQLiteFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
    }

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted = null;

    async public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        if (base.TypedParent.IsNull())
        {
            return false;
        }

        if (await base.TypedParent.DeleteFeatureClass(_fcname))
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
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't rename featureclass...\nUncorrect feature class !!!");
        }

        if (!await ((SQLiteFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
        {
            throw new Exception("Can't rename featureclass...\n" + ((SQLiteFDB)_fc.Dataset.Database).LastErrorMessage);
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
        if (parentExObject is SqLiteFdbDatasetExplorerObject &&
            !((SqLiteFdbDatasetExplorerObject)parentExObject).IsImageDataset)
        {
            return true;
        }

        return false;
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope scope, IExplorerObject parentExObject)
    {
        if (!CanCreate(parentExObject))
        {
            return null;
        }

        var scopeService = scope.ToScopeService();
        var model = await scopeService
                               .ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.NewFdbFeatureClassDialog),
                                          "New FeatureClass",
                                          new NewFdbFeatureClassModel() { Name = "fc_1" });

        if (model == null)
        {
            return null;
        }

        var featureDataset = await parentExObject.GetInstanceAsync() as IFeatureDataset;
        if (featureDataset is null || !(((IDataset)featureDataset).Database is SQLiteFDB))
        {
            return null;
        }
        var featureDatasetGuid = PlugInManager.PlugInID(featureDataset);

        IDictionary<string, object>? parameters = null;
        ICommand? command = null;

        command = new TruncateFeatureClassCommand();
        parameters = new Dictionary<string, object>()
            {
                { "dataset_connstr", featureDataset.ConnectionString },
                { "dataset_guid", featureDatasetGuid.ToString() },
                { "dataset_fc", model.Name },
                // Geometry Type
                { "geometry_type", model.GeometryType.ToString() },
                // Spatial Index Def
                { "bounds_minx", model.SpatialIndex.Bounds.minx },
                { "bounds_miny", model.SpatialIndex.Bounds.miny },
                { "bounds_maxx", model.SpatialIndex.Bounds.maxx },
                { "bounds_maxy", model.SpatialIndex.Bounds.maxy },
                { "max_levels", model.SpatialIndex.MaxLevel },
                // Fields
                { "fields", JsonConvert.SerializeObject(
                    model.Fields.Values.Select(f => new FieldModel()
                    {
                        Name = f.name,
                        Alias = f.aliasname,
                        Type = f.type.ToString(),
                        Size = f.size
                    })) }
            };

        await scopeService.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Create FDB FeatureClass",
                    new ExecuteCommandModel()
                    {
                        CommandItems = new[]
                        {
                            new CommandItem()
                            {
                                Command = command,
                                Parameters = parameters
                            }
                        }
                    });

        // ToDo:
        //FormNewFeatureclass dlg = await FormNewFeaureclass.Create(instance as IFeatureDataset);
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
        //    throw new Exception("ERROR: " + fdb.LastErrorMessage);
        //}

        //ISpatialIndexDef sIndexDef = await fdb.SpatialIndexDef(parentExObject.Name);
        //await fdb.SetSpatialIndexBounds(dlg.FeatureclassName, "BinaryTree2", dlg.SpatialIndexExtents, 0.55, 200, dlg.SpatialIndexLevels);

        //IDatasetElement element = await ((IFeatureDataset)instance).Element(dlg.FeatureclassName);
        //return new SqLiteFdbFeatureClassExplorerObject(
        //    parentExObject as SqLiteFdbDatasetExplorerObject,
        //    _filename,
        //    parentExObject.Name,
        //    element);

        return null;
    }

    #endregion

    #region IExplorerObjectContextTools Member

    public IEnumerable<IExplorerObjectContextTool> ContextTools
        => _contextTools ?? Array.Empty<IExplorerObjectContextTool>();


    #endregion
}
