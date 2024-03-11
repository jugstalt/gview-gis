using gView.Framework.Common.Json.Converters;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Interoperability.GeoServices.Request.Extensions;
using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using JsonPlayground.Extensions;
using JsonPlayground.Models;
using MongoDB.Bson;
using System.Text.Json;
using static gView.Server.AppCode.MapServiceSettings;

namespace JsonPlayground;

internal class JsonGeometryPlayground
{
    static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        Converters = { new Array2DConverter() },
    };

    static public void Do()
    {
        IPoint point = new Point(1.1, 2.2);

        IMultiPoint mPoint = new MultiPoint([
            new Point(1.1, 2.2), new Point(3.3, 4.4)
            ]);

        IPolyline polyline = new Polyline(
            new gView.Framework.Geometry.Path([new Point(1.1, 2.2), new Point(3.3, 4.4)])
            );

        IPolyline mPolyline = new Polyline([
            new gView.Framework.Geometry.Path([new Point(1.1, 2.2), new Point(3.3, 4.4)]),
            new gView.Framework.Geometry.Path([new Point(5.5, 6.6), new Point(7.7, 8.8)])
            ]);

        IPolygon polygon = new Polygon(
            new Ring([new Point(1.1, 2.2), new Point(3.3, 4.4)])
            );

        IPolygon mPolygon = new Polygon();
        mPolygon.AddRing(new Ring([new Point(1.1, 2.2), new Point(3.3, 4.4)]));
        mPolygon.AddRing(new Ring([new Point(5.5, 6.6), new Point(7.7, 8.8)]));

        var pointJsonString = Serialize(point);
        var mPointJsonString = Serialize(mPoint);
        var polylineJsonString = Serialize(polyline);
        var mPolylineJsonString = Serialize(mPolyline);
        var polygonJsonString = Serialize(polygon);
        var mPolygonJsonString = Serialize(mPolygon);

        Deserialize(pointJsonString);
        Deserialize(mPointJsonString);
        Deserialize(polylineJsonString);
        Deserialize(mPolylineJsonString);
        Deserialize(polygonJsonString);
        Deserialize(mPolygonJsonString);
    }

    static public void DoRenderer()
    {
        var jsonSMSRendererString = """
            {
                "type": "simple",
                "symbol": {
                    "type": "esriSMS",
                    "style": "esriSMSCircle",
                    "color": [
                        255,
                        255,
                        255,
                        0
                    ],
                    "size": 5,
                    "angle": 0,
                    "xoffset": 0,
                    "yoffset": 0,
                    "outline": {
                        "type": "esriSLS",
                        "style": null,
                        "color": [
                            255,
                            255,
                            255,
                            0
                        ],
                        "width": 1
                    }
                }
            }
            """;

        var pointFeatureRenderer = JsonRenderer.FromJsonRenderer(JsonSerializer.Deserialize<JsonRenderer>(jsonSMSRendererString));

        var jsonUniqueValueRenderer = """
            {
                "type": "uniqueValue",
                "field1": "TYP",
                "fieldDelimiter": ", ",
                "uniqueValueInfos": [
                    {
                        "symbol": {
                            "type": "esriTS",
                            "color": [
                                255,
                                0,
                                255,
                                255
                            ],
                            "verticalAlignment": null,
                            "horizontalAlignment": null,
                            "rightToLeft": false,
                            "angle": 0,
                            "xoffset": 0,
                            "yoffset": -5,
                            "kerning": false,
                            "font": {
                                "family": "BEV_DKM_Symbole_05_2012",
                                "size": 27,
                                "style": "Regular",
                                "weight": null,
                                "decoration": null
                            }
                        },
                        "value": "EP",
                        "label": "Einschaltpunkte",
                        "description": null
                    },
                    {
                        "symbol": {
                            "type": "esriTS",
                            "color": [
                                255,
                                0,
                                255,
                                255
                            ],
                            "verticalAlignment": null,
                            "horizontalAlignment": null,
                            "rightToLeft": false,
                            "angle": 0,
                            "xoffset": 0,
                            "yoffset": -5,
                            "kerning": false,
                            "font": {
                                "family": "BEV_DKM_Symbole_05_2012",
                                "size": 26,
                                "style": "Regular",
                                "weight": null,
                                "decoration": null
                            }
                        },
                        "value": "HP",
                        "label": "Höhenpunkt",
                        "description": null
                    },
                    {
                        "symbol": {
                            "type": "esriTS",
                            "color": [
                                255,
                                0,
                                255,
                                255
                            ],
                            "verticalAlignment": null,
                            "horizontalAlignment": null,
                            "rightToLeft": false,
                            "angle": 0,
                            "xoffset": 0,
                            "yoffset": -5,
                            "kerning": false,
                            "font": {
                                "family": "BEV_DKM_Symbole_05_2012",
                                "size": 26,
                                "style": "Regular",
                                "weight": null,
                                "decoration": null
                            }
                        },
                        "value": "PP",
                        "label": "Polygon-, Meßpunkt",
                        "description": null
                    },
                    {
                        "symbol": {
                            "type": "esriTS",
                            "color": [
                                255,
                                0,
                                255,
                                255
                            ],
                            "verticalAlignment": null,
                            "horizontalAlignment": null,
                            "rightToLeft": false,
                            "angle": 0,
                            "xoffset": 0,
                            "yoffset": -5,
                            "kerning": false,
                            "font": {
                                "family": "BEV_DKM_Symbole_05_2012",
                                "size": 30,
                                "style": "Regular",
                                "weight": null,
                                "decoration": null
                            }
                        },
                        "value": "TP",
                        "label": "Triangulierungspunkt",
                        "description": null
                    }
                ]
            }
            """;

        var uniqueValueFeatureRenderer = JsonRenderer.FromJsonRenderer(JsonSerializer.Deserialize<JsonRenderer>(jsonUniqueValueRenderer));

        var jsonSFSRenderer = """
            {
                "type": "simple",
                "symbol": {
                    "type": "esriSFS",
                    "style": "esriSFSSolid",
                    "color": [
                        255,
                        255,
                        255,
                        0
                    ],
                    "outline": {
                        "type": "esriSLS",
                        "style": "esriSLSSolid",
                        "color": [
                            128,
                            128,
                            128,
                            255
                        ],
                        "width": 0.5
                    }
                }
            }
            """;

        var polygonFeatureRenderer = JsonRenderer.FromJsonRenderer(JsonSerializer.Deserialize<JsonRenderer>(jsonSFSRenderer));
    }

    static public void DoMapServerSettings()
    {
        var settingsJson = """{"status":0,"accessrules":[{"username":"sys_user_1","servicetypes":["map","query","_all"]}],"refreshticks":0}""";

        var settings = JsonSerializer.Deserialize<gView.Server.AppCode.MapServiceSettings>(settingsJson,
            new JsonSerializerOptions()
            {
                //Converters = { new MapServiceAccessConvertFactory() }
            });

        var serializedSettingsJson = JsonSerializer.Serialize(settings);

        Console.WriteLine(settingsJson);
        Console.WriteLine(serializedSettingsJson);
        Console.WriteLine($"Ident: {serializedSettingsJson.Equals(settingsJson)}");
        Console.WriteLine("---------------------------------------------------------------------------");

    }

    static private string Serialize(IGeometry geometry)
        => Serialize(geometry.ToNJsonGeometry()!, geometry.ToJsonGeometry());


    static private string Serialize(NJsonGeometry nJsonGeometry, JsonGeometry jsonGeometry)
    {
        string jsonString1 = "", jsonString2 = "";
        try
        {
            jsonString1 = Newtonsoft.Json.JsonConvert.SerializeObject(nJsonGeometry);
        }
        catch (Exception ex) { jsonString1 = ex.Message; }

        try
        {
            jsonString2 = System.Text.Json.JsonSerializer.Serialize(jsonGeometry, JsonOptions);
        }
        catch (Exception ex) { jsonString2 = ex.Message; }

        Console.WriteLine("Newtonsoft.Json");
        Console.WriteLine(jsonString1);
        Console.WriteLine("System.Text.Json:");
        Console.WriteLine(jsonString2);
        Console.WriteLine($"Ident: {jsonString2.Equals(jsonString1)}");
        Console.WriteLine("---------------------------------------------------------------------------");

        return jsonString2;
    }

    static private (JsonGeometry newtonSoft, JsonGeometry stj) Deserialize(string jsonString)
    {
        JsonGeometry? jsonGeometry1 = null, jsonGeometry2 = null;

        jsonGeometry1 = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonGeometry>(jsonString);
        jsonGeometry2 = System.Text.Json.JsonSerializer.Deserialize<JsonGeometry>(jsonString, JsonOptions);

        return (jsonGeometry1, jsonGeometry2);
    }
}
