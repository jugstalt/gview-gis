using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Symbology;

namespace gView.Carto.Razor.Extensions;

public static class SymbolExtensions
{
    async public static Task<ISymbol> Compose(this ISymbol symbol, IApplicationScope scope)
    {
        var model = await scope.ShowModalDialog(
            typeof(gView.Carto.Razor.Components.Dialogs.SymbolComposerDialog),
            "Symbol Composer",
            new SymbolComposerModel()
            {
                Symbol = symbol.Clone() as ISymbol,
            });

        return model?.Symbol ?? symbol;
    }
}
