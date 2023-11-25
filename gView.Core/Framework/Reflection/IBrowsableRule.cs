using System.Reflection;

namespace gView.Framework.Reflection;

public interface IBrowsableRule
{
    bool BrowsableFor(PropertyInfo propertyInfo, object instance);
}
