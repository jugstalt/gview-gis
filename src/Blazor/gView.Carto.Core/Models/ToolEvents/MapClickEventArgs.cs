using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Models.ToolEvents;

public class MapClickEventArgs : ToolEventArgs
{
    public IPoint? Point { get; set; }
}
