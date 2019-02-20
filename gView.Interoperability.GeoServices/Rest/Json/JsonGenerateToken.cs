using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonGenerateToken
    {
        public JsonGenerateToken()
        {
            this.Request = "gettoken";
        }

        [JsonProperty("request")]
        public string Request { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("expiration")]
        public int Expiration { get; set; }
    }
}
