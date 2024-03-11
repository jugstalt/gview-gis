using System.Text.Json.Serialization;

namespace gView.Framework.Core.MapServer
{
    public interface IMapServiceAccess
    {
        [JsonPropertyName("username")]
        string Username { get; set; }

        [JsonPropertyName("servicetypes")]
        string[] ServiceTypes { get; }

        void AddServiceType(string serviceType);
        void RemoveServiceType(string serviceType);

        bool IsAllowed(string serviceType);
    }
}
