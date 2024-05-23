using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Models.ToolEvents;

public class MapBBoxEventArgs : ToolEventArgs
{
    public IEnvelope? BBox { get; set; }
}
