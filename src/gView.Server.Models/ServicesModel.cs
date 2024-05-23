using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Server.Models
{
    public class ServicesModel
    {
        [JsonPropertyName("Services")]
        public ICollection<ServiceModel> Services { get; set; }
    }
}
