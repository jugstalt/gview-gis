using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace gView.Cmd.LuceneServer.Lib.Models;

public class Item
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("suggested_text")]
    public string SuggestedText { get; set; } = "";

    [JsonPropertyName("subtext")]
    public string SubText { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; } = "";
}
