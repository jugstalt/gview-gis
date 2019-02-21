using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonServices
    {
        [JsonProperty(PropertyName = "currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonProperty(PropertyName = "folders")]
        [HtmlLink("{url}/{0}")]
        public string[] Folders { get; set; }

        [JsonProperty(PropertyName = "services")]
        public AgsService[] Services { get; set; }

        #region Classes

        public class AgsService
        {
            [JsonProperty(PropertyName = "name")]
            [HtmlLink("{url}/{0}/{Type}")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        #endregion
    }
}
