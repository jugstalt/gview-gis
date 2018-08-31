using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    class JsonRasterResponse
    {

        [JsonProperty("results")]
        public Result[] Results { get; set; }

        #region Classes

        public class Result
        {
            [JsonProperty("layerId")]
            public int LayerId { get; set; }

            [JsonProperty("layerName")]
            public string LayerName { get; set; }

            [JsonProperty("displayFieldName")]
            public string DisplayFieldName { get; set; }

            [JsonProperty("attributes")]
            public object ResultAttributes { get; set; }

            #region Class 

            //public class Attributes
            //{
            //    [JsonProperty("Class Value")]
            //    public string ClassValue { get; set; }

            //    [JsonProperty("Pixel Value")]
            //    public string PixelValue { get; set; }
            //}

            #endregion
        }

        #endregion
    }

}
