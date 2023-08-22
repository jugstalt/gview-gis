using gView.Cmd.Fdb.Lib.Model;
using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("AFDE90FF-D063-4224-BD31-1B30C266D55B")]
public class SqLiteFdbNetworkClassExplorerObject : ExplorerObjectCls<IExplorerObject, SQLiteFDBNetworkFeatureClass>, 
                                                   IExplorerSimpleObject, 
                                                   IExplorerObjectCreatable
{
    public SqLiteFdbNetworkClassExplorerObject() : base(1) { }

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

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache? cache)
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

    public async Task<IExplorerObject?> CreateExplorerObjectAsync(IApplicationScope scope, IExplorerObject? parentExObject)
    {
        if (parentExObject == null)
        {
            throw new ArgumentNullException(nameof(parentExObject));
        }

        var scopeService = scope.ToScopeService();
        var dataset = (await parentExObject.GetInstanceAsync()) as IFeatureDataset;
        if (dataset == null)
        {
            throw new Exception("Can't determine feature dataset instance");
        }

        var model = await scopeService.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.CreateNetworkFeatureClassDialog),
                                                       "Create Network FeatureClass",
                                                       new CreateNetworkFeatureClassModel(dataset));

        if (model == null)
        {
            return null;
        }

        var commandModel = new CreateNetworkModel()
        {
            ConnectionString = dataset.ConnectionString,
            DatasetGuid = PlugInManager.PlugInID(dataset),
            DatasetName = dataset.DatasetName,
            UseSnapTolerance = model.Result.UseSnapTolerance,
            SnapTolerance=model.Result.SnapTolerance
        };

        var edges = new List<CreateNetworkModel.Edge>();
        var nodes = new List<CreateNetworkModel.Node>();

        foreach(var edgeFeatureClass in model.Result.EdgeFeatureClasses)
        {
            edges.Add(new CreateNetworkModel.Edge()
            {
                Name = edgeFeatureClass.Name,
                IsComplexEdge = model.Result.ComplexEdges.Contains(edgeFeatureClass)
            });
        }
        foreach (var nodeFeatureClass in model.Result.Nodes)
        {
            nodes.Add(new CreateNetworkModel.Node()
            {
                Name = nodeFeatureClass.FeatureClass.Name,
                IsSwitch = nodeFeatureClass.IsSwitch,
                Fieldname = nodeFeatureClass.Fieldname,
                NodeType = nodeFeatureClass.NodeType
            });
        }

        XmlStream xmlStream = new XmlStream("network");
        commandModel.Save(xmlStream);
        xmlStream.WriteStream(@$"C:\temp\{model.Result.Name}.xml");

        return null;

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
