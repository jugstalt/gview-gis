using gView.Razor.Leaflet.Models;
using gView.Razor.Leaflet.Models.Events;

namespace BlazorLeaflet.Models.Events;

public class MouseEvent : Event
{
    public LatLng? LatLng { get; set; }

    public Point2d LayerPoint { get; set; }

    public Point2d ContainerPoint { get; set; }

    /// <summary>Ctrl or Cmd was held (from the browser event).</summary>
    public bool CtrlKey { get; set; }

    public bool ShiftKey { get; set; }
}

public class BBoxEvent : Event
{
    public LatLng? NorthWest { get; set; }
    public LatLng? SouthEast { get; set; }
}
