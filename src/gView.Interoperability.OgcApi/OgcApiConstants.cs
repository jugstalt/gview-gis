namespace gView.Interoperability.OgcApi;

public static class OgcApiMediaTypes
{
    public const string Json = "application/json";
    public const string GeoJson = "application/geo+json";
    public const string Html = "text/html";
    public const string OpenApiJson = "application/vnd.oai.openapi+json;version=3.0";
    public const string SchemaJson = "application/schema+json";
}

public static class OgcApiRelations
{
    public const string Self = "self";
    public const string Alternate = "alternate";
    public const string ServiceDesc = "service-desc";
    public const string ServiceDoc = "service-doc";
    public const string Conformance = "conformance";
    public const string Data = "data";
    public const string Items = "items";
    public const string Collection = "collection";
    public const string Next = "next";
    public const string Prev = "prev";
    public const string Queryables = "http://www.opengis.net/def/rel/ogc/1.0/queryables";
}

public static class OgcApiConformance
{
    public const string CommonCore = "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/core";
    public const string CommonLandingPage = "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/landing-page";
    public const string CommonJson = "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/json";
    public const string CommonHtml = "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/html";
    public const string CommonOas30 = "http://www.opengis.net/spec/ogcapi-common-1/1.0/conf/oas30";
    public const string CommonCollections = "http://www.opengis.net/spec/ogcapi-common-2/1.0/conf/collections";

    public const string FeaturesCore = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/core";
    public const string FeaturesGeoJson = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/geojson";
    public const string FeaturesHtml = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/html";
    public const string FeaturesOas30 = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/oas30";
    public const string FeaturesCrs = "http://www.opengis.net/spec/ogcapi-features-2/1.0/conf/crs";
    public const string FeaturesQueryables = "http://www.opengis.net/spec/ogcapi-features-3/1.0/conf/queryables";
    public const string FeaturesQueryablesQueryParameters = "http://www.opengis.net/spec/ogcapi-features-3/1.0/conf/queryables-query-parameters";

    public static readonly string[] All =
    [
        CommonCore,
        CommonLandingPage,
        CommonJson,
        CommonHtml,
        CommonOas30,
        CommonCollections,
        FeaturesCore,
        FeaturesGeoJson,
        FeaturesHtml,
        FeaturesOas30,
        FeaturesCrs,
        FeaturesQueryables,
        FeaturesQueryablesQueryParameters
    ];
}
