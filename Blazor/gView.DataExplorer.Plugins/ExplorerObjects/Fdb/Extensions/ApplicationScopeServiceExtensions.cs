using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Models;
using gView.Cmd.Fdb.Lib;
using gView.DataExplorer.Plugins.ExplorerObjects.Fdb.MsSql;
using gView.DataExplorer.Plugins.ExplorerObjects.Fdb.PostgreSql;
using gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;
using gView.DataExplorer.Plugins.Services;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Blazor;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.system;
using Newtonsoft.Json;
using System;
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

        ICommand command = new CreateFeatureClassCommand();
        var parameters = new Dictionary<string, object>()
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

    async static public Task<IDataset?> CreateDataset(this ExplorerApplicationScopeService scopeService, IExplorerObject parentExObject)
    {
        var model = await scopeService
                               .ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.NewFdbDataset),
                                          "New Dataset",
                                          new NewFdbDatasetModel() { Name = "ds1" });
        if (model == null)
        {
            return null;
        }

        AccessFDB? fdb = null;
        bool opened;
        switch(parentExObject)
        {
            case SqlFdbExplorerObject sqlExObject:
                fdb = new SqlFDB();
                opened = await fdb.Open(sqlExObject.ConnectionString);
                break;
            case PostgreSqlExplorerObject pgExObject:
                fdb = new pgFDB();
                opened = await fdb.Open(pgExObject.ConnectionString);
                break;
            case SqLiteFdbExplorerObject sqliteExObject:
                fdb = new SQLiteFDB();
                opened = await fdb.Open(sqliteExObject.FullName);
                break;
            default:
                throw new Exception("Unknown FDB Engine");
        }

        if (!opened)
        {
            throw new Exception(fdb.LastErrorMessage);
        }
        
        ICommand command = new CreateDatasetCommand();
        var parameters = new Dictionary<string, object>()
        {
            { "fdb", fdb.GetType().Name },
            { "connection_string", fdb.ConnectionString },
            { "ds_name", model.Name },
            { "ds_type", model.DatasetType.ToString() }
        };

        if (model.SpatialReference != null)
        {
            parameters.Add("sref_epsg", model.SpatialReference.EpsgCode);
        }

        if (model.DatasetType == NewFdbDatasetType.ImageDataset)
        {
            var pluginmananger = new PlugInManager();

            parameters.Add("si_bounds_minx", model.SpatialIndex.Bounds.minx);
            parameters.Add("si_bounds_miny", model.SpatialIndex.Bounds.miny);
            parameters.Add("si_bounds_maxx", model.SpatialIndex.Bounds.maxx);
            parameters.Add("si_bounds_maxy", model.SpatialIndex.Bounds.maxy);
            parameters.Add("si_max_levels", model.SpatialIndex.MaxLevel);

            parameters.Add("autofields",
                JsonConvert.SerializeObject(model.AutoFields.Keys
                    .Select(k => model.AutoFields[k] == true ? k : null)
                    .Where(a => a != null)
                    .Select(a => new AutoFieldModel() { Name = a!.name ?? a.AutoFieldPrimayName, PluginGuid = PlugInManager.PlugInID(a) })
                    .ToArray()));
        }

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

        return null;
    }
}
 