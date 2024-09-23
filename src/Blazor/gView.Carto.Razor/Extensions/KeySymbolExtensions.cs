using gView.Carto.Razor.Components.Controls.Renderers.Models;

namespace gView.Carto.Razor.Extensions;

static internal class KeySymbolExtensions
{
    static public bool ContainsKey<TKey>(this IEnumerable<KeySymbol<TKey>> keySymbols, TKey key)
        => keySymbols.Any(k => key?.Equals(k.Key) == true);
}
