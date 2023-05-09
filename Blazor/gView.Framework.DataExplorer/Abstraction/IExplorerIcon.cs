using System;

namespace gView.Framework.DataExplorer.Abstraction
{
    public interface IExplorerIcon
    {
        Guid GUID { get; }
        byte[] Image { get; }
    }

}
