using gView.Framework.UI;

namespace gView.Framework.Data.Joins
{
    public interface IJoinPropertyPanel
    {
        object PropertyPanel(IFeatureLayerJoin join, IMapDocument mapDocument);
    }
}
