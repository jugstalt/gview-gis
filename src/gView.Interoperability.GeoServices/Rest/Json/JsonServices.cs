using gView.Framework.Common.Reflection;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonServices
    {
        [JsonPropertyName("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonPropertyName("folders")]
        [HtmlLink("{url}/{0}")]
        public string[] Folders { get; set; }

        [JsonPropertyName("services")]
        public AgsService[] Services { get; set; }

        #region Classes

        [YamlGroupBy("Type")]
        public class AgsService
        {
            [JsonPropertyName("name")]
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

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        #endregion
    }
}
