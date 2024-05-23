using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using System.Text.Json;

namespace JsonPlayground;

internal class JsonRendererPolyground
{
    static public void Do()
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
}
