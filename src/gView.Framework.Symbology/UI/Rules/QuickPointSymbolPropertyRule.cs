#nullable enable

using gView.Framework.Core.Reflection;
using gView.Framework.Core.Symbology;
using System.Reflection;

namespace gView.Framework.Symbology.UI.Rules;

public class QuickPointSymbolPropertyRule : IBrowsableRule
{
    public bool BrowsableFor(PropertyInfo propertyInfo, object instance)
    {
        ISymbol? symbol = instance switch
        {
            QuickPointSymbolProperties properties => properties.Symbol,
            _ => null
        };

        if (symbol is null)
        {
            return false;
        }

        return propertyInfo.Name switch
        {
            "Color"
                => symbol is IFontColor || symbol is IBrushColor,
            "Size"
                => symbol is ISymbolSize,
            _ => true
        };
    }
}