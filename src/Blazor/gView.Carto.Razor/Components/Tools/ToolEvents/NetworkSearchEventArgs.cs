using gView.Carto.Core.Models.ToolEvents;

namespace gView.Carto.Razor.Components.Tools.ToolEvents;

public enum NetworkSearchAction
{
    Run,
    /// <summary>Reads weights and tiers of the built network again (it may have been built after the tool was opened).</summary>
    Refresh,
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
    ZoomToArea,
    /// <summary>Hides or shows the elements of a legend row (Key) in the map.</summary>
    ToggleLegend,
    /// <summary>Zooms to the elements of a legend row (Key).</summary>
    ZoomToLegend,
    /// <summary>The features of a legend row (Key) become the selection of their layers; Target: add to the selection (Ctrl).</summary>
    SelectLegend,
    /// <summary>Zooms to one element of a legend row (Key, Index in its list).</summary>
    ZoomToElement,
    /// <summary>Draws the result again, e.g. after the feeders colouring changed.</summary>
    Redraw
}

public class NetworkSearchEventArgs(NetworkSearchAction action, int index = 0, bool target = false, string? key = null) : ToolEventArgs
{
    /// <summary>The legend actions: the key of the row (<see cref="Context.NetworkLegendEntry.Key"/> or a group key).</summary>
    public string? Key { get; } = key;

    /// <summary>RemovePick, ZoomToPick: the item is in the target list, not in the start list; SelectLegend: add to the selection.</summary>
    public bool Target { get; } = target;

    public NetworkSearchAction Action { get; } = action;

    /// <summary>ZoomToViolation: the index in the violations of the result; RemovePick, ZoomToPick: the index of the item.</summary>
    public int Index { get; } = index;
}
