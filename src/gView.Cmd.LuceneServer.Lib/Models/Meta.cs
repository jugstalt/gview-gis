using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace gView.Cmd.LuceneServer.Lib.Models;

public class Meta
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("sample")]
    public string Sample { get; set; } = "";

    [JsonPropertyName("description")]
    public string Descrption { get; set; } = "";

    [JsonPropertyName("service")]
    public string Service { get; set; } = "";

    [JsonPropertyName("query")]
    public string Query { get; set; } = "";
}
