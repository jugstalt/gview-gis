using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public class JsonDomain
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("codedValues")]
        public JsonCodedValues[] CodedValues { get; set; }
    }

            public class JsonCodedValues
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                [JsonProperty("code")]
                public string Code { get; set; }
            }
}
