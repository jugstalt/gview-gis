using gView.DataSources.VectorTileCache;
using gView.DataSources.VectorTileCache.Json;
using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Calc;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Cartography.Rendering.Vtc;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;
using gView.Framework.Vtc.Extensions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace gView.Framework.Vtc;

[RegisterPlugIn("F2E27BCF-2E91-4DB9-B65A-1AE3637BBF85")]
public class OnMapLoadedEventHook : IMapEventHook
{
    private const string ResourceName = "OnMapLoadedEventHook.StylesJson";

    #region IMapEventHook

    public HookEventType Type => HookEventType.OnLoaded;

    public async Task InvokeAsync(IMap map)
    {
        var jsonString = map.ResourceContainer[ResourceName];

        var stylesCapabilities = JsonSerializer.Deserialize<StylesCapabilities>(jsonString);
        if (stylesCapabilities == null)
        {
            throw new Exception("can't read styles capabilites");
        }

        await AddLayers(map, stylesCapabilities);
    }

    #endregion

    #region IPersistable

    public void Load(IPersistStream stream)
    {

    }

    public void Save(IPersistStream stream)
    {

    }

    #endregion

    async public Task AddLayers(IMap map, StylesCapabilities stylesCapabilities)
    {
        var datasetIndex = map.Datasets.Count();
        var datasets = new Dictionary<string, IDataset>();

        #region Add Map Datasets

        foreach (var source in stylesCapabilities.Sources)
        {
            if ("vector".Equals(source.Value.Type, StringComparison.OrdinalIgnoreCase))
            {
                var vtcDataset = new gView.DataSources.VectorTileCache.Dataset();
                await vtcDataset.SetConnectionString($"source={source.Value.Url};name={source.Key}");

                await vtcDataset.Open();
                map.AddDataset(vtcDataset, datasetIndex++);

                datasets.Add(source.Key, vtcDataset);

                Console.WriteLine($"Added VTC: {source.Value.Url}");
            }
            else if ("raster".Equals(source.Value.Type, StringComparison.OrdinalIgnoreCase))
            {
                var rasterDataset = new gView.DataSources.TileCache.Dataset();
                int maxZoom = source.Value.MaxZoom ?? 21;
                var grid = new WebMercatorGrid(0, maxZoom);
                var sRef = new SpatialReference("epsg:3857");  // todo

                StringBuilder sb = new StringBuilder();
                sb.Append($"name={source.Key};");
                sb.Append($"extent={grid.Extent.ToBBoxString()};");
                sb.Append($"origin={(int)GridOrientation.UpperLeft};");
                //sb.Append($"origin_x={grid.Extent.MinX.ToString(CultureInfo.InvariantCulture)};");
                //sb.Append($"origin_y={grid.Extent.MinY.ToString(CultureInfo.InvariantCulture)};");
                sb.Append($"sref64={sRef.ToBase64String()};");
                sb.Append($"tilewidth={source.Value.TileSize};");
                sb.Append($"tileheight={source.Value.TileSize};");
                sb.Append($"tileurl={source.Value.Tiles.FirstOrDefault()?.Replace("{x}", "{0}").Replace("{y}", "{1}").Replace("{z}", "{2}")};");

                sb.Append("scales=591658710.909132");  // intial scale 591658710.909132
                for (int i = 1; i < maxZoom; i++)
                    sb.Append($",{(591658710.909132 / Math.Pow(2, i)).ToString(CultureInfo.InvariantCulture)}");
                sb.Append(";");

                await rasterDataset.SetConnectionString(sb.ToString());
                await rasterDataset.Open();
                map.AddDataset(rasterDataset, datasetIndex++);

                datasets.Add(source.Key, rasterDataset);

                Console.WriteLine($"Added Raster Tile Cache: {source.Value.Tiles.FirstOrDefault()}");
            }
            else
            {
                throw new Exception($"Warning: not supported source type: {source.Value.Type}");
            }
        }

        #endregion

        #region Try Parse all layers

        foreach (var layer in stylesCapabilities.Layers.Reverse())
        {
            if (String.IsNullOrEmpty(layer.Source))
            {
                Console.WriteLine($"Warning: layer has no sourece: {layer.SourceLayerId}");
                continue;
            }
            var dataset = datasets.ContainsKey(layer.Source)
                ? datasets[layer.Source]
                : null;

            if (dataset == null)
            {
                //throw new Exception($"Unkown layer dataset: {layer.Source}");
                continue;
            }

            // for debugging
            //if(!layer.Id.Contains("iso", StringComparison.OrdinalIgnoreCase))
            //{
            //    continue;
            //}

            var @class = (await dataset.Element(layer.SourceLayerId))?.Class;
            if (@class == null)
            {
                //if (dataset is gView.DataSources.VectorTileCache.Dataset vtcDataset)
                //{
                //    if(!vtcDataset.TryAddFeatureClass(layer.Id))
                //    {
                //        throw new Exception($"Warning: layer cant added to VTC: {layer.Source}.{layer.SourceLayerId}");
                //    }
                //}
                //else
                {
                    Console.WriteLine($"Warning: unknown layer class: {layer.Source}.{layer.SourceLayerId}");
                    continue;
                    //throw new Exception($"Warning: unknown layer class: {layer.Source}.{layer.SourceLayerId}");
                }
            }

            if (@class is IFeatureClass featureClass)
            {
                //logger?.LogLine($"adding layer: {layer.Id}");

                var featureLayer = new FeatureLayer(featureClass);

                if (layer.MinZoom.HasValue)
                {
                    featureLayer.MaximumScale
                        = featureLayer.MaximumLabelScale
                        = WebMercatorCalc.MapScale(layer.MinZoom.Value);
                }
                if (layer.MaxZoom.HasValue)
                {
                    featureLayer.MinimumScale
                        = featureLayer.MinimumLabelScale
                        = WebMercatorCalc.MapScale(layer.MaxZoom.Value);
                }
                featureLayer.ApplyRefScale = featureLayer.ApplyLabelRefScale = false;
                featureLayer.FilterQuery = VtcStyleFilter.FromJsonElement(layer.Filter);
                featureLayer.Visible = layer.Layout?.Visibility?.ToLower() switch
                {
                    "none" => false,
                    _ => true
                };
                var symbol = layer.ToPaintSymbol(map);

                if (symbol.IsLabelSymbol())
                {
                    featureLayer.LabelRenderer = new VtcLabelRenderer(symbol)
                    {
                        UseExpression = true,
                        LabelExpression = "",
                        LabelExpressionValueFunc = layer.Layout?.TextFieldExpression.ToValueFunc(),
                        LabelPriority = SimpleLabelRenderer.RenderLabelPriority.Normal,
                    };

                    featureLayer.FeatureRenderer = new VtcFeatureRenderer(symbol);
                }
                else
                {
                    featureLayer.FeatureRenderer = layer.Type switch
                    {
                        "fill-extrusion" => new VtcExtrusionRenderer() { Symbol = symbol },
                        _ => new VtcFeatureRenderer(symbol)
                    };
                }

                map.AddLayer(featureLayer);

                var tocElement = map.TOC.GetTocElementByLayerId(featureLayer.ID);
                if (tocElement != null)
                {
                    tocElement.Name = layer.Id;
                }
            }
            else if (@class is IRasterClass rasterClass)
            {
                var rasterLayer = new VtcRasterLayer(rasterClass, layer.ToPaintSymbol(map));

                map.AddLayer(rasterLayer);

                var tocElement = map.TOC.GetTocElementByLayerId(rasterLayer.ID);
                if (tocElement != null)
                {
                    tocElement.Name = layer.Id;
                }
            }
        }

        #endregion
    }

    public void AddToMap(IMap map, StylesCapabilities stylesCapabilities)
    {
        map.ResourceContainer[ResourceName] = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(stylesCapabilities)
            );

        map.MapEventHooks.Add(this);
    }
}
