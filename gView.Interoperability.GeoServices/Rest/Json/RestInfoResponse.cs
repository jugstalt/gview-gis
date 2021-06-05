using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class RestInfoResponse
    {
        public RestInfoResponse()
        {
        }

        [JsonProperty("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonProperty("fullVersion")]
        public string FullVersion { get; set; }

        [JsonProperty("soapUrl")]
        public string SoapUrl { get; set; }

        [JsonProperty("secureSoapUrl")]
        public string SecureSoapUrl { get; set; }

        public AuthInfo AuthInfoInstance { get; set; }

        #region Classes

        public class AuthInfo
        {
            [JsonProperty("isTokenBasedSecurity")]
            public bool IsTokenBasedSecurity { get; set; }

            [JsonProperty("tokenServicesUrl")]
            public string TokenServicesUrl { get; set; }

            [JsonProperty("shortLivedTokenValidity")]
            public int ShortLivedTokenValidity { get; set; }
        }

        #endregion
    }
}
