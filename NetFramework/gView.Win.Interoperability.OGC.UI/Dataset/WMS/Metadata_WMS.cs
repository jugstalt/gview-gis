using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Interoperability.OGC.Dataset.WMS;

namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    public partial class Metadata_WMS : UserControl, IPlugInParameter
    {
        private WMSImportMetadata _metadata = null;
        public Metadata_WMS()
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
                _metadata = value as WMSImportMetadata;
            }
        }

        #endregion

        private void Metadata_WMS_Load(object sender, EventArgs e)
        {
            if (_metadata == null) return;

            if (_metadata.SRSCodes != null)
            {
                foreach (string srsCode in _metadata.SRSCodes)
                {
                    cmbCoordSystem.Items.Add(srsCode);
                    if (srsCode == _metadata.SRSCode)
                        cmbCoordSystem.SelectedIndex = cmbCoordSystem.Items.Count - 1;
                }
                if (cmbCoordSystem.SelectedIndex == -1 && cmbCoordSystem.Items.Count > 0)
                    cmbCoordSystem.SelectedIndex = 0;
            }

            if (_metadata.FeatureInfoFormats != null)
            {
                foreach (string featureInfo in _metadata.FeatureInfoFormats)
                {
                    cmbInfoFormat.Items.Add(featureInfo);
                    if (featureInfo == _metadata.FeatureInfoFormat)
                        cmbInfoFormat.SelectedIndex = cmbInfoFormat.Items.Count - 1;
                }
                if (cmbInfoFormat.SelectedIndex == -1 && cmbInfoFormat.Items.Count > 0)
                    cmbInfoFormat.SelectedIndex = 0;
            }

            if (_metadata.GetMapFormats != null)
            {
                foreach (string getMapInfo in _metadata.GetMapFormats)
                {
                    cmbGetMapFormat.Items.Add(getMapInfo);
                    if (getMapInfo == _metadata.GetMapFormat)
                        cmbGetMapFormat.SelectedIndex = cmbGetMapFormat.Items.Count - 1;
                }
                if (cmbGetMapFormat.SelectedIndex == -1 && cmbGetMapFormat.Items.Count > 0)
                    cmbGetMapFormat.SelectedIndex = 0;
            }
        }

        private void cmbCoordSystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.SRSCode = cmbCoordSystem.SelectedItem.ToString();
        }

        private void cmbGetMapFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.GetMapFormat = cmbGetMapFormat.SelectedItem.ToString();
        }

        private void cmbInfoFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.FeatureInfoFormat = cmbInfoFormat.SelectedItem.ToString();
        }
    }
}
