using System;

namespace gView.Framework.IO
{
    public class XmlStreamOption : XmlStreamObject
    {
        private object[] _values;

        public XmlStreamOption()
            : base()
        {
        }
        public XmlStreamOption(object[] values, object selected)
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
            if (values == null)
            {
                return null;
            }

            object[] ret = new object[values.Length];

            Array.Copy(values, ret, values.Length);
            return ret;
        }
    }
}
