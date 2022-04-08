using gView.Framework.UI;
using System;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer.Icons
{
    public class VectorTileCacheDatasetIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("39D5D2CA-27BC-425B-BBCF-3CF59D56B220"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.VectorTileCache.UI.Properties.Resources.tiles; }
        }

        #endregion
    }
}
