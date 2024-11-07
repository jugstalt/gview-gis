using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.Data
{
    public interface IGridIdentify
    {
        void InitGridQuery();
        float GridQuery(IDisplay display, IPoint point, ISpatialReference sRef);
        void ReleaseGridQuery();
    }
}