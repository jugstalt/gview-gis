using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Interoperability.OgcApi.Extensions;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Models;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi;

/// <summary>
/// OGC API - Features (Part 1, 2 and queryables of Part 3) resources of one map service.
/// The service url is the landing page url, e.g. https://server/gview-server/ogcapi/folder/service
/// </summary>
public sealed class OgcApiFeatureService
{
    private const int MapDescriptionId = -1;  // gView.Framework.Cartography.Map.MapDescriptionId

    public OgcApiFeatureService(IServiceMap map, string serviceUrl)
    {
        Map = map;
        ServiceUrl = serviceUrl.TrimEnd('/');
    }

    public IServiceMap Map { get; }
    public string ServiceUrl { get; }
    public string Title => Map.ServiceTitle();

    public string ApiUrl => $"{ServiceUrl}/api";
    public string ConformanceUrl => $"{ServiceUrl}/conformance";
    public string CollectionsUrl => $"{ServiceUrl}/collections";
    public string CollectionUrl(string collectionId) => $"{CollectionsUrl}/{Uri.EscapeDataString(collectionId)}";
    public string ItemsUrl(string collectionId) => $"{CollectionUrl(collectionId)}/items";
    public string ItemUrl(string collectionId, object featureId) => $"{ItemsUrl(collectionId)}/{Uri.EscapeDataString(featureId.ToString() ?? "")}";
    public string QueryablesUrl(string collectionId) => $"{CollectionUrl(collectionId)}/queryables";

    public IEnumerable<OgcCollectionLayer> CollectionLayers => Map.OgcCollectionLayers();

    public OgcCollectionLayer GetCollectionLayer(string collectionId) => Map.GetOgcCollectionLayer(collectionId);

    public LandingPage GetLandingPage()
        => new LandingPage()
        {
            Title = Title,
            Description = Map.GetLayerDescription(MapDescriptionId),
            Links =
            [
                new Link(ServiceUrl.WithFormat("json"), OgcApiRelations.Self, OgcApiMediaTypes.Json, "This document"),
                new Link(ServiceUrl.WithFormat("html"), OgcApiRelations.Alternate, OgcApiMediaTypes.Html, "This document as HTML"),
                new Link(ApiUrl.WithFormat("json"), OgcApiRelations.ServiceDesc, OgcApiMediaTypes.OpenApiJson, "The API definition"),
                new Link(ApiUrl.WithFormat("html"), OgcApiRelations.ServiceDoc, OgcApiMediaTypes.Html, "The API documentation"),
                new Link(ConformanceUrl.WithFormat("json"), OgcApiRelations.Conformance, OgcApiMediaTypes.Json, "Conformance classes"),
                new Link(CollectionsUrl.WithFormat("json"), OgcApiRelations.Data, OgcApiMediaTypes.Json, "Feature collections")
            ]
        };

    public JsonObject GetOpenApiDocument() => OpenApi.OpenApiDocumentBuilder.Build(this);

    public ConformanceDeclaration GetConformance()
        => new ConformanceDeclaration() { ConformsTo = OgcApiConformance.All };

    public Collections GetCollections()
        => new Collections()
        {
            Links =
            [
                new Link(CollectionsUrl.WithFormat("json"), OgcApiRelations.Self, OgcApiMediaTypes.Json, "This document"),
                new Link(CollectionsUrl.WithFormat("html"), OgcApiRelations.Alternate, OgcApiMediaTypes.Html, "This document as HTML")
            ],
            Items = CollectionLayers.Select(ToCollection).ToList(),
            Crs = Map.SupportedCrs().Select(c => c.Uri).ToArray()
        };

    public Collection GetCollection(string collectionId)
    {
        var collection = ToCollection(GetCollectionLayer(collectionId));

        // self/alternate of the collection resource itself come first
        collection.Links.InsertRange(0,
        [
            new Link(CollectionUrl(collectionId).WithFormat("json"), OgcApiRelations.Self, OgcApiMediaTypes.Json, "This document"),
            new Link(CollectionUrl(collectionId).WithFormat("html"), OgcApiRelations.Alternate, OgcApiMediaTypes.Html, "This document as HTML")
        ]);

        return collection;
    }

    /// <summary>
    /// Part 3 queryables as JSON schema
    /// </summary>
    public JsonObject GetQueryables(string collectionId)
    {
        var collection = GetCollectionLayer(collectionId);
        var properties = new JsonObject();

        foreach (var field in collection.Queryables)
        {
            var (type, format) = field.ToJsonSchemaType();
            var property = new JsonObject()
            {
                ["title"] = field.Title(),
                ["type"] = type
            };
            if (format is not null)
            {
                property["format"] = format;
            }
            properties[field.name] = property;
        }

        return new JsonObject()
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["$id"] = QueryablesUrl(collectionId),
            ["type"] = "object",
            ["title"] = collection.Title,
            ["properties"] = properties,
            ["additionalProperties"] = false
        };
    }

    public async Task<ItemsResult> GetItemsAsync(string collectionId, IReadOnlyDictionary<string, string> parameters)
    {
        var collection = GetCollectionLayer(collectionId);
        var query = ItemsQuery.Parse(
                        parameters,
                        collection,
                        Map.SupportedCrs(collection),
                        Map.MapServiceProperties?.MaxRecordCount ?? ItemsQuery.DefaultLimit);

        var page = await FeatureQuery.QueryItemsAsync(Map, collection, query);

        string itemsUrl = ItemsUrl(collectionId);
        var queryParameters = parameters
            .Where(p => p.Key != "f" && p.Key != "offset" && p.Key != "token")
            .ToList();

        List<KeyValuePair<string, string>> WithPaging(int offset, string format)
        {
            var result = new List<KeyValuePair<string, string>>(queryParameters);
            if (!result.Any(p => p.Key == "limit"))
            {
                result.Add(new("limit", query.Limit.ToString()));
            }
            if (offset > 0)
            {
                result.Add(new("offset", offset.ToString()));
            }
            result.Add(new("f", format));
            return result;
        }

        var links = new List<Link>()
        {
            new Link(itemsUrl.WithQuery(WithPaging(query.Offset, "json")), OgcApiRelations.Self, OgcApiMediaTypes.GeoJson, "This document"),
            new Link(itemsUrl.WithQuery(WithPaging(query.Offset, "html")), OgcApiRelations.Alternate, OgcApiMediaTypes.Html, "This document as HTML"),
            new Link(CollectionUrl(collectionId).WithFormat("json"), OgcApiRelations.Collection, OgcApiMediaTypes.Json, "The collection")
        };

        if (page.HasMore)
        {
            links.Add(new Link(itemsUrl.WithQuery(WithPaging(query.Offset + query.Limit, "json")), OgcApiRelations.Next, OgcApiMediaTypes.GeoJson, "Next page"));
        }
        if (query.Offset > 0)
        {
            links.Add(new Link(itemsUrl.WithQuery(WithPaging(Math.Max(0, query.Offset - query.Limit), "json")), OgcApiRelations.Prev, OgcApiMediaTypes.GeoJson, "Previous page"));
        }

        return new ItemsResult(collection, query, page, links, query.Crs.HasNorthingFirst(query.Crs.ToSpatialReference()));
    }

    public async Task<ItemResult> GetItemAsync(string collectionId, string featureId, IReadOnlyDictionary<string, string> parameters)
    {
        var collection = GetCollectionLayer(collectionId);
        var crs = CrsReference.Crs84;

        foreach (var parameter in parameters.Keys)
        {
            if (parameter is not ("crs" or "f" or "token"))
            {
                throw OgcApiException.BadRequest($"Unknown query parameter '{parameter}'");
            }
        }

        if (parameters.TryGetValue("crs", out var crsParameter))
        {
            if (!CrsReference.TryParse(crsParameter, out crs) || !Map.SupportedCrs(collection).Contains(crs))
            {
                throw OgcApiException.BadRequest($"Unsupported crs: {crsParameter}");
            }
        }

        var feature = await FeatureQuery.QueryItemAsync(Map, collection, featureId, crs);

        string itemUrl = ItemUrl(collectionId, featureId);
        var crsParameters = crs.IsCrs84 ? [] : new List<KeyValuePair<string, string>>() { new("crs", crs.Uri) };

        var links = new List<Link>()
        {
            new Link(itemUrl.WithQuery(crsParameters).WithFormat("json"), OgcApiRelations.Self, OgcApiMediaTypes.GeoJson, "This document"),
            new Link(itemUrl.WithQuery(crsParameters).WithFormat("html"), OgcApiRelations.Alternate, OgcApiMediaTypes.Html, "This document as HTML"),
            new Link(CollectionUrl(collectionId).WithFormat("json"), OgcApiRelations.Collection, OgcApiMediaTypes.Json, "The collection")
        };

        return new ItemResult(collection, feature, crs, links, crs.HasNorthingFirst(crs.ToSpatialReference()));
    }

    #region Helper

    private Collection ToCollection(OgcCollectionLayer collectionLayer)
    {
        string collectionId = collectionLayer.Id;
        var bbox = Crs84Extent(collectionLayer);

        return new Collection()
        {
            Id = collectionId,
            Title = collectionLayer.Title,
            Description = String.IsNullOrWhiteSpace(collectionLayer.Description) ? null : collectionLayer.Description,
            Extent = bbox is null
                ? null
                : new Extent() { Spatial = new SpatialExtent() { BBox = [bbox] } },
            Crs = Map.SupportedCrs(collectionLayer).Select(c => c.Uri).ToArray(),
            StorageCrs = collectionLayer.SpatialReference?.EpsgCode > 0
                ? CrsReference.FromEpsg(collectionLayer.SpatialReference.EpsgCode).Uri
                : null,
            Links =
            [
                new Link(ItemsUrl(collectionId).WithFormat("json"), OgcApiRelations.Items, OgcApiMediaTypes.GeoJson, "Items"),
                new Link(ItemsUrl(collectionId).WithFormat("html"), OgcApiRelations.Items, OgcApiMediaTypes.Html, "Items as HTML"),
                new Link(QueryablesUrl(collectionId).WithFormat("json"), OgcApiRelations.Queryables, OgcApiMediaTypes.SchemaJson, "Queryables")
            ]
        };
    }

    private double[]? Crs84Extent(OgcCollectionLayer collectionLayer)
    {
        try
        {
            var envelope = collectionLayer.FeatureClass.Envelope;
            if (envelope is null || collectionLayer.SpatialReference is null)
            {
                return null;
            }

            using var transformer = GeometricTransformerFactory.Create(Map.Display?.DatumTransformations);
            transformer.SetSpatialReferences(collectionLayer.SpatialReference, CrsReference.Crs84.ToSpatialReference());

            var crs84Envelope = (transformer.Transform2D(envelope) as IGeometry)?.Envelope;
            if (crs84Envelope is null
                || !double.IsFinite(crs84Envelope.MinX) || !double.IsFinite(crs84Envelope.MinY)
                || !double.IsFinite(crs84Envelope.MaxX) || !double.IsFinite(crs84Envelope.MaxY))
            {
                return null;
            }

            return [crs84Envelope.MinX, crs84Envelope.MinY, crs84Envelope.MaxX, crs84Envelope.MaxY];
        }
        catch
        {
            return null;  // the extent is optional
        }
    }

    #endregion
}
