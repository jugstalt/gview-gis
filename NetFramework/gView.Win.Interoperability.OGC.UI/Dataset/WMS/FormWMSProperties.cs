using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Interoperability.OGC.Dataset.WMS;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    [RegisterPlugIn("25010B9F-1A01-41da-88AE-3446496DE416")]
    public partial class FormWMSProperties : Form, ILayerPropertyPage
    {
        private WMSClass _class = null;

        public FormWMSProperties()
        {
            InitializeComponent();
        }

        #region ILayerPropertyPage Member

        public Panel PropertyPage(IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (ShowWith(dataset, layer))
            {
                _class = (WMSClass)layer.Class;
                panel1.Dock = DockStyle.Fill;

                BuildGUI();
                return panel1;
            }
            else
            {
                return null;
            }
        }

        public bool ShowWith(IDataset dataset, gView.Framework.Data.ILayer layer)
        {
            if (layer is IWebServiceLayer &&
                ((IWebServiceLayer)layer).Class is WMSClass &&
                ((IWebServiceLayer)layer).Class.Dataset is WMSDataset)
            {
                return true;
            }
            return false;
        }

        public string Title
        {
            get { return "WMS"; }
        }

        public void Commit()
        {
            if (_class == null) return;

            _class.SRSCode = ((SrsItem)cmbCoordSystem.SelectedItem).Code;
            _class.GetMapFormat = cmbGetMapFormat.SelectedItem.ToString();
            _class.FeatureInfoFormat = cmbInfoFormat.SelectedItem.ToString();
            _class.UseSLD_BODY = chkUseSLD_BODY.Checked;
        }

        #endregion

        private void BuildGUI()
        {
            if (_class == null || _class.Dataset == null) return;

            if (_class.SRSCodes != null)
            {
                string selected = _class.SRSCode;
                foreach (string code in _class.SRSCodes)
                {
                    cmbCoordSystem.Items.Add(new SrsItem(code, _class.SRSCodes.Length < 100));
                    if (code == selected)
                        cmbCoordSystem.SelectedIndex = cmbCoordSystem.Items.Count - 1;
                }
            }

            if (_class.GetMapFormats != null)
            {
                string selected = _class.GetMapFormat;
                foreach (string format in _class.GetMapFormats)
                {
                    cmbGetMapFormat.Items.Add(format);
                    if (format == selected)
                        cmbGetMapFormat.SelectedIndex = cmbGetMapFormat.Items.Count - 1;
                }
            }

            if (_class.FeatureInfoFormats != null)
            {
                string selected = _class.FeatureInfoFormat;
                foreach (string format in _class.FeatureInfoFormats)
                {
                    cmbInfoFormat.Items.Add(format);
                    if (format == selected)
                        cmbInfoFormat.SelectedIndex = cmbInfoFormat.Items.Count - 1;
                }
            }

            chkUseSLD_BODY.Checked = _class.UseSLD_BODY;
            chkUseSLD_BODY.Enabled = _class.SupportSLD;
        }

        #region HelperClasses
        private class SrsItem
        {
            private string _code, _name;

            public SrsItem(string code, bool getDescription)
            {
                _code = _name = code;

                if (getDescription)
                {
                    ISpatialReference sRef = SpatialReference.FromID(_code);
                    if (sRef != null) _name = sRef.Description;
                }
            }

            public string Code
            {
                get
                {
                    return _code;
                }
            }
            public override string ToString()
            {
                return _name;
            }
        }
        #endregion
    }
}