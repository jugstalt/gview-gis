using gView.Carto.Core;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.MapEvents;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Tools.Context;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Network;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.OGC.WFS;
using MongoDB.Driver.Core.Events;
using gView.Framework.Core.Carto;

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

    public Task<bool> OnEvent(ICartoApplicationScopeService scope, MapEvent mapEvent)
        => _networkContext?.ContextTool switch
        {
            NetowrkContextTool.SetStartNode => OnStartTargetPointEvent(scope, mapEvent),
            NetowrkContextTool.SetTargetNode => OnStartTargetPointEvent(scope, mapEvent),
            _ => Task.FromResult(false)
        };

    #endregion

    #region ContextTools

    async private Task<bool> OnStartTargetPointEvent(ICartoApplicationScopeService scope, MapEvent mapEvent)
    {
        if(!(_networkContext?.CurrentNetworkLayer?.Class is INetworkFeatureClass networkFeatureClass))
        {
            return false;
        }

        IGeometry? queryGeometry = mapEvent
            .GetGeometry()
            .ToProjectedEnvelope(scope, scope.DisplayService.SpatialReference);

        if(queryGeometry is null)
        {
            return false;
        }

        var spatialFilter = new SpatialFilter()
        {
            SubFields = "FDB_OID FDB_SHAPE",
            FilterSpatialReference = scope.DisplayService.SpatialReference,
            FeatureSpatialReference = scope.DisplayService.SpatialReference,
            Geometry = queryGeometry
        };

        IFeatureCursor cursor = await networkFeatureClass.GetNodeFeatures(spatialFilter);
        IFeature feature;
        double dist = double.MaxValue;
        int n1 = -1;
        IPoint? p1 = null;

        while ((feature = await cursor.NextFeature()) is not null)
        {
            double d = queryGeometry.Envelope.Center.Distance(feature.Shape as IPoint);
            if (d < dist)
            {
                dist = d;
                n1 = feature.OID;
                p1 = feature.Shape as IPoint;
            }
        }

        if (n1 != -1 && p1 is not null)
        {
            if (_networkContext.ContextTool == NetowrkContextTool.SetStartNode)
            {
                _networkContext.StartNode = n1;
            }
            else if (_networkContext.ContextTool == NetowrkContextTool.SetTargetNode) 
            {
                _networkContext.TargetNode = n1;
            }

            
        
            //scope.Document.Map.Display.GraphicsContainer.Elements.Add(new GraphicStartPoint(p1));
            //_module.StartPoint = p1;
        }

        return true;
    }

    #endregion
}
