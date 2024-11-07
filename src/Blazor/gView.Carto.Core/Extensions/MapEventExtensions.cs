using gView.Carto.Core.Models.ToolEvents;
using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Extensions;

public static class MapEventExtensions
{
    static public IGeometry? GetGeometry(this ToolEventArgs mapEvent)
        => mapEvent switch
        {
            MapClickEventArgs mapClickEvent => mapClickEvent.Point,
            MapBBoxEventArgs mapBBoxEvent => mapBBoxEvent.BBox,
            _ => null
        };

}
