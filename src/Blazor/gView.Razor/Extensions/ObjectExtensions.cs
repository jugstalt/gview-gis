using System.ComponentModel;
using System.Reflection;

namespace gView.Razor.Extensions;

static internal class ObjectExtensions
{
    static public Dictionary<PropertyInfo, object?> InstanceProperties(this object instance)
    {
        var propertyInfos = instance.GetType().GetProperties();
        var properties = new Dictionary<PropertyInfo, object?>();

        foreach (var propertyInfo in propertyInfos)
        {
            var browsableAttribute = propertyInfo.GetCustomAttribute<BrowsableAttribute>();
            if (browsableAttribute?.Browsable == false)
            {
                continue;
            }

            properties.Add(propertyInfo, propertyInfo.GetValue(instance));
        }

        return properties;
    }
}
