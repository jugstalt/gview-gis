using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.MxlUtil.Lib.Extensions;
using gView.Cmd.MxlUtil.Lib.Models.Json;
using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Calc;
using gView.Framework.Cartography;
using gView.Framework.Core.Common;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Vtc;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.Text.Json;

namespace gView.Cmd.MxlUtil.Lib;

public class FromGLStylesJsonCommand : ICommand
{
    public string Name => "MxlUtil.FromGLStylesJson";

    public string Description => "Creates a MXL file from a (TileCache) styles json definition";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
        [
            new RequiredCommandParameter<string>("uri")
            {
                Description="Url or path to style json",
            },
            new RequiredCommandParameter<string>("target-path")
            {
                Description="Target path, where XML file will be stored",
            },
            new RequiredCommandParameter<string>("map-name")
            {
                Description="Map name and name of the MXL file",
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
            HttpClient client = new HttpClient();

            var uri = new Uri(parameters.GetRequiredValue<string>("uri"));
            var targetpath = parameters.GetRequiredValue<string>("target-path");
            var mapName = parameters.GetRequiredValue<string>("map-name")
                                .ToLower()
                                .Replace(" ", "-")
                                .Replace("/", "-")
                                .Replace("\\", "-");

            #region Load Styles Json

            string jsonString;

            if (uri.IsUnc || uri.IsFile)
            {
                jsonString = await File.ReadAllTextAsync(uri.ToString());
            }
            else
            {
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

            var map = new Map()
            {
                Name = mapName,
            };

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

            #region Add Sprite as resources

            if (!String.IsNullOrEmpty(stylesCapabilities.Sprite))
            {
                IBitmap? glSpriteBitmap = null;

                try
                {
                    var spriteUri = new Uri(stylesCapabilities.Sprite);
                    string glSpriteJson;

                    if (spriteUri.IsUnc || spriteUri.IsFile)
                    {
                        glSpriteBitmap = Current.Engine.CreateBitmap($"{spriteUri}@2x.png");
                        glSpriteJson = await File.ReadAllTextAsync($"{spriteUri}@2x.json");
                    }
                    else
                    {
                        glSpriteBitmap = Current.Engine.CreateBitmap(
                            new MemoryStream(await client.GetByteArrayAsync($"{spriteUri}@2x.png")));
                        glSpriteJson = await client.GetStringAsync($"{spriteUri}@2x.json");
                    }

                    var glSprites = JsonSerializer.Deserialize<Dictionary<string, GLSprite>>(
                            glSpriteJson,
                            new JsonSerializerOptions(JsonSerializerDefaults.Web)
                        );
                    if (glSprites != null)
                    {
                        foreach (var glSprite in glSprites)
                        {
                            if (!stylesCapabilities.Layers.RequireSpriteIcon(glSprite.Key))
                            {
                                continue;
                            }

                            logger?.LogLine($"Add Sprite Icon {glSprite.Key}");

                            using (var bitmap = Current.Engine.CreateBitmap(glSprite.Value.Width, glSprite.Value.Height, PixelFormat.Rgba32))
                            using (var canvas = bitmap.CreateCanvas())
                            {
                                canvas.DrawBitmap(glSpriteBitmap,
                                    new CanvasRectangle(0, 0, bitmap.Width, bitmap.Height),
                                    new CanvasRectangle(
                                            glSprite.Value.X, glSprite.Value.Y,
                                            glSprite.Value.Width, glSprite.Value.Height
                                        )
                                    );

                                using (var ms = new MemoryStream())
                                {
                                    bitmap.Save(ms, ImageFormat.Png);
                                    map.ResourceContainer[$"{glSprite.Key}@2x"] = ms.ToArray();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogLine($"Warning: error reading sprites\n{ex.Message}");
                }
                finally
                {
                    if (glSpriteBitmap != null)
                    {
                        glSpriteBitmap.Dispose();
                    }
                }
            }

            #endregion

            #region Save Map

            string outFile = System.IO.Path.Combine(targetpath, $"{mapName}.mxl");

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
