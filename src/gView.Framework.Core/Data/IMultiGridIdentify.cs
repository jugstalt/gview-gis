using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IMultiGridIdentify
    {
        Task<float[]> MultiGridQuery(IDisplay display, IPoint[] Points, double dx, double dy, ISpatialReference sRef, IUserData userdata);
    }
}