using gView.Cmd.Core.Abstraction;
using gView.Cmd.TileCache.Lib;
using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using gView.Server.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("900D0FF7-8CE2-4E87-968C-C4534F285416")]
internal class RenderTileCache : IExplorerToolCommand
{
    public string Name => "TileCache.Render";

    public string ToolTip => "Prerender Tiles for a gView MapServer Service";

    public string Icon => String.Empty;

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        #region Select Server/Service

        var serviceModel = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.SelectMapServerServiceDialog),
                                "Select Service",
                                new SelectMapServerServiceModel());

        if (String.IsNullOrEmpty(serviceModel?.Server) ||
            String.IsNullOrEmpty(serviceModel?.Service))
        {
            return false;
        }

        #endregion

        #region Get TileCache Metadata

        var metadata = await new MapServerClient(serviceModel.Server)
                                .GetTileServiceMetadata(serviceModel.Service);

        if (metadata == null || metadata.Use == false)
        {
            throw new Exception("Service does not support tiles (WMTS)");
        }

        #endregion

        #region Get Render Parameters

        var model = await scope.ShowModalDialog(
                                        typeof(Razor.Components.Dialogs.RenderTileCacheDialog),
                                        "Render Parameters",
                                        new RenderTileCacheModel(serviceModel.Server, serviceModel.Service,
                                                                 metadata));

        if (model == null)
        {
            return false;
        }

        #endregion

        ICommand command = new RenderTileCacheCommand();
        IDictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "server", serviceModel.Server },
                { "service", serviceModel.Service },
                { "epsg", model.EpsgCode },
                { "orientation", model.GridOrientation },
                { "imageformat", model.ImageFormat },
                { "threads", model.ThreadCount }
            };

        if (model.Scales != null && model.Scales.Count > 0)
        {
            parameters.Add("scales", String.Join(",", model.Scales.Select(s => s.ToString(System.Globalization.NumberFormatInfo.InvariantInfo))));
        }

        if (model.Compact)
        {
            parameters.Add("compact", true);
        }

        await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Render Service Tiles",
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
