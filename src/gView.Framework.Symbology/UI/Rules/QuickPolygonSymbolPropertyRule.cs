#nullable enable

using gView.Framework.Core.Reflection;
using gView.Framework.Core.Symbology;
using System.Reflection;

namespace gView.Framework.Symbology.UI.Rules;

public class QuickPolygonSymbolPropertyRule : IBrowsableRule
{
    public bool BrowsableFor(PropertyInfo propertyInfo, object instance)
    {
        ISymbol? outlineSymbol = instance switch
        {
            QuickPolygonSymbolProperties properties
                when properties.Symbol is IOutlineSymbol => ((IOutlineSymbol)properties.Symbol).OutlineSymbol,
            _ => null
        };

        return propertyInfo.Name switch
        {
            "Width"
                => outlineSymbol is not null && outlineSymbol is not ISymbolCollection,
            "DashStyle"
                => outlineSymbol is not null && outlineSymbol is not ISymbolCollection,
            "SymbolSmoothing"
                => outlineSymbol is not null,

            _ => true
        };
    }
}
