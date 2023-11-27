using gView.Framework.Core.Data;
using gView.Framework.Core.UI;

namespace gView.Framework.Data.Joins
{
    public interface IJoinPropertyPanel
    {
        object PropertyPanel(IFeatureLayerJoin join, IMapDocument mapDocument);
    }
}
