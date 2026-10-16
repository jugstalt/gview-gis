namespace gView.Interoperability.OgcApi;

public static class OgcApiMediaTypes
{
    public const string Json = "application/json";
    public const string GeoJson = "application/geo+json";
    public const string Html = "text/html";
    public const string OpenApiJson = "application/vnd.oai.openapi+json;version=3.0";
    public const string SchemaJson = "application/schema+json";
    public const string Png = "image/png";
    public const string Jpeg = "image/jpeg";
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
    public const string Map = "http://www.opengis.net/def/rel/ogc/1.0/map";
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
    public const string FeaturesFilter = "http://www.opengis.net/spec/ogcapi-features-3/1.0/conf/filter";
    public const string FeaturesFeaturesFilter = "http://www.opengis.net/spec/ogcapi-features-3/1.0/conf/features-filter";

    public const string Cql2Text = "http://www.opengis.net/spec/cql2/1.0/conf/cql2-text";
    public const string Cql2Basic = "http://www.opengis.net/spec/cql2/1.0/conf/basic-cql2";
    public const string Cql2AdvancedComparisonOperators = "http://www.opengis.net/spec/cql2/1.0/conf/advanced-comparison-operators";
    public const string Cql2CaseInsensitiveComparison = "http://www.opengis.net/spec/cql2/1.0/conf/case-insensitive-comparison";
    public const string Cql2PropertyProperty = "http://www.opengis.net/spec/cql2/1.0/conf/property-property";

    public const string MapsCore = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/core";
    public const string MapsDatasetMap = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/dataset-map";
    public const string MapsCollectionMap = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/collection-map";
    public const string MapsBackground = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/background";
    public const string MapsCollectionsSelection = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/collections-selection";
    public const string MapsScaling = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/scaling";
    public const string MapsDisplayResolution = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/display-resolution";
    public const string MapsSpatialSubsetting = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/spatial-subsetting";
    public const string MapsCrs = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/crs";
    public const string MapsPng = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/png";
    public const string MapsJpeg = "http://www.opengis.net/spec/ogcapi-maps-1/1.0/conf/jpeg";

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
        FeaturesQueryablesQueryParameters,
        FeaturesFilter,
        FeaturesFeaturesFilter,
        Cql2Text,
        Cql2Basic,
        Cql2AdvancedComparisonOperators,
        Cql2CaseInsensitiveComparison,
        Cql2PropertyProperty,
        MapsCore,
        MapsDatasetMap,
        MapsCollectionMap,
        MapsBackground,
        MapsCollectionsSelection,
        MapsScaling,
        MapsDisplayResolution,
        MapsSpatialSubsetting,
        MapsCrs,
        MapsPng,
        MapsJpeg
    ];
}
