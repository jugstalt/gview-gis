using System.Text.Json;

namespace gView.Cmd.FillLuceneServer
{
    public class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("suggested_text")]
        public string SuggestedText { get; set; }

        [JsonPropertyName("subtext")]
        public string SubText { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }
}
