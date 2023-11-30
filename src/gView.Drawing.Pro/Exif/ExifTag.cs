namespace gView.Drawing.Pro.Exif
{
    public sealed class ExifTag
    {
        private int _id;
        private string _description;
        private string _fieldName;
        private string _value;

        public int Id
        {
            get
            {
                return _id;
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
        }
        public string FieldName
        {
            get
            {
                return _fieldName;
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                this._value = value;
            }
        }

        public ExifTag(int id, string fieldName, string description)
        {
            this._id = id;
            this._description = description;
            this._fieldName = fieldName;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}) = {2}", Description, FieldName, Value);
        }

    }
}
