using gView.Framework.DataExplorer.Abstraction;
using System;

namespace gView.DataExplorer.Plugins.ExplorerObject;

internal class ObjectIcon : IExplorerIcon
{
    int _imageIndex = 0;
    public ObjectIcon(int imageIndex)
    {
        _imageIndex = imageIndex;
    }

    #region IExplorerIcon Members

    public Guid GUID
    {
        get { return Guid.NewGuid(); }
    }

    public byte[] Image
    {
        get
        {
            // ToDo:
            //System.Windows.Forms.ImageList imageList = (new Icons()).imageList1;
            //if (_imageIndex >= imageList.Images.Count)
            //{
            //    return null;
            //}

            //return imageList.Images[_imageIndex];
            //return FormExplorer.globalImageList.Images[_imageIndex];

            return null;
        }
    }

    #endregion
}
