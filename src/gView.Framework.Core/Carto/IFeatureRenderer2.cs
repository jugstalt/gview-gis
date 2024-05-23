using gView.Framework.Core.Symbology;

namespace gView.Framework.Core.Carto
{
    public interface IFeatureRenderer2 : IFeatureRenderer
    {
        ISymbol Symbol { get; set; }
    }
}