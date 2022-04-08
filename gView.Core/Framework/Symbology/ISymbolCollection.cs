using System.Collections.Generic;

namespace gView.Framework.Symbology
{
    public interface ISymbolCollection
    {
        List<ISymbolCollectionItem> Symbols { get; }
        void AddSymbol(ISymbol symbol);
        void AddSymbol(ISymbol symbol, bool visible);
        void RemoveSymbol(ISymbol symbol);
        void InsertBefore(ISymbol symbol, ISymbol before, bool visible);
        int IndexOf(ISymbol symbol);
        void ReplaceSymbol(ISymbol oldSymbol, ISymbol newSymbol);
        bool IsVisible(ISymbol symbol);
    }
}
