using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    class JsonRasterResponseDTO
    {

        [JsonPropertyName("results")]
        public Result[] Results { get; set; }

        #region Classes

        public class Result
        {
            [JsonPropertyName("layerId")]
            public int LayerId { get; set; }

            [JsonPropertyName("layerName")]
            public string LayerName { get; set; }

            [JsonPropertyName("displayFieldName")]
            public string DisplayFieldName { get; set; }

            [JsonPropertyName("attributes")]
            public object ResultAttributes { get; set; }

            #region Class 

            //public class Attributes
            //{
            //    [JsonPropertyName("Class Value")]
            //    public string ClassValue { get; set; }

            //    [JsonPropertyName("Pixel Value")]
            //    public string PixelValue { get; set; }
            //}

            #endregion
        }

        #endregion
    }

}
