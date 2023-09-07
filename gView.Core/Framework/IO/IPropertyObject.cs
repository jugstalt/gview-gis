using System;

namespace gView.Framework.IO
{
    public interface IPropertyObject
    {
        Type PropertyObjectType { get; }
        object GetPropertyObject();
        void SetPropertyObject(object propertyObject);
    }
}
