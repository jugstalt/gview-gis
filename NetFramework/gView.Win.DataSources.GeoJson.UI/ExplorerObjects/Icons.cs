using gView.Framework.UI;
using System;

namespace gView.Win.DataSources.GeoJson.UI.ExplorerObjects
{
    class GeoJsonServiceConnectionsIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("B2533892-923D-4FC7-AD45-115855D67C42");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.GeoJson.UI.Properties.Resources.json_16;
            }
        }

        #endregion
    }

    class GeoJsonServiceNewConnectionIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("CD88930C-1110-49C4-9F2A-A641E416E3A7");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.Win.DataSources.GeoJson.UI.Properties.Resources.gps_point;
            }
        }

        #endregion
    }

    public class GeoJsonServicePointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("D7256D73-2AC9-45F5-AF2B-E60DCFFC0041"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.GeoJson.UI.Properties.Resources.img_32; }
        }

        #endregion
    }

    public class GeoJsonServiceLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("02B673EB-463B-4C73-AA8B-3FFBFE31E9A6"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.GeoJson.UI.Properties.Resources.img_33; }
        }

        #endregion
    }

    public class GeoJsonServicePolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("02B673EB-463B-4C73-AA8B-3FFBFE31E9A6"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Win.DataSources.GeoJson.UI.Properties.Resources.img_34; }
        }

        #endregion
    }
}
