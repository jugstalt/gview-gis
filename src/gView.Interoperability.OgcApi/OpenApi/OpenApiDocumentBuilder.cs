using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Maps;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi.OpenApi;

/// <summary>
/// Builds the OpenAPI 3.0 definition of one service. Each collection gets its own paths,
/// so the queryables can be documented as query parameters of its items operation.
/// </summary>
internal static class OpenApiDocumentBuilder
{
    public static JsonObject Build(OgcApiFeatureService service)
    {
        var crsList = service.Map.SupportedCrs().Select(c => c.Uri).ToArray();
        var mapCrsList = service.Map.SupportedMapCrs().Select(c => c.Uri).ToArray();

        var paths = new JsonObject()
        {
            ["/"] = Get("getLandingPage", "Landing page", "Landing page of the service", [Ref("f")], JsonOrHtml("landingPage")),
            ["/conformance"] = Get("getConformance", "Capabilities", "Conformance classes implemented by the server", [Ref("f")], JsonOrHtml("confClasses")),
            ["/api"] = Get("getApi", "Capabilities", "The API definition (OpenAPI 3.0)", [Ref("f")], OpenApiOrHtml()),
            ["/collections"] = Get("getCollections", "Capabilities", "The collections of the service", [Ref("f")], JsonOrHtml("collections")),
            ["/map"] = Get("getDatasetMap", "Map", "Map image of the service",
                MapParameters(datasetMap: true), ImageResponse())
        };

        foreach (var collection in service.CollectionLayers)
        {
            string id = collection.Id;
            string path = $"/collections/{id}";

            paths[path] = Get($"describe{id}Collection", collection.Title, $"Metadata of the collection '{collection.Title}'", [Ref("f")], JsonOrHtml("collection"));
            paths[$"{path}/map"] = Get($"get{id}Map", collection.Title, $"Map image of '{collection.Title}'", MapParameters(datasetMap: false), ImageResponse());

            if (!collection.HasItems)
            {
                continue;
            }

            paths[$"{path}/queryables"] = Get($"get{id}Queryables", collection.Title, $"Queryables of '{collection.Title}'", [Ref("f")], SchemaResponse());

            var itemsParameters = new JsonArray(Ref("limit"), Ref("offset"), Ref("bbox"), Ref("bbox-crs"), Ref("crs"), Ref("datetime"),
                                                Ref("filter"), Ref("filter-lang"), Ref("filter-crs"), Ref("f"));
            foreach (var field in collection.Queryables)
            {
                var (type, format) = field.ToJsonSchemaType();
                var schema = new JsonObject() { ["type"] = type };
                if (format is not null)
                {
                    schema["format"] = format;
                }
                if (field.IsStringType())
                {
                    schema["description"] = "'*' can be used as wildcard";
                }

                itemsParameters.Add(new JsonObject()
                {
                    ["name"] = field.name,
                    ["in"] = "query",
                    ["description"] = $"Filter by {field.Title()}",
                    ["required"] = false,
                    ["style"] = "form",
                    ["explode"] = false,
                    ["schema"] = schema
                });
            }

            paths[$"{path}/items"] = Get($"get{id}Features", collection.Title, $"Features of '{collection.Title}'", itemsParameters, GeoJsonOrHtml("featureCollectionGeoJSON"));
            paths[$"{path}/items/{{featureId}}"] = Get($"get{id}Feature", collection.Title, $"A feature of '{collection.Title}'",
                [Ref("featureId"), Ref("crs"), Ref("f")], GeoJsonOrHtml("featureGeoJSON", notFound: true));
        }

        return new JsonObject()
        {
            ["openapi"] = "3.0.3",
            ["info"] = new JsonObject()
            {
                ["title"] = service.Title,
                ["description"] = $"OGC API - Features of the gView map service '{service.Title}'",
                ["version"] = "1.0.0"
            },
            ["servers"] = new JsonArray(new JsonObject() { ["url"] = service.ServiceUrl }),
            ["paths"] = paths,
            ["components"] = new JsonObject()
            {
                ["parameters"] = Parameters(crsList, mapCrsList),
                ["schemas"] = Schemas()
            }
        };
    }

    #region Paths

    private static JsonObject Get(string operationId, string tag, string summary, JsonArray parameters, JsonObject responses)
        => new JsonObject()
        {
            ["get"] = new JsonObject()
            {
                ["operationId"] = operationId,
                ["tags"] = new JsonArray(tag),
                ["summary"] = summary,
                ["parameters"] = parameters,
                ["responses"] = responses
            }
        };

    private static JsonObject Ref(string parameter)
        => new JsonObject() { ["$ref"] = $"#/components/parameters/{parameter}" };

    private static JsonObject SchemaRef(string schema)
        => new JsonObject() { ["$ref"] = $"#/components/schemas/{schema}" };

    private static JsonObject JsonOrHtml(string schema)
        => Responses(new JsonObject()
        {
            [OgcApiMediaTypes.Json] = new JsonObject() { ["schema"] = SchemaRef(schema) },
            [OgcApiMediaTypes.Html] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "string" } }
        });

    private static JsonObject GeoJsonOrHtml(string schema, bool notFound = false)
        => Responses(new JsonObject()
        {
            [OgcApiMediaTypes.GeoJson] = new JsonObject() { ["schema"] = SchemaRef(schema) },
            [OgcApiMediaTypes.Html] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "string" } }
        }, notFound);

    private static JsonObject OpenApiOrHtml()
        => Responses(new JsonObject()
        {
            [OgcApiMediaTypes.OpenApiJson] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "object" } },
            [OgcApiMediaTypes.Html] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "string" } }
        });

    private static JsonArray MapParameters(bool datasetMap)
    {
        var parameters = new JsonArray(Ref("map-bbox"), Ref("map-bbox-crs"), Ref("map-crs"), Ref("width"), Ref("height"),
                                       Ref("scale-denominator"), Ref("mm-per-pixel"), Ref("transparent"), Ref("bgcolor"));
        if (datasetMap)
        {
            parameters.Add(Ref("collections"));
        }
        parameters.Add(Ref("map-f"));

        return parameters;
    }

    private static JsonObject ImageResponse()
        => Responses(new JsonObject()
        {
            [OgcApiMediaTypes.Png] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "string", ["format"] = "binary" } },
            [OgcApiMediaTypes.Jpeg] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "string", ["format"] = "binary" } }
        });

    private static JsonObject SchemaResponse()
        => Responses(new JsonObject()
        {
            [OgcApiMediaTypes.SchemaJson] = new JsonObject() { ["schema"] = new JsonObject() { ["type"] = "object" } }
        });

    private static JsonObject Responses(JsonObject content, bool notFound = true)
    {
        var responses = new JsonObject()
        {
            ["200"] = new JsonObject() { ["description"] = "Successful request", ["content"] = content },
            ["400"] = Error("Invalid query parameters")
        };
        if (notFound)
        {
            responses["404"] = Error("The resource does not exist");
        }
        responses["500"] = Error("A server error occurred");

        return responses;
    }

    private static JsonObject Error(string description)
        => new JsonObject()
        {
            ["description"] = description,
            ["content"] = new JsonObject()
            {
                [OgcApiMediaTypes.Json] = new JsonObject() { ["schema"] = SchemaRef("exception") }
            }
        };

    #endregion

    #region Components

    private static JsonObject Parameters(string[] crsList, string[] mapCrsList)
    {
        JsonArray CrsEnum() => new JsonArray(crsList.Select(c => (JsonNode)JsonValue.Create(c)!).ToArray());
        JsonArray MapCrsEnum() => new JsonArray(mapCrsList.Select(c => (JsonNode)JsonValue.Create(c)!).ToArray());

        return new JsonObject()
        {
            ["f"] = QueryParameter("f", "The response format", new JsonObject()
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("json", "html")
            }),
            ["limit"] = QueryParameter("limit", "The maximum number of features in the response", new JsonObject()
            {
                ["type"] = "integer",
                ["minimum"] = 1,
                ["default"] = ItemsQuery.DefaultLimit
            }),
            ["offset"] = QueryParameter("offset", "The number of features to skip (paging)", new JsonObject()
            {
                ["type"] = "integer",
                ["minimum"] = 0,
                ["default"] = 0
            }),
            ["bbox"] = QueryParameter("bbox", "Only features that intersect the bounding box (minx,miny,maxx,maxy in the axis order of bbox-crs)", new JsonObject()
            {
                ["type"] = "array",
                ["minItems"] = 4,
                ["maxItems"] = 6,
                ["items"] = new JsonObject() { ["type"] = "number" }
            }, explode: false),
            ["bbox-crs"] = QueryParameter("bbox-crs", "The CRS of the bbox parameter", new JsonObject()
            {
                ["type"] = "string",
                ["format"] = "uri",
                ["enum"] = CrsEnum(),
                ["default"] = CrsReference.Crs84Uri
            }),
            ["crs"] = QueryParameter("crs", "The CRS of the geometries in the response", new JsonObject()
            {
                ["type"] = "string",
                ["format"] = "uri",
                ["enum"] = CrsEnum(),
                ["default"] = CrsReference.Crs84Uri
            }),
            ["datetime"] = QueryParameter("datetime", "Temporal filter (accepted, but the collections have no temporal property)", new JsonObject()
            {
                ["type"] = "string"
            }),
            ["filter"] = QueryParameter("filter", "CQL2-Text filter expression on the queryables, e.g. NAME LIKE 'Gr%' AND EW > 1000", new JsonObject()
            {
                ["type"] = "string"
            }),
            ["filter-lang"] = QueryParameter("filter-lang", "The language of the filter parameter", new JsonObject()
            {
                ["type"] = "string",
                ["enum"] = new JsonArray(ItemsQuery.FilterLanguage),
                ["default"] = ItemsQuery.FilterLanguage
            }),
            ["filter-crs"] = QueryParameter("filter-crs", "The CRS of geometries in the filter (no spatial predicates supported yet)", new JsonObject()
            {
                ["type"] = "string",
                ["format"] = "uri",
                ["enum"] = CrsEnum(),
                ["default"] = CrsReference.Crs84Uri
            }),
            ["map-bbox"] = QueryParameter("bbox", "The extent of the map (minx,miny,maxx,maxy in the axis order of bbox-crs), default: the extent of the data", new JsonObject()
            {
                ["type"] = "array",
                ["minItems"] = 4,
                ["maxItems"] = 6,
                ["items"] = new JsonObject() { ["type"] = "number" }
            }, explode: false),
            ["map-bbox-crs"] = QueryParameter("bbox-crs", "The CRS of the bbox parameter", new JsonObject()
            {
                ["type"] = "string",
                ["format"] = "uri",
                ["enum"] = MapCrsEnum(),
                ["default"] = CrsReference.Crs84Uri
            }),
            ["map-crs"] = QueryParameter("crs", "The CRS of the map, default: the CRS of the map service", new JsonObject()
            {
                ["type"] = "string",
                ["format"] = "uri",
                ["enum"] = MapCrsEnum()
            }),
            ["width"] = QueryParameter("width", "Width of the map in pixels", new JsonObject() { ["type"] = "integer", ["minimum"] = 1 }),
            ["height"] = QueryParameter("height", "Height of the map in pixels", new JsonObject() { ["type"] = "integer", ["minimum"] = 1 }),
            ["scale-denominator"] = QueryParameter("scale-denominator", "Scale of the map (without width/height, projected CRS only)", new JsonObject() { ["type"] = "number", ["exclusiveMinimum"] = 0 }),
            ["mm-per-pixel"] = QueryParameter("mm-per-pixel", "Display resolution", new JsonObject() { ["type"] = "number", ["default"] = MapQuery.DefaultMmPerPixel }),
            ["transparent"] = QueryParameter("transparent", "Transparent background", new JsonObject() { ["type"] = "boolean", ["default"] = false }),
            ["bgcolor"] = QueryParameter("bgcolor", "Background color (0xRRGGBB)", new JsonObject() { ["type"] = "string", ["default"] = "0xFFFFFF" }),
            ["collections"] = QueryParameter("collections", "The collections (layers) shown on the map, default: the default visible layers", new JsonObject()
            {
                ["type"] = "array",
                ["items"] = new JsonObject() { ["type"] = "string" }
            }, explode: false),
            ["map-f"] = QueryParameter("f", "The image format", new JsonObject()
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("png", "jpg"),
                ["default"] = "png"
            }),
            ["featureId"] = new JsonObject()
            {
                ["name"] = "featureId",
                ["in"] = "path",
                ["description"] = "The id of a feature",
                ["required"] = true,
                ["schema"] = new JsonObject() { ["type"] = "string" }
            }
        };
    }

    private static JsonObject QueryParameter(string name, string description, JsonObject schema, bool explode = true)
        => new JsonObject()
        {
            ["name"] = name,
            ["in"] = "query",
            ["description"] = description,
            ["required"] = false,
            ["style"] = "form",
            ["explode"] = explode,
            ["schema"] = schema
        };

    private static JsonObject Schemas()
    {
        JsonObject Links() => new JsonObject()
        {
            ["type"] = "array",
            ["items"] = SchemaRef("link")
        };

        return new JsonObject()
        {
            ["link"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("href", "rel"),
                ["properties"] = new JsonObject()
                {
                    ["href"] = new JsonObject() { ["type"] = "string" },
                    ["rel"] = new JsonObject() { ["type"] = "string" },
                    ["type"] = new JsonObject() { ["type"] = "string" },
                    ["title"] = new JsonObject() { ["type"] = "string" }
                }
            },
            ["exception"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("code"),
                ["properties"] = new JsonObject()
                {
                    ["code"] = new JsonObject() { ["type"] = "string" },
                    ["description"] = new JsonObject() { ["type"] = "string" }
                }
            },
            ["landingPage"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("links"),
                ["properties"] = new JsonObject()
                {
                    ["title"] = new JsonObject() { ["type"] = "string" },
                    ["description"] = new JsonObject() { ["type"] = "string" },
                    ["links"] = Links()
                }
            },
            ["confClasses"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("conformsTo"),
                ["properties"] = new JsonObject()
                {
                    ["conformsTo"] = new JsonObject() { ["type"] = "array", ["items"] = new JsonObject() { ["type"] = "string" } }
                }
            },
            ["collection"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("id", "links"),
                ["properties"] = new JsonObject()
                {
                    ["id"] = new JsonObject() { ["type"] = "string" },
                    ["title"] = new JsonObject() { ["type"] = "string" },
                    ["description"] = new JsonObject() { ["type"] = "string" },
                    ["links"] = Links(),
                    ["extent"] = new JsonObject() { ["type"] = "object" },
                    ["itemType"] = new JsonObject() { ["type"] = "string" },
                    ["crs"] = new JsonObject() { ["type"] = "array", ["items"] = new JsonObject() { ["type"] = "string" } },
                    ["storageCrs"] = new JsonObject() { ["type"] = "string" }
                }
            },
            ["collections"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("links", "collections"),
                ["properties"] = new JsonObject()
                {
                    ["links"] = Links(),
                    ["collections"] = new JsonObject() { ["type"] = "array", ["items"] = SchemaRef("collection") },
                    ["crs"] = new JsonObject() { ["type"] = "array", ["items"] = new JsonObject() { ["type"] = "string" } }
                }
            },
            ["featureGeoJSON"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("type", "geometry", "properties"),
                ["properties"] = new JsonObject()
                {
                    ["type"] = new JsonObject() { ["type"] = "string", ["enum"] = new JsonArray("Feature") },
                    ["id"] = new JsonObject()
                    {
                        ["oneOf"] = new JsonArray(
                            new JsonObject() { ["type"] = "string" },
                            new JsonObject() { ["type"] = "integer" })
                    },
                    ["geometry"] = new JsonObject() { ["type"] = "object", ["nullable"] = true },
                    ["properties"] = new JsonObject() { ["type"] = "object", ["nullable"] = true },
                    ["links"] = Links()
                }
            },
            ["featureCollectionGeoJSON"] = new JsonObject()
            {
                ["type"] = "object",
                ["required"] = new JsonArray("type", "features"),
                ["properties"] = new JsonObject()
                {
                    ["type"] = new JsonObject() { ["type"] = "string", ["enum"] = new JsonArray("FeatureCollection") },
                    ["features"] = new JsonObject() { ["type"] = "array", ["items"] = SchemaRef("featureGeoJSON") },
                    ["links"] = Links(),
                    ["timeStamp"] = new JsonObject() { ["type"] = "string", ["format"] = "date-time" },
                    ["numberReturned"] = new JsonObject() { ["type"] = "integer", ["minimum"] = 0 }
                }
            }
        };
    }

    #endregion
}
