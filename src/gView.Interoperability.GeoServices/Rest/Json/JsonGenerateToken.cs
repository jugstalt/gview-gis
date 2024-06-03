using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonGenerateToken
    {
        public JsonGenerateToken()
        {
            this.Request = "gettoken";
            Expiration = 30;
        }

        [JsonPropertyName("request")]
        [FormInput(FormInputAttribute.InputTypes.Hidden)]
        public string Request { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        [FormInput(FormInputAttribute.InputTypes.Password)]
        public string Password { get; set; }

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; }
    }
}
