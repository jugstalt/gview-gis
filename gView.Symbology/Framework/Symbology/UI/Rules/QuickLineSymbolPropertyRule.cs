#nullable enable

using gView.Framework.Reflection;
using System.Reflection;

namespace gView.Framework.Symbology.UI.Rules;

public class QuickLineSymbolPropertyRule : IBrowsableRule
{
    public bool BrowsableFor(PropertyInfo propertyInfo, object instance)
    {
        ISymbol? symbol = instance switch
        {
            QuickLineSymbolProperties properties => properties.Symbol,
            _ => null
        };

        if (symbol is null)
        {
            return false;
        }

        return propertyInfo.Name switch
        {
            "Color"
                => symbol is IPenColor,
            "Width"
                => symbol is IPenWidth,
            "DashStyle"
                => symbol is IPenDashStyle,

            _ => true
        };
    }
}
