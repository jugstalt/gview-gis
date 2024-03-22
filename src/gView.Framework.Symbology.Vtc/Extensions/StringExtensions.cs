using gView.Framework.Core.Data;
using Proj4Net.Core.Projection;

namespace gView.Framework.Symbology.Vtc.Extensions;

static public class StringExtensions
{
    static public string ReplacePlaceholders(this string? str, IFeature? feature)
    {
        if (feature?.Fields != null && str?.Contains("{") == true)
        {
            foreach (var field in feature.Fields)
            {
                str = str.Replace($"{{{field.Name}}}", field.Value?.ToString() ?? "");
            }
        }

        return str;
    }
}
