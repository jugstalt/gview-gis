using gView.Framework.Editor.Core;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Maps;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi.OpenApi;

/// <summary>The schemas of the responses (components/schemas)</summary>
internal static partial class OpenApiDocumentBuilder
{
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
}
