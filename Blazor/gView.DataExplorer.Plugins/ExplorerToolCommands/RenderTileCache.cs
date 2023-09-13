using gView.DataExplorer.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.DataExplorer.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[gView.Framework.system.RegisterPlugIn("900D0FF7-8CE2-4E87-968C-C4534F285416")]
internal class RenderTileCache : IExplorerToolCommand
{
    public string Name => "TileCache.Render";

    public string ToolTip => "Prerender Tiles for a gView MapServer Service";

    public string Icon => String.Empty;

    async public Task<bool> OnEvent(IApplicationScope scope)
    {
        var scopeService = scope.ToScopeService();

        var model = await scopeService.ShowModalDialog(
                           typeof(Razor.Components.Dialogs.SelectMapServerServiceDialog),
                           "Select Service",
                           new SelectMapServerServiceModel());

        return true;
    }
}
