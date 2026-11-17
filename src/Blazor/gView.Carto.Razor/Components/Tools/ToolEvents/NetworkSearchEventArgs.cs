using gView.Carto.Core.Models.ToolEvents;

namespace gView.Carto.Razor.Components.Tools.ToolEvents;

public enum NetworkSearchAction
{
    Run,
    Clear,
    ToSelection,
    ExportGeoJson
}

public class NetworkSearchEventArgs(NetworkSearchAction action) : ToolEventArgs
{
    public NetworkSearchAction Action { get; } = action;
}
