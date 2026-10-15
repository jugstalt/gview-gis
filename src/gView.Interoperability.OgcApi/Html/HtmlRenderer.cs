using gView.Framework.Core.Geometry;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Models;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace gView.Interoperability.OgcApi.Html;

/// <summary>
/// Renders the HTML body of the OGC API resources. The server embeds it in its page layout.
/// </summary>
public static class HtmlRenderer
{
    public static string ServiceList(ServiceList serviceList)
    {
        var sb = new StringBuilder();
        AppendHeader(sb, serviceList.Title, null);

        sb.Append("<table class='table'><tr><th>Service</th><th>Folder</th><th></th></tr>");
        foreach (var service in serviceList.Services)
        {
            var landingPage = service.Links.FirstOrDefault(l => l.Type == OgcApiMediaTypes.Html) ?? service.Links.FirstOrDefault();
            sb.Append("<tr>");
            sb.Append($"<td>{A(landingPage?.Href, service.Name)}</td>");
            sb.Append($"<td>{E(service.Folder)}</td>");
            sb.Append($"<td>{A(service.Links.FirstOrDefault(l => l.Type == OgcApiMediaTypes.Json)?.Href, "json")}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");

        AppendLinks(sb, serviceList.Links);
        return sb.ToString();
    }

    public static string LandingPage(LandingPage landingPage)
    {
        var sb = new StringBuilder();
        AppendHeader(sb, landingPage.Title, landingPage.Description);
        AppendLinks(sb, landingPage.Links);
        return sb.ToString();
    }

    public static string Conformance(ConformanceDeclaration conformance, string serviceUrl)
    {
        var sb = new StringBuilder();
        AppendBreadcrumb(sb, (serviceUrl, "Landing page"));
        AppendHeader(sb, "Conformance", null);

        sb.Append("<ul>");
        foreach (var conformsTo in conformance.ConformsTo)
        {
            sb.Append($"<li>{A(conformsTo, conformsTo)}</li>");
        }
        sb.Append("</ul>");

        return sb.ToString();
    }

    public static string Collections(Collections collections, string serviceUrl)
    {
        var sb = new StringBuilder();
        AppendBreadcrumb(sb, (serviceUrl, "Landing page"));
        AppendHeader(sb, "Collections", null);

        sb.Append("<table class='table'><tr><th>Id</th><th>Title</th><th>Description</th><th>Items</th></tr>");
        foreach (var collection in collections.Items)
        {
            string collectionUrl = $"{serviceUrl}/collections/{Uri.EscapeDataString(collection.Id)}?f=html";
            var itemsLink = collection.Links.FirstOrDefault(l => l.Rel == OgcApiRelations.Items && l.Type == OgcApiMediaTypes.Html);

            sb.Append("<tr>");
            sb.Append($"<td>{A(collectionUrl, collection.Id)}</td>");
            sb.Append($"<td>{E(collection.Title)}</td>");
            sb.Append($"<td>{E(collection.Description)}</td>");
            sb.Append($"<td>{A(itemsLink?.Href, "items")}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");

        AppendLinks(sb, collections.Links);
        return sb.ToString();
    }

    public static string Collection(Collection collection, string serviceUrl)
    {
        var sb = new StringBuilder();
        AppendBreadcrumb(sb, (serviceUrl, "Landing page"), ($"{serviceUrl}/collections?f=html", "Collections"));
        AppendHeader(sb, collection.Title ?? collection.Id, collection.Description);

        sb.Append("<table class='table'>");
        sb.Append($"<tr><td>Id</td><td>{E(collection.Id)}</td></tr>");
        sb.Append($"<tr><td>Item type</td><td>{E(collection.ItemType)}</td></tr>");
        if (collection.Extent?.Spatial?.BBox?.FirstOrDefault() is double[] bbox)
        {
            sb.Append($"<tr><td>Extent (CRS84)</td><td>{E(String.Join(", ", bbox.Select(v => v.ToString("0.######", CultureInfo.InvariantCulture))))}</td></tr>");
        }
        sb.Append($"<tr><td>Storage CRS</td><td>{E(collection.StorageCrs)}</td></tr>");
        sb.Append($"<tr><td>CRS</td><td>{String.Join("<br/>", (collection.Crs ?? []).Select(E))}</td></tr>");
        sb.Append("</table>");

        AppendLinks(sb, collection.Links);
        return sb.ToString();
    }

    public static string Queryables(JsonObject queryables, string serviceUrl, string collectionId)
    {
        var sb = new StringBuilder();
        AppendBreadcrumb(sb,
            (serviceUrl, "Landing page"),
            ($"{serviceUrl}/collections?f=html", "Collections"),
            ($"{serviceUrl}/collections/{Uri.EscapeDataString(collectionId)}?f=html", collectionId));
        AppendHeader(sb, $"Queryables: {queryables["title"]}", null);

        sb.Append("<table class='table'><tr><th>Name</th><th>Title</th><th>Type</th></tr>");
        if (queryables["properties"] is JsonObject properties)
        {
            foreach (var property in properties)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{E(property.Key)}</td>");
                sb.Append($"<td>{E(property.Value?["title"]?.ToString())}</td>");
                sb.Append($"<td>{E(property.Value?["type"]?.ToString())}</td>");
                sb.Append("</tr>");
            }
        }
        sb.Append("</table>");

        return sb.ToString();
    }

    public static string Items(ItemsResult items, OgcApiFeatureService service)
    {
        var sb = new StringBuilder();
        string collectionId = items.Collection.Id;

        AppendBreadcrumb(sb,
            (service.ServiceUrl, "Landing page"),
            ($"{service.CollectionsUrl}?f=html", "Collections"),
            ($"{service.CollectionUrl(collectionId)}?f=html", items.Collection.Title));
        AppendHeader(sb, $"{items.Collection.Title}: Items", null);

        sb.Append($"<p>{items.Page.Features.Count} features (offset {items.Query.Offset}), CRS: {E(items.Crs.Uri)}</p>");

        var propertyNames = items.Page.Features
            .SelectMany(f => f.Properties.Select(p => p.Key))
            .Distinct()
            .ToList();

        sb.Append("<div style='overflow-x:auto'><table class='table'><tr><th>Id</th><th>Geometry</th>");
        foreach (var name in propertyNames)
        {
            sb.Append($"<th>{E(name)}</th>");
        }
        sb.Append("</tr>");

        foreach (var feature in items.Page.Features)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{(feature.Id is null ? "" : A($"{service.ItemUrl(collectionId, feature.Id)}?f=html", Value(feature.Id)))}</td>");
            sb.Append($"<td>{E(GeometryTypeName(feature.Geometry))}</td>");
            foreach (var name in propertyNames)
            {
                var value = feature.Properties.FirstOrDefault(p => p.Key == name).Value;
                sb.Append($"<td>{E(Value(value))}</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</table></div>");

        sb.Append("<p>");
        foreach (var rel in new[] { OgcApiRelations.Prev, OgcApiRelations.Next })
        {
            var link = items.Links.FirstOrDefault(l => l.Rel == rel);
            if (link is not null)
            {
                sb.Append($"{A(link.Href.Replace("f=json", "f=html"), rel == OgcApiRelations.Prev ? "&laquo; previous" : "next &raquo;", encodeText: false)}&nbsp;&nbsp;");
            }
        }
        sb.Append("</p>");

        AppendLinks(sb, items.Links);
        return sb.ToString();
    }

    public static string Item(ItemResult item, OgcApiFeatureService service)
    {
        var sb = new StringBuilder();
        string collectionId = item.Collection.Id;

        AppendBreadcrumb(sb,
            (service.ServiceUrl, "Landing page"),
            ($"{service.CollectionsUrl}?f=html", "Collections"),
            ($"{service.CollectionUrl(collectionId)}?f=html", item.Collection.Title),
            ($"{service.ItemsUrl(collectionId)}?f=html", "Items"));
        AppendHeader(sb, $"{item.Collection.Title}: {Value(item.Feature.Id)}", null);

        sb.Append("<table class='table'>");
        sb.Append($"<tr><td>Id</td><td>{E(Value(item.Feature.Id))}</td></tr>");
        sb.Append($"<tr><td>Geometry</td><td>{E(GeometryTypeName(item.Feature.Geometry))}</td></tr>");
        foreach (var property in item.Feature.Properties)
        {
            sb.Append($"<tr><td>{E(property.Key)}</td><td>{E(Value(property.Value))}</td></tr>");
        }
        sb.Append("</table>");

        AppendLinks(sb, item.Links);
        return sb.ToString();
    }

    /// <summary>
    /// Swagger UI (from a CDN) for the API definition
    /// </summary>
    public static string SwaggerUi(string apiJsonUrl, string serviceUrl)
    {
        const string swaggerUiVersion = "5.17.14";

        var sb = new StringBuilder();
        AppendBreadcrumb(sb, (serviceUrl, "Landing page"));
        sb.Append($"<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/swagger-ui-dist@{swaggerUiVersion}/swagger-ui.css' />");
        sb.Append("<div id='swagger-ui' style='background:#fff'></div>");
        sb.Append($"<script src='https://cdn.jsdelivr.net/npm/swagger-ui-dist@{swaggerUiVersion}/swagger-ui-bundle.js'></script>");
        sb.Append($"<script>window.addEventListener('load', function() {{ SwaggerUIBundle({{ url: '{JsString(apiJsonUrl)}', dom_id: '#swagger-ui' }}); }});</script>");

        return sb.ToString();
    }

    #region Helper

    private static void AppendHeader(StringBuilder sb, string? title, string? description)
    {
        sb.Append($"<h1>{E(title)}</h1>");
        if (!String.IsNullOrWhiteSpace(description))
        {
            sb.Append($"<p>{E(description)}</p>");
        }
    }

    private static void AppendBreadcrumb(StringBuilder sb, params (string url, string title)[] items)
    {
        sb.Append("<div class='ogcapi-breadcrumb'>");
        sb.Append(String.Join(" / ", items.Select(i => A(i.url.Contains("f=html") ? i.url : $"{i.url}?f=html", i.title))));
        sb.Append("</div>");
    }

    private static void AppendLinks(StringBuilder sb, IEnumerable<Link> links)
    {
        sb.Append("<h3>Links</h3><ul>");
        foreach (var link in links)
        {
            sb.Append($"<li>{A(link.Href, link.Title ?? link.Href)} <small>({E(link.Rel)}{(String.IsNullOrEmpty(link.Type) ? "" : $", {E(link.Type)}")})</small></li>");
        }
        sb.Append("</ul>");
    }

    private static string A(string? href, string? text, bool encodeText = true)
        => String.IsNullOrEmpty(href)
            ? E(text)
            : $"<a href='{E(href)}'>{(encodeText ? E(text) : text)}</a>";

    private static string E(string? text) => WebUtility.HtmlEncode(text ?? "");

    private static string JsString(string text) => text.Replace("\\", "\\\\").Replace("'", "\\'");

    private static string Value(object? value)
        => value switch
        {
            null => "",
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };

    private static string GeometryTypeName(IGeometry? geometry)
        => geometry switch
        {
            null => "",
            IPoint => "Point",
            IMultiPoint => "MultiPoint",
            IPolyline polyline => polyline.PathCount > 1 ? "MultiLineString" : "LineString",
            IPolygon polygon => polygon.OuterRingCount > 1 ? "MultiPolygon" : "Polygon",
            IEnvelope => "Polygon",
            IAggregateGeometry => "GeometryCollection",
            _ => geometry.GetType().Name
        };

    #endregion
}
