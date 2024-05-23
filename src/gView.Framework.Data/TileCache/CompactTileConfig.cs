using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Framework.Data.TileCache
{
    public class CompactTileConfig
    {
        [JsonPropertyName("Epsg")]
        public int Epsg { get; set; }

        [JsonPropertyName("Dpi")]
        public double Dpi { get; set; }

        [JsonPropertyName("Origin")]
        public double[] Origin { get; set; }

        [JsonPropertyName("Extent")]
        public double[] Extent { get; set; }

        [JsonPropertyName("Orientation")]
        public string Orientation { get; set; }

        [JsonPropertyName("TileSize")]
        public int[] TileSize { get; set; }

        [JsonPropertyName("Format")]
        public string Format { get; set; }

        [JsonPropertyName("Levels")]
        public IEnumerable<LevelConfig> Levels { get; set; }

        public class LevelConfig
        {
            [JsonPropertyName("Level")]
            public int Level { get; set; }

            [JsonPropertyName("Scale")]
            public double Scale { get; set; }
        }
    }
}
