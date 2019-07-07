using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.IO;
using gView.Framework.system;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.IO
{
	/// <summary>
	/// Zusammenfassung für IO.
	/// </summary>
	public class XmlStream : IPersistStream
	{
		private XmlDocument _doc;
		private XmlNodePlus _parent;
        private NumberFormatInfo _nhi = global::System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private bool _performEncryption = true;

		public XmlStream(string rootname) 
		{
			_doc=new XmlDocument();
            _nhi = global::System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            
			if(rootname=="") rootname="root";
			
			_parent=new XmlNodePlus(this.CreateNode(rootname));
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
			XmlNode node=_doc.CreateNode(XmlNodeType.Element,name,"");
			return node;
		}

		private XmlAttribute CreateAttribute(string name,string val) 
		{
			XmlAttribute attr=_doc.CreateAttribute(name);
			attr.Value=val;

			return attr;
		}
        private void CreateXmlStreamObjectAttributes(XmlNode node, XmlStreamObject xso)
        {
            node.Attributes.Append(
                CreateAttribute("_category", xso.Category));
            node.Attributes.Append(
                CreateAttribute("_description", xso.Description));
            node.Attributes.Append(
                CreateAttribute("_displayname",xso.DisplayName));
            node.Attributes.Append(
                CreateAttribute("_isreadonly", xso.IsReadOnly.ToString()));
            node.Attributes.Append(
                CreateAttribute("_visible", xso.Visible.ToString()));
        }
        internal static void ApplyXmlStreamObjectAttributes(XmlNode node, XmlStreamObject xso)
        {
            if (node.Attributes["_category"] != null)
                xso.Category = node.Attributes["_category"].Value;
            if (node.Attributes["_description"] != null)
                xso.Description = node.Attributes["_description"].Value;
            if (node.Attributes["_displayname"] != null)
                xso.DisplayName = node.Attributes["_displayname"].Value;
            if (node.Attributes["_isreadonly"] != null)
                xso.IsReadOnly = Convert.ToBoolean(node.Attributes["_isreadonly"].Value);
            if (node.Attributes["_visible"] != null)
                xso.Visible = Convert.ToBoolean(node.Attributes["_visible"].Value);
        }
        private void RemoveXmlDeclataion()
        {
            if (_doc == null || _doc.ChildNodes == null) return;
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
            XmlTextWriter xmlWriter = new XmlTextWriter(path, Encoding.Unicode);
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
            XmlTextWriter xmlWriter = new XmlTextWriter(stream, Encoding.Unicode);
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
				_doc=new XmlDocument();
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
			catch 
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
        public bool ReadStream(TextReader stream)
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

        public bool ReduceDocument(string xPath)
        {
            if (_doc == null) return false;
            XmlNode node = _doc.SelectSingleNode(xPath);
            if (node == null) return false;

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
			return Load(key,null,null);
		}
		public object Load(string key,object defValue) 
		{
			return Load(key,defValue,null);
		}
		public object Load(string key,object defValue,object objectInstance)
		{
            if (_parent == null) return null;
			XmlNode xmlnode=_parent.Next(key);
			if(xmlnode==null) return defValue;
		
			if(xmlnode.Attributes["GUID"]!=null) 
			{
				PlugInManager compManager=new PlugInManager();
				object comp=compManager.CreateInstance(new Guid(xmlnode.Attributes["GUID"].Value));
				if(comp==null) return defValue;

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
			if(xmlnode.Attributes["type"]==null) 
			{
				if(objectInstance is IPersistable) 
				{
					XmlNodePlus parent=_parent;
					_parent=new XmlNodePlus(xmlnode,_parent.NumberFormat);
					((IPersistable)objectInstance).Load(this);
					_parent=parent;
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
                    if (obj == null) return defValue;

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
                return default(T);

            XmlNode xmlnode = _parent.Next(key);
            if (xmlnode == null) return defaultValue;

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
            if (_parent == null) return default(T);
            XmlNode xmlnode = _parent.Next(key);
            if (xmlnode == null) return default(T);

            if (xmlnode.Attributes["GUID"] != null)
            {
                PlugInManager compManager = new PlugInManager();
                T comp = (T)compManager.CreateInstance(new Guid(xmlnode.Attributes["GUID"].Value));
                if (comp == null)
                {
                    if (unknownPlugin is IErrorMessage)
                        ((IErrorMessage)unknownPlugin).LastErrorMessage = "Unknown plugin type: "+ xmlnode.Attributes["GUID"].Value;
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
            XmlNode node=this.CreateNode(key);
			_parent.Node.AppendChild(node);
            if (PlugInManager.IsPlugin(val))
            {
                node.Attributes.Append(
                    this.CreateAttribute("GUID", PlugInManager.PlugInID(val).ToString()));
                node.Attributes.Append(
                    this.CreateAttribute("type", val.ToString()));
            }
			if(val is IPersistable) 
			{
				_parent.Node=node;
				((IPersistable)val).Save(this);
				_parent.Node=node.ParentNode;
				return;
			}
            if(val is IPersistableLoadAsync)
            {
                _parent.Node = node;
                ((IPersistableLoadAsync)val).Save(this);
                _parent.Node = node.ParentNode;
                return;
            }
            if (val is XmlStreamObject )
            {
                if (((XmlStreamObject)val).Value == null) return;
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
                        this.CreateAttribute("value",((double)val).ToString(_parent.NumberFormat)));
                }
                else if (val.GetType() == typeof(float))
                {
                    node.Attributes.Append(
                        this.CreateAttribute("value",((float)val).ToString(_parent.NumberFormat)));
                }
                else if (val.GetType() == typeof(decimal))
                {
                    node.Attributes.Append(
                        this.CreateAttribute("value",((decimal)val).ToString(_parent.NumberFormat)));
                }
                else
                {
                    string v = val.ToString();
                    if(encrypt==true) {
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

        public override string ToString()
        {
            if (_doc == null)
                return base.ToString();
            else
                return _doc.OuterXml;
        }
	}

    [TypeConverter("CustomProperty")]
    public class PersistableClass : Component, ICustomTypeDescriptor,IPersistable
    {
        private PropertyDescriptorCollection _propertyCollection;
        private List<PersistableClass> _childClasses;
        private int _maxLength;
        private string _name=String.Empty;
        private NumberFormatInfo _nhi;
       
        private PersistableClass()
        {
            _propertyCollection = new PropertyDescriptorCollection(new PropertyDescriptor[] { });
            _childClasses = new List<PersistableClass>();
        }

        public PersistableClass(XmlStream stream) : this()
        {
            if (stream == null || stream.ParentNode == null ||
                stream.ParentNode.Node == null) return;

            _nhi = stream.ParentNode.NumberFormat;

            FromNode(stream.ParentNode.Node);
        }
        private PersistableClass(XmlNode parent, NumberFormatInfo nhi) : this()
        {
            _nhi = nhi;
            if (parent == null) return;

            FromNode(parent);
        }

        private void FromNode(XmlNode parent) 
        {
            if (parent == null) return;
            _name = parent.Name;

            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Attributes["type"] != null)
                {
                    if (node.Attributes["type"].Value != "System.String")
                    {
                        Type type = global::System.Type.GetType(node.Attributes["type"].Value, false, true);
                        object obj = Activator.CreateInstance(type);

                        if (obj is XmlStreamObject)
                        {
                            XmlStream.ApplyXmlStreamObjectAttributes(node, (XmlStreamObject)obj);
                            if (obj is XmlStreamOption)
                            {
                                List<object> options = new List<object>();
                                foreach (XmlNode optionNode in node.SelectNodes("Option[@type]"))
                                {
                                    if (optionNode.Attributes["value"] == null) continue;

                                    Type optionType = global::System.Type.GetType(optionNode.Attributes["type"].Value, false, true);
                                    object optionObj = Convert.ChangeType(optionNode.Attributes["value"].Value, optionType, _nhi);
                                    options.Add(optionObj);
                                }
                                ((XmlStreamOption)obj).Options = options.ToArray();

                                XmlNode valueNode = node.SelectSingleNode("Value[@type]");
                                if (valueNode != null && valueNode.Attributes["value"] != null)
                                {
                                    Type valueType = global::System.Type.GetType(valueNode.Attributes["type"].Value, false, true);
                                    object valueObj = Convert.ChangeType(valueNode.Attributes["value"].Value, valueType, _nhi);
                                    ((XmlStreamOption)obj).Value = valueObj;
                                }
                                else if (options.Count > 0)
                                {
                                    ((XmlStreamOption)obj).Value = options[0];
                                }

                                this.Add(new CustomProperty(
                                        node.Name, obj,
                                        ((XmlStreamObject)obj).Description,
                                        ((XmlStreamObject)obj).Category, 
                                        ((XmlStreamOption)obj).Value.GetType(),
                                        ((XmlStreamObject)obj).IsReadOnly, 
                                        true));
                                
                            }
                            else if (obj is XmlStreamStringArray)
                            {
                                List<object> values = new List<object>();
                                foreach (XmlNode valueNode in node.SelectNodes("Value[@type]"))
                                {
                                    if (valueNode.Attributes["value"] == null) continue;

                                    Type valueType = global::System.Type.GetType(valueNode.Attributes["type"].Value, false, true);
                                    object valueObj = Convert.ChangeType(valueNode.Attributes["value"].Value, valueType, _nhi);
                                    values.Add(valueObj);
                                }
                                ((XmlStreamStringArray)obj).Value = values.ToArray();

                                this.Add(new CustomProperty(
                                    node.Name, obj,
                                    ((XmlStreamObject)obj).Description,
                                    ((XmlStreamObject)obj).Category,
                                    typeof(string[]),
                                    ((XmlStreamObject)obj).IsReadOnly,
                                    true));
                            }
                            else
                            {
                                XmlNode valueNode = node.SelectSingleNode("Value[@type]");
                                if (valueNode != null && valueNode.Attributes["value"] != null)
                                {
                                    Type valueType = global::System.Type.GetType(valueNode.Attributes["type"].Value, false, true);
                                    object valueObj = Convert.ChangeType(valueNode.Attributes["value"].Value, valueType, _nhi);
                                    ((XmlStreamObject)obj).Value = valueObj;
                                }

                                if (((XmlStreamObject)obj).Value != null)
                                {
                                    this.Add(new CustomProperty(
                                        node.Name, obj,
                                        ((XmlStreamObject)obj).Description,
                                        ((XmlStreamObject)obj).Category,
                                        ((XmlStreamObject)obj).Value.GetType(),
                                        ((XmlStreamObject)obj).IsReadOnly,
                                        true));
                                }
                            }
                            continue;
                        }

                        if (node.Attributes["value"] != null)
                        {
                            obj = Convert.ChangeType(node.Attributes["value"].Value, type, _nhi);
                            this.Add(new CustomProperty(
                                node.Name, obj, "", "", type, false, false));
                            continue;
                        }
                    }
                    else
                    {
                        if (node.Attributes["value"] != null)
                        {
                            Type type = typeof(global::System.String);
                            string obj = String.Empty;
                            obj = node.Attributes["value"].Value;

                            this.Add(new CustomProperty(
                                node.Name, obj, "", "", type, false, false));
                            continue;
                        }
                    }
                }
                
                _childClasses.Add(new PersistableClass(node, _nhi));
            }
        }

        [Browsable(false)]
        public int MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                if (value > _maxLength)
                    _maxLength = value;
            }
        }		

        private void Add(CustomProperty Value)
        {
            _propertyCollection.Add(Value);
        }

        private void Remove(string Name)
        {
            foreach (CustomProperty prop in _propertyCollection)
            {
                if (prop.Name == Name)
                {
                    _propertyCollection.Remove(prop);
                    return;
                }
            }
        }

        private CustomProperty this[int index]
        {
            get
            {
                return (CustomProperty)_propertyCollection[index];
            }
        }

        private PropertyDescriptorCollection GetAllProperties()
        {
            return _propertyCollection;
        }

        [Browsable(false)]
        public string Name
        {
            get { return _name; }
        }

        [Browsable(false)]
        public List<PersistableClass> ChildClasses
        {
            get { return _childClasses; }
        }

        public XmlStream ToXmlStream()
        {
            XmlStream stream = new XmlStream(this.Name);

            Save(stream);
            return stream;
        }

        #region ICustomTypeDescriptor Member

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            PropertyDescriptorCollection props = GetAllProperties();

            if (props.Count > 0)
                return (props[0]);
            else
                return (null);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] newProps = new PropertyDescriptor[_propertyCollection.Count];
            for (int i = 0; i < _propertyCollection.Count; i++)
            {
                CustomProperty prop = (CustomProperty)_propertyCollection[i];

                List<Attribute> propAttributes = new List<Attribute>();
                if (prop.propValueObject is XmlStreamObject)
                {
                    XmlStreamObject xso=(XmlStreamObject)prop.propValueObject;
                    if (!String.IsNullOrEmpty(xso.DisplayName))
                        propAttributes.Add(new DisplayNameAttribute(xso.DisplayName));

                    if (!xso.Visible)
                        propAttributes.Add(new BrowsableAttribute(false));
                }
                if (prop.propValueObject is XmlStreamOption)
                {
                    propAttributes.Add(new TypeConverterAttribute(typeof(StringListConverter)));
                }

                newProps[i] = new CustomPropertyDescriptor(ref prop, propAttributes.ToArray());
            }

            return new PropertyDescriptorCollection(newProps);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return (GetAllProperties());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion

        #region HelperClasses
        public class CustomProperty : PropertyDescriptor
        {
            private string propName;
            private object propValue;
            private string propDescription;
            private string propCategory;
            private Type propType;
            private bool isReadOnly;
            private bool isExpandable;

            public CustomProperty(string pName, object pValue, string pDesc, string pCat, Type pType, bool readOnly, bool expandable)
                : base(pName, new Attribute[] { })
            {
                propName = pName;
                propValue = pValue;
                propDescription = pDesc;
                propCategory = pCat;
                propType = pType;
                isReadOnly = readOnly;
                isExpandable = expandable;
            }

            public override global::System.Type ComponentType
            {
                get
                {
                    return null;
                }
            }

            public override string Category
            {
                get
                {
                    return propCategory;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return isReadOnly;
                }
            }

            public override global::System.Type PropertyType
            {
                get
                {
                    return propType;
                }
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override object GetValue(object component)
            {
                if (propValue is XmlStreamObject)
                {
                    return ((XmlStreamObject)propValue).Value;
                }
                else
                {
                    return propValue;
                }
            }

            public override void SetValue(object component, object value)
            {
                if (propValue is XmlStreamObject)
                {
                    ((XmlStreamObject)propValue).Value = value;
                }
                else
                {
                    propValue = value;
                }
            }

            public override void ResetValue(object component)
            {
                propValue = null;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override string Description
            {
                get
                {
                    return propDescription;
                }
            }

            internal object propValueObject
            {
                get { return propValue; }
            }
        }
        public class CustomPropertyDescriptor : PropertyDescriptor
        {
            CustomProperty m_Property;
            public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs)
                : base(myProperty.Name, attrs)
            {
                m_Property = myProperty;
            }

            #region PropertyDescriptor specific

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get
                {
                    return null;
                }
            }

            public override object GetValue(object component)
            {
                return m_Property.GetValue(component);
            }

            public override string Description
            {
                get
                {
                    return m_Property.Name;
                }
            }

            public override string Category
            {
                get
                {
                    return m_Property.Category;
                }
            }

            public override string DisplayName
            {
                get
                {
                    return m_Property.Name;
                }

            }

            public override bool IsReadOnly
            {
                get
                {
                    return m_Property.IsReadOnly;
                }
            }

            public override void ResetValue(object component)
            {
                //Have to implement
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override void SetValue(object component, object value)
            {
                m_Property.SetValue(component, value);
            }

            public override Type PropertyType
            {
                get
                {
                    return m_Property.PropertyType;
                }
            }

            #endregion

            public CustomProperty CustomProperty
            {
                get { return m_Property; }
            }
        }

        private class StringListConverter : TypeConverter
        {
            public StringListConverter()
            {
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true; // display drop
            }
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true; // drop-down vs combo
            }
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                CustomPropertyDescriptor propDecriptor = context.PropertyDescriptor as CustomPropertyDescriptor;
                if (propDecriptor == null) return null;

                object ValueObject = propDecriptor.CustomProperty.propValueObject; // context.PropertyDescriptor.GetValue(context.Instance);
                if (ValueObject is XmlStreamOption)
                {
                    //List<string> strArray = new List<string>();
                    //foreach (object obj in ((XmlStreamOption)ValueObject).Options)
                    //{
                    //    if (obj == null) continue;
                    //    strArray.Add(obj.ToString());
                    //}

                    //return new StandardValuesCollection(strArray.ToArray());
                    return new StandardValuesCollection(((XmlStreamOption)ValueObject).Options);
                }

                return null;
            }
        }
        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Save(IPersistStream stream)
        {
            PropertyInfo[] props_of_class = this.GetType().GetProperties();

            foreach (CustomProperty property in _propertyCollection)
            {
                if (property.propValueObject is XmlStreamObject)
                {
                    stream.Save(property.Name, property.propValueObject);
                }
                else
                {
                    stream.Save(property.Name, property.GetValue(this));
                }
            }

            foreach (PersistableClass child in _childClasses)
            {
                stream.Save(child.Name, child);
            }
        }

        #endregion
    }

	public class ConfigTextStream 
	{
		//private static string _configPath=System.Environment.CurrentDirectory+@"/config";
		private string _path;
		private global::System.IO.StreamReader tr=null;
		private global::System.IO.StreamWriter tw=null;
		private ArrayList _ids=new ArrayList();
        private string _dp = "&chr(" + ((byte)':').ToString() + ");";

		public ConfigTextStream(string name) 
		{
            if (name == "") return;

			_path=SystemVariables.MyApplicationConfigPath+@"/"+name+".config";
			getIDs();
			try 
			{
				FileInfo fi=new FileInfo(_path);
				if(fi.Exists) tr = new global::System.IO.StreamReader(_path);
			} 
			catch 
			{
				Close();
			}
		}
		public ConfigTextStream(string name,bool write,bool append) 
		{
			try 
			{
				_path=SystemVariables.MyApplicationConfigPath+@"/"+name+".config";
				getIDs();
				if(!write) 
				{
					FileInfo fi=new FileInfo(_path);
					if(fi.Exists) tr = new global::System.IO.StreamReader(_path);
				}
				else 
				{
                    tw = new global::System.IO.StreamWriter(_path, append); 
				}
			}
            catch/*(Exception ex)*/ 
			{
				Close();
			}
		}
		private void getIDs()  
		{
			FileInfo fi=new FileInfo(_path);
			if(!fi.Exists) return;
			StreamReader sr=new StreamReader(_path);
			string line;
			while((line=sr.ReadLine())!=null) 
			{
				string [] id_=line.Split(':');
				_ids.Add(id_[0]);
			}
			sr.Close();
		}
		public void Close() 
		{
			if(tr!=null) tr.Close();
			if(tw!=null) tw.Close();
			tr=null;
			tw=null;
		}

		private string modifyID(string id) 
		{
			int i=1;
			string id_=id;
			while(_ids.Contains(id)) 
			{
				id=id_+"("+i.ToString()+")";
				i++;
			}
            return id;
		}

		public bool Write(string line,ref string id) 
		{
            id = id.Replace(":", _dp);

			if(tw==null) return false;
			id=modifyID(id);
			tw.WriteLine(id+":"+line);
			return true;
		}

		public bool Remove(string id,string line) 
		{
            id = id.Replace(":", _dp);

			if(tw==null) return false;
			try 
			{
				FileInfo fi=new FileInfo(_path);
				fi.CopyTo(_path+".tmp");
				
				tw.Close();
				tw=new StreamWriter(_path,false);

				StreamReader sr=new StreamReader(_path+".tmp");
				string l,i="";
				while((l=sr.ReadLine())!=null) 
				{
					int pos=l.IndexOf(":");
					if(pos!=-1) 
					{
						i =l.Substring(0,pos);
						l =l.Substring(pos+1,l.Length-pos-1);
					}
					if(l==line && i==id) continue;
					tw.WriteLine(i+":"+l);
				}
				sr.Close();
				tw.Close();
				fi=new FileInfo(_path+".tmp");
				fi.Delete();

				tw=new StreamWriter(_path,true);
				return true;
			} 
			catch 
			{
				return false;
			}
		}

		public bool Replace(string line,string to) 
		{
			if(tw==null) return false;
			try 
			{
				FileInfo fi=new FileInfo(_path);
				fi.CopyTo(_path+".tmp");
				
				tw.Close();
				tw=new StreamWriter(_path, false);

				StreamReader sr=new StreamReader(_path+".tmp");
				string l,id="";
				while((l=sr.ReadLine())!=null) 
				{
					int pos=l.IndexOf(":");
					if(pos!=-1) 
					{
						id=l.Substring(0,pos);
						l =l.Substring(pos+1,l.Length-pos-1);
					}
					if(l==line)
						tw.WriteLine(id+":"+to);
					else	
						tw.WriteLine(id+":"+l);
				}
				sr.Close();
				tw.Close();
				fi=new FileInfo(_path+".tmp");
				fi.Delete();

				tw=new StreamWriter(_path,true);
				return true;
			} 
			catch 
			{
				return false;
			}
		}
        public bool ReplaceHoleLine(string line, string to)
        {
            if (tw == null) return false;
            try
            {
                bool ret = false;
                FileInfo fi = new FileInfo(_path);
                fi.CopyTo(_path + ".tmp");

                tw.Close();
                tw = new StreamWriter(_path, false);

                StreamReader sr = new StreamReader(_path + ".tmp");
                string l;
                while ((l = sr.ReadLine()) != null)
                {
                    if (l == line)
                    {
                        tw.WriteLine(to);
                        ret = true;
                    }
                    else
                    {
                        tw.WriteLine(l);
                    }
                }
                sr.Close();
                tw.Close();
                fi = new FileInfo(_path + ".tmp");
                fi.Delete();

                tw = new StreamWriter(_path, true);
                return ret;
            }
            catch
            {
                return false;
            }
        }

		public string Read(out string id) 
		{
			id="";
			if(tr==null) return null;
			string l=tr.ReadLine();
			if(l==null) return null;

			int pos=l.IndexOf(":");
			if(pos!=-1) 
			{
                id = l.Substring(0, pos).Replace(_dp, ":");
				l =l.Substring(pos+1,l.Length-pos-1);
			} 
			return l;
		}

        public static string ExtractValue(string Params, string Param)
        {
            Param = Param.Trim();

            foreach (string a in Params.Split(';'))
            {
                string aa = a.Trim();
                if (aa.ToLower().IndexOf(Param.ToLower() + "=") == 0)
                {
                    if (aa.Length == Param.Length + 1) return "";
                    return aa.Substring(Param.Length + 1, aa.Length - Param.Length - 1);
                }
            }
            return String.Empty;
        }

        public static string ExtractOracleValue(string @params, string param)
        {
            @params = @params.Trim();
            int pos = @params.ToLower().IndexOf("(" + param.ToLower() + "=");
            if(pos>=0)
            {
                int pos2 = @params.IndexOf(")", pos + param.Length + 1);
                if (pos2 > pos)
                    return @params.Substring(pos + param.Length + 2, pos2 - pos - param.Length - 2);
            }
            return String.Empty;
        }

        public static string RemoveValue(string Params, string Param)
        {
            Param = Param.ToLower().Trim();

            StringBuilder sb = new StringBuilder();
            foreach (string a in Params.Split(';'))
            {
                if (String.IsNullOrEmpty(a.Trim()) ||
                    a.ToLower().StartsWith(Param + "="))
                    continue;
                if (sb.Length > 0) sb.Append(";");
                sb.Append(a);
            }
            return sb.ToString();
        }

        public static string BuildLine(string id, string config)
        {
            return id + ":" + config;
        }
        
        public static Dictionary<string, string> Extract(string Params)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (string a in Params.Split(';'))
            {
                int pos = a.IndexOf("=");
                if (pos == -1)
                {
                    parameters.Add(a, null);
                }
                else
                {
                    parameters.Add(
                        a.Substring(0, pos),
                        a.Substring(pos + 1, a.Length - pos - 1));
                }
            }
            return parameters;
        }

        public static string SecureConfig(string Params)
        {
            if (!Params.Contains("="))
                return Params;

            List<string> secure1 = new List<string>() { "user", "username", "userid", "usr", "usrid", "uid"};
            List<string> secure2 = new List<string>() { "password", "pwd", "passwd", "passwrd", "pw" };
            Dictionary<string, string> parameters = Extract(Params);

            StringBuilder sb = new StringBuilder();
            foreach (string key in parameters.Keys)
            {
                if (key == String.Empty)
                    continue;

                if(sb.Length>0) sb.Append(";");
                if (secure2.Contains(key.Trim().ToLower()) ||
                    secure2.Contains(key.Trim().ToLower().Replace(" ", "")))
                {
                    sb.Append(key + "=********");
                }
                else if (secure1.Contains(key.Trim().ToLower()) ||
                    secure1.Contains(key.Trim().ToLower().Replace(" ", "")))
                {
                    string v = parameters[key], val = v;
                    if (v.Length > 2)
                    {
                        val = v[0].ToString();
                        for (int i = 1; i < v.Length - 2; i++)
                            val += "*";
                        val += v[v.Length - 1];
                    }
                    sb.Append(key + "=" + val);
                }
                else
                {
                    sb.Append(key + "=" + parameters[key]);
                }
            }

            return sb.ToString();
        }
	}

	internal class XmlNodePlus 
	{
		private XmlNode _node;
		private Hashtable _hash;
        private NumberFormatInfo _nhi = global::System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
        
		public XmlNodePlus(XmlNode node) 
		{
			_node=node;
			_hash=new Hashtable();

            if (_node.Attributes["culture"] != null)
            {
                foreach (CultureInfo cu in global::System.Globalization.CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if (cu.TextInfo.CultureName == _node.Attributes["culture"].Value)
                    {
                        _nhi = cu.NumberFormat;
                        break;
                    }
                }
            }
        }
        public XmlNodePlus(XmlNode node, NumberFormatInfo nhi) : this(node)
        {
            _nhi = nhi;
        }
		public XmlNode Node 
		{
			get { return _node; }
			set { _node=value; }
		}

		public XmlNode Next(string xPath) 
		{
			if(_hash[xPath]==null) 
			{
				_hash.Add(xPath,new XmlNodeCursor(_node.SelectNodes(xPath)));
			}
			XmlNodeCursor cursor=(XmlNodeCursor)_hash[xPath];
			if(cursor==null) return null;

			return cursor.Next;
		}

        public NumberFormatInfo NumberFormat
        {
            get { return _nhi; }
        }

        public CultureInfo Culture
        {
            set
            {
                if (value == null) return;

                if (_node.Attributes["culture"] != null)
                {
                    _node.Attributes["culture"].Value = value.TextInfo.CultureName;
                }
                else
                {
                    XmlAttribute attr = _node.OwnerDocument.CreateAttribute("culture");
                    attr.Value = value.TextInfo.CultureName;

                    _node.Attributes.Append(attr);
                }

                _nhi = value.NumberFormat;
            }
        }
	}

	internal class XmlNodeCursor 
	{
		private int _pos=0;
		private XmlNodeList _list;

		public XmlNodeCursor(XmlNodeList list) 
		{
			_list=list;
		}

		public XmlNode Next 
		{
			get 
			{
				if(_list==null) return null;
				if(_pos>=_list.Count) return null;
				return _list[_pos++];
			}
		}
	}

    public class XmlStreamObject
    {
        private object _value;
        private string _description = String.Empty, _displayName = String.Empty, _category = String.Empty;
        private bool _isReadOnly = false, _visible = true;

        public XmlStreamObject() { }
        public XmlStreamObject(object val)
        {
            _value = val;
        }

        public object Value
        {
            get { return _value; }
            internal set { _value = value; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
    }
    public class XmlStreamOption : XmlStreamObject
    {
        private object[] _values;

        public XmlStreamOption()
            : base()
        {
        }
        public XmlStreamOption(object [] values, object selected)
            : base(selected)
        {
            _values = values;
        }

        public object[] Options
        {
            get { return _values; }
            internal set { _values = value; }
        }
    }
    public class XmlStreamOption<T> : XmlStreamOption
    {
        public XmlStreamOption(T[] values, T selected)
            : base(Options(values), selected)
        {
        }

        new internal static object[] Options(T[] values)
        {
            if (values == null) return null;
            object[] ret = new object[values.Length];

            Array.Copy(values, ret, values.Length);
            return ret;
        }
    }
    public class XmlStreamStringArray : XmlStreamObject
    {
        public XmlStreamStringArray()
            : base()
        {
        }
        public XmlStreamStringArray(string[] values)
            : base(values)
        {
        }
    }

    public class ConfigConnections
    {
        private string _root=String.Empty;
        private string _encKey = String.Empty;
        private Encoding _encoding = Encoding.Default;

        public ConfigConnections(string name)
        {
            if (name == null) return;

            _root = SystemVariables.MyApplicationConfigPath + @"/connections/" + name;
            DirectoryInfo di = new DirectoryInfo(_root);
            if (!di.Exists)
                di.Create();
        }
        public ConfigConnections(string name, string encKey)
            : this(name)
        {
            _encKey = encKey;
        }

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public Dictionary<string, string> Connections
        {
            get
            {
                Dictionary<string, string> connections = new Dictionary<string, string>();

                if (String.IsNullOrEmpty(_root)) return connections;
                DirectoryInfo di = new DirectoryInfo(_root);
                if (!di.Exists) return connections;

                foreach (FileInfo fi in di.GetFiles("*.con"))
                {
                    StreamReader sr = new StreamReader(fi.FullName, _encoding);
                    string conn = sr.ReadToEnd();
                    sr.Close();

                    string name = InvReplaceSlash(fi.Name.Substring(0, fi.Name.Length - 4));
                    if (String.IsNullOrEmpty(_encKey))
                    {
                        connections.Add(name, conn);
                    }
                    else
                    {
                        byte[] bytes = Convert.FromBase64String(conn);
                        bytes = Crypto.Decrypt(bytes, _encKey);
                        connections.Add(name, _encoding.GetString(bytes));
                    }
                }
                return connections;
            }
        }

        public bool Add(string name, string connectionstring)
        {
            if (String.IsNullOrEmpty(_root)) return false;
            DirectoryInfo di = new DirectoryInfo(_root);
            if (!di.Exists) di.Create();

            if (!String.IsNullOrEmpty(_encKey))
            {
                byte[] bytes = Crypto.Encrypt(_encoding.GetBytes(connectionstring), _encKey);
                connectionstring = Convert.ToBase64String(bytes);
            }

            StreamWriter sw = new StreamWriter(di.FullName + @"/" + ReplaceSlash(name) + ".con",false, _encoding);
            sw.Write(connectionstring);
            sw.Close();

            return true;
        }

        public bool Remove(string name)
        {
            if (String.IsNullOrEmpty(_root)) return false;
            DirectoryInfo di = new DirectoryInfo(_root);
            if (!di.Exists) di.Create();

            try
            {
                FileInfo fi = new FileInfo(di.FullName + @"/" + ReplaceSlash(name) + ".con");
                if (fi.Exists) fi.Delete();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Rename(string oldName, string newName)
        {
            if (String.IsNullOrEmpty(_root)) return false;
            DirectoryInfo di = new DirectoryInfo(_root);
            if (!di.Exists) di.Create();

            try
            {
                FileInfo fi = new FileInfo(di.FullName + @"/" + ReplaceSlash(oldName) + ".con");
                if (fi.Exists)
                {
                    fi.MoveTo(di.FullName + @"/" + ReplaceSlash(newName) + ".con");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetName(string name)
        {
            if (String.IsNullOrEmpty(_root)) return String.Empty;
            DirectoryInfo di = new DirectoryInfo(_root);
            if (!di.Exists) di.Create();

            string ret = name;

            int i = 2;
            while (di.GetFiles(ReplaceSlash(ret) + ".con").Length != 0)
            {
                ret = name + "(" + i++ + ")";
            }

            return ret;
        }

        private string ReplaceSlash(string name)
        {
            return name.Replace("/", "&slsh;").Replace("\\", "&bkslsh;").Replace(":", "&colon;");
        }
        private string InvReplaceSlash(string name)
        {
            return name.Replace("&slsh;", "/").Replace("&bkslsh;", "\\").Replace("&colon;", ":");
        }
    }

    public class ExceptionConverter
    {
        public static string ToString(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Details:\r\n");
            while (ex != null)
            {
                sb.Append("Message:" + ex.Message + "\r\n");
                sb.Append("Source:" + ex.Source + "\r\n");
                sb.Append("Stacktrace:" + ex.StackTrace + "\r\n");

                ex = ex.InnerException;
                if (ex != null) sb.Append("Inner Exception:\r\n");
            }
            return sb.ToString();
        }
    }
}
