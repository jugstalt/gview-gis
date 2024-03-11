using System.Text.Json.Serialization;

namespace gView.Cmd.FillLuceneServer
{
    public class Meta
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("sample")]
        public string Sample { get; set; }

        [JsonPropertyName("description")]
        public string Descrption { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }
    }
}
