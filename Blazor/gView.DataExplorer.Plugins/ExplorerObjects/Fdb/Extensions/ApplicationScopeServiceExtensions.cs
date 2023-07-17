using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Models;
using gView.Cmd.Fdb.Lib;
using gView.DataExplorer.Plugins.Services;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Blazor;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.Extensions;
static internal class ApplicationScopeServiceExtensions
{
    async static public Task<IDatasetElement?> CreateCeatureClass(this ExplorerApplicationScopeService scopeService, IExplorerObject parentExObject)
    {
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

        command = new CreateFeatureClassCommand();
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



        IDatasetElement createElement = await (featureDataset).Element(model.Name);

        return createElement;
    }
}
