using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using gView.Framework.Data;

namespace gView.system.UI
{
    public class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        IDictionary _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
        }

        public DictionaryPropertyGridAdapter(IFeature feature)
            : this(feature, null)
        {
        }

        public DictionaryPropertyGridAdapter(IFeature feature, IFields fields)
        {
            _dictionary = new Dictionary<string, object>();

            foreach (FieldValue fv in feature.Fields)
            {
                if (fields != null && fields.FindField(fv.Name) == null)
                    continue;

                _dictionary.Add(fv.Name, feature[fv.Name]);
            }
        }

        #region Members

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        #endregion

        #region ICustomTypeDescriptor Member

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }

        #endregion

        #region Property Classes

        class DictionaryPropertyDescriptor : PropertyDescriptor
        {
            IDictionary _dictionary;
            object _key;

            internal DictionaryPropertyDescriptor(IDictionary d, object key)
                : base(key.ToString(), null)
            {
                _dictionary = d;
                _key = key;
            }

            public override Type PropertyType
            {
                get { return _dictionary[_key].GetType(); }
            }

            public override void SetValue(object component, object value)
            {
                _dictionary[_key] = value;
            }

            public override object GetValue(object component)
            {
                return _dictionary[_key];
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type ComponentType
            {
                get { return null; }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }

        #endregion
    }

}
