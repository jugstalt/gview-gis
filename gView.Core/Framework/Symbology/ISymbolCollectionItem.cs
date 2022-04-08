namespace gView.Framework.Symbology
{
    public interface ISymbolCollectionItem
    {
        bool Visible { get; }
        ISymbol Symbol { get; }
    }
}
