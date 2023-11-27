using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml;
using gView.Framework.Core.IO;

namespace gView.Framework.IO
{
    [TypeConverter("CustomProperty")]
    public class PersistableClass : Component, ICustomTypeDescriptor, IPersistable
    {
        private PropertyDescriptorCollection _propertyCollection;
        private List<PersistableClass> _childClasses;
        private int _maxLength;
        private string _name = String.Empty;
        private NumberFormatInfo _nhi;

        private PersistableClass()
        {
            _propertyCollection = new PropertyDescriptorCollection(new PropertyDescriptor[] { });
            _childClasses = new List<PersistableClass>();
        }

        public PersistableClass(XmlStream stream) : this()
        {
            if (stream == null || stream.ParentNode == null ||
                stream.ParentNode.Node == null)
            {
                return;
            }

            _nhi = stream.ParentNode.NumberFormat;

            FromNode(stream.ParentNode.Node);
        }
        private PersistableClass(XmlNode parent, NumberFormatInfo nhi) : this()
        {
            _nhi = nhi;
            if (parent == null)
            {
                return;
            }

            FromNode(parent);
        }

        private void FromNode(XmlNode parent)
        {
            if (parent == null)
            {
                return;
            }

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
                                    if (optionNode.Attributes["value"] == null)
                                    {
                                        continue;
                                    }

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
                                    if (valueNode.Attributes["value"] == null)
                                    {
                                        continue;
                                    }

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
                {
                    _maxLength = value;
                }
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
            {
                return (props[0]);
            }
            else
            {
                return (null);
            }
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
                    XmlStreamObject xso = (XmlStreamObject)prop.propValueObject;
                    if (!String.IsNullOrEmpty(xso.DisplayName))
                    {
                        propAttributes.Add(new DisplayNameAttribute(xso.DisplayName));
                    }

                    if (!xso.Visible)
                    {
                        propAttributes.Add(new BrowsableAttribute(false));
                    }
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
                if (propDecriptor == null)
                {
                    return null;
                }

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
}
