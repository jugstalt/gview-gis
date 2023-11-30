using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonGenerateToken
    {
        public JsonGenerateToken()
        {
            this.Request = "gettoken";
            Expiration = 30;
        }

        [JsonProperty("request")]
        [FormInput(FormInputAttribute.InputTypes.Hidden)]
        public string Request { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        [FormInput(FormInputAttribute.InputTypes.Password)]
        public string Password { get; set; }

        [JsonProperty("expiration")]
        public int Expiration { get; set; }
    }
}
