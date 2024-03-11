using gView.Framework.Core.MapServer;
using System.Text.Json;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using gView.Framework.Common.Json.Converters;

namespace gView.Server.AppCode
{
    public class MapServiceSettings : IMapServiceSettings
    {
        public MapServiceSettings()
        {
            this.Status = MapServiceStatus.Idle;
            this.RefreshServiceTicks = 0;
        }

        [JsonPropertyName("status")]
        public MapServiceStatus Status { get; set; }

        [JsonPropertyName("accessrules")]
        [JsonConverter(typeof(TypeMappingConverter<IMapServiceAccess[], MapServiceAccess[]>))]
        public IMapServiceAccess[] AccessRules { get; set; }

        [JsonIgnore]
        public DateTime RefreshService { get; set; }

        [JsonPropertyName("refreshticks")]
        public long RefreshServiceTicks
        {
            get { return RefreshService.Ticks; }
            set { RefreshService = new DateTime(value, DateTimeKind.Utc); }
        }

        [JsonPropertyName("onlineresource")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string OnlineResource { get; set; }
        [JsonPropertyName("outputurl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string OutputUrl { get; set; }

        #region Classes

        public class MapServiceAccess : IMapServiceAccess
        {
            [JsonPropertyName("username")]
            public string Username { get; set; }

            private string[] _serviceTypes = null;

            [JsonPropertyName("servicetypes")]
            public string[] ServiceTypes
            {
                get { return _serviceTypes; }
                set { _serviceTypes = value; }
            }

            public void AddServiceType(string serviceType)
            {
                if (String.IsNullOrWhiteSpace(serviceType))
                {
                    return;
                }

                serviceType = serviceType.ToLower();

                if (_serviceTypes == null)
                {
                    _serviceTypes = new string[] { serviceType.ToLower() };
                }
                else
                {
                    if (this.ServiceTypes?.Where(t => t.ToLower() == serviceType.ToLower()).Count() > 0)
                    {
                        return;
                    }

                    Array.Resize(ref _serviceTypes, _serviceTypes.Length + 1);
                    _serviceTypes[_serviceTypes.Length - 1] = serviceType;
                }
            }

            public void RemoveServiceType(string serviceType)
            {
                if (_serviceTypes == null || this.ServiceTypes?.Where(t => t.ToLower() == serviceType.ToLower()).Count() == 0)
                {
                    return;
                }

                _serviceTypes = _serviceTypes.Where(s => s.ToLower() != serviceType.ToLower()).ToArray();
            }

            public bool IsAllowed(string serviceType)
            {
                if (_serviceTypes == null || _serviceTypes.Length == 0)
                {
                    return false;
                }

                serviceType = serviceType.ToLower();

                if (_serviceTypes.Contains(serviceType))
                {
                    return true;
                }

                if (serviceType.StartsWith("_") && _serviceTypes.Contains("_all"))
                {
                    return true;
                }

                return false;
            }
        }

        #endregion
    }
}
