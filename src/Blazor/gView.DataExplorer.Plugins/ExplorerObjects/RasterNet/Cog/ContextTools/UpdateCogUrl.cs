using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.RasterNet.Cog.ContextTools;

/// <summary>Changes the URL of a COG connection (e.g. a renewed presigned URL), checked like a new one.</summary>
internal class UpdateCogUrl : IExplorerObjectContextTool
{
    public string Name => "URL";

    public string Icon => "basic:edit-database";

    public bool IsEnabled(IExplorerApplicationScopeService scope, IExplorerObject exObject) => exObject is CogConnectionExplorerObject;

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope, IExplorerObject exObject)
    {
        var connection = (CogConnectionExplorerObject)exObject;
        var model = await scope.ShowModalDialog(typeof(gView.DataExplorer.Razor.Components.Dialogs.CogConnectionDialog),
                                                "Cloud Optimized GeoTIFF",
                                                new CogConnectionModel { Urls = connection.Url, Single = true, Check = CogConnections.CheckAsync });
        if (model?.Lines.FirstOrDefault() is not { } url)
        {
            return false;
        }
        connection.UpdateUrl(url);
        return true;
    }
}
