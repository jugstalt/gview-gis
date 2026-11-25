using gView.Framework.Editor.Core;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Maps;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi.OpenApi;

/// <summary>The operations of the paths and their responses (content types, errors, ETag)</summary>
internal static partial class OpenApiDocumentBuilder
{
    private static JsonObject Edit(string operationId, string tag, string summary, JsonArray parameters, bool withBody, string successCode, string successDescription, bool ifMatch = false)
    {
        if (withBody)
        {
            parameters.Add(new JsonObject()
            {
                ["name"] = "Content-Crs",
                ["in"] = "header",
                ["description"] = "The CRS of the geometry in the body, e.g. <http://www.opengis.net/def/crs/EPSG/0/31256>. Default: CRS84",
                ["required"] = false,
                ["schema"] = new JsonObject() { ["type"] = "string" }
            });
        }
        if (ifMatch)
        {
            parameters.Add(new JsonObject()
            {
                ["name"] = "If-Match",
                ["in"] = "header",
                ["description"] = "The ETag of the feature from GET .../items/{featureId}: the feature is only changed, if it was not changed in the meantime (otherwise 412)",
                ["required"] = false,
                ["schema"] = new JsonObject() { ["type"] = "string" }
            });
        }

        var operation = new JsonObject()
        {
            ["operationId"] = operationId,
            ["tags"] = new JsonArray(tag),
            ["summary"] = summary,
            ["parameters"] = parameters,
            ["responses"] = new JsonObject()
            {
                [successCode] = new JsonObject() { ["description"] = successDescription },
                ["400"] = Error("Invalid feature"),
                ["404"] = Error("The feature does not exist"),
                ["405"] = Error("The operation is not allowed for this collection"),
                ["500"] = Error("A server error occurred")
            }
        };

        if (ifMatch)
        {
            ((JsonObject)operation["responses"]!)["412"] = Error("The feature was changed in the meantime (If-Match)");
        }

        if (withBody)
        {
            operation["requestBody"] = new JsonObject()
            {
                ["required"] = true,
                ["content"] = new JsonObject()
                {
                    [OgcApiMediaTypes.GeoJson] = new JsonObject() { ["schema"] = SchemaRef("featureGeoJSON") }
                }
            };
        }

        return operation;
    }

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

    private static JsonObject WithETag(JsonObject responses)
    {
        // the strong ETag of a feature (the same for every crs), sent back as If-Match when editing it
        ((JsonObject)responses["200"]!)["headers"] = new JsonObject()
        {
            ["ETag"] = new JsonObject()
            {
                ["description"] = "The version of the feature, for If-Match when replacing, updating or deleting it",
                ["schema"] = new JsonObject() { ["type"] = "string" }
            }
        };

        return responses;
    }

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
}
