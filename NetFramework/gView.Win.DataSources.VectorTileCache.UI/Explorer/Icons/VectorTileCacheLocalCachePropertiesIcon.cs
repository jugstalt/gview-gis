using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer.Icons
{
    public class VectorTileCacheLocalCachePropertiesIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("B410A260-2800-4DB6-BAC1-3E570C794836"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.VectorTileCache.UI.Properties.Resources.properties; }
        }

        #endregion
    }
}
