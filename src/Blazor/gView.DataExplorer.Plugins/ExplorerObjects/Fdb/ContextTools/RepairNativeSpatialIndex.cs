using gView.Cmd.Core.Abstraction;
using gView.Cmd.Fdb.Lib;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Data;
using gView.Framework.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.ContextTools;

/// <summary>
/// Recalculates the extent and rebuilds the database-native spatial index (PostGIS GiST /
/// SQL Server GEOMETRY_GRID / GEOGRAPHY_GRID) - the native counterpart of
/// <see cref="RepairSpatialIndex"/>. Shown only for native storage; classic / WKB use the
/// gView tools.
/// </summary>
internal class RepairNativeSpatialIndex : IExplorerObjectContextTool
{
    public string Name => "Rebuild spatial index (native)...";

    public string Icon => "basic:warning_yellow";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject)
        => !FdbGeometryStorageContextExtensions.UsesGViewSpatialIndex(exObject);

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        var instance = await exObject.GetInstanceAsync();

        List<CommandItem> commandItems = new();

        if (instance is IFeatureDataset featureDataset)
        {
            var featureDatasetGuid = PlugInManager.PlugInID(featureDataset);

            foreach (var datasetElement in await featureDataset.Elements())
            {
                if (datasetElement.Class is not IFeatureClass)
                {
                    continue;
                }

                commandItems.Add(NativeCommandItem(featureDataset.ConnectionString, featureDatasetGuid.ToString(), datasetElement.Class.Name));
            }
        }
        else if (instance is IFeatureClass featureClass)
        {
            var featureDatasetGuid = PlugInManager.PlugInID(featureClass.Dataset);
            commandItems.Add(NativeCommandItem(featureClass.Dataset.ConnectionString, featureDatasetGuid.ToString(), featureClass.Name));
        }
        else
        {
            return false;
        }

        if (commandItems.Count > 0)
        {
            await scope.ShowKnownDialog(
                        KnownDialogs.ExecuteCommand,
                        "Rebuild native spatial index",
                        new ExecuteCommandModel() { CommandItems = commandItems.ToArray() });
        }

        return true;
    }

    private static CommandItem NativeCommandItem(string connectionString, string datasetGuid, string fcName)
        => new()
        {
            Command = new RepairNativeSpatialIndexCommand(),
            Parameters = new Dictionary<string, object>()
            {
                { "dataset_connstr", connectionString },
                { "dataset_guid", datasetGuid },
                { "dataset_fc", fcName },
            }
        };
}
