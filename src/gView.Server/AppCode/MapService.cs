using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.Common;
using gView.Framework.Common;
using gView.Server.AppCode.Extensions;
using gView.Server.Services.MapServer;
using System.Text.Json;
using System;
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
        private MapServiceSettings _folderSettings = null;
        private DateTime? _lastServiceRefresh = null;
        private readonly MapServiceManager _mss;

        public MapService() { }
        public MapService(MapServiceManager mss, string filename, string folder, MapServiceType type)
        {
            _mss = mss;
            _type = type;
            try
            {
                _filename = filename;
                if (type == MapServiceType.Folder)
                {
                    DirectoryInfo di = new DirectoryInfo(_filename);
                    _name = di.Name.ToLower();
                }
                else
                {
                    FileInfo fi = new FileInfo(filename);
                    _name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                }
                _folder = (folder ?? String.Empty).ToLower();
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
                await File.WriteAllTextAsync(fi.FullName, JsonSerializer.Serialize(_settings));
            }
        }

        async public Task<bool> RefreshRequired()
        {
            await ReloadServiceSettings();
            if (_settings.IsRunningOrIdle())
            {
                return _lastServiceRefresh.HasValue == false || _settings.RefreshService > _lastServiceRefresh;
            }

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
                {
                    return String.Empty;
                }

                if (this.Type == MapServiceType.Folder)
                {
                    return _filename + "/_folder.settings";
                }
                else
                {

                    if (_filename.Contains("."))
                    {
                        return _filename.Substring(0, _filename.LastIndexOf(".")) + ".settings";
                    }

                    return _filename + ".settings";
                }
            }
        }

        private DateTime? _settingsLastWriteTime = null, _lastSettingsReload = null;
        async private Task ReloadServiceSettings(bool ifNewer = true)
        {
            try
            {
                // Performance: Do not load a every Request
                if (_lastSettingsReload.HasValue && (DateTime.UtcNow - _lastSettingsReload.Value).TotalSeconds > 10)
                {
                    return;
                }

                FileInfo fi = new FileInfo(this.SettingsFilename);
                if (ifNewer == true)
                {
                    if (_settingsLastWriteTime.HasValue && _settingsLastWriteTime.Value >= fi.LastWriteTimeUtc)
                    {
                        return;
                    }
                }

                if (fi.Exists)
                {
                    _settings = JsonSerializer.Deserialize<MapServiceSettings>(
                        await File.ReadAllTextAsync(fi.FullName));
                    _settingsLastWriteTime = fi.LastWriteTimeUtc;
                }
            }
            finally
            {
                if (_settings == null)
                {
                    _settings = new MapServiceSettings();
                }

                await ReloadParentFolderSettings(ifNewer);
            }

            _lastSettingsReload = DateTime.UtcNow;
        }

        async private Task ReloadParentFolderSettings(bool ifNewer = true)
        {
            try
            {
                if (this.Type == MapServiceType.Folder || String.IsNullOrWhiteSpace(this.Folder))
                {
                    return;
                }

                var folderMapService = _mss.MapServices
                    .Where(s => s.Type == MapServiceType.Folder && this.Folder.Equals(s.Name, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault() as MapService;

                if (folderMapService == null)
                {
                    return;
                }

                await folderMapService.ReloadServiceSettings(ifNewer);
                _folderSettings = folderMapService._settings;
            }
            finally
            {
                if (_folderSettings == null)
                {
                    _folderSettings = new MapServiceSettings();
                }
            }
        }

        async public Task CheckAccess(IServiceRequestContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            await ReloadServiceSettings();

            if (!_settings.IsRunningOrIdle())
            {
                throw new Exception("Service not running: " + this.Fullname);
            }

            if (_folderSettings != null)
            {
                CheckAccess(context, _folderSettings);

                if (context.ServiceRequest != null)
                {
                    if (!String.IsNullOrEmpty(_folderSettings.OnlineResource))
                    {
                        context.ServiceRequest.OnlineResource = _folderSettings.OnlineResource;
                    }

                    if (!String.IsNullOrEmpty(_folderSettings.OutputUrl))
                    {
                        context.ServiceRequest.OutputUrl = _folderSettings.OutputUrl;
                    }
                }
            }

            CheckAccess(context, _settings);
        }

        private void CheckAccess(IServiceRequestContext context, IMapServiceSettings settings)
        {
            if (settings?.AccessRules == null || settings.AccessRules.Length == 0)  // No Settings -> free service
            {
                return;
            }

            string userName = context.ServiceRequest?.Identity?.UserName;

            var accessRule = settings
                .AccessRules
                .Where(r => r.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            // if user not found, use rules for anonymous
            if (accessRule == null)
            {
                accessRule = settings
                    .AccessRules
                    .Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
            }

            if (accessRule == null || accessRule.ServiceTypes == null)
            {
                throw new TokenRequiredException("forbidden (user:" + userName + ")");
            }

            if (!accessRule.ServiceTypes.Contains("_all") && !accessRule.ServiceTypes.Contains("_" + context.ServiceRequestInterpreter.IdentityName.ToLower()))
            {
                throw new NotAuthorizedException(context.ServiceRequestInterpreter.IdentityName + " interface forbidden (user: " + userName + ")");
            }

            var accessTypes = context.ServiceRequestInterpreter.RequiredAccessTypes(context);
            foreach (AccessTypes accessType in Enum.GetValues(typeof(AccessTypes)))
            {
                if (accessType != AccessTypes.None && accessTypes.HasFlag(accessType))
                {
                    if (!accessRule.ServiceTypes.Contains(accessType.ToString().ToLower()))
                    {
                        throw new NotAuthorizedException("Forbidden: " + accessType.ToString() + " access required (user: " + userName + ")");
                    }
                }
            }
        }

        async public Task CheckAccess(IIdentity identity, IServiceRequestInterpreter interpreter)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (interpreter == null)
            {
                throw new ArgumentNullException("interpreter");
            }

            await ReloadServiceSettings();

            if (!_settings.IsRunningOrIdle())
            {
                throw new Exception("Service not running: " + this.Fullname);
            }

            if (_folderSettings != null)
            {
                CheckAccess(identity, interpreter, _folderSettings);
            }

            CheckAccess(identity, interpreter, _settings);
        }

        private void CheckAccess(IIdentity identity, IServiceRequestInterpreter interpreter, IMapServiceSettings settings)
        {
            if (settings?.AccessRules == null || settings.AccessRules.Length == 0)  // No Settings -> free service
            {
                return;
            }

            string userName = identity.UserName;

            var accessRule = settings
                .AccessRules
                .Where(r => r.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            // if user not found, use rules for anonymous
            if (accessRule == null)
            {
                accessRule = settings
                    .AccessRules
                    .Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();
            }

            if (accessRule == null || accessRule.ServiceTypes == null)
            {
                throw new TokenRequiredException("forbidden (user:" + userName + ")");
            }

            if (!accessRule.ServiceTypes.Contains("_all") && !accessRule.ServiceTypes.Contains("_" + interpreter.IdentityName.ToLower()))
            {
                throw new NotAuthorizedException(interpreter.IdentityName + " interface forbidden (user: " + userName + ")");
            }
        }

        async public Task<bool> HasAnyAccess(IIdentity identity)
        {
            await ReloadServiceSettings();

            if ((_folderSettings.AccessRules == null || _folderSettings.AccessRules.Length == 0) &&
                (_settings.AccessRules == null || _settings.AccessRules.Length == 0))
            {
                return true;
            }

            var accessRule =
                _folderSettings?.AccessRules?.Where(r => r.Username.Equals(identity.UserName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() ??
                _settings?.AccessRules?.Where(r => r.Username.Equals(identity.UserName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (accessRule == null)
            {
                accessRule =
                    _folderSettings?.AccessRules?.Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() ??
                    _settings?.AccessRules?.Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            if (accessRule != null && accessRule.ServiceTypes != null && accessRule.ServiceTypes.Length > 0)
            {
                return true;
            }

            return false;
        }

        async public Task<AccessTypes> GetAccessTypes(IIdentity identity)
        {
            await ReloadServiceSettings();

            var accessTypes = AccessTypes.None;

            if ((_folderSettings.AccessRules == null || _folderSettings.AccessRules.Length == 0) &&
                (_settings.AccessRules == null || _settings.AccessRules.Length == 0))  // no rules -> service is open for everone
            {
                foreach (AccessTypes accessType in Enum.GetValues(typeof(AccessTypes)))
                {
                    accessTypes |= accessType;
                }

                return accessTypes;
            }

            IMapServiceAccess folderAcccessRule = null;
            IMapServiceAccess accessRule = null;

            if (_folderSettings?.AccessRules != null && _folderSettings.AccessRules.Length > 0)
            {
                folderAcccessRule =
                    _folderSettings?.AccessRules?.Where(r => r.Username.Equals(identity.UserName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (folderAcccessRule == null)
                {
                    folderAcccessRule =
                        _folderSettings?.AccessRules?.Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                }

                if (folderAcccessRule == null)
                {
                    return accessTypes;
                }
            }

            accessRule =
               _settings?.AccessRules?.Where(r => r.Username.Equals(identity.UserName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (accessRule == null)
            {
                accessRule =
                    _settings?.AccessRules?.Where(r => r.Username.Equals(Identity.AnonyomousUsername, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            if (accessRule == null) // if there only folder settings use them for the service
            {
                accessRule = folderAcccessRule;
            }

            if (accessRule?.ServiceTypes == null || accessRule.ServiceTypes.Length == 0)
            {
                return AccessTypes.None;
            }

            foreach (var serviceType in accessRule.ServiceTypes.Where(s => !String.IsNullOrWhiteSpace(s) && !s.StartsWith("_")))
            {
                if (Enum.TryParse<AccessTypes>(serviceType, true, out AccessTypes accessType))
                {
                    if (folderAcccessRule != null) // if folder settings exists use the service access thats also included in folder settings
                    {
                        if (folderAcccessRule.ServiceTypes.Where(r => serviceType.Equals(r, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null)
                        {
                            accessTypes |= accessType;
                        }
                    }
                    else  // if no folder settings use the service access
                    {
                        accessTypes |= accessType;
                    }
                }
            }

            return accessTypes;
        }

        async public Task<bool> HasPublishAccess(IIdentity identity)
        {
            if (!identity.IsAnonymous) // Never allow anonymous to publish/delete services
            {
                await ReloadServiceSettings();

                if ((_folderSettings.AccessRules == null || _folderSettings.AccessRules.Length == 0) &&
                    (_settings.AccessRules == null || _settings.AccessRules.Length == 0))  // no rules -> service is open for everone
                {
                    return true;
                }

                if (_folderSettings.AccessRules != null && _folderSettings.AccessRules.Length > 0)
                {
                    if (_folderSettings.AccessRules.Where(r => r.Username == identity.UserName && r.ServiceTypes.Contains("publish")).Count() > 0)
                    {
                        return true;
                    }
                }

                if (_type == MapServiceType.Folder && _settings.AccessRules != null && _settings.AccessRules.Length > 0)
                {
                    if (_settings.AccessRules.Where(r => r.Username == identity.UserName && r.ServiceTypes.Contains("publish")).Count() > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
