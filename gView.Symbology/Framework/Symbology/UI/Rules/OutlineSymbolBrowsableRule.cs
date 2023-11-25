#nullable enable

using gView.Framework.Reflection;
using gView.Framework.Symbology;
using gView.GraphicsEngine;
using System;
using System.Reflection;

namespace gView.Symbology.Framework.Symbology.UI.Rules
{
    internal class OutlineSymbolBrowsableRule : IBrowsableRule
    {
        public bool BrowsableFor(PropertyInfo propertyInfo, object instance)
        {
            ISymbol? outlineSymbol = instance switch
            {
                SimpleFillSymbol fillSymbol => fillSymbol.OutlineSymbol,
                HatchSymbol hatchSymbol => hatchSymbol.OutlineSymbol,
                GradientFillSymbol gradientSymbol => gradientSymbol.OutlineSymbol,
                _ => null
            };

            if (outlineSymbol is null)
            {
                return false;
            }

            return (instance, propertyInfo.PropertyType) switch
            {
                (IPenWidth, Type t) when t == typeof(float) => outlineSymbol is not ISymbolCollection,
                (IPenColor, Type t) when t == typeof(ArgbColor) => outlineSymbol is not ISymbolCollection,
                (IPenDashStyle, Type t) when t == typeof(LineDashStyle) => outlineSymbol is not ISymbolCollection,
                _ => true
            };
        }
    }
}
