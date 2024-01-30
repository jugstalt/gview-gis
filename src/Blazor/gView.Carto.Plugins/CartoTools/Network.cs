using gView.Carto.Core;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.MapEvents;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Tools.Context;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Data.Extensions;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("8F2B6C49-D24A-4017-9554-AD15F54A1B06")]
internal class Network : ICartoTool
{
    #region ICartoButton

    public string Name => "Network";

    public string ToolTip => "Network Tools";

    public ToolType ToolType => ToolType.Click;

    public string Icon => "webgis:construct-edge-intersect";

    public CartoToolTarget Target => CartoToolTarget.Tools;

    public int SortOrder => 10;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
        => scope.SelectedTocTreeNode?.TocElement.CollectionNetworkLayers()?.Any() == true;

    public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var networkContext = scope.Tools.GetCurrentToolContext<NetworkContext>();

        if (networkContext is null)
        {
            throw new Exception("Can't determine network context");
        }

        networkContext.NetworkLayers = scope.NetworkLayers();
        networkContext.CurrentNetworkLayer = scope.SelectedTocTreeNode?.TocElement.CollectionNetworkLayers().FirstOrDefault();

        return Task.FromResult(true);
    }

    #endregion

    #region ICartoTool

    private NetworkContext? _networkContext = null;

    public object? ToolContext => _networkContext;

    public string ToolBoxTitle(ICartoApplicationScopeService scope)
        => $"Network - {_networkContext?.CurrentNetworkLayer?.TocNameOrLayerTitle(scope.Document.Map)}";

    public void InitializeScope(ICartoApplicationScopeService scope) 
    { 
        _networkContext = new NetworkContext(); 
    }

    public Type UIComponent => typeof(gView.Carto.Razor.Components.Tools.Network);

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope, MapEvent mapEvent)
    {
        return true;
    }


    #endregion
}
