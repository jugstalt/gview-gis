using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;

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

        [YamlGroupBy("Type")]
        public class AgsService
        {
            [JsonProperty(PropertyName = "name")]
            //[HtmlLink("{url}/{0}/{Type}")]
            [HtmlLink("{url}/{ServiceName}/{Type}")]
            public string Name { get; set; }

            [JsonIgnore]
            public string ServiceName
            {
                get
                {
                    if (this.Name.Contains("/"))
                    {
                        return Name.Substring(Name.LastIndexOf("/") + 1);
                    }

                    return Name;
                }
            }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        #endregion
    }
}
