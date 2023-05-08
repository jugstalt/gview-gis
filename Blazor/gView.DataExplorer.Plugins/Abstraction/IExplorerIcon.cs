using System;

namespace gView.DataExplorer.Plugins.Abstraction
{
    public interface IExplorerIcon
    {
        Guid GUID { get; }
        byte[] Image { get; }
    }

}
