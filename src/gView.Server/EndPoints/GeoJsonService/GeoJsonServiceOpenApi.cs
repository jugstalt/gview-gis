#nullable enable

using gView.Framework.Core.MapServer;
using gView.GeoJsonService.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace gView.Server.EndPoints.GeoJsonService;

/// <summary>
/// The OpenAPI 3.1 description of one GeoJson service: only the operations the user has the rights for. The schemas
/// come from the request/response classes (JsonSchemaExporter), so the description follows the code.
/// </summary>
internal static class GeoJsonServiceOpenApi
{
    // not JsonSerializerDefaults.Web: it allows numbers as strings, the schema then says "string or number" and
    // clients (Scalar) fill in "" - the service reads numbers strictly
    private static readonly JsonSerializerOptions SchemaOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter<GeometryType>(), new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },  // GeoJSON types as in RFC 7946: Point, Polygon, ...
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <param name="example">values of the service for the examples (extent, crs, layers); without, no examples</param>
    public static JsonObject Build(string serviceUrl, string title, AccessTypes accessTypes, ServiceExample? example = null)
    {
        var schemas = new JsonObject();
        var paths = new JsonObject();

        JsonObject Ref<T>()
        {
            string name = typeof(T).Name;
            if (!schemas.ContainsKey(name))
            {
                schemas[name] = Schema(typeof(T), name);
            }
            return new JsonObject { ["$ref"] = $"#/components/schemas/{name}" };
        }

        JsonObject Operation<TRequest, TResponse>(string summary, string tag, (string name, string description)[] queryParameters, bool post, bool pathLayerId = false)
            where TRequest : BaseRequest, new()
        {
            var path = new JsonObject();
            var parameters = new JsonArray();
            if (pathLayerId)
            {
                parameters.Add(Parameter("layerId", "path", "The id of the layer (see capabilities)", "integer", required: true,
                                         example: example?.QueryLayerId));
            }
            var body = example is null ? null : ExampleOf<TRequest>(example);

            var get = new JsonObject
            {
                ["tags"] = new JsonArray(tag),
                ["summary"] = summary,
                ["parameters"] = Clone(parameters),
                ["responses"] = Responses(Ref<TResponse>())
            };
            foreach (var (name, description) in queryParameters)
            {
                ((JsonArray)get["parameters"]!).Add(Parameter(name, "query", description, "string", example: QueryExample(body, name)));
            }
            if (queryParameters.Length > 0 || !post)
            {
                path["get"] = get;
            }

            if (post)
            {
                path["post"] = new JsonObject
                {
                    ["tags"] = new JsonArray(tag),
                    ["summary"] = summary,
                    ["parameters"] = Clone(parameters),
                    ["requestBody"] = new JsonObject
                    {
                        ["required"] = true,
                        ["content"] = new JsonObject { ["application/json"] = Content(Ref<TRequest>(), body) }
                    },
                    ["responses"] = Responses(Ref<TResponse>())
                };
            }
            return path;
        }

        paths["/capabilities"] = Operation<GetServiceCapabilitiesRequest, GetServiceCapabilitiesResponse>(
            "The capabilities of the service: requests, layers, properties", "Service", [("crs", "CRS of the extents, e.g. EPSG:4326")], post: false);
        paths["/layers"] = Operation<GetLayersRequest, GetLayersResponse>(
            "All layers with extent, properties, domains and renderer", "Service", [("crs", "CRS of the extents")], post: false);
        paths["/layers/{layerId}"] = Operation<GetLayersRequest, GetLayersResponse>(
            "One layer with extent, properties, domains and renderer", "Service", [("crs", "CRS of the extent")], post: false, pathLayerId: true);

        if (accessTypes.HasFlag(AccessTypes.Map))
        {
            paths["/map"] = Operation<GetMapRequest, GetMapResponse>("A map image", "Map",
                [("bbox", "minx,miny,maxx,maxy"), ("crs", "CRS of bbox and image"), ("width", "pixels"), ("height", "pixels"),
                 ("format", "png or jpg"), ("transparent", "true/false"), ("layers", "ids, or +id/-id to change the defaults"),
                 ("rotation", "degrees"), ("dpi", "resolution"), ("responseFormat", "url, base64 or image"),
                 ("layerDefs", "JSON: { \"layerId\": { \"cql2\": \"...\" } }"), ("highlight", "JSON: { \"layerId\": [objectIds] }")], post: true);
            paths["/legend"] = Operation<GetLegendRequest, GetLegendResponse>("The legend of the layers", "Map",
                [("width", "symbol width (1-1024)"), ("height", "symbol height (1-1024)"), ("dpi", "resolution")], post: true);
        }

        if (accessTypes.HasFlag(AccessTypes.Query))
        {
            paths["/query/{layerId}"] = Operation<GetFeaturesRequest, FeatureCollection>("Features, ids, a count, distinct values or statistics of a layer", "Query",
                [("command", "select, distinct, countOnly, idsOnly or statistics"), ("outFields", "fields, comma separated (default *)"),
                 ("returnGeometry", "none, geometry, bbox or centroid"), ("outCRS", "EPSG:code"), ("where", "SQL where clause"),
                 ("filter", "CQL2 filter"), ("filter-lang", "cql2-text (default) or cql2-json"), ("bbox", "minx,miny,maxx,maxy"),
                 ("bbox-crs", "CRS of bbox"), ("objectIds", "comma separated"), ("orderBy", "field [ASC|DESC], comma separated"),
                 ("limit", "max. features"), ("offset", "paging"), ("statistics", "type:field[:alias], comma separated"),
                 ("groupBy", "fields of the statistics"), ("having", "CQL2 filter on the statistics rows"),
                 ("maxAllowableOffset", "generalize lines and polygons"), ("geometryPrecision", "decimal places")], post: true, pathLayerId: true);
            paths["/identify"] = Operation<IdentifyRequest, IdentifyResponse>("The features of several layers at a point or in a geometry", "Query",
                [("geometry", "x,y or a GeoJSON geometry"), ("crs", "CRS of geometry and map extent"), ("layerOption", "visible, top or all"),
                 ("layers", "ids, or +id/-id"), ("tolerance", "pixels"), ("mapExtent", "minx,miny,maxx,maxy"), ("imageWidth", "pixels"),
                 ("imageHeight", "pixels"), ("dpi", "resolution"), ("returnGeometry", "none, geometry, bbox or centroid"),
                 ("outCRS", "EPSG:code"), ("limit", "max. features per layer"), ("maxAllowableOffset", "generalize"),
                 ("geometryPrecision", "decimal places")], post: true);
            paths["/find"] = Operation<FindRequest, FindResponse>("A text search over the text fields of several layers", "Query",
                [("searchText", "the text"), ("searchFields", "fields, comma separated (default: all text fields)"),
                 ("layers", "layer ids, comma separated (default: all)"), ("contains", "true (default) or false (equals)"),
                 ("returnGeometry", "none, geometry, bbox or centroid"), ("outCRS", "EPSG:code"), ("limit", "max. features per layer"),
                 ("maxAllowableOffset", "generalize"), ("geometryPrecision", "decimal places")], post: true);
        }

        if (accessTypes.HasFlag(AccessTypes.Edit))
        {
            var features = new JsonObject();
            foreach (var (method, summary) in new[] { ("post", "Insert features"), ("put", "Update features (with oid)"), ("delete", "Delete features (objectIds)") })
            {
                features[method] = new JsonObject
                {
                    ["tags"] = new JsonArray("Edit"),
                    ["summary"] = summary,
                    ["parameters"] = new JsonArray(Parameter("layerId", "path", "The id of the layer", "integer", required: true)),
                    ["requestBody"] = new JsonObject
                    {
                        ["required"] = true,
                        ["content"] = new JsonObject { ["application/json"] = Content(Ref<EditFeaturesRequest>(), null) }
                    },
                    ["responses"] = Responses(Ref<EditFeaturesResponse>())
                };
            }
            paths["/features/{layerId}"] = features;
            paths["/applyEdits"] = Operation<ApplyEditsRequest, ApplyEditsResponse>(
                "Adds, updates and deletes of several layers in one transaction (versions: 412). The example inserts one feature!", "Edit", [], post: true);
        }

        schemas[nameof(ErrorResponse)] = Schema(typeof(ErrorResponse), nameof(ErrorResponse));

        return new JsonObject
        {
            ["openapi"] = "3.1.0",
            ["info"] = new JsonObject
            {
                ["title"] = $"{title} (GeoJson Service)",
                ["version"] = "1.0",
                ["description"] = "gView GeoJson Service. Specification: https://docs.gviewonline.com/en/spec/geojson_service/index.html"
            },
            ["servers"] = new JsonArray(new JsonObject { ["url"] = serviceUrl }),
            ["paths"] = paths,
            ["components"] = new JsonObject
            {
                ["schemas"] = schemas,
                ["securitySchemes"] = new JsonObject
                {
                    ["bearer"] = new JsonObject { ["type"] = "http", ["scheme"] = "bearer" }
                }
            }
        };
    }

    private static JsonNode Schema(Type type, string name)
    {
        var schema = JsonSchemaExporter.GetJsonSchemaAsNode(SchemaOptions, type, new JsonSchemaExporterOptions() { TreatNullObliviousAsNonNullable = true });
        RebaseRefs(schema, $"#/components/schemas/{name}");

        // requests and responses carry their kind in "type" (e.g. GetMap): the service needs exactly this value
        if ((typeof(BaseRequest).IsAssignableFrom(type) || typeof(BaseResponse).IsAssignableFrom(type))
            && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) is not null
            && type.GetProperty("Type")?.GetValue(Activator.CreateInstance(type)) is string typeValue
            && schema["properties"]?["type"] is JsonObject typeSchema)
        {
            typeSchema["type"] = "string";
            typeSchema["const"] = typeValue;
            typeSchema["default"] = typeValue;
        }
        return schema;
    }

    private static JsonObject Content(JsonObject schema, object? example)
    {
        var content = new JsonObject { ["schema"] = schema };
        if (example is not null)
        {
            content["example"] = JsonSerializer.SerializeToNode(example, example.GetType(), SchemaOptions);
        }
        return content;
    }

    /// <summary>
    /// A request that can be sent right away, like the request forms of Browse Services: the full extent and crs of the
    /// map, the layers shown by default, the queryable layers, the center of the map for identify
    /// </summary>
    internal static BaseRequest ExampleOf<TRequest>(ServiceExample example) where TRequest : BaseRequest, new()
    {
        var crs = example.CrsObject;
        BaseRequest request = new TRequest();
        switch (request)
        {
            case GetMapRequest map:
                map.Layers = example.VisibleLayers;
                map.BBox = example.Extent!;
                map.CRS = crs;
                map.Width = 800;
                map.Height = 600;
                map.Dpi = 96;
                map.Format = "png";
                map.Transparent = true;
                break;
            case GetLegendRequest legend:
                legend.Dpi = 96;
                break;
            case GetFeaturesRequest query:
                query.OutFields = ["*"];
                query.ReturnGeometry = GeometryResult.Geometry;
                query.OutCRS = crs;
                query.Limit = 10;
                break;
            case IdentifyRequest identify:
                identify.Geometry = example.Center is null ? null : new Geometry { Type = GeometryType.Point, Coordinates = example.Center };
                identify.CRS = crs;
                identify.Layers = example.FeatureLayers;
                identify.LayerOption = "all";
                identify.MapExtent = example.Extent;
                identify.ImageWidth = 800;
                identify.ImageHeight = 600;
                identify.OutCRS = crs;
                identify.Limit = 10;
                break;
            case FindRequest find:
                find.SearchText = "a";
                find.Layers = example.FeatureLayers;
                find.ReturnGeometry = GeometryResult.Centroid;
                find.OutCRS = crs;
                find.Limit = 10;
                break;
            case GetServiceCapabilitiesRequest capabilities:
                capabilities.CRS = crs;
                break;
            case GetLayersRequest layers:
                layers.CRS = crs;
                break;
            case ApplyEditsRequest applyEdits when example.InsertLayer is not null:
                // inserts one feature: sending the example changes data (the user has the edit right)
                applyEdits.CRS = crs;
                applyEdits.Edits =
                [
                    new LayerEdits
                    {
                        LayerId = example.InsertLayer.Id,
                        Adds = [new Feature { Geometry = example.GeometryAtCenter(example.InsertLayer.GeometryType) }]
                    }
                ];
                break;
        }
        return request;
    }

    /// <summary>The value of a GET parameter from the example request: bbox as "minx,miny,maxx,maxy", crs as name, arrays comma separated</summary>
    private static string? QueryExample(BaseRequest? example, string parameter)
    {
        if (example is null)
        {
            return null;
        }
        var node = JsonSerializer.SerializeToNode(example, example.GetType(), SchemaOptions) as JsonObject;
        var value = node?.FirstOrDefault(p => p.Key.Equals(parameter, StringComparison.OrdinalIgnoreCase)).Value;
        return value switch
        {
            null => null,
            JsonObject crs when crs["properties"]?["name"] is JsonValue name => name.ToString(),
            JsonObject bbox when bbox["minX"] is not null
                => string.Join(",", new[] { "minX", "minY", "maxX", "maxY" }.Select(k => bbox[k]!.ToJsonString())),
            JsonObject geometry when geometry["coordinates"] is JsonArray xy && xy.Count == 2 => $"{xy[0]!.ToJsonString()},{xy[1]!.ToJsonString()}",
            JsonObject => null,
            JsonArray array => array.Count == 0 ? null : string.Join(",", array.Select(a => a is JsonValue v && v.TryGetValue<string>(out var text) ? text : a?.ToJsonString())),
            JsonValue v when v.TryGetValue<string>(out var text) => text,
            _ => value.ToJsonString()
        };
    }

    /// <summary>
    /// JsonSchemaExporter writes repeated types as refs relative to the schema root ("#/properties/..."),
    /// inside components/schemas they have to point below the schema.
    /// </summary>
    private static void RebaseRefs(JsonNode? node, string root)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj["$ref"] is JsonValue refValue
                    && refValue.TryGetValue<string>(out var reference)
                    && (reference == "#" || (reference.StartsWith("#/") && !reference.StartsWith("#/components/"))))
                {
                    obj["$ref"] = root + reference[1..];
                }
                foreach (var property in obj.ToArray())
                {
                    RebaseRefs(property.Value, root);
                }
                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    RebaseRefs(item, root);
                }
                break;
        }
    }

    private static JsonObject Parameter(string name, string @in, string description, string type, bool required = false, string? example = null)
    {
        var parameter = new JsonObject
        {
            ["name"] = name,
            ["in"] = @in,
            ["description"] = description,
            ["required"] = required,
            ["schema"] = new JsonObject { ["type"] = type }
        };
        if (!string.IsNullOrEmpty(example))
        {
            parameter["example"] = example;
        }
        return parameter;
    }

    private static JsonObject Responses(JsonObject schema)
        => new()
        {
            ["200"] = new JsonObject
            {
                ["description"] = "Successful request",
                ["content"] = new JsonObject { ["application/json"] = new JsonObject { ["schema"] = schema } }
            },
            ["400"] = Error("Invalid request parameters"),
            ["403"] = Error("Missing right"),
            ["404"] = Error("Unknown layer or object"),
            ["500"] = Error("Server error")
        };

    private static JsonObject Error(string description)
        => new()
        {
            ["description"] = description,
            ["content"] = new JsonObject
            {
                ["application/json"] = new JsonObject { ["schema"] = new JsonObject { ["$ref"] = $"#/components/schemas/{nameof(ErrorResponse)}" } }
            }
        };

    private static JsonArray Clone(JsonArray array) => (JsonArray)array.DeepClone();
}
