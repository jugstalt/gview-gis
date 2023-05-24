using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Ogc;

public interface IOgcGroupExplorerObject : IExplorerObject
{
    void SetParentExplorerObject(IExplorerObject parentExplorerObject);
}
