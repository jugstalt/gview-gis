using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonError
    {
        [JsonProperty("error")]
        public ErrorDef Error { get; set; }

        public class ErrorDef
        {
            [JsonProperty("code")]
            public int Code { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("details")]
            public object Details { get; set; }
        }
    }
}
