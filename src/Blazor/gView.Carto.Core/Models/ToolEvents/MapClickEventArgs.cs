using gView.Framework.Core.Geometry;

namespace gView.Carto.Core.Models.ToolEvents;

public class MapClickEventArgs : ToolEventArgs
{
    public IPoint? Point { get; set; }

    /// <summary>Ctrl (or Cmd) was held: tools may add to what is picked instead of replacing it.</summary>
    public bool CtrlKey { get; set; }
}
