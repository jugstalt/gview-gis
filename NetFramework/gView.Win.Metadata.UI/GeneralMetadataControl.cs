using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;

namespace gView.Framework.Metadata.UI
{
    public partial class GeneralMetadataControl : UserControl, IPlugInParameter
    {
        private GeneralMetadata _metadata = null;

        public GeneralMetadataControl()
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
                _metadata = value as GeneralMetadata;
            }
        }

        #endregion

        private void GeneralMetadataControl_Load(object sender, EventArgs e)
        {
            if (_metadata != null)
            {
                txtAbstract.Text = _metadata.Abstract;
                txtPurpose.Text = _metadata.Purpose;
                txtSupplInfo.Text = _metadata.Supplemental_Information;
                txtLanguage.Text = _metadata.Language;
                txtAccessConstraints.Text = _metadata.Access_Constraints;
                txtUseConstraints.Text = _metadata.Use_Constraints;
                txtContact.Text = _metadata.Contact;
                txtCredits.Text = _metadata.Credits;
            }
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Abstract = txtAbstract.Text;
        }

        private void txtPurpose_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Purpose = txtPurpose.Text;
        }

        private void txtSupplInfo_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Supplemental_Information = txtSupplInfo.Text;
        }

        private void txtLanguage_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Language = txtLanguage.Text;
        }

        private void txtAccessConstraints_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Access_Constraints = txtAccessConstraints.Text;
        }

        private void txtUseConstraints_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Use_Constraints = txtUseConstraints.Text;
        }

        private void txtContact_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Contact = txtContact.Text;
        }

        private void txtCredits_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.Credits = txtCredits.Text;
        }
    }
}
