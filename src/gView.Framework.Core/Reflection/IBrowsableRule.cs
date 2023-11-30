using System.Reflection;

namespace gView.Framework.Core.Reflection;

public interface IBrowsableRule
{
    bool BrowsableFor(PropertyInfo propertyInfo, object instance);
}
