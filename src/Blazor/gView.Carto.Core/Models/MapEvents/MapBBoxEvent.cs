using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Models.MapEvents;

public class MapBBoxEvent : MapEvent
{
    public IEnvelope? BBox { get; set; }
}
