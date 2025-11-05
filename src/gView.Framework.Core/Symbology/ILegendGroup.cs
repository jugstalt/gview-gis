using gView.Framework.Core.Data;

namespace gView.Framework.Core.Symbology
{
    public interface ILegendGroup
    {
        int LegendItemCount { get; }
        ILegendItem LegendItem(int index);
        void SetSymbol(ILegendItem item, ISymbol symbol);
    }

    public interface ILegendDependentFields
    {
        string[] LegendDependentFields { get; }
        string LegendSymbolOrderField { get; }

        string LegendSymbolKeyFromFeature(IFeature feature);
        ILegendItem LegendItemFromSymbolKey(string symbolKey);
    } 
}
