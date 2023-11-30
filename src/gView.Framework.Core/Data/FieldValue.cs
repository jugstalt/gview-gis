namespace gView.Framework.Core.Data
{

    public class FieldValue : IFieldValue
    {
        private string _name;
        private object _value;

        public FieldValue(string name)
        {
            _name = name;
        }
        public FieldValue(string name, object val)
        {
            _name = name;
            _value = val;
        }

        public void Rename(string newName)
        {
            _name = newName;
        }

        #region IFieldValue Member

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        #endregion
    }
}
