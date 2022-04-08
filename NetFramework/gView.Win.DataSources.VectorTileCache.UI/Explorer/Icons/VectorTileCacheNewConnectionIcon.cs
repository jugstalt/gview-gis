using gView.Framework.UI;
using System;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer.Icons
{
    public class VectorTileCacheNewConnectionIcon : IExplorerIcon
    {

        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("84528B6B-C383-4B90-8AE9-7718748DFF0A"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.VectorTileCache.UI.Properties.Resources.pointer_new; }
        }

        #endregion
    }
}
