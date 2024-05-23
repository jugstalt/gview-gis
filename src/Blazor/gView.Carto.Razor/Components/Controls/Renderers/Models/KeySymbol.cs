using gView.Framework.Core.Symbology;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;

public class KeySymbol<TKey>
{
    public KeySymbol(TKey key, ISymbol symbol)
    {
        Key = key;
        Symbol = symbol;
    }

    public TKey Key { get; }
    public ISymbol Symbol { get; set; }

    public string LegendLabel
    {
        get => Symbol switch
        {
            ILegendItem item => item.LegendLabel,
            _ => ""
        };

        set
        {
            if (Symbol is ILegendItem item)
            {
                item.LegendLabel = value;
            }
        }
    }

    public KeySymbol<TKey> Copy()
        => new KeySymbol<TKey>(Key, Symbol);
}
