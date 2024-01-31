using gView.Carto.Core;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.ToolEvents;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.CartoTools.Network.Graphics;
using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Tools.Context;
using gView.Carto.Razor.Components.Tools.ToolEvents;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Network;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;

namespace gView.Carto.Plugins.CartoTools.Network;

[RegisterPlugIn("8F2B6C49-D24A-4017-9554-AD15F54A1B06")]
internal class NetworkTool : ICartoTool
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

    public Type UIComponent => typeof(Razor.Components.Tools.Network);

    public Task<bool> OnEvent(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
        => _networkContext?.ContextTool switch
        {
            NetworkContextTool.SetStartNode => OnStartTargetPointEvent(scope, eventArgs),
            NetworkContextTool.SetTargetNode => OnStartTargetPointEvent(scope, eventArgs),
            NetworkContextTool.Trace => OnTrance(scope, eventArgs),
            _ => Task.FromResult(false)
        };

    #endregion

    #region ContextTools

    async private Task<bool> OnStartTargetPointEvent(ICartoApplicationScopeService scope, ToolEventArgs mapEvent)
    {
        if (!(_networkContext?.CurrentNetworkLayer?.Class is INetworkFeatureClass networkFeatureClass))
        {
            return false;
        }

        IGeometry? queryGeometry = mapEvent
            .GetGeometry()
            .ToProjectedEnvelope(scope, scope.DisplayService.SpatialReference);

        if (queryGeometry is null)
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
            if (_networkContext.ContextTool == NetworkContextTool.SetStartNode)
            {
                _networkContext.StartNode = n1;
                scope.Document.Map.Display.GraphicsContainer.Elements.Remove(typeof(GraphicStartPoint));
                scope.Document.Map.Display.GraphicsContainer.Elements.Add(new GraphicStartPoint(p1));
            }
            else if (_networkContext.ContextTool == NetworkContextTool.SetTargetNode)
            {
                _networkContext.TargetNode = n1;
                scope.Document.Map.Display.GraphicsContainer.Elements.Remove(typeof(GraphicTargetPoint));
                scope.Document.Map.Display.GraphicsContainer.Elements.Add(new GraphicTargetPoint(p1));
            }

            await scope.EventBus.FireRefreshMapAsync(Framework.Core.Carto.DrawPhase.Graphics);
        }

        return true;
    }

    async private Task<bool> OnTrance(ICartoApplicationScopeService scope, ToolEventArgs eventArgs)
    {
        if (!(_networkContext?.CurrentNetworkLayer?.Class is INetworkFeatureClass networkFeatureClass))
        {
            return false;
        }

        if (!(eventArgs is TraceEventArgs traceEventArgs))
        {
            throw new ArgumentException("Invalid trace event args");
        }

        var display = scope.Document.Map.Display;
        display.GraphicsContainer.Elements.Remove(typeof(GraphicNetworkPathEdge));
        display.GraphicsContainer.Elements.Remove(typeof(GraphicFlagPoint));

        var outputCollection = await traceEventArgs.Tracer.Trace(networkFeatureClass,
            TracerInput(),
            new CancelTracker());

        if (outputCollection is not null)
        {
            foreach (INetworkTracerOutput output in outputCollection)
            {
                if (output is NetworkEdgeCollectionOutput edgeCollection)
                {
                    IFeatureCursor? cursor = await NetworkPathEdges(scope, networkFeatureClass, edgeCollection);
                    if (cursor == null)
                    {
                        continue;
                    }

                    IFeature feature;
                    while ((feature = await cursor.NextFeature()) != null)
                    {
                        if (!(feature.Shape is IPolyline polyline))
                        {
                            continue;
                        }

                        var transformedPolyline = networkFeatureClass switch
                        {
                            IFeatureClass featureClass => scope.GeoTransformer.Transform(
                                                                polyline,
                                                                featureClass.SpatialReference,
                                                                display.SpatialReference
                                                          ) as IPolyline,
                            _ => polyline
                        };

                        if (transformedPolyline is not null)
                        {
                            display.GraphicsContainer.Elements.Add(new GraphicNetworkPathEdge(transformedPolyline));
                        }
                    }
                }
                else if (output is NetworkPolylineOutput polyline)
                {
                    display.GraphicsContainer.Elements.Add(new GraphicNetworkPathEdge(polyline.Polyline));
                }
                else if (output is NetworkFlagOutput flag)
                {
                    string text = flag.UserData?.ToString() ?? "Flag";

                    display.GraphicsContainer.Elements.Add(new GraphicFlagPoint(flag.Location, text));
                }
            }
        }

        await scope.EventBus.FireRefreshMapAsync(Framework.Core.Carto.DrawPhase.Graphics);

        return true;
    }

    async public Task<IFeatureCursor?> NetworkPathEdges(
            ICartoApplicationScopeService scope,
            INetworkFeatureClass networkFeatureClass, 
            NetworkEdgeCollectionOutput edgeCollection
        )
    {
        if (networkFeatureClass == null)
        {
            return null;
        }

        RowIDFilter filter = new RowIDFilter(String.Empty)
        {
            FeatureSpatialReference = scope.DisplayService.SpatialReference,
        };
        foreach (NetworkEdgeOutput edge in edgeCollection)
        {
            filter.IDs.Add(edge.EdgeId);
        }

        return await networkFeatureClass.GetEdgeFeatures(filter);
    }

    private NetworkTracerInputCollection? TracerInput()
    {
        if (_networkContext is null)
        {
            return null;
        }

        NetworkTracerInputCollection input = new NetworkTracerInputCollection();
        if (_networkContext.StartNode >= 0)
        {
            input.Add(new NetworkSourceInput(_networkContext.StartNode));
        }

        if (_networkContext.TargetNode >= 0)
        {
            input.Add(new NetworkSinkInput(_networkContext.TargetNode));
        }

        //if (_module.GraphWeight != null)
        //{
        //    input.Add(new NetworkWeighInput(_module.GraphWeight, _module.WeightApplying));
        //}

        //if (_module.StartEdgeIndex >= 0)
        //{
        //    input.Add(new NetworkSourceEdgeInput(_module.StartEdgeIndex, _module.StartPoint));
        //}

        return input;
    }

    #endregion
}
