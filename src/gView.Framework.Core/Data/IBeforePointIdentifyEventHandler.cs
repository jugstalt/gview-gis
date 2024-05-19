using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;

namespace gView.Framework.Core.Data
{
    public interface IBeforePointIdentifyEventHandler
    {
        event BeforePointIdentifyEventHandler BeforePointIdentify;
        void FireBeforePointIdentify(IDisplay display, ref IPoint point, ref ISpatialReference sRef, IUserData userdata);
    }
}