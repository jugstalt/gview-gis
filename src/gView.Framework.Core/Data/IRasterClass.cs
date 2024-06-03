using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IRasterClass : IClass
    {
        IPolygon Polygon { get; }

        double oX { get; }
        double oY { get; }
        double dx1 { get; }
        double dx2 { get; }
        double dy1 { get; }
        double dy2 { get; }

        ISpatialReference SpatialReference { get; set; }

        Task<IRasterPaintContext> BeginPaint(IDisplay display, ICancelTracker cancelTracker);
    }
}