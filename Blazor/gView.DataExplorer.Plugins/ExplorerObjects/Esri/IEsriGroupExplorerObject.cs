using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Esri
{
    internal interface IEsriGroupExplorerObject : IExplorerObject
    {
        void SetParentExplorerObject(IExplorerObject parentExplorerObject);
    }
}
