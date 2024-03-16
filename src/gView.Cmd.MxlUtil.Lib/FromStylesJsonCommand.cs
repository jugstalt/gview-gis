using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.MxlUtil.Lib.Extensions;
using gView.DataSources.VectorTileCache.Json.Styles;
using gView.Framework.Cartography;
using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using System.Text;
using System.Text.Json;

namespace gView.Cmd.MxlUtil.Lib;

public class FromStylesJsonCommand : ICommand
{
    public string Name => "MxlUtil.FromStylesJson";

    public string Description => "Creates a MXL file from a (TileCache) styles json definition";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
        [
            new RequiredCommandParameter<string>("uri")
            {
                Description="Url or path to style json",
            }
        ];

    async public Task<bool> Run(
            IDictionary<string, object> parameters,
            ICancelTracker? cancelTracker = null,
            ICommandLogger? logger = null
        )
    {
        try
        {
            var uri = new Uri(parameters.GetRequiredValue<string>("uri"));

            #region Load Styles Json

            string jsonString;

            if (uri.IsUnc || uri.IsFile)
            {
                jsonString = await File.ReadAllTextAsync(uri.ToString());
            }
            else
            {
                HttpClient client = new HttpClient();
                jsonString = await client.GetStringAsync(uri.ToString());
            }

            if (String.IsNullOrEmpty(jsonString))
            {
                throw new Exception("Can't load styles json or json is empty");
            }

            var stylesCapabilities = JsonSerializer.Deserialize<StylesCapabilities>(jsonString);
            if (stylesCapabilities == null)
            {
                throw new Exception("Can't deserialize styles json");
            }

            #endregion

            var map = new Map();

            #region Map Properties

            map.ImageWidth = 600;
            map.ImageHeight = 600;

            var sRef4326 = SpatialReference.FromID("epsg:4326");
            var sRef = SpatialReference.FromID("epsg:3857");
            map.SpatialReference = sRef;

            if (stylesCapabilities.Center != null && stylesCapabilities.Center.Length == 2)
            {
                var centerPoint = new Point(stylesCapabilities.Center[0], stylesCapabilities.Center[1]);
                centerPoint = GeometricTransformerFactory.Transform2D(centerPoint, sRef4326, sRef) as Point;

                if (centerPoint != null)
                {
                    map.ZoomTo(new Envelope(centerPoint.X - 1, centerPoint.Y - 1, centerPoint.X + 1, centerPoint.Y + 1));
                    map.MapScale = 10000;
                }
            }

            #endregion

            var datasetIndex = 0;
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
                    logger?.LogLine($"Warning: not supported source type: {source.Value.Type}");
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
                    logger?.LogLine($"Warning: unkown layer dataset: {layer.Source}");
                    continue;
                }

                var @class = (await dataset.Element(layer.SourceLayerId)).Class;
                if (@class == null)
                {
                    logger?.LogLine($"Warning: unknown layer class: {layer.Source}.{layer.SourceLayerId}");
                    continue;
                }

                if (@class is IFeatureClass featureClass)
                {
                    logger?.LogLine($"adding layer: {layer.Id}");

                    var featureLayer = new FeatureLayer(featureClass);
                    //featureLayer.Title = layer.Id;

                    var symbol = layer.ToPaintSymbol();
                    featureLayer.FeatureRenderer = new UniversalGeometryRenderer();

                    //map.AddLayer(featureLayer);
                }
            }

            #endregion

            #region Add Json as Resoure

            map.ResourceContainer["StylesJson"] = Encoding.UTF8.GetBytes(jsonString);

            #endregion

            #region Save Map

            string outFile = @"c:\temp\geodaten\test1.mxl";

            MxlDocument doc = new MxlDocument();
            doc.AddMap(map);
            doc.FocusMap = map;

            var stream = new XmlStream("");
            stream.Save("MapDocument", doc);

            logger?.LogLine($"Write: {outFile}");
            stream.WriteStream(outFile);
            logger?.LogLine("succeeded...");

            #endregion

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogLine($"ERROR: {ex.Message}");

            return false;
        }
    }
}
