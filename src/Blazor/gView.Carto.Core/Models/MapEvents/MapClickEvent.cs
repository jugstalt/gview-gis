using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Models.MapEvents;

public class MapClickEvent : MapEvent
{
    public IPoint? Point { get; set; }
}
