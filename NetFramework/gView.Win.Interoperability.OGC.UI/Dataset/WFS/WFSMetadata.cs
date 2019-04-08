using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;

namespace gView.Interoperability.OGC.UI.Dataset.WFS
{
    public partial class WFSMetadata : UserControl, IPlugInParameter
    {
        private WFS_Export_Metadata _metadata = null;


        public WFSMetadata()
        {
            InitializeComponent();
        }

        #region IPlugInParameter Member

        public object Parameter
        {
            get
            {
                return _metadata;
            }
            set
            {
                _metadata = value as WFS_Export_Metadata;
                epsgSelector1.Metadata = value as IEPSGMetadata;
            }
        }

        #endregion
    }
}
