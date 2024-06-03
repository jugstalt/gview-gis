using System.Text.Json;
using System.Text.Json.Serialization;

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

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Message { get; set; }
    }
}
