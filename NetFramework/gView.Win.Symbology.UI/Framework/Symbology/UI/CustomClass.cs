using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Specialized;
using System.Drawing.Design;
using System.Drawing;
using System.Resources;
using System.Windows.Forms.Design;
using System.Text;
using gView.Framework.Symbology;

namespace gView.Framework.Symbology.UI
{
    /// <summary>
    /// CustomClass (Which is binding to property grid)
    /// </summary>
    [TypeConverter("CustomProperty")]
    public class CustomClass : CollectionBase, ICustomTypeDescriptor
    {
        private object _object = null;

        public CustomClass() { }
        public CustomClass(object obj)
        {
            _object = obj;
            if (obj == null) return;

            // get all the fields in the class
            PropertyInfo[] props_of_class = obj.GetType().GetProperties();

            // copy each property over to 'this'
            foreach (PropertyInfo pi in props_of_class)
            {
                bool browsable = true;
                foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(pi))
                {
                    if (attr is BrowsableAttribute)
                    {
                        browsable = ((BrowsableAttribute)attr).Browsable;
                    }
                }

                if (browsable)
                    this.Add(new CustomProperty(obj, pi));
            }
        }

        /// <summary>
        /// Add CustomProperty to Collectionbase List
        /// </summary>
        /// <param name="Value"></param>
        public void Add(CustomProperty Value)
        {

            base.List.Add(Value);
        }

        /// <summary>
        /// Remove item from List
        /// </summary>
        /// <param name="Name"></param>
        public void Remove(string Name)
        {
            foreach (CustomProperty prop in base.List)
            {
                if (prop.Name == Name)
                {
                    base.List.Remove(prop);
                    return;
                }
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public CustomProperty this[int index]
        {
            get
            {
                return (CustomProperty)base.List[index];
            }
            set
            {
                base.List[index] = (CustomProperty)value;
            }
        }


        #region "TypeDescriptor Implementation"
        /// <summary>
        /// Get Class Name
        /// </summary>
        /// <returns>String</returns>
        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        /// <summary>
        /// GetAttributes
        /// </summary>
        /// <returns>AttributeCollection</returns>
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        /// <summary>
        /// GetComponentName
        /// </summary>
        /// <returns>String</returns>
        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        /// <summary>
        /// GetConverter
        /// </summary>
        /// <returns>TypeConverter</returns>
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        /// <summary>
        /// GetDefaultEvent
        /// </summary>
        /// <returns>EventDescriptor</returns>
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        /// <summary>
        /// GetDefaultProperty
        /// </summary>
        /// <returns>PropertyDescriptor</returns>
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        /// <summary>
        /// GetEditor
        /// </summary>
        /// <param name="editorBaseType">editorBaseType</param>
        /// <returns>object</returns>
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
            PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                CustomProperty prop = (CustomProperty)this[i];

                Attribute[] propAttributes = new Attribute[attributes.Length + prop.CustomAttributes.Length];
                attributes.CopyTo(propAttributes, 0);
                prop.CustomAttributes.CopyTo(propAttributes, attributes.Length);

                for (int p = 0; p < propAttributes.Length; p++)
                {
                    if (propAttributes[p] is UseColorPicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.ColorTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseWidthPicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.PenWidthTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseDashStylePicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.DashStyleTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseHatchStylePicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.HatchStyleTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseLineSymbolPicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.LineSymbolTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseCharacterPicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.CharacterTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseFilePicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.FileTypeEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                    if (propAttributes[p] is UseColorGradientPicker)
                    {
                        propAttributes[p] = new EditorAttribute(typeof(gView.Framework.Symbology.UI.ColorGradientEditor), typeof(System.Drawing.Design.UITypeEditor));
                        break;
                    }
                }

                newProps[i] = new CustomPropertyDescriptor(ref prop, propAttributes);
            }

            return new PropertyDescriptorCollection(newProps);
        }

        public PropertyDescriptorCollection GetProperties()
        {

            return TypeDescriptor.GetProperties(this, true);

        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion

        public object ObjectInstance
        {
            get { return _object; }
        }
    }

    public class CustomProperty : StringConverter
    {
        private object _object;
        private PropertyInfo _pi;
        private string _category = String.Empty;

        public CustomProperty(object obj, PropertyInfo pi)
        {
            _object = obj;
            _pi = pi;

            foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(pi))
            {
                if (attr is CategoryAttribute)
                {
                    _category = ((CategoryAttribute)attr).Category;
                }
            }
        }

        public string Name
        {
            get { return _pi.Name; }
        }

        public object Value
        {
            get
            {
                return _pi.GetValue(_object, null);
            }
            set
            {
                _pi.SetValue(_object, value, null);
            }
        }

        public bool ReadOnly
        {
            get { return false; }
        }

        public string Category
        {
            get { return _category; }
        }

        public Attribute[] CustomAttributes
        {
            get { return System.Attribute.GetCustomAttributes(_pi); }
        }

        public Type PropertyType
        {
            get
            {
                return _pi.PropertyType;
            }
        }
    }

    /// <summary>
    /// Custom PropertyDescriptor
    /// </summary>
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
            return m_Property.Value;
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
                return m_Property.ReadOnly;
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
            m_Property.Value = value;
        }

        public override Type PropertyType
        {
            get
            {
                return m_Property.PropertyType;
            }
        }

        #endregion
    }
}
