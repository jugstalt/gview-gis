using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Framework.Cartography.Rendering.UI
{
    public interface IPropertyPanel
    {
        object PropertyPanel(IFeatureRenderer renderer, IFeatureLayer fc);
    }

    public interface IPropertyPanel2
    {
        object PropertyPanel(ILabelRenderer renderer, IFeatureLayer fc);
    }
}
