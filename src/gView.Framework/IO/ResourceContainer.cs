using gView.Framework.Core.IO;
using System.Text.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace gView.Framework.IO
{
    public class ResourceContainer : IResourceContainer
    {
        private ConcurrentDictionary<string, Resource> _resources = new ConcurrentDictionary<string, Resource>();

        #region IResourceContainer

        public byte[] this[string name]
        {
            get
            {
                if (_resources.ContainsKey(name))
                {
                    return _resources[name]?.Data;
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    if (_resources.ContainsKey(name))
                    {
                        _resources.TryRemove(name, out Resource resource);
                    }
                }
                else
                {
                    _resources[name] = new Resource() { Name = name, Data = value };
                }
            }
        }

        public IEnumerable<string> Names => _resources.Keys.OrderBy(n => n?.ToLower());

        public bool HasResources => _resources.Count > 0;

        #endregion

        #region IPersistable

        public void Load(IPersistStream stream)
        {
            try
            {
                string json = (string)stream.Load("data", String.Empty);

                if (!String.IsNullOrEmpty(json))
                {
                    Dictionary<string, Resource> dict = JsonSerializer.Deserialize<Dictionary<string, Resource>>(json);

                    foreach (string key in dict.Keys)
                    {
                        _resources[key] = dict[key];
                    }
                }
            }
            catch { }
        }

        public void Save(IPersistStream stream)
        {
            if (_resources.Count > 0)
            {
                string json = JsonSerializer.Serialize(_resources);

                stream.SaveEncrypted("data", json);
            }
        }

        #endregion
    }

    public class Resource : IResource
    {
        public Resource()
        {

        }

        public Resource(IResource resource)
        {
            this.Name = resource.Name;
            this.Data = resource.Data;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("data")]
        public byte[] Data { get; set; }
    }
}
