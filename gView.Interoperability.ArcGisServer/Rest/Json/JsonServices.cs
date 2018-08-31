using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public class JsonServices
    {
        [JsonProperty(PropertyName = "currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonProperty(PropertyName = "folders")]
        public string[] Folders { get; set; }

        [JsonProperty(PropertyName = "services")]
        public AgsServices[] Services { get; set; }

        #region Classes

        public class AgsServices
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        #endregion
    }
}
