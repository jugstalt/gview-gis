using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using gView.Framework.system;
using gView.Web.Framework.Web.Authorization;

namespace gView.Win.Dialogs.Framework.UI.Controls.Wizard
{
    public partial class WebAuthorizationControl : UserControl
    {
        public WebAuthorizationControl()
        {
            InitializeComponent();

            foreach(var authType in Enum.GetValues(typeof(AuthorizationType)))
            {
                cmbAuthType.Items.Add(authType.ToString());
            }
            cmbAuthType.SelectedIndex = 0;
        }

        [Browsable(false)]
        public string ConnectionString
        {
            get
            {
                var webAuthCredentials = new WebAuthorizationCredentials(
                    txtUsername.Text,
                    txtPassword.Text,
                    (AuthorizationType)Enum.Parse(typeof(AuthorizationType), cmbAuthType.Text),
                    txtAuthTokenService.Text,
                    txtGrantType.Text,
                    txtScope.Text);

                return webAuthCredentials.ConnectionString;
            }
            set
            {
                var webAuthCredentials = new WebAuthorizationCredentials(value);

                txtUsername.Text = webAuthCredentials.Username;
                txtPassword.Text = webAuthCredentials.Password;

                cmbAuthType.Text = webAuthCredentials.AuthType.ToString();

                txtAuthTokenService.Text = webAuthCredentials.AccessTokenTokenServiceUrl;
                txtGrantType.Text = webAuthCredentials.GrantType;
                txtScope.Text = webAuthCredentials.Scope;
            }
        }
    }
}
