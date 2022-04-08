using Newtonsoft.Json;

namespace gView.Cmd.FillLuceneServer
{
    public class Meta
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "sample")]
        public string Sample { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Descrption { get; set; }

        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }

        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }
    }
}
