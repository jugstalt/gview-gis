using gView.Framework.Core.Carto;

namespace gView.Framework.Core.UI
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