using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.DataSources.VectorTileCache.Json.Styles;
using gView.Framework.Calc;
using gView.Framework.Cartography;
using gView.Framework.Core.Common;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Vtc;
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
                    map.MapScale = WebMercatorCalc.MapScale(
                            stylesCapabilities.Zoom,
                            stylesCapabilities.Center[1]
                        );
                }
            } 
            else
            {
                map.ZoomTo(new Envelope(-1, -1, 1, 1));
                map.MapScale = WebMercatorCalc.MapScale(7);
            }

            #endregion

            var hook = new OnMapLoadedEventHook();

            await hook.AddLayers(new Map(), stylesCapabilities); // test on dummy map
            hook.AddToMap(map, stylesCapabilities);

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
