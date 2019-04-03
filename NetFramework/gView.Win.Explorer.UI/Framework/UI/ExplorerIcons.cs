using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Explorer.UI.Framework.UI
{
    class ExplorerIcons
    {
        #region Icons
        private static Dictionary<Guid, int> _iconIndices = new Dictionary<Guid, int>();
        internal static int ImageIndex(IExplorerObject exObject)
        {
            return ImageIndex(exObject.Icon);
        }
        private static object lockThis = new object();
        private static int ImageIndex(IExplorerIcon icon)
        {
            lock (lockThis)
            {
                int imageIndex = -1;
                if (icon != null)
                {
                    if (!_iconIndices.TryGetValue(icon.GUID, out imageIndex))
                    {
                        int index = ExplorerImageList.List.CountImages;
                        _iconIndices.Add(icon.GUID, index);
                        ExplorerImageList.List.AddImage(icon.Image);
                        return index;
                    }

                }
                return imageIndex;
            }
        }
        #endregion
    }
}
