using gView.Framework.Carto;

namespace gView.Framework.UI
{
    public interface IMapDocumentEvents
    {
        event LayerAddedEvent LayerAdded;
        event LayerRemovedEvent LayerRemoved;
        event MapAddedEvent MapAdded;
        event MapDeletedEvent MapDeleted;
        event MapScaleChangedEvent MapScaleChanged;
        event AfterSetFocusMapEvent AfterSetFocusMap;
    }
}