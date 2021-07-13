using System;

namespace gView.Framework.IO
{
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
}
