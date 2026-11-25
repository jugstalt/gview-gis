using gView.Interoperability.Extensions.Extensions;
using gView.Framework.Editor.Core;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Maps;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi.OpenApi;

/// <summary>
/// Builds the OpenAPI 3.0 definition of one service. Each collection gets its own paths,
/// so the queryables can be documented as query parameters of its items operation.
/// </summary>
internal static partial class OpenApiDocumentBuilder
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

            var itemsPath = Get($"get{id}Features", collection.Title, $"Features of '{collection.Title}'", itemsParameters, GeoJsonOrHtml("featureCollectionGeoJSON"));
            var itemPath = Get($"get{id}Feature", collection.Title, $"A feature of '{collection.Title}'",
                [Ref("featureId"), Ref("crs"), Ref("f")], WithETag(GeoJsonOrHtml("featureGeoJSON", notFound: true)));

            #region Part 4

            var statements = service.EditStatements(id);
            if (statements.HasFlag(EditStatements.INSERT))
            {
                itemsPath["post"] = Edit($"create{id}Feature", collection.Title, $"Add a feature to '{collection.Title}'", [], true, "201", "Created, the Location header contains the url of the new feature");
            }
            if (statements.HasFlag(EditStatements.UPDATE))
            {
                itemPath["put"] = Edit($"replace{id}Feature", collection.Title, "Replace a feature (missing properties are set to null)", [Ref("featureId")], true, "204", "Replaced", ifMatch: true);
                itemPath["patch"] = Edit($"update{id}Feature", collection.Title, "Update the properties/geometry of a feature (JSON merge patch)", [Ref("featureId")], true, "204", "Updated", ifMatch: true);
            }
            if (statements.HasFlag(EditStatements.DELETE))
            {
                itemPath["delete"] = Edit($"delete{id}Feature", collection.Title, "Delete a feature", [Ref("featureId")], false, "204", "Deleted", ifMatch: true);
            }

            #endregion

            paths[$"{path}/items"] = itemsPath;
            paths[$"{path}/items/{{featureId}}"] = itemPath;
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
}
