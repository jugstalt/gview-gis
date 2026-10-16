namespace gView.Interoperability.OgcApi.Request;

/// <summary>
/// Operation names passed as <c>ServiceRequest.Method</c> to the <see cref="OgcApiRequestInterpreter"/>.
/// They determine the access types (map/query/edit) a request requires.
/// </summary>
public static class OgcApiOperations
{
    public const string LandingPage = "landingpage";
    public const string Conformance = "conformance";
    public const string Api = "api";
    public const string Collections = "collections";
    public const string Collection = "collection";
    public const string Queryables = "queryables";
    public const string Items = "items";
    public const string Item = "item";
    public const string Map = "map";
    public const string Options = "options";
    public const string CreateItem = "createitem";
    public const string ReplaceItem = "replaceitem";
    public const string UpdateItem = "updateitem";
    public const string DeleteItem = "deleteitem";
}
