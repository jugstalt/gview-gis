using gView.DataExplorer.Plugins.Extensions;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Razor.Base;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;
internal class SpatialIndexDefinition : IExplorerObjectContextTool
{
    public string Name => "Spatial Index Definition...";

    public string Icon => "basic:warning_yellow";

    public bool IsEnabled(IApplicationScope scope, IExplorerObject exObject)
    {
        return true;
    }

    async public Task<bool> OnEvent(IApplicationScope scope, IExplorerObject exObject)
    {
        var scopeService = scope.ToScopeService();
        var featureClass = await exObject.GetInstanceAsync() as IFeatureClass;
        var fdb = featureClass?.Dataset?.Database as AccessFDB;

        if (featureClass == null || fdb == null)
        {
            throw new Exception("Instance is not an valid FDB Featureclass!");
        }

        var binarayTreeDef = await fdb.BinaryTreeDef(featureClass.Name);

        var model = await scopeService.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.SpatialIndexDefDialog),
                                           "Spatial Index Definition",
                                           new BaseDialogModel<BinaryTreeDef>() { Value = binarayTreeDef });

        if (model == null)
        {
            return false;
        }


        //IDictionary<string, object>? parameters = null;
        //ICommand? command = null;

        //if (instance is IFeatureDataset)
        //{
        //    var featureDataset = (IFeatureDataset)instance;
        //    var featureDatasetGuid = PlugInManager.PlugInID(featureDataset);

        //    command = new ShrinkDatasetSpatialIndexCommand();
        //    parameters = new Dictionary<string, object>()
        //    {
        //        { "dataset_connstr", featureDataset.ConnectionString },
        //        { "dataset_guid", featureDatasetGuid.ToString() }
        //    };
        //}
        //else if (instance is IFeatureClass)
        //{
        //    var featureClass = (IFeatureClass)instance;
        //    var featureDataset = featureClass.Dataset;
        //    var featureDatasetGuid = PlugInManager.PlugInID(featureDataset);

        //    command = new ShrinkFeatureClassSpatialIndexCommand();
        //    parameters = new Dictionary<string, object>()
        //    {
        //        { "dataset_connstr", featureDataset.ConnectionString },
        //        { "dataset_guid", featureDatasetGuid.ToString() },
        //        { "dataset_fc", featureClass.Name },
        //    };
        //}
        //else
        //{
        //    return false;
        //}

        //await scopeService.ShowKnownDialog(
        //            KnownDialogs.ExecuteCommand,
        //            $"Shrink spatial index",
        //            new ExecuteCommandModel()
        //            {
        //                CommandItems = new[]
        //                {
        //                    new CommandItem()
        //                    {
        //                        Command = command,
        //                        Parameters = parameters
        //                    }
        //                }
        //            });

        return true;
    }
}
