using gView.Carto.Core.Models.ToolEvents;

namespace gView.Carto.Razor.Components.Tools.ToolEvents;

public enum NetworkSearchAction
{
    Run,
    /// <summary>Cancels the run in progress.</summary>
    Cancel,
    Clear,
    ToSelection,
    ExportGeoJson,
    ZoomToViolation,
    /// <summary>Removes a picked item (Index; Target for the target list).</summary>
    RemovePick,
    /// <summary>Zooms to a picked item (Index; Target for the target list).</summary>
    ZoomToPick,
    /// <summary>The area becomes the current map extent.</summary>
    AreaFromExtent,
    /// <summary>The area becomes the selected polygon features (all layers, together).</summary>
    AreaFromSelection,
    /// <summary>Removes the area: the searches go over the whole network again.</summary>
    RemoveArea,
    /// <summary>Zooms to the area.</summary>
    ZoomToArea
}

public class NetworkSearchEventArgs(NetworkSearchAction action, int index = 0, bool target = false) : ToolEventArgs
{
    /// <summary>RemovePick, ZoomToPick: the item is in the target list, not in the start list.</summary>
    public bool Target { get; } = target;

    public NetworkSearchAction Action { get; } = action;

    /// <summary>ZoomToViolation: the index in the violations of the result; RemovePick, ZoomToPick: the index of the item.</summary>
    public int Index { get; } = index;
}
