using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Core.Symbology;
using gView.Razor.Abstractions;
using System.Reflection;

namespace gView.Carto.Plugins.PropertyGridEditors;

internal class SymbolPropertyEditor : IPropertyGridEditAsync
{
    public Type PropertyType => typeof(ISymbol);

    async public Task<object?> EditAsync(IApplicationScopeFactory scope,
                                         object instance,
                                         PropertyInfo propertyInfo)
    {
        var symbol = propertyInfo.GetValue(instance) as ISymbol;
        if (symbol == null)
        {
            return null;
        }

        var model = await scope.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.SymbolComposerDialog),
            $"Edit: {symbol.Name}",
            new SymbolComposerModel()
            {
                Symbol = symbol
            });

        return model?.Symbol;
    }
}
