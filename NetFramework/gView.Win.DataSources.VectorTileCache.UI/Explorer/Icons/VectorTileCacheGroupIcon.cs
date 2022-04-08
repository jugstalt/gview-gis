using gView.Framework.UI;
using System;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer.Icons
{
    public class VectorTileCacheGroupIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("86EFDEA1-1BBF-422F-B862-C49072D2F685"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.VectorTileCache.UI.Properties.Resources.tiles; }
        }

        #endregion
    }
}
