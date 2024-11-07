using Nest;
using System.Text.Json.Serialization;

namespace gView.Cmd.ElasticSearch.Lib.Models;

[ElasticsearchType(RelationName = "meta")]
public class Meta
{
    [Text(Name = "id", Index = false)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [Text(Name = "category", Index = true)]
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [Text(Name = "sample", Index = false, Store = false)]
    [JsonPropertyName("sample")]
    public string Sample { get; set; } = "";

    [Text(Name = "description", Index = false, Store = false)]
    [JsonPropertyName("description")]
    public string Descrption { get; set; } = "";

    [Text(Name = "service", Index = false, Store = false)]
    [JsonPropertyName("service")]
    public string Service { get; set; } = "";

    [Text(Name = "query", Index = false, Store = false)]
    [JsonPropertyName("query")]
    public string Query { get; set; } = "";
}
