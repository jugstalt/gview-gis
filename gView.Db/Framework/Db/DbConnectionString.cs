using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.IO;
using System.Xml;
using System.IO;

namespace gView.Framework.Db
{
    public class DbConnectionString : UserData, IPersistable
    {
        private string _providerID = String.Empty;
        private string _schemaName = String.Empty;
        private bool _useProvider = true;

        public string ProviderID
        {
            get { return _providerID; }
            set { _providerID = value; }
        }
        
        public bool UseProviderInConnectionString
        {
            get { return _useProvider; }
            set { _useProvider = value; }
        }

        public string SchemaName
        {
            get { return _schemaName; }
            set { _schemaName = value; }
        }

        public string ConnectionString
        {
            get
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SystemVariables.ApplicationDirectory + @"\gView.DB.UI.xml");

                    XmlNode providerNode = doc.SelectSingleNode("//connectionStrings/provider[@id='" + _providerID + "']");
                    if (providerNode == null) return String.Empty;

                    XmlNode schemaNode = providerNode.SelectSingleNode("schema[@name='" + _schemaName + "']");
                    if (schemaNode == null) return string.Empty;

                    string connectionString = schemaNode.InnerText.Trim();
                    if (providerNode.Attributes["provider"] != null &&
                        !String.IsNullOrEmpty(providerNode.Attributes["provider"].Value))
                    {
                        if (_useProvider)
                            connectionString = providerNode.Attributes["provider"].Value + ":" + connectionString;
                    }

                    foreach (string key in this.UserDataTypes)
                    {
                        string val = (this.GetUserData(key) != null) ? this.GetUserData(key).ToString() : String.Empty;
                        connectionString = connectionString.Replace("[" + key + "]", val);
                    }

                    return connectionString;
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            foreach (String key in this.UserDataTypes)
            {
                this.RemoveUserData(key);
            }

            _providerID = (string)stream.Load("ProviderID", String.Empty);
            _schemaName = (string)stream.Load("SchemaName", String.Empty);
            _useProvider = (bool)stream.Load("UseProvider", true);

            KeyValue kv;
            while ((kv = (KeyValue)stream.Load("KeyValue", null, new KeyValue())) != null)
            {
                this.SetUserData(kv.Key, kv.Value);
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("ProviderID", _providerID);
            stream.Save("SchemaName", _schemaName);
            stream.Save("UseProvider", _useProvider);

            foreach (String key in this.UserDataTypes)
            {
                if (this.GetUserData(key) == null)
                {
                    stream.Save("KeyValue", new KeyValue(key, String.Empty));
                }
                else
                {
                    stream.Save("KeyValue", new KeyValue(key, this.GetUserData(key).ToString()));
                }
            }
        }

        #endregion

        #region HelperClasses
        private class KeyValue : IPersistable
        {
            private string _key = String.Empty, _value = String.Empty;

            public KeyValue() { }
            public KeyValue(string key, string val)
            {
                _key = key;
                _value = val;
            }

            public string Key
            {
                get { return _key; }
                set { _key = value; }
            }

            public string Value
            {
                get { return _value; }
                set { _value = value; }
            }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                _key = (string)stream.Load("Key", String.Empty);
                if (_key.ToLower() == "password")
                {
                    _value = Crypto.Decrypt((string)stream.Load("Val", String.Empty), "gView.Encrypted.Password");
                }
                else
                {
                    _value = (string)stream.Load("Val", String.Empty);
                }
            }

            public void Save(IPersistStream stream)
            {
                stream.Save("Key", _key);
                if (_key.ToLower() == "password")
                {
                    stream.Save("Val", Crypto.Encrypt(_value, "gView.Encrypted.Password"));
                }
                else
                {
                    stream.Save("Val", _value);
                }
            }

            #endregion
        }
        #endregion

        public override string ToString()
        {
            XmlStream stream = new XmlStream("DbConnectionString");
            Save(stream);

            return stream.ToString();
        }
        public void FromString(string connection)
        {
            XmlStream stream = new XmlStream("DbConnectionString");
            StringReader sr=new StringReader(connection);
            stream.ReadStream(sr);

            Load(stream);
        }

        public bool TryFromConnectionString(string providerId, string connectionString)
        {
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                
                XmlDocument doc = new XmlDocument();
                doc.Load(appPath + @"\gView.DB.UI.xml");

                while (connectionString.EndsWith(";"))
                    connectionString = connectionString.Substring(0, connectionString.Length - 1).Trim();

                Dictionary<string, string> connStrDic = ConfigTextStream.Extract(connectionString.Trim());

                XmlNode providerNode = doc.SelectSingleNode("configuration/connectionStrings/provider[@id='" + providerId + "']");
                foreach (XmlNode schemaNode in providerNode.SelectNodes("schema[@name]"))
                {
                    string schema = schemaNode.InnerText.Trim();
                    while (schema.EndsWith(";"))
                        schema = schema.Substring(0, schema.Length - 1).Trim();

                    Dictionary<string, string> schemaDic = ConfigTextStream.Extract(schema);

                    if (CompareConnectionStringDictionary(connStrDic, schemaDic))
                    {
                        SetUserParameters(connStrDic, schemaDic);
                        _providerID = providerId;
                        _schemaName = schemaNode.Attributes["name"].Value;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        #region Helper
        private void SetUserParameters(
            Dictionary<string, string> connStrDic,
            Dictionary<string, string> schemaDic)
        {
            if (connStrDic.Count != schemaDic.Count)
                return;

            for (int i = 0; i < schemaDic.Count; i++)
            {
                string schemaKey = DictionaryKey(schemaDic, i);

                string connStrValue = connStrDic[schemaKey].Trim();
                string schemaValue = schemaDic[schemaKey].Trim();

                if (schemaValue.StartsWith("[") &&
                    schemaValue.EndsWith("]"))
                {
                    this.SetUserData(schemaKey, connStrValue);
                }
            }
        }

        private bool CompareConnectionStringDictionary(
            Dictionary<string, string> connStrDic,
            Dictionary<string, string> schemaDic)
        {
            if (connStrDic.Count != schemaDic.Count)
                return false;

            for (int i = 0; i < schemaDic.Count; i++)
            {
                string connStrKey = DictionaryKey(connStrDic, i);
                string schemaKey = DictionaryKey(schemaDic, i);

                if (connStrKey.ToLower() != schemaKey.ToLower())
                    return false;

                string connStrValue = connStrDic[connStrKey].Trim();
                string schemaValue = schemaDic[schemaKey].Trim();

                if (schemaValue.StartsWith("[") &&
                    schemaValue.EndsWith("]")) continue;

                if (connStrValue.ToLower() != schemaValue.ToLower())
                    return false;
            }
            return true;
        }
        private string DictionaryKey(Dictionary<string, string> dic, int index)
        {
            int i = 0;
            foreach (string key in dic.Keys)
            {
                if (i == index)
                    return key;
                i++;
            }
            return String.Empty;
        }
        #endregion
    }
}
