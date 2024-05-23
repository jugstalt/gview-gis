namespace gView.Framework.Core.Symbology
{
    public interface ILegendGroup
    {
        int LegendItemCount { get; }
        ILegendItem LegendItem(int index);
        void SetSymbol(ILegendItem item, ISymbol symbol);
    }
}
