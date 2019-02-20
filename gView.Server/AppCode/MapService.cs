using gView.Framework.system;
using gView.MapServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    class MapService : IMapService
    {
        private string _filename = String.Empty,
                       _folder = String.Empty,
                       _name = String.Empty;
        private MapServiceType _type = MapServiceType.MXL;
        private MapServiceSettings _settings = null;
        private DateTime? _lastServiceRefresh = null;

        public MapService() { }
        public MapService(string filename, string folder, MapServiceType type)
        {
            _type = type;
            try
            {
                _filename = filename;
                FileInfo fi = new FileInfo(filename);
                _name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                _folder = folder ?? String.Empty;
            }
            catch { }
        }

        #region IMapService Member

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public MapServiceType Type
        {
            get { return _type; }
        }

        public string Folder { get { return _folder; } }

        public string Fullname
        {
            get
            {
                return (String.IsNullOrEmpty(_folder) ? "" : _folder + "/") + _name;
            }
        }

        async public Task<IMapServiceSettings> GetSettingsAsync()
        {
            await ReloadServiceSettings();
            return _settings;
        }

        async public Task SaveSettingsAsync()
        {
            FileInfo fi = new FileInfo(this.SettingsFilename);
            if (_settings == null && fi.Exists)
            {
                File.Delete(fi.FullName);
            }
            else
            {
                await File.WriteAllTextAsync(fi.FullName, JsonConvert.SerializeObject(_settings));
            }
        }

        async public Task<bool> RefreshRequired()
        {
            await ReloadServiceSettings();
            if (_settings.Status == MapServiceStatus.Running)
                return _lastServiceRefresh.HasValue == false || _settings.RefreshService > _lastServiceRefresh;

            return false;
        }
        public void ServiceRefreshed()
        {
            _lastServiceRefresh = DateTime.UtcNow;
        }
        public DateTime? RunningSinceUtc { get { return _lastServiceRefresh; } }

        #endregion

        private string SettingsFilename
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_filename))
                    return String.Empty;
                if (_filename.Contains("."))
                    return _filename.Substring(0, _filename.LastIndexOf(".")) + ".settings";

                return _filename + ".settings";
            }
        }

        private DateTime? _settingsLastWriteTime = null, _lastReload = null;
        async private Task ReloadServiceSettings(bool ifNewer = true)
        {
            try
            {
                // Performance: Do not load a every Request
                if (_lastReload.HasValue && (DateTime.UtcNow - _lastReload.Value).TotalSeconds > 10)
                    return;

                FileInfo fi = new FileInfo(this.SettingsFilename);
                if (ifNewer == true)
                {
                    if (_settingsLastWriteTime.HasValue && _settingsLastWriteTime.Value >= fi.LastWriteTimeUtc)
                        return;
                }

                if (fi.Exists)
                {
                    _settings = JsonConvert.DeserializeObject<MapServiceSettings>(
                        await File.ReadAllTextAsync(fi.FullName));
                    _settingsLastWriteTime = fi.LastWriteTimeUtc;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_settings == null)
                    _settings = new MapServiceSettings();
            }

            _lastReload = DateTime.UtcNow;
        }

        async public Task CheckAccess(IServiceRequestContext context)
        {
            try
            {
                if (context == null)
                    throw new ArgumentNullException("context");

                await ReloadServiceSettings();

                if (_settings.Status != MapServiceStatus.Running)
                    throw new Exception("Service not running: " + this.Fullname);

                if (_settings.AccessRules == null)
                    return;

                var accessRule = _settings.AccessRules.Where(r => r.Username.Equals(context.ServiceRequest?.Identity?.UserName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (accessRule == null || accessRule.ServiceTypes == null)
                    throw new NotAuthorizedException("forbidden");

                if (!accessRule.ServiceTypes.Contains("_all") && !accessRule.ServiceTypes.Contains("_" + context.ServiceRequestInterpreter.IntentityName.ToLower()))
                    throw new NotAuthorizedException(context.ServiceRequestInterpreter.IntentityName + " interface forbidden");

                var accessTypes = context.ServiceRequestInterpreter.RequiredAccessTypes(context);
                foreach (AccessTypes accessType in Enum.GetValues(typeof(AccessTypes)))
                {
                    if (accessType != AccessTypes.None && accessType.HasFlag(accessType))
                    {
                        if (!accessRule.ServiceTypes.Contains(accessType.ToString().ToLower()))
                            throw new NotAuthorizedException("Forbidden: " + accessType.ToString() + " access required");
                    }
                }
            }
            catch (NotAuthorizedException nae)
            {
                if (Identity.AnonyomousUsername.Equals(context?.ServiceRequest?.Identity?.UserName))
                    throw nae;

                throw new TokenRequiredException(nae.Message);
            }
        }

        async public Task<bool> HasAnyAccess(IIdentity identity)
        {
            await ReloadServiceSettings();

            if (_settings.AccessRules == null || _settings.AccessRules.Length == 0)
                return true;

            var accessRule = _settings.AccessRules.Where(r => r.Username.Equals(identity.UserName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (accessRule == null)
                accessRule = _settings.AccessRules.Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (accessRule != null && accessRule.ServiceTypes != null && accessRule.ServiceTypes.Length > 0)
                return true;

            return false;
        }
    }

    public class MapServiceSettings : IMapServiceSettings
    {
        public MapServiceSettings()
        {
            this.Status = MapServiceStatus.Running;
            this.RefreshServiceTicks = 0;
        }

        [JsonProperty("status")]
        public MapServiceStatus Status { get; set; }

        [JsonProperty("accessrules")]
        [JsonConverter(typeof(ConcreteTypeConverter<MapServiceAccess[]>))]
        public IMapServiceAccess[] AccessRules { get; set; }

        [JsonIgnore]
        public DateTime RefreshService { get; set; }

        [JsonProperty("refreshticks")]
        public long RefreshServiceTicks
        {
            get { return RefreshService.Ticks; }
            set { RefreshService = new DateTime(value, DateTimeKind.Utc); }
        }

        #region Classes

        public class MapServiceAccess : IMapServiceAccess
        {
            [JsonProperty("username")]
            public string Username { get; set; }

            private string[] _serviceTypes = null;

            [JsonProperty("servicetypes")]
            public string[] ServiceTypes {
                get { return _serviceTypes;  }
                internal set { _serviceTypes = value; }
            }

            public void AddServiceType(string serviceType)
            {
                if (String.IsNullOrWhiteSpace(serviceType))
                    return;

                serviceType = serviceType.ToLower();

                if (_serviceTypes == null)
                {
                    _serviceTypes = new string[] { serviceType.ToLower() };
                }
                else
                {
                    if (this.ServiceTypes?.Where(t => t.ToLower() == serviceType.ToLower()).Count() > 0)
                        return;

                    Array.Resize(ref _serviceTypes, _serviceTypes.Length + 1);
                    _serviceTypes[_serviceTypes.Length - 1] = serviceType;
                }
            }

            public void RemoveServiceType(string serviceType)
            {
                if (_serviceTypes == null || this.ServiceTypes?.Where(t => t.ToLower() == serviceType.ToLower()).Count() == 0)
                    return;

                _serviceTypes = _serviceTypes.Where(s => s.ToLower() != serviceType.ToLower()).ToArray();
            }

            public bool IsAllowed(string serviceType)
            {
                if (_serviceTypes == null || _serviceTypes.Length==0)
                    return false;

                serviceType = serviceType.ToLower();

                if (_serviceTypes.Contains(serviceType))
                    return true;

                if (serviceType.StartsWith("_") && _serviceTypes.Contains("_all"))
                    return true;

                return false;
            }
        }

        #endregion

        #region Json Converter

        public class ConcreteTypeConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return serializer.Deserialize<T>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

        #endregion
    }
}
