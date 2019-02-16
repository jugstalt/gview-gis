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

        public MapService() { }
        public MapService(string filename, string folder, MapServiceType type)
        {
            _type = type;
            try
            {
                _filename = filename;
                FileInfo fi = new FileInfo(filename);
                _name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                _folder = folder;
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
            catch
            {
            }
            finally
            {
                if (_settings == null)
                    _settings = new MapServiceSettings();
            }

            _lastReload = DateTime.UtcNow;
        }
    }

    class MapServiceSettings : IMapServiceSettings
    {
        public MapServiceSettings()
        {
            this.Status = MapServiceStatus.Running;
        }

        [JsonProperty("status")]
        public MapServiceStatus Status { get; set; }

        [JsonProperty("accessrules")]
        public IMapServiceAccess[] AccessRules { get; set; }

        #region Classes

        public class MapServiceAccess : IMapServiceAccess
        {
            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("servicetypes")]
            public string[] ServiceTypes { get; set; }
        }

        #endregion
    }
}
