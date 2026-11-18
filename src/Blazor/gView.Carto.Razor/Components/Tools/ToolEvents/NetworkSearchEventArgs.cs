using gView.Carto.Core.Models.ToolEvents;

namespace gView.Carto.Razor.Components.Tools.ToolEvents;

public enum NetworkSearchAction
{
    Run,
    Clear,
    ToSelection,
    ExportGeoJson,
    ZoomToViolation,
    /// <summary>Removes a picked item (Index; Target for the target list).</summary>
    RemovePick,
    /// <summary>Zooms to a picked item (Index; Target for the target list).</summary>
    ZoomToPick
}

public class NetworkSearchEventArgs(NetworkSearchAction action, int index = 0, bool target = false) : ToolEventArgs
{
    /// <summary>RemovePick, ZoomToPick: the item is in the target list, not in the start list.</summary>
    public bool Target { get; } = target;

    public NetworkSearchAction Action { get; } = action;

    /// <summary>ZoomToViolation: the index in the violations of the result; RemovePick, ZoomToPick: the index of the item.</summary>
    public int Index { get; } = index;
}
