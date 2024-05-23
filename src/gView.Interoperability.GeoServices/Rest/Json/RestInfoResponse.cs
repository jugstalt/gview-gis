using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class RestInfoResponse
    {
        public RestInfoResponse()
        {
        }

        [JsonPropertyName("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonPropertyName("fullVersion")]
        public string FullVersion { get; set; }

        [JsonPropertyName("soapUrl")]
        public string SoapUrl { get; set; }

        [JsonPropertyName("secureSoapUrl")]
        public string SecureSoapUrl { get; set; }

        [JsonPropertyName("authInfoInstance")]
        public AuthInfo AuthInfoInstance { get; set; }

        #region Classes

        public class AuthInfo
        {
            [JsonPropertyName("isTokenBasedSecurity")]
            public bool IsTokenBasedSecurity { get; set; }

            [JsonPropertyName("tokenServicesUrl")]
            public string TokenServicesUrl { get; set; }

            [JsonPropertyName("shortLivedTokenValidity")]
            public int ShortLivedTokenValidity { get; set; }
        }

        #endregion
    }
}
