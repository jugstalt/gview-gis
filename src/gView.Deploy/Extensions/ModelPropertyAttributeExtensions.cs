using gView.Deploy.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gView.Deploy.Extensions;

internal static class ModelPropertyAttributeExtensions
{
    static public string GetDefaultValue(this ModelPropertyAttribute modelPropertyAttribute, object model)
    {
        var defaultValue = modelPropertyAttribute.DefaultValue;

        if (!String.IsNullOrEmpty(defaultValue))
        {
            defaultValue = defaultValue.Replace("{{ROOT}}",
                Platform.IsLinux
                    ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    : Platform.IsWindows
                        ? @"C:"
                        : "");
            defaultValue = defaultValue.Replace(@"{{\}}",
                Platform.IsWindows
                ? @"\"
                : "/");

            var modelType = model.GetType();

            var matches = Regex.Matches(defaultValue, @"{([^{}]*)");
            var propertyNames = matches.Cast<Match>().Select(m => m.Groups[1].Value).Distinct();

            foreach (var propertyName in propertyNames)
            {
                defaultValue = defaultValue.Replace($"{{{propertyName}}}", modelType.GetProperty(propertyName)?.GetValue(model)?.ToString());
            }
        }

        return defaultValue;
    }
}
