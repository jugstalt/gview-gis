using gView.Carto.Core.Models.MapEvents;
using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Extensions;

public static class MapEventExtensions
{
    static public IGeometry? GetGeometry(this MapEvent mapEvent)
        => mapEvent switch
        {
            MapClickEvent mapClickEvent => mapClickEvent.Point,
            MapBBoxEvent mapBBoxEvent => mapBBoxEvent.BBox,
            _ => null
        };

}
