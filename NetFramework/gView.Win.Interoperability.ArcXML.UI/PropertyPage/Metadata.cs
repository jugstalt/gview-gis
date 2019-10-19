using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;

namespace gView.Interoperability.ArcXML.UI.PropertyPage
{
    public partial class Metadata : UserControl, IPlugInParameter
    {
        gView.Interoperability.ArcXML.Dataset.Metadata _metadata = null;

        public Metadata()
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
                _metadata = value as gView.Interoperability.ArcXML.Dataset.Metadata;

                if (_metadata != null)
                {
                    switch (_metadata.CredentialMethod)
                    {
                        case gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.none:
                            btnCredentialsIgnore.Checked = true;
                            break;
                        case gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.def:
                            btnCredentialsDefault.Checked = true;
                            break;
                        case gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.net:
                            btnCredentialsDefaultNet.Checked = true;
                            break;
                        case gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.custom:
                            btnCredentialCustom.Checked = true;
                            break;
                    }
                    txtDomain.Text = _metadata.CredentialDomain;
                    txtUser.Text = _metadata.CredentialUser;
                    txtPwd.Text = _metadata.CredentialPwd;

                    txtModifyFile.Text = _metadata.ModifyOutputFile;
                    txtModifyUrl.Text = _metadata.ModifyOutputUrl;
                }
            }
        }

        #endregion

        #region Events
        private void btnCredentialsIgnore_CheckedChanged(object sender, EventArgs e)
        {
            if (_metadata != null && btnCredentialsIgnore.Checked)
                _metadata.CredentialMethod = gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.none;
        }

        private void btnCredentialsDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (_metadata != null && btnCredentialsDefault.Checked)
                _metadata.CredentialMethod = gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.def;
        }

        private void btnCredentialsDefaultNet_CheckedChanged(object sender, EventArgs e)
        {
            if (_metadata != null && btnCredentialsDefaultNet.Checked)
                _metadata.CredentialMethod = gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.net;
        }

        private void btnCredentialCustom_CheckedChanged(object sender, EventArgs e)
        {
            if (_metadata != null && btnCredentialCustom.Checked) 
                _metadata.CredentialMethod = gView.Interoperability.ArcXML.Dataset.Metadata.credentialMethod.custom;

            panelCredentialsCustom.Enabled = btnCredentialCustom.Checked;
        }

        private void txtDomain_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.CredentialDomain = txtDomain.Text;
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.CredentialUser = txtUser.Text;
        }

        private void txtPwd_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.CredentialPwd = txtPwd.Text;
        }

        private void txtModifyFile_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.ModifyOutputFile = txtModifyFile.Text;
        }

        private void txtModifyUrl_TextChanged(object sender, EventArgs e)
        {
            if (_metadata != null)
                _metadata.ModifyOutputUrl = txtModifyUrl.Text;
        }
        #endregion
    }
}
