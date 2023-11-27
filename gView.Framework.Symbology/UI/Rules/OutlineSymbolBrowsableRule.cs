#nullable enable

using gView.Framework.Core.Reflection;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;
using System;
using System.Reflection;

namespace gView.Framework.Symbology.UI.Rules
{
    internal class OutlineSymbolBrowsableRule : IBrowsableRule
    {
        public bool BrowsableFor(PropertyInfo propertyInfo, object instance)
        {
            ISymbol? outlineSymbol = instance switch
            {
                IOutlineSymbol polygonSymbol => polygonSymbol.OutlineSymbol,
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
