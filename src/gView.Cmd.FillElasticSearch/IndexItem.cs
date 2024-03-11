using Nest;
using System.Text.Json;

namespace gView.Cmd.FillElasticSearch
{
    [ElasticsearchType(RelationName = "item")]
    public class Item
    {
        [Text(Name = "id", Index = false)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [Text(Name = "suggested_text", Boost = 2.0, Index = true, Analyzer = "german")]
        [JsonPropertyName("suggested_text")]
        public string SuggestedText { get; set; }

        [Text(Name = "subtext", Boost = 1.0, Index = true, Analyzer = "german")]
        [JsonPropertyName("subtext")]
        public string SubText { get; set; }

        [Text(Name = "category", Boost = 0.8, Index = true, Analyzer = "german")]
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [Text(Name = "thumbnail_url", Index = false, Store = false)]
        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [GeoPoint()]
        public Nest.GeoLocation Geo { get; set; }

        [Text(Name = "bbox", Index = false, Store = false)]
        public string BBox { get; set; }
    }

    [ElasticsearchType(RelationName = "meta")]
    public class Meta
    {
        [Text(Name = "id", Index = false)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [Text(Name = "category", Index = true)]
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [Text(Name = "sample", Index = false, Store = false)]
        [JsonPropertyName("sample")]
        public string Sample { get; set; }

        [Text(Name = "description", Index = false, Store = false)]
        [JsonPropertyName("description")]
        public string Descrption { get; set; }

        [Text(Name = "service", Index = false, Store = false)]
        [JsonPropertyName("service")]
        public string Service { get; set; }

        [Text(Name = "query", Index = false, Store = false)]
        [JsonPropertyName("query")]
        public string Query { get; set; }
    }
}
