using gView.Framework.Symbology;

namespace gView.Framework.Carto
{
    public interface IFeatureRenderer2 : IFeatureRenderer
    {
        ISymbol Symbol { get; set; }
    }
}