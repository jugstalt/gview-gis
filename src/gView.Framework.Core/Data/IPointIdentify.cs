using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IPointIdentify
    {
        IPointIdentifyContext CreatePointIdentifyContext();
        Task<ICursor> PointQuery(IDisplay display, IPoint point, ISpatialReference sRef, IUserData userdata, IPointIdentifyContext context);
    }
}