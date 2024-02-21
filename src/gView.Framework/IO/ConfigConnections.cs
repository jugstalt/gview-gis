using gView.Framework.Common;
using gView.Framework.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.IO
{
    public class ConfigConnections
    {
        private readonly IConfigConnectionStorage _storage;
        private readonly string _schema = String.Empty;
        private readonly string _encKey = String.Empty;

        private ConfigConnections(IConfigConnectionStorage storage, string schema, string encKey)
        {
            _storage = storage;
            _schema = schema;
            _encKey = encKey;
        }

        public static ConfigConnections Create(IConfigConnectionStorage storage, string schame, string encKey = "")
            => new(storage, schame, encKey);

        public Dictionary<string, string> Connections
        {
            get
            {
                var connections = _storage.GetAll(_schema);

                if (!String.IsNullOrEmpty(_encKey))
                {
                    foreach (var key in connections.Keys)
                    {
                        string connectionString = connections[key];

                        byte[] bytes = Convert.FromBase64String(connectionString);
                        bytes = Crypto.Decrypt(bytes, _encKey);
                        connections[key] = Encoding.UTF8.GetString(bytes);
                    }
                }

                return connections;
            }
        }

        public bool Add(string name, string connectionstring)
        {
            if (!String.IsNullOrEmpty(_encKey))
            {
                byte[] bytes = Crypto.Encrypt(Encoding.UTF8.GetBytes(connectionstring), _encKey);
                connectionstring = Convert.ToBase64String(bytes);
            }

            return _storage.Store(_schema, name, connectionstring);
        }

        public bool Remove(string name) => _storage.Delete(_schema, name);

        public bool Rename(string oldName, string newName) => _storage.Rename(_schema, oldName, newName);

        public string GetName(string name) => _storage.GetNewName(_schema, name);

        private string ReplaceSlash(string name)
        {
            return name.Replace("/", "&slsh;").Replace("\\", "&bkslsh;").Replace(":", "&colon;");
        }
        private string InvReplaceSlash(string name)
        {
            return name.Replace("&slsh;", "/").Replace("&bkslsh;", "\\").Replace("&colon;", ":");
        }
    }
}
