using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Threading.Tasks;
using System;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Cmd.TileCache.Lib;
using gView.Server.Models;
using System.Collections.Generic;
using gView.Cmd.Core.Abstraction;
using gView.Framework.Blazor;
using gView.Framework.Common;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("0CA2DC1A-A2FB-47FE-BECA-2D265781E8D3")]
public class ClipTileCache : IExplorerToolCommand
{
    public string Name => "TileCache.Clip";

    public string ToolTip => "Clip a compact tile cache folder";

    public string Icon => String.Empty;

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.ClipTileCacheDialog),
                                "Clip Tile Cache",
                                new ClipTileCacheModel());

        if(model is null) return false;

        if(model.Clipper is null)
        {
            throw new ArgumentException("Clipper featureclass is required");
        }

        var sourceDataset = model.Clipper.Dataset;
        var sourceDatasetGuid = PlugInManager.PlugInID(sourceDataset);

        if (sourceDatasetGuid == Guid.Empty)
        {
            throw new ArgumentException("Can't determine features class dataset plugin guid"); ;
        }

        ICommand command = new ClipTileCacheCommand();
        IDictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "clip-type", model.ClipType.ToString() },
            { "source", model.SourceCache },
            { "max-level", model.MaxLevel },
            { "clipper_connstr", sourceDataset.ConnectionString },
            { "clipper_guid", sourceDatasetGuid.ToString() },
            { "clipper_fc", model.Clipper.Name },
            { "clipper-query", model.ClipperDefinitionQuery },
            { "target", model.TargetCache },
            { "jpeg-qual", model.JpegQuality }
        };

        await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Clip Tile Cache",
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

        return true;
    }
}
