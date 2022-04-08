using gView.Framework.Carto;

namespace gView.Framework.UI
{
    public delegate void PropertyChangedEvent(object propertryObject);

    public delegate void MapAddedEvent(IMap map);
    public delegate void MapDeletedEvent(IMap map);
    public delegate void AfterSetFocusMapEvent(IMap map);

    public delegate void ActiveMapToolChangedEvent(ITool OldTool, ITool NewTool);
    public delegate void OnCursorPosChangedEvent(double X, double Y);

    public delegate void ProgressReporterEvent(ProgressReport progressEventReport);
}
