using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.FillElasticSearch
{
    [ElasticsearchType(Name="item")]
    public class Item
    {
        [Text(Name = "id", Index = false)]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [Text(Name="suggested_text", Boost = 2.0, Index = true, Analyzer = "german")]
        [JsonProperty(PropertyName = "suggested_text")]
        public string SuggestedText { get; set; }

        [Text(Name = "subtext", Boost = 1.0, Index = true, Analyzer = "german")]
        [JsonProperty(PropertyName = "subtext")]
        public string SubText { get; set; }

        [Text(Name = "category", Boost = 0.8, Index = true, Analyzer = "german")]
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [Text(Name = "thumbnail_url", Index = false, Store = false)]
        [JsonProperty(PropertyName = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [GeoPoint()]
        public Nest.GeoLocation Geo { get; set; }

        [Text(Name = "bbox", Index = false, Store = false)]
        public string BBox { get; set; }
    }

    [ElasticsearchType(Name = "meta")]
    public class Meta
    {
        [Text(Name = "id", Index = false)]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [Text(Name = "category",Index = true)]
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [Text(Name = "sample", Index = false, Store = false)]
        [JsonProperty(PropertyName = "sample")]
        public string Sample { get; set; }

        [Text(Name = "description", Index = false, Store = false)]
        [JsonProperty(PropertyName = "description")]
        public string Descrption { get; set; }

        [Text(Name = "service", Index = false, Store = false)]
        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }

        [Text(Name = "query", Index = false, Store = false)]
        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }
    }
}
