using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace gView.Framework.UI.Dialogs.Network
{
    public partial class NetworkToleranceControl : UserControl
    {
        public NetworkToleranceControl()
        {
            InitializeComponent();
        }

        #region Properties

        public double Tolerance
        {
            get
            {
                if (chkUseSnapTolerance.Checked)
                    return (double)numSnapTolerance.Value;

                return double.Epsilon;
            }
            set
            {
                numSnapTolerance.Value = (decimal)value;
            }
        }

        public Serialized Serialize
        {
            get
            {
                return new Serialized()
                {
                    UseTolerance = chkUseSnapTolerance.Checked,
                    Tolerance = this.Tolerance
                };
            }
            set
            {
                if(value!=null)
                {
                    this.chkUseSnapTolerance.Checked = value.UseTolerance;
                    this.Tolerance = value.Tolerance;
                }
            }
        }

        #endregion

        #region Serializer Class

        public class Serialized
        {
            [JsonProperty(PropertyName = "use_tolerance")]
            public bool UseTolerance { get; set; }

            [JsonProperty(PropertyName = "tolerance")]
            public double Tolerance { get; set; }
        }

        #endregion
    }
}
