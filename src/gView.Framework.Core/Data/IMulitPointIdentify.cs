using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IMulitPointIdentify
    {
        Task<ICursor> MultiPointQuery(IDisplay display, IPointCollection points, ISpatialReference sRef, IUserData userdata);
    }
}