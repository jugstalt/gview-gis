using Newtonsoft.Json;

namespace gView.Cmd.FillLuceneServer
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "suggested_text")]
        public string SuggestedText { get; set; }

        [JsonProperty(PropertyName = "subtext")]
        public string SubText { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }
}
