using System;

namespace gView.Framework.Core.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UseWithinSelectableDatasetElements : Attribute
    {
        private bool _value = true;

        public UseWithinSelectableDatasetElements(bool val)
        {
            _value = val;
        }

        public bool Value
        {
            get { return _value; }
        }
    }
}