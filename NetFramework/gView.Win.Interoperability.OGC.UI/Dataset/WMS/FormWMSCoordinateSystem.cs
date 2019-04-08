using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Interoperability.OGC.Dataset.WMS;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.UI.Dataset.WMS
{
    public partial class FormWMSCoordinateSystem : Form
    {
        WMSClass _class = null;

        public FormWMSCoordinateSystem(WMSClass Class)
        {
            InitializeComponent();

            _class = Class;
            if (_class == null || _class.SRSCodes == null) return;

            string selected = _class.SRSCode;
            foreach (string code in _class.SRSCodes)
            {
                cmbCoordSystem.Items.Add(new Item(code, _class.SRSCodes.Length < 100));
                if (code == selected)
                    cmbCoordSystem.SelectedIndex = cmbCoordSystem.Items.Count - 1;
            }
        }

        public string SRSCode
        {
            get
            {
                return ((Item)cmbCoordSystem.SelectedItem).Code;
            }
        }

        private class Item
        {
            private string _code,_name;

            public Item(string code, bool getDescription)
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
    }
}