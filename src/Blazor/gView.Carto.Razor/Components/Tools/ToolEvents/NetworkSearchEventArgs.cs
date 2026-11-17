using gView.Carto.Core.Models.ToolEvents;

namespace gView.Carto.Razor.Components.Tools.ToolEvents;

public enum NetworkSearchAction
{
    Run,
    Clear,
    ToSelection,
    ExportGeoJson,
    Build,
    ZoomToViolation
}

public class NetworkSearchEventArgs(NetworkSearchAction action, int index = 0) : ToolEventArgs
{
    public NetworkSearchAction Action { get; } = action;

    /// <summary>ZoomToViolation: the index in the violations of the result.</summary>
    public int Index { get; } = index;
}
