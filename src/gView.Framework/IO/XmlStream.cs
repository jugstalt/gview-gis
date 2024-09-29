using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Framework.IO
{
    /// <summary>
    /// Zusammenfassung für IO.
    /// </summary>
    public class XmlStream : ErrorReport, IPersistStream
    {
        private static Encoding _defaultEncoding = Encoding.Unicode;
        public static Encoding DefaultEncoding { get => _defaultEncoding; }

        private XmlDocument _doc;
        private XmlNodePlus _parent;
        private NumberFormatInfo _nhi = global::System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private bool _performEncryption = true;

        public XmlStream(string rootname)
        {
            _doc = new XmlDocument();
            _nhi = global::System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            if (rootname == "")
            {
                rootname = "root";
            }

            _parent = new XmlNodePlus(this.CreateNode(rootname));
            _parent.Culture = global::System.Globalization.CultureInfo.InvariantCulture;

            _doc.AppendChild(_parent.Node);
        }
        public XmlStream(string rootname, bool performEncryption)
            : this(rootname)
        {
            _performEncryption = performEncryption;
        }

        private XmlNode CreateNode(string name)
        {
            XmlNode node = _doc.CreateNode(XmlNodeType.Element, name, "");
            return node;
        }

        private XmlAttribute CreateAttribute(string name, string val)
        {
            XmlAttribute attr = _doc.CreateAttribute(name);
            attr.Value = val;

            return attr;
        }
        private void CreateXmlStreamObjectAttributes(XmlNode node, XmlStreamObject xso)
        {
            node.Attributes.Append(
                CreateAttribute("_category", xso.Category));
            node.Attributes.Append(
                CreateAttribute("_description", xso.Description));
            node.Attributes.Append(
                CreateAttribute("_displayname", xso.DisplayName));
            node.Attributes.Append(
                CreateAttribute("_isreadonly", xso.IsReadOnly.ToString()));
            node.Attributes.Append(
                CreateAttribute("_visible", xso.Visible.ToString()));
        }
        internal static void ApplyXmlStreamObjectAttributes(XmlNode node, XmlStreamObject xso)
        {
            if (node.Attributes["_category"] != null)
            {
                xso.Category = node.Attributes["_category"].Value;
            }

            if (node.Attributes["_description"] != null)
            {
                xso.Description = node.Attributes["_description"].Value;
            }

            if (node.Attributes["_displayname"] != null)
            {
                xso.DisplayName = node.Attributes["_displayname"].Value;
            }

            if (node.Attributes["_isreadonly"] != null)
            {
                xso.IsReadOnly = Convert.ToBoolean(node.Attributes["_isreadonly"].Value);
            }

            if (node.Attributes["_visible"] != null)
            {
                xso.Visible = Convert.ToBoolean(node.Attributes["_visible"].Value);
            }
        }
        private void RemoveXmlDeclataion()
        {
            if (_doc == null || _doc.ChildNodes == null)
            {
                return;
            }

            for (int i = 0; i < _doc.ChildNodes.Count; i++)
            {
                XmlNode node = _doc.ChildNodes[i];
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    _doc.RemoveChild(node);
                    i--;
                }
            }
        }

        public bool WriteStream(string path)
        {
            return WriteStream(path, Formatting.Indented);
        }
        public bool WriteStream(string path, Formatting formatting)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(path, _defaultEncoding);
            xmlWriter.Formatting = formatting;
            xmlWriter.WriteStartDocument();
            RemoveXmlDeclataion();
            _doc.WriteTo(xmlWriter);
            xmlWriter.Close();

            return true;
        }
        public bool WriteStream(Stream stream)
        {
            return WriteStream(stream, Formatting.Indented);
        }
        public bool WriteStream(Stream stream, Formatting formatting)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(stream, _defaultEncoding);
            xmlWriter.Formatting = formatting;
            xmlWriter.WriteStartDocument();
            RemoveXmlDeclataion();
            _doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            //xmlWriter.Close();  // sonst wird auch der eingehende Stream geschlossen
            return true;
        }
        public bool WriteStream(TextWriter stream)
        {
            return WriteStream(stream, Formatting.Indented);
        }
        public bool WriteStream(TextWriter stream, Formatting formatting)
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(stream);
            xmlWriter.Formatting = formatting;
            xmlWriter.WriteStartDocument();
            RemoveXmlDeclataion();
            _doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            //xmlWriter.Close();  // sonst wird auch der eingehende Stream geschlossen
            return true;
        }
        public bool ReadStream(string path)
        {
            try
            {
                _doc = new XmlDocument();
                _doc.Load(path);

                // find first Element (== Parent)
                foreach (XmlNode node in _doc.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        _parent = new XmlNodePlus(node);
                        return true;
                    }
                }

                return false;
            }
            catch(Exception /*ex*/)
            {
                return false;
            }
        }
        public bool ReadStream(Stream stream)
        {
            try
            {
                _doc = new XmlDocument();
                _doc.Load(stream);

                // find first Element (== Parent)
                foreach (XmlNode node in _doc.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        _parent = new XmlNodePlus(node);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool ReadStream(TextReader stream, string cultureName = null)
        {
            try
            {
                _doc = new XmlDocument();
                _doc.Load(stream);

                // find first Element (== Parent)
                foreach (XmlNode node in _doc.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        _parent = new XmlNodePlus(node, cultureName);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ReduceDocument(string xPath)
        {
            if (_doc == null)
            {
                return false;
            }

            XmlNode node = _doc.SelectSingleNode(xPath);
            if (node == null)
            {
                return false;
            }

            CultureInfo culture = global::System.Globalization.CultureInfo.CurrentCulture;
            if (_parent.Node.Attributes["culture"] != null)
            {
                foreach (CultureInfo cu in global::System.Globalization.CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if (cu.TextInfo.CultureName == _parent.Node.Attributes["culture"].Value)
                    {
                        culture = cu;
                        break;
                    }
                }
            }
            _doc.LoadXml(node.OuterXml);

            _parent = new XmlNodePlus(_doc.ChildNodes[0]);
            _parent.Culture = culture;

            return true;
        }
        internal XmlNodePlus ParentNode
        {
            get { return _parent; }
        }

        #region IPersistStream Member

        public object Load(string key)
        {
            return Load(key, null, null);
        }
        public object Load(string key, object defValue)
        {
            return Load(key, defValue, null);
        }
        public object Load(string key, object defValue, object objectInstance)
        {
            if (_parent == null)
            {
                return null;
            }

            XmlNode xmlnode = _parent.Next(key);
            if (xmlnode == null)
            {
                return defValue;
            }

            if (xmlnode.Attributes["GUID"] != null)
            {
                PlugInManager compManager = new PlugInManager();
                object comp = compManager.CreateInstance(new Guid(xmlnode.Attributes["GUID"].Value));
                if (comp == null)
                {
                    return defValue;
                }

                if (comp is IPersistable)
                {
                    XmlNodePlus parent = _parent;
                    _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);
                    ((IPersistable)comp).Load(this);
                    _parent = parent;
                }
                else if (comp is IPersistableLoadAsync)
                {
                    throw new Exception("Can't laod async in this context (" + comp.GetType().ToString() + ")");
                }

                return comp;
            }
            if (xmlnode.Attributes["type"] == null)
            {
                if (objectInstance is IPersistable)
                {
                    XmlNodePlus parent = _parent;
                    _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);
                    ((IPersistable)objectInstance).Load(this);
                    _parent = parent;
                    return objectInstance;
                }
                else if (objectInstance is IPersistableLoadAsync)
                {
                    throw new Exception("Can't laod async in this context (" + objectInstance.GetType().ToString() + ")");
                }
                else
                {
                    return defValue;
                }
            }
            try
            {
                if (xmlnode.Attributes["type"].Value != "System.String")
                {
                    Type type = global::System.Type.GetType(xmlnode.Attributes["type"].Value, false, true);
                    object obj = Activator.CreateInstance(type);
                    if (obj == null)
                    {
                        return defValue;
                    }

                    if (obj is XmlStreamObject)
                    {
                        ApplyXmlStreamObjectAttributes(xmlnode, (XmlStreamObject)obj);
                        if (obj is XmlStreamOption)
                        {
                            XmlNodePlus parent = _parent;
                            _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);

                            List<object> options = new List<object>();
                            object option;
                            while ((option = Load("Option", null)) != null)
                            {
                                options.Add(option);
                            }
                            ((XmlStreamOption)obj).Options = options.ToArray();
                            ((XmlStreamOption)obj).Value = Load("Value", null);
                            _parent = parent;
                        }
                        else if (obj is XmlStreamStringArray)
                        {
                            XmlNodePlus parent = _parent;
                            _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);

                            List<string> values = new List<string>();
                            object Value;
                            while ((Value = Load("Value", null)) != null)
                            {
                                values.Add(Value.ToString());
                            }
                            ((XmlStreamObject)obj).Value = values.ToArray();
                            _parent = parent;
                        }
                        else
                        {
                            XmlNodePlus parent = _parent;
                            _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);
                            ((XmlStreamObject)obj).Value = Load("Value", null);
                            _parent = parent;
                        }
                        return obj;
                    }

                    obj = Convert.ChangeType(xmlnode.Attributes["value"].Value, type, _parent.NumberFormat);
                    return obj;
                }
                else
                {
                    string v = xmlnode.Attributes["value"].Value;
                    if (xmlnode.Attributes["encryption_type"] != null)
                    {
                        switch (xmlnode.Attributes["encryption_type"].Value)
                        {
                            case "1":
                                v = Crypto.Decrypt(v, "3f9932916f9746e1a3df048cc70dd30a");
                                break;
                        }
                    }
                    return v;
                }
            }
            catch/*(Exception ex)*/
            {
                return defValue;
            }
        }

        async public Task<T> LoadAsync<T>(string key, T objectInstance, T defaultValue = default(T))
            where T : IPersistableLoadAsync
        {
            if (_parent == null)
            {
                return default(T);
            }

            XmlNode xmlnode = _parent.Next(key);
            if (xmlnode == null)
            {
                return defaultValue;
            }

            if (xmlnode.Attributes["type"] == null)
            {
                if (objectInstance != null)
                {
                    XmlNodePlus parent = _parent;
                    _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);
                    await objectInstance.LoadAsync(this);
                    _parent = parent;
                    return objectInstance;
                }
            }

            return defaultValue;
        }

        async public Task<T> LoadPluginAsync<T>(string key, T unknownPlugin = default(T))
            where T : IPersistableLoadAsync
        {
            if (_parent == null)
            {
                return default(T);
            }

            XmlNode xmlnode = _parent.Next(key);
            if (xmlnode == null)
            {
                return default(T);
            }

            if (xmlnode.Attributes["GUID"] != null)
            {
                PlugInManager compManager = new PlugInManager();
                T comp = (T)compManager.CreateInstance(new Guid(xmlnode.Attributes["GUID"].Value));
                if (comp == null)
                {
                    if (unknownPlugin is IErrorMessage)
                    {
                        ((IErrorMessage)unknownPlugin).LastErrorMessage = "Unknown plugin type: " + xmlnode.Attributes["GUID"].Value;
                    }

                    return unknownPlugin;
                }

                //if (comp is IPersistableLoad)
                {
                    XmlNodePlus parent = _parent;
                    _parent = new XmlNodePlus(xmlnode, _parent.NumberFormat);
                    //((IPersistable)comp).Load(this);
                    await comp.LoadAsync(this);
                    _parent = parent;
                }

                return comp;
            }

            return unknownPlugin;
        }

        public void Save(string key, object val)
        {
            Save(key, val, false);
        }
        public void SaveEncrypted(string key, string val)
        {
            Save(key, val, _performEncryption);
        }
        private void Save(string key, object val, bool encrypt)
        {
            XmlNode node = this.CreateNode(key);
            _parent.Node.AppendChild(node);
            if (PlugInManager.IsPlugin(val))
            {
                node.Attributes.Append(
                    this.CreateAttribute("GUID", PlugInManager.PlugInID(val).ToString()));
                node.Attributes.Append(
                    this.CreateAttribute("type", val.ToString()));
            }
            if (val is IPersistable)
            {
                _parent.Node = node;
                ((IPersistable)val).Save(this);
                _parent.Node = node.ParentNode;
                return;
            }
            if (val is IPersistableLoadAsync)
            {
                _parent.Node = node;
                ((IPersistableLoadAsync)val).Save(this);
                _parent.Node = node.ParentNode;
                return;
            }
            if (val is XmlStreamObject)
            {
                if (((XmlStreamObject)val).Value == null)
                {
                    return;
                }

                CreateXmlStreamObjectAttributes(node, (XmlStreamObject)val);

                if (val is XmlStreamOption &&
                    ((XmlStreamOption)val).Options != null)
                {
                    XmlStreamOption streamObject = (XmlStreamOption)val;

                    node.Attributes.Append(
                        this.CreateAttribute("type", typeof(XmlStreamOption).ToString()));

                    _parent.Node = node;
                    Save("Value", streamObject.Value);
                    foreach (object option in streamObject.Options)
                    {
                        Save("Option", option);
                    }
                    _parent.Node = node.ParentNode;
                }
                else if (val is XmlStreamStringArray &&
                    ((XmlStreamStringArray)val).Value is Array)
                {
                    XmlStreamStringArray streamObject = (XmlStreamStringArray)val;

                    node.Attributes.Append(
                        this.CreateAttribute("type", typeof(XmlStreamStringArray).ToString()));
                    _parent.Node = node;
                    foreach (object Value in (Array)streamObject.Value)
                    {
                        Save("Value", Value.ToString());
                    }
                    _parent.Node = node.ParentNode;
                }
                else
                {
                    XmlStreamObject streamObject = (XmlStreamObject)val;

                    node.Attributes.Append(
                        this.CreateAttribute("type", typeof(XmlStreamObject).ToString()));

                    _parent.Node = node;
                    Save("Value", streamObject.Value);
                    _parent.Node = node.ParentNode;
                }
                return;
            }

            if (val != null)
            {
                if (node.Attributes["type"] == null)
                {
                    node.Attributes.Append(
                        this.CreateAttribute("type", val.GetType().ToString()));
                }
                if (val.GetType() == typeof(double))
                {
                    node.Attributes.Append(
                        this.CreateAttribute("value", ((double)val).ToString(_parent.NumberFormat)));
                }
                else if (val.GetType() == typeof(float))
                {
                    node.Attributes.Append(
                        this.CreateAttribute("value", ((float)val).ToString(_parent.NumberFormat)));
                }
                else if (val.GetType() == typeof(decimal))
                {
                    node.Attributes.Append(
                        this.CreateAttribute("value", ((decimal)val).ToString(_parent.NumberFormat)));
                }
                else
                {
                    string v = val.ToString();
                    if (encrypt == true)
                    {
                        v = Crypto.Encrypt(v, "3f9932916f9746e1a3df048cc70dd30a");
                        node.Attributes.Append(this.CreateAttribute("encryption_type", "1"));
                    }
                    node.Attributes.Append(
                        this.CreateAttribute("value", v));
                }
            }
            //object f=Activator.CreateInstance(System.Type.GetType("System.String",false,true));
            //f=null;
        }

        #endregion

        public string ToXmlString() 
            => _doc?.OuterXml ?? string.Empty;

        public override string ToString()
        {
            if (_doc == null)
            {
                return base.ToString();
            }
            else
            {
                return _doc.OuterXml;
            }
        }
    }
}
