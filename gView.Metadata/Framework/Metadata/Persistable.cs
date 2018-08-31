using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.Data;

namespace gView.Framework.Metadata
{
    [gView.Framework.system.RegisterPlugIn("5CF9FD50-16B2-4650-A3E3-229E60848CA6")]
    public class PersistableDictionary : IPersistableDictionary
    {
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        #region IPersistableDictionary Member

        public object this[string key]
        {
            get
            {
                object obj;
                if (_dictionary.TryGetValue(key, out obj))
                    return obj;

                return null;
            }
            set
            {
                if (key.Contains(";"))
                    throw new Exception("The charakter ';' is not allowed in this dictionary keys!");
                if(key=="__DictionaryKeys")
                    throw new Exception("The name '__DiretoryKeys' is not allowed in this dictionary keys!");

                object obj;
                if (_dictionary.TryGetValue(key, out obj))
                {
                    throw new Exception("Key already exists!");
                }
                else
                {
                    _dictionary.Add(key, value);
                }
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _dictionary.Clear();

            foreach (string key in stream.Load("__DictionaryKeys", "").ToString().Split(';'))
            {
                if (String.IsNullOrEmpty(key)) continue;

                _dictionary.Add(key, stream.Load(key, null));
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("__DictionaryKeys", GetKeys());

            foreach (string key in _dictionary.Keys)
            {
                if (_dictionary[key] != null) 
                    stream.Save(key, _dictionary[key]);
            }
        }

        #endregion

        #region Helper
        private string GetKeys()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in _dictionary.Keys)
            {
                if (sb.Length > 0) sb.Append(";");
                sb.Append(key);
            }
            return sb.ToString();
        }
        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("24373762-127A-4125-B87F-AC0D091D947E")]
    public class PersistableMetadata : IPlugInParameter, IPersistable
    {
        IMetadata _metadata = null;

        #region IPlugInParameter Member

        public object Parameter
        {
            get
            {
                return _metadata;
            }
            set
            {
                _metadata = value as IMetadata;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_metadata != null)
                _metadata.ReadMetadata(stream);
        }

        public void Save(IPersistStream stream)
        {
            if (_metadata != null)
                _metadata.WriteMetadata(stream);
        }

        #endregion
    }
}
