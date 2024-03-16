using gView.DataSources.VectorTileCache.Json.Styles;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Data;
using gView.Framework.Vtc.Extensions;
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
        if(stylesCapabilities == null)
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

            }
            else
            {
                throw new Exception($"Warning: not supported source type: {source.Value.Type}");
            }
        }

        #endregion

        #region Try Parse all layers

        foreach (var layer in stylesCapabilities.Layers)
        {
            var dataset = datasets.ContainsKey(layer.Source)
                ? datasets[layer.Source]
                : null;

            if (dataset == null)
            {
                //throw new Exception($"Unkown layer dataset: {layer.Source}");
                continue;
            }

            var @class = (await dataset.Element(layer.SourceLayerId)).Class;
            if (@class == null)
            {
                throw new Exception($"Warning: unknown layer class: {layer.Source}.{layer.SourceLayerId}");
            }

            if (@class is IFeatureClass featureClass)
            {
                //logger?.LogLine($"adding layer: {layer.Id}");

                var featureLayer = new FeatureLayer(featureClass);
                //featureLayer.Title = layer.Id;

                var symbol = layer.ToPaintSymbol();
                featureLayer.FeatureRenderer = new UniversalGeometryRenderer();

                map.AddLayer(featureLayer);
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
