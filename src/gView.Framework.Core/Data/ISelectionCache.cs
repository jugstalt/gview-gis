using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Cursors;

namespace gView.Framework.Core.Data
{
    public interface ISelectionCache
    {
        IFeatureCursor GetSelectedFeatures();
        IFeatureCursor GetSelectedFeatures(IDisplay display);
    }
}