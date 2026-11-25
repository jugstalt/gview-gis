using gView.Framework.Editor.Core;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Maps;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi.OpenApi;

/// <summary>The parameters shared by the paths (components/parameters)</summary>
internal static partial class OpenApiDocumentBuilder
{
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
}
