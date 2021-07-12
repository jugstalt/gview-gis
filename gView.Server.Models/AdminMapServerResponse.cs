using Newtonsoft.Json;
using System;

namespace gView.Server.Models
{
    public class AdminMapServerResponse
    {
        public AdminMapServerResponse()
        {
            this.Success = true;
        }

        public AdminMapServerResponse(bool success, string message = null)
        {
            this.Success = success;
            this.Message = message;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}
