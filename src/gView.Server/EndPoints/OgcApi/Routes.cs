namespace gView.Server.EndPoints.OgcApi;

internal class Routes
{
    public const string Base = "ogcapi";

    /// <summary>
    /// Placeholder for services in the root folder: /ogcapi/_/{service}
    /// </summary>
    public const string RootFolder = "_";

    public const string Service = $"{Base}/{{folder}}/{{service}}";
    public const string Conformance = $"{Service}/conformance";
    public const string Api = $"{Service}/api";
    public const string Collections = $"{Service}/collections";
    public const string Collection = $"{Collections}/{{collectionId}}";
    public const string Queryables = $"{Collection}/queryables";
    public const string Items = $"{Collection}/items";
    public const string Item = $"{Items}/{{featureId}}";
}
