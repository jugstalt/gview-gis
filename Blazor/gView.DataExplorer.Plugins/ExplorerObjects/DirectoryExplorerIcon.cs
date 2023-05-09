using gView.Blazor.Core;
using gView.Framework.DataExplorer.Abstraction;
using System;

namespace gView.DataExplorer.Plugins.ExplorerObject;

internal class DirectoryExplorerIcon : IExplorerIcon
{
    #region IExplorerIcon Members

    public Guid GUID
    {
        get { return KnownExplorerObjectIDs.Directory; }
    }

    public byte[] Image
    {
        get
        {
            return new byte[0];
            //return (new Icons()).imageList1.Images[1];
        }
    }

    #endregion
}
