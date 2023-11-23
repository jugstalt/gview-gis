using gView.Carto.Plugins.Extensions;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Symbology;
using gView.Razor.Abstractions;

namespace gView.Carto.Plugins.PropertyGridEditors
{
    internal class SymbolPropertyEditor : IPropertyGridEditor
    {
        public Type PropertyType => typeof(ISymbol);

        async public Task<object?> EditAsync(IApplicationScope scope, object? propertyValue)
        {
            var scopeService = scope.ToCartoScopeService();

            var symbol = propertyValue as ISymbol;
            if (symbol == null)
            {
                return null;
            }

            var model = await scopeService.ShowModalDialog(
                typeof(gView.Carto.Razor.Components.Dialogs.SymbolComposerDialog),
                $"Edit: {symbol.Name}",
                new SymbolComposerModel()
                {
                    Symbol = symbol
                });

            return model?.Symbol;
        }
    }
}
